using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service
{
    internal interface IGameServerQueryer
    {
        QueryState Query(byte flags = 0);
    }
}
