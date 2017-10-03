using MapleLib.PacketLib;
using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.REGISTER_PIN)]
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
                var pin = packet.ReadMapleString();
                // SET PIN
                // c.updateLoginState(MapleClient.LOGIN_NOTLOGGEDIN);
            }
        }
    }
}