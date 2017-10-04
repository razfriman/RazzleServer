using RazzleServer.Player;
using MapleLib.PacketLib;

namespace RazzleServer.Packet
{
    public abstract class APacketHandler
    {
        public abstract void HandlePacket(PacketReader packet, MapleClient client);
    }
}