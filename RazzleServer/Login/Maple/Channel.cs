using MapleLib.PacketLib;

namespace RazzleServer.Login.Maple
{
    public sealed class Channel
    {
        public byte ID { get; set; }
        public ushort Port { get; set; }
        public int Population { get; set; }

        public Channel(PacketReader inPacket)
        {
            ID = inPacket.ReadByte();
            Port = inPacket.ReadUShort();
            Population = inPacket.ReadInt();
        }
    }
}
