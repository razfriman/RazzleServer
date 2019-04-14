using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Util
{
    public interface IControllable
    {
        PacketWriter GetControlRequestPacket();
        PacketWriter GetControlCancelPacket();
    }
}
