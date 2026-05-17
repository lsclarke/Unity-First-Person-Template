using Newtonsoft.Json;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types
{
    internal class AllocationRequestV5
    {
        public AllocationRequestV5(string allocationId, string[] preferredLocations)
        {
            AllocationId = allocationId;
            PreferredLocations = preferredLocations;
        }

        [JsonProperty("allocationId")]
        public string AllocationId { get; set; }
        [JsonProperty("preferredLocations")]
        public string[] PreferredLocations { get; set; }
    }
}
