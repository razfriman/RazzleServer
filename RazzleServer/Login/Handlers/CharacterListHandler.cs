using MapleLib.PacketLib;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.CHARLIST_REQUEST)]
    public class CharacterListHandler : APacketHandler<MapleClient>
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            packet.Skip(1);
            var worldID = packet.ReadByte();
            byte channel = packet.ReadByte();

            client.Account.MigrationData.ToChannel = channel;
            client.Channel = channel;

            client.Send(ShowCharacters(client.Account));
            //client.SendPacket(CreateCharacterOptions());
        }

        public static PacketWriter ShowCharacters(MapleAccount acc)
        {
            var chars = acc.GetCharsFromDatabase();

            var pw = new PacketWriter(); pw.WriteHeader(ServerOperationCode.CHARLIST);
            pw.WriteByte(0);
            pw.WriteByte((byte)chars.Count);
            chars.ForEach(chr => MapleCharacter.AddCharEntry(pw, chr));
            pw.WriteByte(1);
            pw.WriteInt(acc.CharacterSlots);
            return pw;
        }
    }
}