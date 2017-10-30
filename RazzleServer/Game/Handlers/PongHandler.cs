using System;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PONG)]
    public class PongHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client) => client.LastPong = DateTime.Now;
    }
}