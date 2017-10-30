using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.REGISTER_PIN)]
    public class RegisterPinHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var status = packet.ReadByte();

            if (status == 0)
            {
                // c.updateLoginState(MapleClient.LOGIN_NOTLOGGEDIN);
            }
            else
            {
                var pin = packet.ReadString();
                // SET PIN
                // c.updateLoginState(MapleClient.LOGIN_NOTLOGGEDIN);
            }
        }
    }
}