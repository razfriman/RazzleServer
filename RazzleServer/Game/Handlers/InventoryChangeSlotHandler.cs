using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.InventoryChangeSlot)]
    public class InventoryChangeSlotHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var type = (ItemType)packet.ReadByte();
            var source = packet.ReadShort();
            var destination = packet.ReadShort();
            var quantity = packet.ReadShort();

            try
            {
                var item = client.Character.Items[type, source];

                if (destination < 0)
                {
                    item.Equip();
                }
                else if (source < 0 && destination > 0)
                {
                    item.Unequip(destination);
                }
                else if (destination == 0)
                {
                    item.Drop(quantity);
                }
                else
                {
                    item.Move(destination);
                }
            }
            catch (InventoryFullException)
            {
                client.Character.Items.NotifyFull();
            }
        }
    }
}
