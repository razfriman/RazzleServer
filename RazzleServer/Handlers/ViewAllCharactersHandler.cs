using RazzleServer.Packet;
using RazzleServer.Player;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.VIEW_ALL_CHAR)]
    public class ViewAllCharactersHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            System.Console.WriteLine("TODO");
        }
    }
}