using MapleLib.PacketLib;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Server;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.SERVERSTATUS_REQUEST)]
    public class ServerStatusHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var worldIndex = packet.ReadShort();

            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.SERVERSTATUS);
            pw.WriteShort((short)ServerLoadType.NORMAL);
            client.SendPacket(pw);
        }
    }
}