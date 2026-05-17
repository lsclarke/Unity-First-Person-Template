using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Multiplayer.PlayMode.Editor;
using Unity.Multiplayer.PlayMode.Scenarios.Editor.ExtensionApi.Instances.MultiplayLocalDevelopment.Services.ProxyServer;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.API;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment;
using Unity.Services.Core.Editor.Environments;
using Unity.Services.DeploymentApi.Editor;
using Unity.Services.Multiplay.Authoring.Core.Assets;
using Unity.Services.Multiplay.Authoring.Core.Deployment;
using Unity.Services.Multiplay.Authoring.Core.Model;
using Unity.Services.Multiplay.Authoring.Core.MultiplayApi;
using Unity.Services.Multiplay.Authoring.Editor;
using Unity.Services.Multiplayer.Editor.Shared.DependencyInversion;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using static Unity.Multiplayer.PlayMode.Editor.IPlayModeServices;

namespace Unity.Multiplayer.PlayMode.Services.Editor
{
    internal class PlayModeServices : IPlayModeServices
    {
        private const float k_LocalServerStartTimeoutSeconds = 60;
        // This cache is a workaround to avoid making changes to the Engine.
        // It stores the last created server info for a given projectId+environmentId pair
        // so that we can reuse them when we need to provide headers for the SimProxyWrapper
        private const string k_ConnectionCacheKeyPrefix = "PlayModeServices.ConnectionCache.";

        protected IScopedServiceProvider CreateServiceProviderScope()
            => MultiplayAuthoringServices.Provider.CreateScope();

        private static void SetConnectionCache(string cacheKey, ServerInfoConnectionV5 connection)
        {
            if (connection == null) return;

            var serializable = new ConnectionCacheData(connection);
            var json = JsonUtility.ToJson(serializable);
            SessionState.SetString(k_ConnectionCacheKeyPrefix + cacheKey, json);
        }

        private static Dictionary<string, string> GetConnectionHeaders(string cacheKey)
        {
            var json = SessionState.GetString(k_ConnectionCacheKeyPrefix + cacheKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                var connection = JsonUtility.FromJson<ConnectionCacheData>(json);
                return connection?.GetHeadersDictionary();
            }
            catch (Exception e)
            {
                // If deserialization fails, clear the cache entry and log a warning
                UnityEngine.Debug.LogWarning($"Failed to deserialize connection cache for key '{cacheKey}'. Clearing entry. Error: {e.Message}");
                SessionState.EraseString(k_ConnectionCacheKeyPrefix + cacheKey);
                return null;
            }
        }

        private async Task ValidateEnvironment(IScopedServiceProvider provider)
        {
            var environments = provider.GetService<IEnvironmentsApi>();
            await environments.RefreshAsync();
            var environmentValidation = await environments.ValidateEnvironmentAsync();
            if (environmentValidation.Failed)
                throw new InvalidOperationException(environmentValidation.ErrorMessage);
        }

        protected async Task<IScopedServiceProvider> CreateAndValidateServiceProvider()
        {
            var provider = CreateServiceProviderScope();
            try
            {
                await ValidateEnvironment(provider);
                return provider;
            }
            catch (Exception)
            {
                provider.Dispose();
                throw;
            }
        }

        public async Task<CreateAndSyncTestAllocationResult> CreateAndSyncTestAllocationAsync(
            string fleetName,
            string buildConfigurationName,
            CancellationToken cancellationToken)
        {
            using var provider = await CreateAndValidateServiceProvider();
            var deployer = provider.GetService<IMultiplayDeployer>();
            await deployer.InitAsync();

            var info = await deployer.CreateAndSyncTestAllocationAsync(
                new FleetName { Name = fleetName },
                new BuildConfigurationName { Name = buildConfigurationName },
                cancellationToken);

            if (info == null)
            {
                throw new InvalidOperationException(
                    "Failed to create and sync test allocation. Allocation information is null.");
            }

            return new CreateAndSyncTestAllocationResult
            {
                ServerId = info.ServerId,
                Ipv4Address = info.Ipv4Address,
                GamePort = (ushort)info.GamePort
            };
        }

