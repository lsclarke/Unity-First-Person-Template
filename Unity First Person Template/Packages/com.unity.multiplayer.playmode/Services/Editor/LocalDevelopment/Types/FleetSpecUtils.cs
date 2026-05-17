using System;
using System.Collections.Generic;
using System.Text;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types
{
    internal static class FleetSpecUtils
    {
        // Labels used to provide additional context needed for backwards compatibility with the v4 Multiplay APIs.
        public const string k_FleetLabelBuildConfigQueryType = "compatibility.multiplay.unity.com/build-config-query-type";

        // Labels used to add characteristics to the v5 APIs for filtering, exposing functionality, etc.
        public const string k_FleetLabelLocalDevelopment = "multiplay.unity.com/placement-type";
        public const string k_FleetLabelLocalServerIP = "multiplay.unity.com/local-fleet-ip";
        public const string k_FleetLabelLocalServerPort = "multiplay.unity.com/local-fleet-port";
        public const string k_FleetLabelValueLocalPlacementType = "local";

        private const int k_FleetSuffixLength = 5;

        public static FleetSpecV5 CreateLocalFleetSpec(string queryType, string localHost, string localPort)
        {
            var suffix = GenerateRandomString(k_FleetSuffixLength);

            var metadata = new FleetV5ApiItemMetadata($"local-fleet-{suffix}",
                new Dictionary<string, string>
                {
                    { k_FleetLabelBuildConfigQueryType, queryType },
                    { k_FleetLabelLocalServerIP, localHost },
                    { k_FleetLabelLocalServerPort, localPort },
                    { k_FleetLabelLocalDevelopment, k_FleetLabelValueLocalPlacementType }
                });
            return new FleetSpecV5(metadata);
        }

        static string GenerateRandomString(int length)
        {
            var rand = new Random();
            var outString = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                outString.Append(Convert.ToChar(rand.Next(0, 26) + 65));
            }

            return outString.ToString();
        }
    }
}
