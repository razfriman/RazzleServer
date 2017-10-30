using RazzleServer.Common.Packet;
using RazzleServer.Login.Maple;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.LOGIN_REQUEST)]
    public class LoginHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var accountName = packet.ReadString();
            var accountPassword = packet.ReadString();
        }

        public static PacketWriter LoginAccountSuccess(Account acc)
        {
            var pw = new PacketWriter(ServerOperationCode.LOGIN_RESPONSE);

            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteInt(0);
            pw.WriteInt(acc.ID);
            pw.WriteByte((int)acc.Gender);
            pw.WriteBool(acc.IsMaster);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteMapleString(acc.Username);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteLong(0);
            pw.WriteLong(0); // Creation Time
            pw.WriteInt(0);
            pw.WriteShort(2);
            return pw;
        }

        static PacketWriter LoginAccountFailed(byte reason)
        {
            var pw = new PacketWriter(ServerOperationCode.LOGIN_RESPONSE);
            pw.WriteByte(reason);
            pw.WriteByte(0);
            pw.WriteInt(0);
            return pw;
        }
    }
}