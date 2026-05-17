using Newtonsoft.Json;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types
{
    // Message example:
    // {"result":{"channel":"server#1","data":{"data":{"EventType":"AllocateEventType","EventID":"7d9881b9-1619-48c2-8289-7fcbcfd308c2","ServerID":1883262176879104,"AllocationID":"cd3b28de-88dd-41c8-bc8c-e6f9c2015c6c"}}}}
    internal class WebsocketEventV4
    {
        public WebsocketEventV4(WebsocketEventV4Result result)
        {
            Result = result;
        }

        [JsonProperty("result")]
        public WebsocketEventV4Result Result { get; set; }
    }

    internal class WebsocketEventV4Result
    {
        public WebsocketEventV4Result(string channel, WebsocketEventV4Data data)
        {
            Channel = channel;
            Data = data;
        }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("data")]
        public WebsocketEventV4Data Data { get; set; }
    }

    internal class WebsocketEventV4Data
    {
        public WebsocketEventV4Data(WebsocketEventV4InnerData innerData)
        {
            InnerData = innerData;
        }

        [JsonProperty("data")]
        public WebsocketEventV4InnerData InnerData { get; set; }
    }

    internal class WebsocketEventV4InnerData
    {
        public WebsocketEventV4InnerData(WebsocketEventType eventType, string eventId, long serverId, string allocationId)
        {
            EventType = eventType;
            EventId = eventId;
            ServerId = serverId;
            AllocationId = allocationId;
        }

        [JsonProperty("EventType")]
        public WebsocketEventType EventType { get; set; }

        [JsonProperty("EventID")]
        public string EventId { get; set; }

        [JsonProperty("ServerID")]
        public long ServerId { get; set; }

        [JsonProperty("AllocationID")]
        public string AllocationId { get; set; }
    }

    internal enum WebsocketEventType
    {
        [JsonProperty("AllocateEventType")]
        AllocateEventType,

        [JsonProperty("DeallocateEventType")]
        DeallocateEventType
    }
}
