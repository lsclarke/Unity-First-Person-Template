using System;
using UnityEngine;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.API
{
    internal static class CloudEnvironment
    {
        private const string CloudEnvironmentArg = "-cloudEnvironment";
        private const string StagingEnvironment = "staging";

        private static readonly bool isStaging;

        static CloudEnvironment()
        {
            var cloudEnv = GetCloudEnvironment(Environment.GetCommandLineArgs());
            if (cloudEnv == StagingEnvironment)
            {
                isStaging = true;
            }
        }

        public static string GetHost()
        {
            return isStaging ? "https://staging.services.unity.com" : "https://services.unity.com";
        }


        private static string GetCloudEnvironment(string[] commandLineArgs)
        {
            try
            {
                var cloudEnvironmentIndex = Array.IndexOf(commandLineArgs, CloudEnvironmentArg);

                if (cloudEnvironmentIndex >= 0 && cloudEnvironmentIndex <= commandLineArgs.Length - 2)
                {
                    return commandLineArgs[cloudEnvironmentIndex + 1];
                }
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get Cloud Environment", e);
            }

            return null;
        }
    }
}
