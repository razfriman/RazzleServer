using RazzleServer.Data;
using RazzleServer.Packet;
using RazzleServer.Player;
using System.Linq;
using MapleLib.PacketLib;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.DELETE_CHAR)]
    public class DeleteCharacterHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var pic = packet.ReadMapleString();
            var characterID = packet.ReadInt();

            byte state = 20;
            if (client.Account.HasCharacter(characterID))
            {
                using (var context = new MapleDbContext())
                {
                    context.InventorySlots.RemoveRange(context.InventorySlots.Where(x => x.CharacterID == characterID));
                    context.KeyMaps.RemoveRange(context.KeyMaps.Where(x => x.CharacterID == characterID));
                    context.QuickSlotKeyMaps.RemoveRange(context.QuickSlotKeyMaps.Where(x => x.CharacterID == characterID));
                    context.Characters.Remove(context.Characters.SingleOrDefault(x => x.ID == characterID));
                    context.SaveChanges();
                }

                state = 0;
            }

            var pw = new PacketWriter((ushort)SMSGHeader.DELETE_CHAR_RESPONSE);
            pw.WriteInt(characterID);
            pw.WriteByte(state);
            client.SendPacket(pw);
        }
    }
}