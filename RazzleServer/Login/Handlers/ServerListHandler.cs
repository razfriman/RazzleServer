using RazzleServer.Common.Packet;
using RazzleServer.Server;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.SERVERLIST_REQUEST)]
    [PacketHandler(ClientOperationCode.SERVERLIST_REREQUEST)]
    public class ServerListHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var pw = new PacketWriter(ServerOperationCode.SERVERLIST);

            foreach(var world in client.Server.Worlds)
            {
                pw.WriteByte(world.ID);
                pw.WriteMapleString(world.Name);
                pw.WriteByte((byte)world.Status);
                pw.WriteMapleString(world.EventMessage);
                pw.WriteByte(0x64);
                pw.WriteByte(0);
                pw.WriteByte(0x64);
                pw.WriteByte(0);
                pw.WriteByte(0);

                pw.WriteByte(world.Count);

                for (short i = 0; i < world.Count; i++)
                {
                    pw.WriteMapleString($"{world.Name}-{i}");
                    pw.WriteInt(0); //load
                    pw.WriteByte(world.ID); 
                    pw.WriteShort(i);
                }
            }
          
            pw.WriteShort(0);
            client.Send(pw);
            
            pw = new PacketWriter(ServerOperationCode.SERVERLIST);
            pw.WriteByte(0xFF);
            pw.WriteByte(0);
            client.Send(pw);
        }
    }
}