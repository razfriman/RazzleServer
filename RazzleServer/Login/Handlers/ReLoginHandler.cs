using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    //[PacketHandler(ClientOperationCode.RELOG)]
    public class ReLoginHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            //var pw = new PacketWriter(ServerOperationCode.RELOG_RESPONSE);
            //pw.WriteByte(1);
            //client.Send(pw);
        }
    }
}