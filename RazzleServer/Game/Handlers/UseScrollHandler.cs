using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.InventoryUseScrollOnItem)]
    public class UseScrollHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var slot = packet.ReadShort(); //0x0E
            var xxx = packet.ReadShort(); // 0xFFFF
        }
    }
}
