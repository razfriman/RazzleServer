using System;
using RazzleServer.Common.Packet;
using RazzleServer.Game;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.PONG)]
    public class PongHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client) => client.LastPong = DateTime.Now;

        public static PacketWriter PingPacket() => new PacketWriter(ServerOperationCode.Ping);
    }
}