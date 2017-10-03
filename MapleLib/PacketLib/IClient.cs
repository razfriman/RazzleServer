namespace MapleLib.PacketLib
{
    public interface IClient
    {
        void RecvPacket(PacketReader packet);

        void Disconnected();
	}
}
