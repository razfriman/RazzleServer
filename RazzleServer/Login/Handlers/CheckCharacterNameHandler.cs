using MapleLib.PacketLib;
using RazzleServer.Common.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.CHECK_CHAR_NAME)]
    public class CheckCharacterNameHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var name = packet.ReadString();

            var pw = new PacketWriter(ServerOperationCode.CHAR_NAME_RESPONSE);
            pw.WriteMapleString(name);
            pw.WriteBool(MapleCharacter.CharacterExists(name));
            client.Send(pw);
        }
    }
}