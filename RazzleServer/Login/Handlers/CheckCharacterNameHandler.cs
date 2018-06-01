using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.CharacterNameCheck)]
    public class CheckCharacterNameHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var name = packet.ReadString();
            var characterExists = client.Server.CharacterExists(name, client.World);
            client.Send(LoginPackets.CharacterNameResult(name, characterExists));
        }
    }
}