using System;
using RazzleServer.Net.Packet;

namespace RazzleServer.Shop.Handlers
{
    [PacketHandler(ClientOperationCode.Pong)]
    public class PongHandler : ShopPacketHandler
    {
        public override void HandlePacket(PacketReader packet, ShopClient client) => client.LastPong = DateTime.UtcNow;
    }
}
