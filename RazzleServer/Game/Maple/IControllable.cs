using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Maple
{
    public interface IControllable
    {
        PacketWriter GetControlRequestPacket();
        PacketWriter GetControlCancelPacket();
    }
}
