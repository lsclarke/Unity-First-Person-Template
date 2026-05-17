using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types
{
    internal class ServerJson
    {
        [JsonProperty("fleetID")]
        public string FleetId { get; set; }

        [JsonProperty("serverID")]
        public string ServerId { get; set; }

        [JsonProperty("allocatedUUID")]
        public string AllocationId { get; set; }

        [JsonProperty("machineID")]
        public string MachineId { get; set; }

        [JsonProperty("regionID")]
        public string RegionId { get; set; }

        [JsonProperty("regionName")]
        public string RegionName { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("port")]
        public string Port { get; set; }

        [JsonProperty("queryPort")]
        public string QueryPort { get; set; }

        [JsonProperty("queryType")]
        public string QueryType { get; set; }

        [JsonProperty("serverLogDir")]
        public string ServerLogDir { get; set; }

        [JsonProperty("homeDir")]
        public string HomeDir { get; set; }

        [JsonProperty("localProxyUrl")]
        public string LocalProxyUrl { get; set; }

        public static ServerJson CreateWithValues(string fleetId,
            string serverId,
            string allocationId,
            string regionId,
            string ip,
            string port,
            string queryPort,
            string queryType)
        {
            var projectDir = Directory.GetParent(Application.dataPath).FullName;
            var logsDir = Path.Combine(projectDir, "Logs");
            var homeDir = Path.Combine(projectDir, "Library/CloudSimulation");

            return new ServerJson
            {
                FleetId = fleetId,
                ServerId = serverId,
                AllocationId = allocationId,
                MachineId = "0",
                RegionId = regionId,
                RegionName = regionId,
                Ip = ip,
                Port = port,
                QueryPort = queryPort,
                QueryType = queryType,
                ServerLogDir = logsDir,
                HomeDir = homeDir,
                LocalProxyUrl = "http://127.0.0.1:8086",
            };
        }
    }
}
