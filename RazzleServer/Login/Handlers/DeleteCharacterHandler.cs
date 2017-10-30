using RazzleServer.Data;
using RazzleServer.Common.Packet;
using System.Linq;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.DELETE_CHAR)]
    public class DeleteCharacterHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var pic = packet.ReadString();
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

            var pw = new PacketWriter(ServerOperationCode.DELETE_CHAR_RESPONSE);
            pw.WriteInt(characterID);
            pw.WriteByte(state);
            client.Send(pw);
        }
    }
}