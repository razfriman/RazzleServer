using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CharacterNameCheck)]
    public class CheckCharacterNameHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var name = packet.ReadString();
            var characterExists = Character.CharacterExists(name);
            client.Send(LoginPackets.CharacterNameResult(name, characterExists));
        }
    }
}