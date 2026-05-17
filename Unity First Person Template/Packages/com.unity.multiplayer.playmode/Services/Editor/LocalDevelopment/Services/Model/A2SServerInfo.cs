using System;
using System.IO;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model
{
    internal class A2SServerInfo
    {
        public string ServerName { get; }
        public string GameMap { get; }
        public string GameFolder { get; }
        public string GameName { get; }
        public short SteamAppId { get; }
        public int PlayerCount { get; }
        public int MaxPlayers { get; }
        public int NumBots { get; }
        public int ServerType { get; }
        public int Environment { get; }
        public int Visibility { get; }
        public int VACEnabled { get; }
        public string Version { get; }

        public A2SServerInfo()
        {
            ServerName = "";
            GameMap = "";
            GameFolder = "";
            GameName = "";
            SteamAppId = 0;
            PlayerCount = 0;
            MaxPlayers = 0;
            NumBots = 0;
            ServerType = 0;
            Environment = 0;
            Visibility = 0;
            VACEnabled = 0;
            Version = "";
        }


        /// <summary>
        /// Generate a new A2SServerInfo from supplied MemoryStream.
        /// </summary>
        /// <param name="stream">MemoryStream of bytes received from UDP datagram.</param>
        public A2SServerInfo(MemoryStream stream)
        {
            stream.Position = A2SPacket.k_PacketHeader.Length;
            _ = WrapCatch("Header", stream.ReadByte);
            _ = WrapCatch("Protocol", stream.ReadByte);
            ServerName = WrapCatch("ServerName", stream.ReadNullTerminatedString);
            GameMap = WrapCatch("GameMap", stream.ReadNullTerminatedString);
            GameFolder = WrapCatch("GameFolder", stream.ReadNullTerminatedString);
            GameName = WrapCatch("GameName", stream.ReadNullTerminatedString);
            SteamAppId = WrapCatch("SteamAppId", stream.ReadBigEndianInt16);
            PlayerCount = WrapCatch("PlayerCount", stream.ReadByte);
            MaxPlayers = WrapCatch("MaxPlayers", stream.ReadByte);
            NumBots = WrapCatch("NumBots", stream.ReadByte);
            ServerType = WrapCatch("ServerType", stream.ReadByte);
            Environment = WrapCatch("Environment", stream.ReadByte);
            Visibility = WrapCatch("Visibility", stream.ReadByte);
            VACEnabled = WrapCatch("VACEnabled", stream.ReadByte);
            Version = WrapCatch("Version", stream.ReadNullTerminatedString);
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
