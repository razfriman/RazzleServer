using MapleLib.PacketLib;
using RazzleServer.Common.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.REGISTER_PIN)]
    public class RegisterPinHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
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