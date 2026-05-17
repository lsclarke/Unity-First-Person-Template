using System;
using System.IO;
using System.Linq;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Exceptions;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model
{
    internal class SqpPacket
    {
        /// <summary>
        /// RequestServerInfo requests that the server return information about itself.
        /// </summary>
        public const byte RequestServerInfo = (1 << 0);

        /// <summary>
        /// k_PacketTypeChallengeRequest is the header ID for issuing a challenge request.
        /// </summary>
        const byte k_PacketTypeChallengeRequest = 0;

        /// <summary>
        /// k_PacketTypeQueryRequest is the header ID for issuing a query request.
        /// </summary>
        const byte k_PacketTypeQueryRequest = 1;

        /// <summary>
        /// k_PacketTypeChallengeResponse is the header ID for the response to a challenge request.
        /// </summary>
        const byte k_PacketTypeChallengeResponse = 0;

        /// <summary>
        /// k_PacketTypeQueryResponse is the header ID for the response to a query request.
        /// </summary>
        const byte k_PacketTypeQueryResponse = 1;

        /// <summary>
        /// ChallengeRequestBytes returns the byte payload for issuing a challenge request.
        /// </summary>
        /// <returns>Bytes for SQP challenge request.</returns>
        public static byte[] ChallengeRequestBytes()
        {
            return new byte[]
            {
                k_PacketTypeChallengeRequest,
                // Add 4 bytes of padding to make the request equal in size to the response so
                // these requests aren't attractive amplification vectors.
                0x0, 0x0, 0x0, 0x0
            };
        }

        /// <summary>
        /// QueryRequestBytes returns the byte payload for issuing a query request.
        /// </summary>
        /// <param name="flags">SQP flags (i.e. RequestServerInfo).</param>
        /// <param name="challengeId">Challenge ID receive from prior challenge response.</param>
        /// <returns>Bytes for SQP query request.</returns>
        public static byte[] QueryRequestBytes(byte flags, int challengeId)
        {
            var packetHeader = new[] { k_PacketTypeQueryRequest };
            var queryVersion = new byte[] { 0x0, 0x1 };
            var requestedChunks = new[] { flags };

            return packetHeader
                .Concat(GetChallengeBytes(challengeId))
                .Concat(queryVersion)
                .Concat(requestedChunks)
                .ToArray();
        }

        /// <summary>
        /// GetChallengeBytes returns the provided Challenge ID, as bytes, in network order (big endian).
        /// </summary>
        /// <returns>Byte array of challenge ID as bytes in big endian.</returns>
        static byte[] GetChallengeBytes(int challengeId)
        {
            var challengeBytes = BitConverter.GetBytes(challengeId);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(challengeBytes);
            }

            return challengeBytes;
        }

        /// <summary>
        /// ParseChallengeResponse parses the challenge response returns the challenge ID.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="SqpParseException"></exception>
        public static int ParseChallengeResponse(MemoryStream payload)
        {
            var responseType = WrapCatch("Challenge:Type", payload.ReadByte);
            if (responseType != k_PacketTypeChallengeResponse)
            {
                throw new ArgumentOutOfRangeException(
                    $"Game server query handler did not return the correct response type, expected 0x0, got {responseType}");
            }

            return WrapCatch("Challenge:ChallengeToken", payload.ReadBigEndianInt32);
        }

        /// <summary>
        /// ParseQueryResponse parses the query response and returns the information.
        /// </summary>
        /// <returns>QueryState, with the information relating to the connected game server instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="SqpParseException"></exception>
        public static QueryState ParseQueryResponse(byte flags, int expectedChallengeId, MemoryStream payload)
        {
            var responseType = WrapCatch("Query:Type", payload.ReadByte);
            if (responseType != k_PacketTypeQueryResponse)
            {
                throw new ArgumentOutOfRangeException(
                    $"Game server query handler did not return the correct response type, expected 0x1, got {responseType}");
            }

            var challengeId = WrapCatch("Query:ChallengeToken", payload.ReadBigEndianInt32);
            if (challengeId != expectedChallengeId)
            {
                throw new ArgumentOutOfRangeException(
                    $"Game server query handler did not respond with a valid challenge ID, expected {expectedChallengeId}, got {challengeId}");
            }

            var version = WrapCatch("Query:Version", payload.ReadBigEndianInt16);
            if (version is < 1 or > 2)
            {
                throw new ArgumentOutOfRangeException(
                    $"Game server query handler did not respond with a valid version, expected 0x1 or 0x2, got {version}");
            }

            // CurrentPacket and LastPacket need to be read, but are unused.
            _ = WrapCatch("Query:CurrentPacket", payload.ReadByte);
            _ = WrapCatch("Query:LastPacket", payload.ReadByte);

            // Read packet length. Use this to inform the remaining number of bytes to read.
            // Do not use for memory allocation as this is untrusted input!
            _ = WrapCatch("Query:PacketLength", payload.ReadBigEndianInt16);

            var serverInfo = new SqpServerInfo();

            if ((flags & RequestServerInfo) == 1)
            {
                serverInfo = new SqpServerInfo(payload);
            }

            return new QueryState(
                serverInfo.CurrentPlayers,
                serverInfo.MaxPlayers,
                serverInfo.ServerName,
                serverInfo.GameType,
                serverInfo.BuildId,
                serverInfo.Map,
                serverInfo.Port
            );
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
