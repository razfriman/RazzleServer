using RazzleServer.Data;
using RazzleServer.Packet;
using RazzleServer.Player;
using System.Linq;

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
                    //context.InventorySlots.RemoveRange(DBContext.InventorySlots.Where(x => x.CharacterId == characterId));
                    //context.KeyMaps.RemoveRange(DBContext.KeyMaps.Where(x => x.CharacterId == characterId));
                    //context.QuickSlotKeyMaps.RemoveRange(DBContext.QuickSlotKeyMaps.Where(x => x.CharacterId == characterId));
                    context.Characters.Remove(context.Characters.SingleOrDefault(x => x.ID == characterID));
                    context.SaveChanges();
                }

                state = 0;
            }

            var pw = new PacketWriter(SMSGHeader.DELETE_CHAR_RESPONSE);
            pw.WriteInt(characterID);
            pw.WriteByte(state);
            client.SendPacket(pw);
        }
    }
}