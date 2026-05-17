namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model
{

    internal class QueryState
    {
        public int CurrentPlayers { get; }
        public int MaxPlayers { get; }
        public string ServerName { get; }
        public string GameType { get; }
        public string BuildId { get; }
        public string Map { get; }
        public ushort Port { get; }

        public QueryState(
            int currentPlayers,
            int maxPlayers,
            string serverName,
            string gameType,
            string buildId,
            string map,
            ushort port
        )
        {
            CurrentPlayers = currentPlayers;
            MaxPlayers = maxPlayers;
            ServerName = serverName;
            GameType = gameType;
            BuildId = buildId;
            Map = map;
            Port = port;
        }

        public override string ToString()
        {
            return $@"Current Players : {CurrentPlayers}
Max Players     : {MaxPlayers}
Server Name     : {ServerName}
Game Type       : {GameType}
Build Id        : {BuildId}
Map             : {Map}
Port            : {Port}";
        }
    }
}
