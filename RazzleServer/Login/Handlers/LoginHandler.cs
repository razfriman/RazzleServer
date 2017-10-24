using RazzleServer.Player;
using RazzleServer.Common.Packet;
using RazzleServer.Server;
using MapleLib.PacketLib;

namespace RazzleServer.Handlers
{
    [PacketHandler(ClientOperationCode.LOGIN_REQUEST)]
    public class LoginHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var accountName = packet.ReadString();
            var accountPassword = packet.ReadString();

            var account = MapleAccount.GetAccountFromDatabase(accountName);

            if (account != null)
            {
                if (account.CheckPassword(accountPassword))
                {
                    client.Account = account;
                    client.Send(LoginAccountSuccess(account));
                }
                else
                {
                    client.Send(LoginAccountFailed(4));
                }
            
            }
            else
            {
                if(ServerConfig.Instance.LoginCreatesNewAccount)
                {
                    var newAccount = MapleAccount.CreateAccount(accountName, accountPassword);
                    client.Account = newAccount;
                    client.Send(LoginAccountSuccess(newAccount));
                }
                else
                {
                    client.Send(LoginAccountFailed(5));

                }
            }
        }

        public static PacketWriter LoginAccountSuccess(MapleAccount acc)
        {
            var pw = new PacketWriter(ServerOperationCode.LOGIN_RESPONSE);

            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteInt(0);
            pw.WriteInt(acc.ID);
            pw.WriteByte(acc.Gender);
            pw.WriteBool(acc.IsGM);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteMapleString(acc.Name);
            pw.WriteByte(0);
            pw.WriteByte(0);
            pw.WriteLong(0);
            pw.WriteLong(0); // Creation Time
            pw.WriteInt(0);
            pw.WriteShort(2);

            var migration = new MigrationData
            {                
                CharacterID = 0,
                AccountName = acc.Name
            };
            acc.MigrationData = migration;
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