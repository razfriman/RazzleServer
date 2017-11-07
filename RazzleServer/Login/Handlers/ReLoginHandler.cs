using RazzleServer.Common.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.Relogin)]
    public class ReLoginHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var pw = new PacketWriter(ServerOperationCode.ReloginResponse);
            pw.WriteByte(1);
            client.Send(pw);
        }
    }
}