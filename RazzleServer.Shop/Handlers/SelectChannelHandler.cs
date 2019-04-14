using RazzleServer.Net.Packet;

namespace RazzleServer.Shop.Handlers
{
    [PacketHandler(ClientOperationCode.SelectChannel)]
    public class SelectChannelHandler : ShopPacketHandler
    {
        public override void HandlePacket(PacketReader packet, ShopClient client)
        {
        }
    }
}
