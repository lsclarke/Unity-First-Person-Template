using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment
{
    internal class FleetAndServerDataV5
    {
        public FleetSpecV5 Fleet { get; }
        public ServerInfoV5 Server { get; }

        public FleetAndServerDataV5(FleetSpecV5 fleet, ServerInfoV5 server)
        {
            Fleet = fleet;
            Server = server;
        }
    }
}
