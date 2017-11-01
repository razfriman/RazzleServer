using RazzleServer.Common.Packet;

namespace RazzleServer.Center.Handlers
{
    [InteroperabilityPacketHandler(InteroperabilityOperationCode.UpdateChannelPopulation)]
    public class UpdateChannelPopulationHandler : CenterPacketHandler
    {
        public override void HandlePacket(PacketReader packet, CenterClient client)
        {
            int population = packet.ReadInt();

            var pw = new PacketWriter(InteroperabilityOperationCode.UpdateChannelPopulation);
            pw.WriteByte(client.World.ID);
            pw.WriteByte(client.ID);
            pw.WriteInt(population);
            client.Server.Login.Send(pw);
        }
    }
}
