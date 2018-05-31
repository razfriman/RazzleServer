using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.InventoryAction)]
    public class InventoryActionHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.ReadInt();
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