using Unity.Multiplayer.PlayMode.Editor;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model
{
    internal class WebsocketLogger : ILogger
    {
        public void Log(LogLevel level, string message, params object[] args)
        {
            MppmLog.Debug(string.Format(message, args));
        }

        public void Verbose(string message, params object[] args)
        {
            MppmLog.Debug(string.Format(message, args));
        }

        public void Debug(string message, params object[] args)
        {
            MppmLog.Debug(string.Format(message, args));
        }

        public void Info(string message, params object[] args)
        {
            MppmLog.Debug(string.Format(message, args));
        }

        public void Warn(string message, params object[] args)
        {
            MppmLog.Warning(string.Format(message, args));
        }

        public void Error(string message, params object[] args)
        {
            MppmLog.Error(string.Format(message, args));
        }
    }
}
