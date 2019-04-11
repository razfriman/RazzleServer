using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.UseItem)]
    public class UseItemHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
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
                client.Character.PrimaryStats.Health += item.CHealth;
            }

            if (item.CMana > 0)
            {
                client.Character.PrimaryStats.Mana += item.CMana;
            }

            if (item.CHealthPercentage != 0)
            {
                client.Character.PrimaryStats.Health +=
                    (short)(item.CHealthPercentage * client.Character.PrimaryStats.MaxHealth / 100);
            }

            if (item.CManaPercentage != 0)
            {
                client.Character.PrimaryStats.Mana +=
                    (short)(item.CManaPercentage * client.Character.PrimaryStats.MaxMana / 100);
            }

            if (item.CBuffTime > 0 || !string.IsNullOrEmpty(item.CCureAilments))
            {
                client.Character.Buffs.AddBuff(item.MapleId);
            }
        }
    }
}
