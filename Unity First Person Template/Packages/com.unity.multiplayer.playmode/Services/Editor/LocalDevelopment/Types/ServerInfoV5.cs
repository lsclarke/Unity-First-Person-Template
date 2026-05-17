
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types
{
    internal class ServerListV5
    {
        public ServerListV5(ServerInfoV5[] results)
        {
            Results = results;
        }

        [JsonProperty("results")]
        public ServerInfoV5[] Results { get; set; }
    }

    internal class ServerInfoV5
    {
        public ServerInfoV5(ServerInfoConnectionV5[] connections, string fleetId, long serverId, string locationId, string state)
        {
            Connections = connections;
            FleetId = fleetId;
            ServerId = serverId;
            LocationId = locationId;
            State = state;
        }

        [JsonProperty("connections")]
        public ServerInfoConnectionV5[] Connections { get; set; }
        [JsonProperty("fleetId")]
        public string FleetId { get; set; }
        [JsonProperty("id")]
        public long ServerId { get; set; }
        [JsonProperty("locationId")]
        public string LocationId { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
    }

    internal class ServerInfoConnectionV5
    {
        public ServerInfoConnectionV5(string host)
        {
            Host = host;
        }

        [JsonProperty("host")]
        public string Host { get; set; }
        [JsonProperty("port")]
        public int Port { get; set; }
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }
    }
}
