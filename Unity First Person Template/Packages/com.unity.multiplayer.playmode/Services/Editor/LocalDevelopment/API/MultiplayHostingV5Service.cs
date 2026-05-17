using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model;
using UnityEngine;
using UnityEngine.Networking;
using static Unity.Multiplayer.PlayMode.Editor.SimulatorSettings;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.API
{

    internal class MultiplayHostingV5Service
    {
        static readonly string k_InternalBaseUrl = $"{CloudEnvironment.GetHost()}/api/multiplay";

        static readonly JsonSerializerSettings k_JsonSerializerSettings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private static async Task<T> DoRequest<T>(string token, string verb, string url, string body = null)
        {
            using var www = new UnityWebRequest(url, verb);
            if (body != null)
            {
                www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            }
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {token}");
            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                throw new Exception($"{url} request failed: {www.error} (requestID: {www.GetResponseHeader("x-request-id")})");

            var responseStr = www.downloadHandler.text;
            var responseObj = JsonConvert.DeserializeObject<T>(responseStr, k_JsonSerializerSettings);
            return responseObj!;
        }

        public async Task<FleetSpecV5> CreateFleetAsync(
            string projectId,
            string environmentId,
            FleetSpecV5 inSpec,
            string accessToken)
        {
            var body = JsonConvert.SerializeObject(inSpec);
            var uri = $"{k_InternalBaseUrl}/fleets/v5/projects/{projectId}/environments/{environmentId}/local-fleet";

            var responseObj = await DoRequest<FleetSpecV5>(accessToken, UnityWebRequest.kHttpVerbPOST, uri, body);
            return responseObj!;
        }

        [ItemCanBeNull]
        public async Task<FleetSpecV5> PickLocalModeFleetAsync(
            string projectId,
            string environmentId,
            string accessToken,
            string queryType,
            string localHost,
            string localPort)
        {
            var labels = new[]
            {
                $"{FleetSpecUtils.k_FleetLabelLocalDevelopment}={FleetSpecUtils.k_FleetLabelValueLocalPlacementType}",
                $"{FleetSpecUtils.k_FleetLabelLocalServerIP}={localHost}",
                $"{FleetSpecUtils.k_FleetLabelLocalServerPort}={localPort}",
                $"{FleetSpecUtils.k_FleetLabelBuildConfigQueryType}={queryType}"
            };
            var labelSelector = string.Join(",", labels);
            var encodedLabelSelector = UnityWebRequest.EscapeURL(labelSelector);
            var uri = $"{k_InternalBaseUrl}/fleets/v5/projects/{projectId}/environments/{environmentId}/fleets?labelSelector={encodedLabelSelector}";
            var listFleetsResp = await DoRequest<FleetListV5>(accessToken, UnityWebRequest.kHttpVerbGET, uri);

            if (listFleetsResp == null || listFleetsResp.Results!.Length == 0)
            {
                return null;
            }

            // Pick the first one in the list to re-use. Consider making this a random one?
            var chosenFleetId = listFleetsResp.Results![0].Id;
            uri = $"{k_InternalBaseUrl}/fleets/v5/projects/{projectId}/environments/{environmentId}/fleets/{chosenFleetId}";
            var getFleetResp = await DoRequest<FleetSpecV5>(accessToken, UnityWebRequest.kHttpVerbGET, uri);
            return getFleetResp!;
        }

        [ItemCanBeNull]
        public async Task<ServerInfoV5> GetReadyServerForFleet(
            string projectId,
            string environmentId,
            string fleetId,
            string accessToken)
        {
            var uri = $"{k_InternalBaseUrl}/servers/v5/projects/{projectId}/environments/{environmentId}/servers?filters[fleetId]={fleetId}&filters[state]=Ready";
            var responseObj = await DoRequest<ServerListV5>(accessToken, UnityWebRequest.kHttpVerbGET, uri);
            if (responseObj == null || responseObj.Results!.Length == 0)
            {
                return null;
            }

            return responseObj.Results?[0];
        }

        public async Task AllocateAsync(
            string projectId,
            string environmentId,
            string fleetId,
            AllocationRequestV5 allocationRequest,
            string accessToken)
        {
            var body = JsonConvert.SerializeObject(allocationRequest);
            var uri = $"{k_InternalBaseUrl}/allocations/v5/projects/{projectId}/environments/{environmentId}/fleets/{fleetId}/allocations";
            using var www = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbPOST);
            www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                throw new Exception($"{nameof(AllocateAsync)} failed: {www.error} (requestID: {www.GetResponseHeader("x-request-id")})");
        }

        public async Task DeallocateAsync(
            string projectId,
            string environmentId,
            string allocationId,
            string accessToken)
        {
            var uri = $"{k_InternalBaseUrl}/allocations/v5/projects/{projectId}/environments/{environmentId}/allocations/{allocationId}";
            using var www = new UnityWebRequest(uri, UnityWebRequest.kHttpVerbDELETE);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", $"Bearer {accessToken}");

            await www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                throw new Exception($"{nameof(DeallocateAsync)} failed: {www.error} (requestID: {www.GetResponseHeader("x-request-id")})");
        }

        [ItemCanBeNull]
        public async Task<QueryState> QueryServerMetrics(ProtocolType queryProtocol)
        {
            string ip = null;
            int port = 0;
            try
            {
                const string k_filename = "server.json";
                var file = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/{k_filename}";

                if (File.Exists(file))
                {
                    using var r = new StreamReader(file);
                    var content = await r.ReadToEndAsync();
                    var json = JsonConvert.DeserializeObject<ServerJson>(content);
                    ip = json.Ip;
                    port = int.Parse(json.QueryPort);
                }
                else
                {
                    throw new Exception("The $HOME/server.json file was not found");
                }

                IGameServerQueryer client = queryProtocol.Equals(ProtocolType.A2S) ? new A2SClient(ip, port) : new SqpClient(ip, port);
                byte flags = queryProtocol.Equals(ProtocolType.A2S) ? new byte() : SqpPacket.RequestServerInfo;
                QueryState state = client.Query(flags);
                return state;
            }
            catch (Exception e)
            {
                throw new Exception($"Error caught while retrieving metrics via {queryProtocol} query from {ip}:{port}: {e.Message}", e);
            }
        }
    }
}
