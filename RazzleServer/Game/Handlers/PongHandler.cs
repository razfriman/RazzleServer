using System;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.Pong)]
    public class PongHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client) => client.LastPong = DateTime.Now;
    }
}
