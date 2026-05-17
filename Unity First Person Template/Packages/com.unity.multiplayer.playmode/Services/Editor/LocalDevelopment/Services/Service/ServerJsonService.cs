using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Types;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service
{
    internal static class ServerJsonService
    {
        private static string ServerJsonFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "server.json");

        internal static async Task Upsert(ServerJson serverJson, CancellationToken cancellationToken)
        {
            var jsonString = JsonConvert.SerializeObject(serverJson, Formatting.Indented);
            await File.WriteAllTextAsync(ServerJsonFilePath, jsonString, cancellationToken);
        }

        internal static async Task<ServerJson> Read(CancellationToken cancellationToken)
        {
            var jsonString = await File.ReadAllTextAsync(ServerJsonFilePath, cancellationToken);
            return JsonConvert.DeserializeObject<ServerJson>(jsonString);
        }

        internal static void Delete()
        {
            File.Delete(ServerJsonFilePath);
        }
    }
}
