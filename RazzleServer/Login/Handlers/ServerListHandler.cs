using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.WorldList)]
    [PacketHandler(ClientOperationCode.WorldRelist)]
    public class ServerListHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var pw = new PacketWriter(ServerOperationCode.WorldInformation);

            foreach (var world in client.Server.Manager.Worlds)
            {
                pw.WriteByte(world.ID);
                pw.WriteString(world.Name);
                pw.WriteByte((byte)world.Flag);
                pw.WriteString(world.EventMessage);
                pw.WriteShort(world.EventExperienceRate);
                pw.WriteShort(world.EventDropRate);
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

            pw = new PacketWriter(ServerOperationCode.WorldInformation);
            pw.WriteByte(0xFF);
            pw.WriteByte(0);
            client.Send(pw);
        }
    }
}