        public async Task<DeployBuildConfigsResult> DeployBuildConfigurationAsync(
            long buildId,
            string buildConfigurationName,
            string buildName,
            string binaryPath,
            string commandLineArguments,
            int coresCount,
            int memoryMiB,
            int speedMhz,
            Action<float> onProgress,
            CancellationToken cancellationToken)
        {
            var buildConfigItem = new BuildConfigurationItem()
            {
                OriginalName = new BuildConfigurationName
                {
                    Name = buildConfigurationName
                },
                Definition = new MultiplayConfig.BuildConfigurationDefinition
                {
                    Build = new BuildName
                    {
                        Name = buildName
                    },
                    BinaryPath = binaryPath,
                    CommandLine = commandLineArguments,
                    Cores = coresCount,
                    MemoryMiB = memoryMiB,
                    SpeedMhz = speedMhz,
                    QueryType = MultiplayConfig.Query.None
                }
            };

            buildConfigItem.PropertyChanged += (sender, args) =>
            {
                onProgress?.Invoke(((BuildConfigurationItem)sender).Progress);
            };

            using var scope = await CreateAndValidateServiceProvider();

            var deployer = scope.GetService<IMultiplayDeployer>();
            await deployer.InitAsync();

            var (buildConfigIds, failedBuildConfigs) = await deployer.DeployBuildConfigs(
                new List<BuildConfigurationItem> { buildConfigItem },
                new Dictionary<BuildName, BuildId>() {
                {
                    new BuildName { Name = buildConfigItem.Definition.Build.Name },
                    new BuildId { Id = buildId }
                }},
                cancellationToken);

            // Validate
            if (failedBuildConfigs.Count > 0)
            {
                var errors = failedBuildConfigs.Select(
                    bi => bi.Status.Message
                ).ToList();
                var errList = string.Join("; ", errors);
                throw new Exception($"Failed to deploy the build configuration\n{errList}");
            }

            return new DeployBuildConfigsResult
            {
                BuildConfigurationId = buildConfigIds[buildConfigItem.OriginalName].Id
            };
        }

        public async Task<UploadAndSyncBuildsResult> UploadAndSyncBuildsAsync(
            string buildName,
            string buildPath,
            string executablePath,
            Action<float> onProgress,
            CancellationToken cancellationToken)
        {
            using var provider = await CreateAndValidateServiceProvider();
            var deployer = provider.GetService<IMultiplayDeployer>();
            await deployer.InitAsync();

            var buildItem = new BuildItem()
            {
                OriginalName = new BuildName
                {
                    Name = buildName
                },
                Definition = new MultiplayConfig.BuildDefinition
                {
                    BuildPath = buildPath,
                    ExecutableName = Path.GetFileName(executablePath)
                }
            };
            buildItem.PropertyChanged += (sender, args) =>
            {
                onProgress?.Invoke(((BuildItem)sender).Progress);
            };

            var uploadResult = await deployer.UploadAndSyncBuilds(new List<BuildItem> { buildItem }, cancellationToken);

            // Validate upload
            if (uploadResult.FailedUploads.Count > 0)
            {
                var errors = uploadResult.FailedUploads.Select(
                    bi => bi.Status.Message
                ).ToList();
                var errList = string.Join("; ", errors);
                ValidationErrors.LogMultiplaySetupErrors();
                throw new Exception(
                    $"Failed to upload the build. Have you configured the Multiplay Service for this project?\n{errList}");
            }

            if (uploadResult.FailedSyncs.Count > 0)
            {
                var errors = uploadResult.FailedSyncs.Select(
                    bi => bi.Status.Message
                ).ToList();
                var errList = string.Join("; ", errors);
                throw new Exception($"Failed to sync the build\n{errList}");
            }

            return new UploadAndSyncBuildsResult
            {
                BuildId = uploadResult.SuccessfulSyncs[buildItem.OriginalName].Id,
            };
        }

