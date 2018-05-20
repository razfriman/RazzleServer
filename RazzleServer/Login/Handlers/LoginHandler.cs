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
                    pw.WriteInt(acc.Id);
                    pw.WriteByte((int)acc.Gender);
                    pw.WriteBool(acc.IsMaster);
                    pw.WriteByte(0);
                    pw.WriteString(acc.Username);
                    pw.WriteByte(0);
                    pw.WriteByte(0);
                    pw.WriteByte(0);
                    pw.WriteLong(0);
                    pw.WriteDateTime(acc.Creation);
                    pw.WriteByte((byte)(ServerConfig.Instance.RequestPin ? 0 : 1)); 
                    pw.WriteByte((byte)(ServerConfig.Instance.RequestPin ? 0 : 1));
                    pw.WriteByte((byte)(ServerConfig.Instance.RequestPin ? 0 : 1));
                }

                return pw;
            }
        }
    }
}