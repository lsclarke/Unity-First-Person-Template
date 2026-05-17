using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Multiplayer.PlayMode.Editor;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types;
using UnityEditor;

namespace Unity.Multiplayer.PlayMode.Scenarios.Editor.ExtensionApi.Instances.MultiplayLocalDevelopment.Services.ProxyServer
{
    /// <summary>
    /// A wrapper class for handling Local Simulator specific Local Proxy events and callbacks.
    /// This is helps keep IPlayModeServices more lightweight and to avoid polluting it with
    /// Proxy-specific function calls.
    /// </summary>
    internal class SimProxyWrapper
    {
        private readonly string m_EnvironmentId;
        private readonly string m_ServerHost;
        private readonly int m_ServerPort;
        private readonly Dictionary<string, string> m_Headers;
        internal SimProxyWrapper(string environmentId, string serverHost, int serverPort, Dictionary<string, string> headers = null)
        {
            m_EnvironmentId = environmentId;
            m_ServerHost = serverHost;
            m_ServerPort = serverPort;
            m_Headers = headers;
        }

        internal async Task RunProxyAndWait(CancellationToken cancellationToken)
        {
            LocalProxy proxy = null;
            try
            {
                proxy = await RunProxyAsync(m_EnvironmentId, m_ServerHost, m_ServerPort, cancellationToken);

                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Expected when task is cancelled. Do nothing.
                }
            }
            catch (Exception e)
            {
                MppmLog.Debug($"An error occurred during proxy run: {e.Message}.");
                throw;
            }
            finally
            {
                proxy?.Dispose();
            }
        }

        private async Task<LocalProxy> RunProxyAsync(
            string activeEnv,
            string serverHost,
            int serverPort,
            CancellationToken cancellationToken)
        {
            if (activeEnv == null)
            {
                throw new Exception("No active environment found");
            }

            WebsocketLogger logger = new();
            RemoteLocalProxyService remoteLocalProxy = new(
                new HttpClient(),
                CloudProjectSettings.projectId,
                activeEnv
            );
            if (m_Headers != null) remoteLocalProxy.SetHeaders(m_Headers);
            var proxy = new LocalProxy(logger, remoteLocalProxy);
            proxy.MessageReceived += (_, args) =>
            {
                // We handle only text messages
                if (args.MessageType is not WebSocketMessageType.Text)
                {
                    return;
                }

                var message = args.ReadTextMessage();
                HandleWebsocketEvent(message);
            };

            await proxy.StartAsync(
                new LocalProxyConfig
                {
                    UpstreamHost = $"{serverHost}:{serverPort}",
                    ServerAddress = IPAddress.Loopback,
                    ServerPort = 8086,
                    ClientAuth = new WebsocketClientAuth(CloudProjectSettings.projectId, activeEnv.ToString()),
                    Headers = m_Headers
                }, cancellationToken);

            if (proxy.ProxyState is not ProxyState.Awaiting)
            {
                proxy.Dispose();
                throw new Exception("Proxy could not connect");
            }

            return proxy;
        }

        private void HandleWebsocketEvent(string message)
        {
            try
            {
                var websocketEventV4 = JsonConvert.DeserializeObject<WebsocketEventV4>(message);

                if (websocketEventV4?.Result?.Channel != null)
                {
                    HandleV4WebsocketEvent(websocketEventV4);
                }
            }
            catch
            {
                MppmLog.Debug($"Failed to process WebSocket event message: {message}");
            }
        }

#nullable enable
        private void HandleV4WebsocketEvent(WebsocketEventV4? websocketEvent)
        {
            var data = websocketEvent?.Result?.Data?.InnerData;
            switch (data?.EventType)
            {
                case WebsocketEventType.AllocateEventType:
                    ProcessAllocationWebsocketEvent(data.AllocationId ?? string.Empty);
                    break;
                case WebsocketEventType.DeallocateEventType:
                    ProcessDeallocationWebsocketEvent(data.AllocationId ?? string.Empty);
                    break;
                default:
                    MppmLog.Debug(
                        $"Received unsupported event [EventType: {websocketEvent!.Result!.Data!.InnerData!.EventType}]");
                    break;
            }
        }
#nullable disable

        private void ProcessAllocationWebsocketEvent(string allocationId)
        {
            if (string.IsNullOrEmpty(allocationId))
            {
                MppmLog.Debug("Allocation ID is empty in the event data.");
                return;
            }
        }

        private void ProcessDeallocationWebsocketEvent(string allocationId)
        {
            if (allocationId == string.Empty)
            {
                MppmLog.Debug("Allocation ID is null in the event data.");
                return;
            }
        }
    }
}