        public async Task<GetVersionOfBuildResult> GetVersionOfBuildAsync(
            long buildId,
            CancellationToken cancellationToken)
        {
            using var provider = await CreateAndValidateServiceProvider();
            var deployer = provider.GetService<IMultiplayDeployer>();
            await deployer.InitAsync();

            var builds = await deployer.GetBuilds(cancellationToken);
            foreach (var build in builds)
            {
                if (build.BuildId.Id == buildId)
                {
                    // TODO: We don't have access to the version of the build in the API yet.
                    // For now, the updated time would do the same.
                    return new GetVersionOfBuildResult
                    {
                        Version = build.Updated.ToBinary()
                    };
                }
            }

            return new GetVersionOfBuildResult
            {
                Version = -1
            };
        }

        public async Task DeployFleetsAsync(
            string fleetName,
            string region,
            string architecture,
            string buildConfigurationName,
            long buildConfigurationId,
            Action<float> onProgress,
            CancellationToken cancellationToken)
        {
            var fleetItem = new FleetItem()
            {
                OriginalName = new() { Name = fleetName },
                Definition = new()
                {
                    BuildConfigurations = new List<BuildConfigurationName>()
                    {
                        new() { Name = buildConfigurationName }
                    },
                    Regions = new Dictionary<string, MultiplayConfig.ScalingDefinition>()
                    {
                        {
                            region, new MultiplayConfig.ScalingDefinition
                            {
                                MaxServers = 1,
                                MinAvailable = 0,
                            }
                        }
                    },
                    Architecture = architecture == "arm64"
                        ? MultiplayConfig.FleetDefinition.CpuArchitectureOptions.Arm64
                        : MultiplayConfig.FleetDefinition.CpuArchitectureOptions.Amd64,
                }
            };

            fleetItem.PropertyChanged += (sender, args) =>
            {
                onProgress?.Invoke(((FleetItem)sender).Progress);
            };

            using var provider = await CreateAndValidateServiceProvider();
            var deployer = provider.GetService<IMultiplayDeployer>();
            await deployer.InitAsync();

            var fleets = new List<FleetItem> { fleetItem };
            var buildConfigIds = new Dictionary<BuildConfigurationName, BuildConfigurationId>
            {
                {
                    new BuildConfigurationName { Name = buildConfigurationName },
                    new BuildConfigurationId { Id = buildConfigurationId }
                }
            };
            await deployer.DeployFleets(fleets, buildConfigIds, cancellationToken);

            if (fleetItem.Status.MessageSeverity == SeverityLevel.Error)
            {
                throw new Exception($"Failed to deploy the fleet.\n{fleetItem.Status.MessageDetail}");
            }
        }

        public async Task RunFleetServersAsync(
            long serverId,
            CancellationToken cancellationToken)
        {
            using var provider = await CreateAndValidateServiceProvider();
            var servers = provider.GetService<IServersApi>();
            await servers.InitAsync();

            var success = await servers.TriggerServerActionAsync(serverId, ServerAction.Start, cancellationToken);
            if (!success)
                throw new Exception($"Failed to start server {serverId}");
        }

        public async Task MonitorRunningServersAsync(
            long serverId,
            ServerLogConfiguration logConfig,
            Action<bool> onServerRunStateChanged,
            CancellationToken cancellationToken)
        {
            using var provider = await CreateAndValidateServiceProvider();

            var serversApi = provider.GetService<IServersApi>();
            await serversApi.InitAsync();

            var logsApi = provider.GetService<ILogsApi>();
            await logsApi.InitAsync();

            var serverInfo = await serversApi.GetServerAsync(serverId, cancellationToken);
            var fleetId = serverInfo.FleetID;
            var isRunning = false;
            List<string> logsCache = new List<string>();
            while (!cancellationToken.IsCancellationRequested)
            {
                if (logConfig.StreamLogs)
                    await StreamLogsStepAsync(logsCache, logConfig, logsApi, fleetId, serverId, cancellationToken);

                // Grab and update the current running state of the servers. Notify callbacks as needed.
                serverInfo = await serversApi.GetServerAsync(serverId, cancellationToken);
                var updatedIsRunning = serverInfo.Status is ServerStatus.ONLINE or ServerStatus.ALLOCATED;
                if (updatedIsRunning != isRunning)
                {
                    onServerRunStateChanged(updatedIsRunning);
                    isRunning = updatedIsRunning;
                }

                // Grab the Server action logs
                if (!isRunning)
                {
                    Debug.LogWarning($"Server {serverId} is not online anymore. Review the logs for more details.");

                    var logs = await serversApi.GetServerActionLogsAsync(serverId, cancellationToken);
                    foreach (var log in logs)
                    {
                        Debug.Log($"Time: {log.Date} Server {serverId} log: {log.Message}, attachment: {log.Attachment}");
                    }
                    break;
                }

                Assert.IsTrue(logConfig.MonitorStepIntervalMS > 0, "Monitor step interval must be greater than 0");

                await Task.Delay(logConfig.MonitorStepIntervalMS, cancellationToken);
                await Task.Yield();
            }
        }

