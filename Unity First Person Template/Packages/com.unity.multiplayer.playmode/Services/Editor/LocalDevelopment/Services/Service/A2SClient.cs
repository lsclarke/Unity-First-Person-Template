using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Exceptions;
using Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Model;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service
{
    internal class A2SClient : IGameServerQueryer, IDisposable
    {
        readonly UdpClient m_UdpClient;
        IPEndPoint m_IpEndPoint;

        public A2SClient(string ip, int port)
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
        /// The A2S query format is documented here:
        /// https://docs.unity.com/ugs/manual/game-server-hosting/manual/concepts/A2S
        /// </summary>
        /// <returns>QueryState of the configured UDP endpoint.</returns>
        public QueryState Query(byte flags)
        {
            SendQuery();

            var response = new MemoryStream(m_UdpClient.Receive(ref m_IpEndPoint));

            if (A2SPacket.GetPacketType(response) == A2SPacket.PacketType.ChallengeResponse)
            {
                SendQuery(A2SPacket.GetChallengeId(response));
            }

            return A2SPacket.ParseInfoResponse(new MemoryStream(m_UdpClient.Receive(ref m_IpEndPoint)));
        }

        /// <summary>
        /// SendQuery queries the target game server for information.
        /// </summary>
        void SendQuery(int challengeId = 0)
        {
            byte[] request = A2SPacket.CreateInfoRequest(challengeId);
            m_UdpClient.Send(request, request.Length);
        }
    }
}
