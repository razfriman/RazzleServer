using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.RELOG)]
    public class ReLoginHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var pw = new PacketWriter(SMSGHeader.RELOG_RESPONSE);
            pw.WriteByte(1);
            client.SendPacket(pw);
        }
    }
}