        private async Task StreamLogsStepAsync(List<string> logCache,
                                               ServerLogConfiguration logConfig,
                                               ILogsApi logsApi,
                                               Guid fleetId,
                                               long serverId,
                                               CancellationToken cancellationToken)
        {
            var now = DateTime.Now;
            var paginationToken = string.Empty;

            logCache.Clear();

            // Collect the logs, potentially through multiple requests if there are many logs.
            do
            {
                var searchParams = new LogSearchParams(fleetId, serverId, "", logConfig.MaxLogsPerRequest, DateTime.FromBinary(logConfig.LastLogsTime), now, paginationToken);
                var result = await logsApi.SearchLogsAsync(searchParams, cancellationToken);

                if (result.Entries != null)
                {
                    foreach (var log in result.Entries)
                        logCache.Add(log.Message);
                }

                paginationToken = result.Count == 0 ? string.Empty : result.PaginationToken;
            } while (!string.IsNullOrEmpty(paginationToken) && !cancellationToken.IsCancellationRequested);

            // Logs are received from newest to oldest, so we print them in reverse order.
            for (int i = logCache.Count - 1; i >= 0; i--)
                logConfig.OnRecievedlLog(logCache[i]);

            logConfig.LastLogsTime = now.ToBinary();
            logCache.Clear();
        }

        public async Task StopFleetServersAsync(long serverId)
        {
            using var provider = await CreateAndValidateServiceProvider();
            var servers = provider.GetService<IServersApi>();
            await servers.InitAsync();

            var success = await servers.TriggerServerActionAsync(serverId, ServerAction.Stop);
            if (!success)
                Debug.LogError($"Failed to stop server {serverId}");
        }

        public async Task<InitaliseSimFleetResult> InitialiseSimFleetAsync(
            string projectId,
            string environmentId,
            string authToken,
            bool autoAllocate,
            string queryType,
            string localHost,
            string localPort,
            CancellationToken cancellationToken)
        {
            MultiplayHostingV5Service multiplayAPI = new();
            var data = await CreateOrGetFleet(projectId, environmentId, authToken, queryType, localHost, localPort, multiplayAPI, cancellationToken);

            var fleetId = data.Fleet?.Metadata?.Id;
            if (string.IsNullOrEmpty(fleetId))
            {
                throw new Exception("Fleet could not be found or created");
            }

            var allocationId = "";
            if (autoAllocate)
            {
                allocationId = Guid.NewGuid().ToString();
                await multiplayAPI.AllocateAsync(
                    projectId,
                    environmentId,
                    fleetId,
                    new AllocationRequestV5(allocationId, Array.Empty<string>()),
                    authToken);
            }

            return new InitaliseSimFleetResult
            {
                FleetID = fleetId,
                FleetName = data.Fleet?.Metadata?.Name,
                ServerLocation = data.Server?.LocationId,
                ServerRemoteHost = data.Server?.Connections?.Length > 0 ? data.Server.Connections[0].Host : "",
                ServerRemotePort = data.Server?.Connections?.Length > 0 ? data.Server.Connections[0].Port : 0,
                ServerState = data.Server?.State,
                AllocationId = allocationId,
                ServerId = data.Server?.ServerId,
            };
        }

