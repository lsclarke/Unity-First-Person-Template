using System;
using System.IO;
using System.Linq;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Exceptions;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model
{
    internal class A2SPacket
    {
        /// <summary>
        /// Header for all A2S packets.
        /// </summary>
        internal static readonly byte[] k_PacketHeader = {
            0xFF,0xFF,0xFF,0xFF
        };

        /// <summary>
        /// Single byte that follows the A2S header and identifies the type of the received packet.
        /// </summary>
        public enum PacketType : byte
        {
            Undefined = 0xFF, // -1
            InfoRequest = 0x54,     // 'T'
            InfoResponse = 0x49,    // 'I'
            ChallengeResponse = 0x41 // 'A'
        }

        /// <summary>
        /// Payload for an A2S_INFO request. Spells 'Source Engine Query', null-terminated.
        /// </summary>
        static readonly byte[] k_InfoRequestPayload =
        {
            0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67,
            0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00
        };

        /// <summary>
        /// Gets the type of the A2S packet.
        /// </summary>
        /// <param name="payload">The A2S packet to inspect</param>
        /// <param name="position">The stream position (so that we can reset it after having read the type)</param>
        /// <returns>PacketType</returns>
        public static PacketType GetPacketType(MemoryStream payload, int position = 0)
        {
            payload.Seek(k_PacketHeader.Length, SeekOrigin.Begin);
            var responseType = WrapCatch("PacketType", payload.ReadByte);
            payload.Seek(position, SeekOrigin.Begin);
            if (!Enum.IsDefined(typeof(PacketType), (byte)responseType))
            {
                return PacketType.Undefined;
            }
            return (PacketType)responseType;
        }

        /// <summary>
        /// Returns the byte payload for issuing an info request.
        /// </summary>
        /// <param name="challengeId">The challenge received in response to a previous request.</param>
        /// <returns>byte[] Bytes for the A2S_INFO request.</returns>
        public static byte[] CreateInfoRequest(int challengeId)
        {
            byte[] headerBytes = k_PacketHeader;
            byte[] typeQueryBytes = { (byte)PacketType.InfoRequest };

            byte[] result = headerBytes
                .Concat(typeQueryBytes)
                .Concat(k_InfoRequestPayload)
                .Concat(GetChallengeBytes(challengeId))
                .ToArray();

            return result;
        }

        /// <summary>
        /// Returns the provided Challenge ID, as bytes, in network order (big endian).
        /// </summary>
        /// <param name="challengeId">The raw challenge to parse</param>
        /// <returns>byte[] Byte array of challenge ID as bytes in big endian.</returns>
        internal static byte[] GetChallengeBytes(int challengeId)
        {
            var challengeBytes = BitConverter.GetBytes(challengeId);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(challengeBytes);
            }

            return challengeBytes;
        }

        /// <summary>
        /// Parses the challenge response and returns the challenge ID.
        /// </summary>
        /// <param name="payload">The A2S packet to parse</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="A2SParseException"></exception>
        /// <returns>int The challenge id</returns>
        public static int GetChallengeId(MemoryStream payload)
        {
            var responseType = GetPacketType(payload);

            if (responseType != PacketType.ChallengeResponse)
            {
                throw new ArgumentOutOfRangeException(
                    $"Game server query handler did not return the correct response type, expected 0x41, got {responseType}");
            }

            payload.Seek(k_PacketHeader.Length + 1, SeekOrigin.Begin);

            return WrapCatch("Challenge", payload.ReadBigEndianInt32);
        }

        /// <summary>
        /// Parses the server info response and returns the information.
        /// </summary>
        /// <returns>QueryState, with the information relating to the connected game server instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="SqpParseException"></exception>
        public static QueryState ParseInfoResponse(MemoryStream payload)
        {
            var responseType = GetPacketType(payload);
            if (responseType != PacketType.InfoResponse)
            {
                throw new ArgumentOutOfRangeException(
                    $"Game server query handler did not return the correct response type, expected 0x49, got {responseType}");
            }

            var serverInfo = new A2SServerInfo(payload);

            return new QueryState(
                serverInfo.PlayerCount,
                serverInfo.MaxPlayers,
                serverInfo.ServerName,
                serverInfo.GameName,
                serverInfo.Version,
                serverInfo.GameMap,
                0
            );
        }

        static T WrapCatch<T>(string field, Func<T> f) {
            try
            {
                return f();
            }
            catch (Exception e)
            {
                throw new A2SParseException(field, e);
            }
        }
    }
}
