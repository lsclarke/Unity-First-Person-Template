using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Exceptions;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service
{

    internal class SqpClient : IGameServerQueryer, IDisposable
    {
        readonly UdpClient m_UdpClient;
        IPEndPoint m_IpEndPoint;

        public SqpClient(string ip, int port)
        {
            m_IpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            m_UdpClient = new UdpClient(ip, port);
            m_UdpClient.Client.SendTimeout = 2500;
            m_UdpClient.Client.ReceiveTimeout = 2500;
        }

        public void Dispose()
        {
            m_UdpClient.Dispose();
        }

        /// <summary>
        /// Query executes one query cycle against the configured UDP endpoint. If successful, it returns the information
        /// the UDP endpoint returns.
        ///
        /// ServerRules, PlayerInfo, TeamInfo and Metrics are ignored, as they are not documented
        /// or properly supported.
        ///
        /// The SQP query format is documented here:
        /// https://docs.unity.com/ugs/manual/game-server-hosting/manual/concepts/sqp
        /// </summary>
        /// <returns>QueryState of the configured UDP endpoint.</returns>
        public QueryState Query(byte flags)
        {
            // Ask the remote server to generate a challenge ID, which we should send and validate on each subsequent
            // request and response.
            SendChallenge();

            // Receive the challenge ID, and store it for later.
            var expectedChallengeId =
                SqpPacket.ParseChallengeResponse(new MemoryStream(m_UdpClient.Receive(ref m_IpEndPoint)));

            // As the remote server to generate information regarding its current state.
            SendQuery(flags, expectedChallengeId);

            // Receive the server state.
            return SqpPacket.ParseQueryResponse(flags, expectedChallengeId,
                new MemoryStream(m_UdpClient.Receive(ref m_IpEndPoint)));
        }

        /// <summary>
        /// SendChallenge sends the challenge request.
        /// </summary>
        void SendChallenge()
        {
            byte[] request = SqpPacket.ChallengeRequestBytes();
            m_UdpClient.Send(request, request.Length);
        }

        /// <summary>
        /// SendQuery queries the target game server for information.
        /// </summary>
        void SendQuery(byte flags, int challengeId)
        {
            byte[] request = SqpPacket.QueryRequestBytes(flags, challengeId);
            m_UdpClient.Send(request, request.Length);
        }
    }
}
