using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Login.Maple;
using RazzleServer.Server;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.AccountLogin)]
    public class LoginHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var accountName = packet.ReadString();
            var accountPassword = packet.ReadString();
            var result = client.Login(accountName, accountPassword);
            client.Send(SendLoginResult(result, client.Account));
        }

        public static PacketWriter SendLoginResult(LoginResult result, Account acc)
        {
            using (var pw = new PacketWriter(ServerOperationCode.CheckPasswordResult))
            {
                pw.WriteInt((int)result);
                pw.WriteByte(0);
                pw.WriteByte(0);

                if (result == LoginResult.Valid)
                {
                    pw.WriteInt(acc.ID);
                    pw.WriteByte((int)acc.Gender);
                    pw.WriteBool(acc.IsMaster);
                    pw.WriteByte(0);
                    pw.WriteByte(0);
                    pw.WriteByte(0x4E); // ??
                    pw.WriteString(acc.Username);
                    pw.WriteByte(0x03); // ??
                    pw.WriteByte(0);
                    pw.WriteByte(0); // NOTE: Quiet ban reason. 
                    pw.WriteLong(0); // NOTE: Quiet ban lift date.
                    pw.WriteDateTime(acc.Creation);
                    pw.WriteByte(0); // ??
                    pw.WriteByte(0); // ??
                    pw.WriteByte((byte)(ServerConfig.Instance.RequestPin ? 0 : 2)); // NOTE: 1 seems to not do anything.
                }

                return pw;
            }
        }
    }
}