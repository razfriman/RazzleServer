using MapleLib.PacketLib;
using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.CHECK_CHAR_NAME)]
    public class CheckCharacterNameHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var name = packet.ReadMapleString();

            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.CHAR_NAME_RESPONSE);
            pw.WriteMapleString(name);
            pw.WriteBool(MapleCharacter.CharacterExists(name));
            client.SendPacket(pw);
        }
    }
}