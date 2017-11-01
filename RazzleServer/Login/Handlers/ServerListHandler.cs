using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.SERVERLIST_REQUEST)]
    [PacketHandler(ClientOperationCode.SERVERLIST_REREQUEST)]
    public class ServerListHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var pw = new PacketWriter(ServerOperationCode.SERVERLIST);

            foreach (var world in client.Server.Worlds)
            {
                pw.WriteByte(world.ID);
                pw.WriteString(world.Name);
                pw.WriteByte((byte)world.Status);
                pw.WriteString(world.EventMessage);
                pw.WriteShort(100);// Event EXP Rate
                pw.WriteShort(100); // Event Drop Rate
                pw.WriteBool(!world.EnableCharacterCreation);
                pw.WriteByte(world.Count);

                for (short i = 0; i < world.Count; i++)
                {
                    pw.WriteString($"{world.Name}-{i}");
                    pw.WriteInt(world.Population);
                    pw.WriteByte(world.ID);
                    pw.WriteShort(i);
                }
            }

            pw.WriteShort(0); // login balloons count (format: writePoint, writeString)

            client.Send(pw);

            pw = new PacketWriter(ServerOperationCode.SERVERLIST);
            pw.WriteByte(0xFF);
            pw.WriteByte(0);
            client.Send(pw);
        }
    }
}