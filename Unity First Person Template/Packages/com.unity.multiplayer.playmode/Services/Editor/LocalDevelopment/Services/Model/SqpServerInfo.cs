using System;
using System.IO;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Exceptions;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model
{
    internal class SqpServerInfo
    {
        public int CurrentPlayers { get; }
        public int MaxPlayers { get; }
        public string ServerName { get; }
        public string GameType { get; }
        public string BuildId { get; }
        public string Map { get; }
        public ushort Port { get; }

        public SqpServerInfo()
        {
            CurrentPlayers = 0;
            MaxPlayers = 0;
            ServerName = "";
            GameType = "";
            BuildId = "";
            Map = "";
            Port = 0;
        }

        /// <summary>
        /// Generate a new SqpServerInfo from supplied MemoryStream.
        /// </summary>
        /// <param name="s">MemoryStream of bytes received from UDP datagram.</param>
        public SqpServerInfo(MemoryStream s)
        {
            // Read ServerInfo ChunkLength, but ignore it.
            _ = WrapCatch("Query:ServerInfo:ChunkLength", s.ReadBigEndianUInt32);

            CurrentPlayers = WrapCatch("Query:ServerInfo:CurrentPlayers", s.ReadBigEndianInt16);
            MaxPlayers = WrapCatch("Query:ServerInfo:MaxPlayers", s.ReadBigEndianInt16);
            ServerName = WrapCatch("Query:ServerInfo:ServerName", s.ReadLengthPrefixedString);
            GameType = WrapCatch("Query:ServerInfo:GameType", s.ReadLengthPrefixedString);
            BuildId = WrapCatch("Query:ServerInfo:BuildId", s.ReadLengthPrefixedString);
            Map = WrapCatch("Query:ServerInfo:Map", s.ReadLengthPrefixedString);
            Port = WrapCatch("Query:ServerInfo:Port", s.ReadBigEndianUInt16);
        }

        static T WrapCatch<T>(string field, Func<T> f)
        {
            try
            {
                return f();
            }
            catch (Exception e)
            {
                throw new SqpParseException(field, e);
            }
        }
    }
}
