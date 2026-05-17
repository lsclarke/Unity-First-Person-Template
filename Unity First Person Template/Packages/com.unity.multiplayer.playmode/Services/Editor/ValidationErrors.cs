using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.PlayMode.Services.Editor
{
    internal class ValidationErrors
    {
        internal static void LogCloudSetupErrors()
        {
            Debug.LogError(
                "Play Mode Scenario: To use a Multiplay Hosting server, this project needs to be connected to Unity Cloud. " +
                "You can set this up in <a href=\"MPPM-OpenProjectServices\">Edit > Project Settings > Services</a>.");
            LogEnvironmentSetupErrors();
        }

        internal static void LogEnvironmentSetupErrors()
        {
            Debug.LogError(
                "Play Mode Scenario: After connecting your project to the Unity Cloud, an environment needs to be selected. " +
                "You can set this up in <a href=\"MPPM-OpenEnvironmentSettings\">Edit > Project Settings > Services > Environment</a>.");
        }

        internal static void LogMultiplaySetupErrors(bool isProjectDashboardLinkAvailable = true)
        {
            if (isProjectDashboardLinkAvailable)
            {
                var organizationKey = CloudProjectSettings.organizationKey;
                var projectId = CloudProjectSettings.projectId;
                var dashboardUrl = $"https://cloud.unity.com/home/organizations/{organizationKey}/projects/{projectId}";
                Debug.LogError(
                    "Play Mode Scenario: To use a Multiplay Hosting remote instance, you must have Multiplay enabled on your project dashboard. " +
                    $"Please check your <a href=\"{dashboardUrl}\">project dashboard</a> for more details.");
            }
            else
            {
                Debug.LogError(
                    "Play Mode Scenario: To use a Multiplay Hosting remote instance, you must have Multiplay enabled on your project dashboard. " +
                    "Please check your project dashboard for more details.");
            }
        }

        internal static void LogServiceTokenErrors()
        {
            const string unityCloudServiceStatusUrl = "https://status.unity.com/";
            Debug.LogError(
                $"Unexpected error: failed to get service token. Please try again later. For more information, " +
                $"check the <a href=\"{unityCloudServiceStatusUrl}\">Unity Cloud Service Status</a>.");
        }

        // This class handles the hyperlink clicks in the logs for Multiplay Services Settings
        [InitializeOnLoad]
        internal static class MultiplayProjectSettingsHyperlinkHandler
        {
            static MultiplayProjectSettingsHyperlinkHandler()
            {
                EditorGUI.hyperLinkClicked += OnHyperlinkClicked;
            }

            private static void OnHyperlinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
            {
                if (!args.hyperLinkData.TryGetValue("href", out var link)) return;
                switch (link)
                {
                    case "MPPM-OpenProjectServices":
                        SettingsService.OpenProjectSettings("Project/Services");
                        break;
                    case "MPPM-OpenEnvironmentSettings":
                        SettingsService.OpenProjectSettings("Project/Services/Environments");
                        break;
                }
            }
        }
    }
}
