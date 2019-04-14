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

            var item = client.GameCharacter.Items[ItemType.Usable, slot];

            if (item == null || itemId != item.MapleId)
            {
                return;
            }

            client.GameCharacter.Items.Remove(itemId, 1);

            if (item.CHealth > 0)
            {
                client.GameCharacter.PrimaryStats.Health += item.CHealth;
            }

            if (item.CMana > 0)
            {
                client.GameCharacter.PrimaryStats.Mana += item.CMana;
            }

            if (item.CHealthPercentage != 0)
            {
                client.GameCharacter.PrimaryStats.Health +=
                    (short)(item.CHealthPercentage * client.GameCharacter.PrimaryStats.MaxHealth / 100);
            }

            if (item.CManaPercentage != 0)
            {
                client.GameCharacter.PrimaryStats.Mana +=
                    (short)(item.CManaPercentage * client.GameCharacter.PrimaryStats.MaxMana / 100);
            }

            if (item.CBuffTime > 0 || !string.IsNullOrEmpty(item.CCureAilments))
            {
                client.GameCharacter.Buffs.AddBuff(item.MapleId);
            }
        }
    }
}
