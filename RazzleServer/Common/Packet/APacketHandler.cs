using RazzleServer.Player;

namespace RazzleServer.Common.Packet
{
    public abstract class APacketHandler
    {
        public abstract void HandlePacket(PacketReader packet, MapleClient client);
    }
}