        private async Task<FleetAndServerDataV5> CreateOrGetFleet(
            string projectId,
            string environmentId,
            string authToken,
            string queryType,
            string localHost,
            string localPort,
            MultiplayHostingV5Service multiplayAPI,
            CancellationToken cancellationToken)
        {
            // Create a cache key based on projectId and environmentId
            string cacheKey = $"{projectId}:{environmentId}";

            var fleet = await multiplayAPI.PickLocalModeFleetAsync(
                projectId,
                environmentId,
                authToken,
                queryType,
                localHost,
                localPort
            );

            ServerInfoV5 server;
            if (fleet != null)
            {
                if (string.IsNullOrEmpty(fleet.Metadata?.Id))
                {
                    throw new Exception("Found fleet has an empty ID");
                }

                server = await multiplayAPI.GetReadyServerForFleet(
                    projectId,
                    environmentId,
                    fleet.Metadata?.Id,
                    authToken
                );

                if (server != null)
                {
                    var result = new FleetAndServerDataV5(fleet, server);
                    // Cache the connection data for SimProxyWrapper
                    if (server.Connections?.Length > 0)
                    {
                        SetConnectionCache(cacheKey, server.Connections[0]);
                    }
                    return result;
                }
            }

            // If we're here, it means either there are no fleets, or the selected fleet has no servers. So create a new one.
            fleet = await multiplayAPI.CreateFleetAsync(
                projectId,
                environmentId,
                FleetSpecUtils.CreateLocalFleetSpec(queryType, localHost, localPort),
                authToken);

            if (string.IsNullOrEmpty(fleet.Metadata?.Id))
            {
                throw new Exception("Found fleet has an empty ID");
            }

            server = await WaitUntilLocalServerStartedAsync(
                multiplayAPI,
                projectId,
                environmentId,
                fleet.Metadata?.Id,
                authToken,
                cancellationToken
            );

            var finalResult = new FleetAndServerDataV5(fleet, server);
            // Cache the connection data for SimProxyWrapper
            if (server.Connections?.Length > 0)
            {
                SetConnectionCache(cacheKey, server.Connections[0]);
            }
            return finalResult;
        }

        private async Task<ServerInfoV5> WaitUntilLocalServerStartedAsync(MultiplayHostingV5Service multiplayAPI,
            string projectId,
            string environmentId,
            string fleetId,
            string accessToken,
            CancellationToken cancellationToken)
        {
            var start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup < start + k_LocalServerStartTimeoutSeconds)
            {
                try
                {
                    var serverInfo = await multiplayAPI.GetReadyServerForFleet(
                        projectId,
                        environmentId,
                        fleetId,
                        accessToken);

                    if (serverInfo?.State == "Ready")
                    {
                        return serverInfo;
                    }
                }
                catch (Exception e)
                {
                    // We don't want to retry if the API raises an error, as it might be a permanent issue.
                    throw new Exception($"Failed to setup the local sever: {e.Message}", e);
                }

                await Task.Delay(1000, cancellationToken);
            }

            throw new Exception("Server did not start in time");
        }

        public async Task InitialiseSimServerJsonAsync(string fleetId,
            string serverId,
            string allocationId,
            string regionId,
            string ip,
            string localPort,
            string queryPort,
            string queryType,
            CancellationToken cancellationToken)
        {
            await ServerJsonService.Upsert(ServerJson.CreateWithValues(fleetId,
                serverId,
                allocationId,
                regionId,
                ip,
                localPort,
                queryPort,
                queryType), cancellationToken);
        }

        private bool ValidateCloudProject()
        {
            return !string.IsNullOrEmpty(CloudProjectSettings.projectId);
        }

