using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model;

namespace Unity.Multiplayer.PlayMode.Scenarios.Editor.ExtensionApi.Instances.MultiplayLocalDevelopment.Services.ProxyServer
{
    internal interface ILocalProxy
    {
        public ProxyState ProxyState { get; }
        public string DisconnectReason { get; }

        public event EventHandler<ProxyStateChangedEventArgs> ProxyStateChanged;
        public event EventHandler<WebsocketMessageReceivedEventArgs> MessageReceived;

        public Task StartAsync(LocalProxyConfig config, CancellationToken cancellationToken = default);
        public void Stop();
        public void Dispose();
    }
}
