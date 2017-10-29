using RazzleServer.Common.Packet;

namespace RazzleServer.Center.Handlers
{
    [InteroperabilityPacketHandler(InteroperabilityOperationCode.ChannelPortRequest)]
    public class ChannelPortRequestHandler : CenterPacketHandler
    {
        public override void HandlePacket(PacketReader packet, CenterClient client)
        {
            var id = packet.ReadByte();

            var outPacket = new PacketWriter();
            outPacket.WriteHeader(InteroperabilityOperationCode.ChannelPortResponse);
            outPacket.WriteByte(id);
            outPacket.WriteUShort(client.World[id].Port);
            client.Send(outPacket);
        }
    }
}