        public async Task<SetupSimEnvironmentResult> SetupSimEnvironmentAsync(CancellationToken cancellationToken)
        {
            if (!ValidateCloudProject())
            {
                ValidationErrors.LogCloudSetupErrors();
                throw new InvalidOperationException("Project is not connected to Unity Cloud");
            }

            var environmentValidation = await EnvironmentsApi.Instance.ValidateEnvironmentAsync();
            if (environmentValidation.Failed)
            {
                ValidationErrors.LogEnvironmentSetupErrors();
                throw new InvalidOperationException("Unity Cloud environment not specified");
            }

            var activeEnv = EnvironmentsApi.Instance.ActiveEnvironmentId;
            if (activeEnv == null || string.IsNullOrEmpty(activeEnv.ToString()) || activeEnv == Guid.Empty)
            {
                ValidationErrors.LogEnvironmentSetupErrors();
                throw new InvalidOperationException("Unity Cloud environment not specified");
            }

            var token = await CloudProjectSettings.GetServiceTokenAsync(cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                ValidationErrors.LogServiceTokenErrors();
                throw new InvalidOperationException("Failed to get Unity Cloud service token");
            }

            return new SetupSimEnvironmentResult()
            {
                AuthToken = token,
                EnvironmentId = activeEnv.ToString(),
                ProjectId = CloudProjectSettings.projectId
            };
        }

        public async Task MonitorSimFleetAsync(
            bool waitForEditorPlaying,
            string projectId,
            string environmentId,
            string authToken,
            string allocationId,
            string serverHost,
            int serverPort,
            CancellationToken cancellationToken)
        {
            if (waitForEditorPlaying)
            {
                while (!EditorApplication.isPlaying) { await Task.Delay(100, cancellationToken); }
            }

            // Check cache for headers
            string cacheKey = $"{projectId}:{environmentId}";
            var headers = GetConnectionHeaders(cacheKey);

            var simProxy = new SimProxyWrapper(environmentId, serverHost, serverPort, headers);
            await simProxy.RunProxyAndWait(cancellationToken);

            try
            {
                if (!string.IsNullOrEmpty(allocationId))
                {
                    await new MultiplayHostingV5Service().DeallocateAsync(projectId, environmentId, allocationId, authToken);
                }
            }
            catch (Exception e)
            {
                MppmLog.Debug($"Failed to deallocate on exit: {e.Message}.");
            }
        }

        public async Task<AllocateServerInFleetResult> AllocateServerInSimFleetAsync(
            string projectId,
            string environmentId,
            string fleetId,
            string authToken,
            CancellationToken cancellationToken)
        {
            // Allocate the game server
            var allocationId = Guid.NewGuid().ToString();
            MultiplayHostingV5Service multiplayAPI = new();
            await multiplayAPI.AllocateAsync(
                projectId,
                environmentId,
                fleetId,
                new AllocationRequestV5(allocationId, Array.Empty<string>()),
                authToken);

            // Update Allocated server Id as needed in corresponding services
            var serverJson = await ServerJsonService.Read(cancellationToken);
            serverJson.AllocationId = allocationId;
            await ServerJsonService.Upsert(serverJson, cancellationToken);

            // Return the final allocated server Id
            return new AllocateServerInFleetResult()
            {
                AllocationId = allocationId
            };
        }

        public async Task DeallocateServerInSimFleetAsync(
            string projectId,
            string environmentId,
            string allocationId,
            string authToken,
            CancellationToken cancellationToken)
        {
            // Dellocate the game server
            await new MultiplayHostingV5Service().DeallocateAsync(projectId, environmentId, allocationId, authToken);

            // Update Allocated server Id as needed in corresponding services
            var serverJson = await ServerJsonService.Read(cancellationToken);
            serverJson.AllocationId = String.Empty;
            await ServerJsonService.Upsert(serverJson, cancellationToken);
        }

        public async Task<QueryServerMetricsResult> QueryServerMetricsAsync(
            SimulatorSettings.ProtocolType queryProtocol,
            CancellationToken cancellationToken)
        {
            var multiplayAPI = new MultiplayHostingV5Service();
            var queryState = await multiplayAPI.QueryServerMetrics(queryProtocol);

            if (queryState == null)
            {
                throw new InvalidOperationException("Failed to query server metrics. No data received.");
            }

            return new QueryServerMetricsResult
            {
                CurrentPlayers = queryState.CurrentPlayers,
                MaxPlayers = queryState.MaxPlayers,
                ServerName = queryState.ServerName,
                GameType = queryState.GameType,
                BuildId = queryState.BuildId,
                Map = queryState.Map,
                Port = queryState.Port
            };
        }

        public void ClearSimFleetAllocationLogs()
        {
            ServerJsonService.Delete();
        }
    }
}
