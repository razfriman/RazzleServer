using MapleLib.PacketLib;
using RazzleServer.Common.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.RELOG)]
    public class ReLoginHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.RELOG_RESPONSE);
            pw.WriteByte(1);
            client.Send(pw);
        }
    }
}