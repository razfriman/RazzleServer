using RazzleServer.Center.Maple;
using RazzleServer.Common.Packet;

namespace RazzleServer.Center.Handlers
{
    [InteroperabilityPacketHandler(InteroperabilityOperationCode.MigrationRegisterRequest)]
    public class MigrationRegisterRequestHandler : CenterPacketHandler
    {
        public override void HandlePacket(PacketReader packet, CenterClient client)
        {
            string host = packet.ReadString();
            int accountID = packet.ReadInt();
            int characterID = packet.ReadInt();

            var valid = false;

            if (!client.Server.Migrations.Contains(host))
            {
                valid = true;
                client.Server.Migrations.Add(new Migration(host, accountID, characterID));
            }

            var outPacket = new PacketWriter(InteroperabilityOperationCode.MigrationRegisterResponse);
            outPacket.WriteString(host);
            outPacket.WriteBool(valid);
            client.Send(outPacket);
        }
    }
}

