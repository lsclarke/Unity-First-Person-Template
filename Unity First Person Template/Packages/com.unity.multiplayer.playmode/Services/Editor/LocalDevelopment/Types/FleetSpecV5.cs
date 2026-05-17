using System.Collections.Generic;
using Newtonsoft.Json;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types
{
    internal class FleetListV5
    {
        public FleetListV5(FleetV5ApiItemMetadata[] results)
        {
            Results = results;
        }

        [JsonProperty("results")]
        public FleetV5ApiItemMetadata[] Results { get; set; }
    }

    internal class FleetSpecV5
    {
        public FleetSpecV5(FleetV5ApiItemMetadata metadata)
        {
            Metadata = metadata;
        }

        [JsonProperty("metadata")] public FleetV5ApiItemMetadata Metadata { get; set; }

        // Other properties have been omitted as they are not used here.
    }

    internal class FleetV5ApiItemMetadata
    {
        [JsonConstructor]
        public FleetV5ApiItemMetadata(string name, string id, bool deleting, string status, Dictionary<string, string> labels)
        {
            Name = name;
            Id = id;
            Deleting = deleting;
            Status = status;
            Labels = labels;
        }

        public FleetV5ApiItemMetadata(string name,  Dictionary<string, string> labels)
        {
            Name = name;
            Labels = labels;
        }

        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("labels")] public Dictionary<string,string> Labels { get; set; }
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)] public string Id { get; set; }
        [JsonProperty("deleting", DefaultValueHandling = DefaultValueHandling.Ignore)] public bool Deleting { get; set; }
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)] public string Status { get; set; }
    }
}
