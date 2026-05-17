using System;
using System.Collections.Generic;
using System.Linq;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment
{
    [Serializable]
    internal class SerializableKeyValuePair
    {
        public string Key;
        public string Value;

        public SerializableKeyValuePair() { }

        public SerializableKeyValuePair(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    [Serializable]
    internal class ConnectionCacheData
    {
        public string Host;
        public int Port;
        public SerializableKeyValuePair[] Headers;

        public ConnectionCacheData() { }

        public ConnectionCacheData(ServerInfoConnectionV5 connection)
        {
            Host = connection.Host;
            Port = connection.Port;
            
            if (connection.Headers != null)
            {
                Headers = connection.Headers.Select(kvp => new SerializableKeyValuePair(kvp.Key, kvp.Value)).ToArray();
            }
            else
            {
                Headers = new SerializableKeyValuePair[0];
            }
        }

        public Dictionary<string, string> GetHeadersDictionary()
        {
            var dict = new Dictionary<string, string>();
            if (Headers == null)
            {
                return dict;
            }

            foreach (var header in Headers)
            {
                if (!string.IsNullOrEmpty(header.Key))
                {
                    dict[header.Key] = header.Value ?? string.Empty;
                }
            }
            return dict;
        }
    }
}
