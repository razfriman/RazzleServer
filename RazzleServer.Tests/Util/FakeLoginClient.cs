using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Internal;
using RazzleServer.Login;
using RazzleServer.Net.Packet;

namespace RazzleServer.Tests.Util
{
    public class FakeLoginClient : LoginClient
    {
        private readonly Dictionary<ServerOperationCode, Queue<PacketReader>> _serverToClientPackets =
            new Dictionary<ServerOperationCode, Queue<PacketReader>>();

        private readonly Dictionary<ClientOperationCode, Queue<PacketReader>> _clientToServerPackets =
            new Dictionary<ClientOperationCode, Queue<PacketReader>>();

        public FakeLoginClient(LoginServer server) : base(null, server)
        {
            ThrowOnExceptions = true;
        }

        public override void Receive(PacketReader packet)
        {
            base.Receive(packet);
            var queuePacket = new PacketReader(packet.ToArray());
            var header = (ClientOperationCode)queuePacket.ReadByte();
            if (!_clientToServerPackets.ContainsKey(header))
            {
                _clientToServerPackets[header] = new Queue<PacketReader>();
            }

            _clientToServerPackets[header].Enqueue(queuePacket);
        }

        public override void Send(PacketWriter packet)
        {
            base.Send(packet);
            var queuePacket = new PacketReader(packet.ToArray());
            var header = (ServerOperationCode)queuePacket.ReadByte();
            if (!_serverToClientPackets.ContainsKey(header))
            {
                _serverToClientPackets[header] = new Queue<PacketReader>();
            }

            _serverToClientPackets[header].Enqueue(queuePacket);
        }

        public PacketReader GetPacket(ServerOperationCode header)
        {
            if (_serverToClientPackets.ContainsKey(header) && _serverToClientPackets[header].Any())
            {
                return _serverToClientPackets[header].Dequeue();
            }

            return null;
        }

        public PacketReader GetPacket(ClientOperationCode header)
        {
            if (_clientToServerPackets.ContainsKey(header) && _clientToServerPackets[header].Any())
            {
                return _clientToServerPackets[header].Dequeue();
            }

            return null;
        }
    }
}
