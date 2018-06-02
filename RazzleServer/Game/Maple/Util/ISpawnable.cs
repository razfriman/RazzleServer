using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Maple.Util
{
    public interface ISpawnable
    {
        PacketWriter GetCreatePacket();
        PacketWriter GetSpawnPacket();
        PacketWriter GetDestroyPacket();
    }
}
