using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Maple
{
    public interface ISpawnable
    {
        PacketWriter GetCreatePacket();
        PacketWriter GetSpawnPacket();
        PacketWriter GetDestroyPacket();
    }
}
