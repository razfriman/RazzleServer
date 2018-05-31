using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseItem)]
    public class UseItemHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.ReadInt(); // NOTE: Ticks.
            var slot = packet.ReadShort();
            var itemId = packet.ReadInt();

            var item = client.Character.Items[ItemType.Usable, slot];

            if (item == null || itemId != item.MapleId)
            {
                return;
            }

            client.Character.Items.Remove(itemId, 1);

            if (item.CHealth > 0)
            {
                client.Character.Health += item.CHealth;
            }

            if (item.CMana > 0)
            {
                client.Character.Mana += item.CMana;
            }

            if (item.CHealthPercentage != 0)
            {
                client.Character.Health += (short)(item.CHealthPercentage * client.Character.MaxHealth / 100);
            }

            if (item.CManaPercentage != 0)
            {
                client.Character.Mana += (short)(item.CManaPercentage * client.Character.MaxMana / 100);
            }

            if (item.CBuffTime > 0 && item.CProb == 0)
            {
                // TODO: Add buff.
            }

            if (false)
            {
                // TODO: Add Monster Book card.
            }
        }
    }
}