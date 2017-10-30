using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.PLAYER_LOGGEDIN)]
    public class CharacterLoggedInHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            int characterId = packet.ReadInt();
        }
    }
}

