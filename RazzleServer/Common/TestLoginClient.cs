using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using RazzleServer.Login;
using RazzleServer.Net;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Common
{
    public class TestLoginClient : LoginClient
    {
        public delegate void OnServerToClientPacket(PacketReader packet);

        public event OnServerToClientPacket ServerToClientPacket;

        public delegate void OnClientToServerPacket(PacketReader packet);

        public event OnClientToServerPacket ClientToServerPacket;


        public TestLoginClient(LoginServer server) : base(new Socket(SocketType.Stream, ProtocolType.Tcp), server)
        {
        }

        public override void Receive(PacketReader packet)
        {
            base.Receive(packet);
            ClientToServerPacket?.Invoke(new PacketReader(packet.ToArray()));
        }

        public override void Send(PacketWriter packet)
        {
            base.Send(packet);
            ServerToClientPacket?.Invoke(new PacketReader(packet.ToArray()));
        }

        public override async Task SendAsync(byte[] packet)
        {
            await base.SendAsync(packet);
            ServerToClientPacket?.Invoke(new PacketReader(packet));
        }
    }
}