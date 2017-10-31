using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CHECK_CHAR_NAME)]
    public class CheckCharacterNameHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var name = packet.ReadString();
            var pw = new PacketWriter(ServerOperationCode.CHAR_NAME_RESPONSE);
            pw.WriteMapleString(name);
            pw.WriteBool(Character.CharacterExists(name));

            client.Send(pw);
        }
    }
}