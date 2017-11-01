using RazzleServer.Common.Packet;

namespace RazzleServer.Center.Handlers
{
    [InteroperabilityPacketHandler(InteroperabilityOperationCode.MigrationRegisterRequest)]
    public class MigrationRegisterRequestHandler : CenterPacketHandler
    {
        public override void HandlePacket(PacketReader packet, CenterClient client)
        {
            string host = packet.ReadString();
            int characterID = packet.ReadInt();

            int accountID = client.Server.Migrations.Validate(host, characterID);

            var outPacket = new PacketWriter(InteroperabilityOperationCode.MigrationResponse);
            outPacket.WriteString(host);
            outPacket.WriteInt(accountID);
            client.Send(outPacket);
        }
    }
}
