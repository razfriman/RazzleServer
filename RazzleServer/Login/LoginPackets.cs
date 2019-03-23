using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Login.Maple;

namespace RazzleServer.Login
{
    public static class LoginPackets
    {
        public static PacketWriter SendLoginResult(LoginResult result, LoginAccount acc)
        {
            using (var pw = new PacketWriter(ServerOperationCode.CheckPasswordResult))
            {
                pw.WriteByte((byte)result);
                pw.WriteByte(0);
                pw.WriteInt(0);

                if (result == LoginResult.Banned)
                {
                    pw.WriteByte((byte)acc.BanReason);
                    pw.WriteDateTime(LoginAccount.PermanentBanDate);
                }
                else if (result == LoginResult.Valid)
                {
                    pw.WriteInt(acc.Id);
                    pw.WriteByte((int)acc.Gender);
                    pw.WriteBool(acc.IsMaster);
                    pw.WriteByte(1);
                    pw.WriteString(acc.Username);
                }

                pw.WriteLong(0);
                pw.WriteLong(0);
                pw.WriteLong(0);


                return pw;
            }
        }
    }
}
