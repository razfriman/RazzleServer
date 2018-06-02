using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Login.Maple;

namespace RazzleServer.Login
{
    public static class LoginPackets
    {
        public static PacketWriter SendLoginResult(LoginResult result, Account acc)
        {
            using (var pw = new PacketWriter(ServerOperationCode.CheckPasswordResult))
            {
                pw.WriteShort((short)result);
                pw.WriteInt(0);

                if (result == LoginResult.Banned)
                {
                    pw.WriteByte(1); // ban reason
                    pw.WriteDateTime(DateTime.Now.AddYears(2)); // ban expiration time. Over 2 years = permanent
                }
                else if (result == LoginResult.Valid)
                {
                    pw.WriteInt(acc.Id);
                    pw.WriteByte((int)acc.Gender);
                    pw.WriteBool(acc.IsMaster);
                    pw.WriteByte(0); // 0x80 == usergm == gmlevel 5
                    pw.WriteString(acc.Username);
                    pw.WriteByte(0);
                    pw.WriteByte(0); // quiet ban reason
                    pw.WriteLong(0); // quiet ban time
                    pw.WriteDateTime(acc.Creation);
                    pw.WriteInt(0);
                }

                return pw;
            }
        }

        internal static PacketWriter PinResult(PinResult result)
        {
            using (var pw = new PacketWriter(ServerOperationCode.PinCodeOperation))
            {
                pw.WriteByte((byte)result);
                return pw;
            }
        }
    }
}