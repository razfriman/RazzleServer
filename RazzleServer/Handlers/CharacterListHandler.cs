using RazzleServer.Packet;
using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.CHARLIST_REQUEST)]
    public class CharacterListHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            packet.Skip(1);
            var worldID = packet.ReadByte();
            byte channel = packet.ReadByte();

            client.Account.MigrationData.ToChannel = channel;
            client.Channel = channel;

            client.SendPacket(ShowCharacters(client.Account));
            //client.SendPacket(CreateCharacterOptions());
        }

        public static PacketWriter ShowCharacters(MapleAccount acc)
        {
            var chars = acc.GetCharsFromDatabase();

            var pw = new PacketWriter(SMSGHeader.CHARLIST);

            pw.WriteByte(0);
            pw.WriteByte((byte)chars.Count);

            foreach (var chr in chars)
            {
                MapleCharacter.AddCharEntry(pw, chr);
            }

            pw.WriteByte(1);
            pw.WriteInt(acc.CharacterSlots);
            return pw;
        }
    }
}