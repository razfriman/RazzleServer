using RazzleServer.Player;
using MapleLib.PacketLib;
using RazzleServer.Server;
using RazzleServer.Packet;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.LOGIN_REQUEST)]
    public class LoginHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader packet, MapleClient client)
        {
            var accountName = packet.ReadMapleString();
            var accountPassword = packet.ReadMapleString();

            var account = MapleAccount.GetAccountFromDatabase(accountName);

            if (account != null)
            {
                if (account.CheckPassword(accountPassword))
                {
                    client.Account = account;
                    client.SendPacket(LoginAccountSuccess(account));
                }
                else
                {
                    client.SendPacket(LoginAccountFailed(4));
                }
            
            }
            else
            {
                if(ServerConfig.Instance.LoginCreatesNewAccount)
                {
                    var newAccount = MapleAccount.CreateAccount(accountName, accountPassword);
                    client.Account = newAccount;
                    client.SendPacket(LoginAccountSuccess(newAccount));
                }
                else
                {
                    client.SendPacket(LoginAccountFailed(5));

                }
            }
        }

        public static PacketWriter LoginAccountSuccess(MapleAccount acc)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.LOGIN_RESPONSE);

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
            PacketWriter pw = new PacketWriter(SMSGHeader.LOGIN_RESPONSE);
            pw.WriteByte(reason);
            pw.WriteByte(0);
            pw.WriteInt(0);
            return pw;
        }
    }
}