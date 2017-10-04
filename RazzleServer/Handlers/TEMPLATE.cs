using MapleLib.PacketLib;
using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.PONG)]
    public class Handler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
        }
    }
}