using System;
using RazzleServer.Common;
using RazzleServer.Common.Server;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Login.Maple;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.Login)]
    public class LoginHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var accountName = packet.ReadString();
            var accountPassword = packet.ReadString();
            var result = Login(client, accountName, accountPassword);
            client.Send(LoginPackets.LoginResult(result, client.Account));


            if (result != LoginResult.Valid)
            {
                return;
            }

            client.Send(LoginPackets.ListWorlds(client.Server.Manager.Worlds));
            client.Send(LoginPackets.EndListWorlds());
        }


        public static LoginResult Login(LoginClient client, string username, string password)
        {
            client.Account = new LoginAccount(client);

            try
            {
                client.Account.Username = username;
                client.Account.Load();

                if (Functions.GetSha512(password + client.Account.Salt) != client.Account.Password)
                {
                    return LoginResult.InvalidPassword;
                }

                return client.Account.BanReason != BanReasonType.None
                    ? LoginResult.Banned
                    : LoginResult.Valid;
            }
            catch (NoAccountException)
            {
                if (ServerConfig.Instance.EnableAutoRegister && username == client.LastUsername &&
                    password == client.LastPassword)
                {
                    AutoRegisterAccount(client, username, password);
                }
                else
                {
                    client.LastUsername = username;
                    client.LastPassword = password;
                    return LoginResult.InvalidUsername;
                }
            }

            return LoginResult.Valid;
        }

        private static void AutoRegisterAccount(LoginClient client, string username, string password)
        {
            client.Account.Username = username;
            client.Account.Salt = Functions.RandomString();
            client.Account.Password = Functions.GetSha512(password + client.Account.Salt);
            client.Account.Gender = Gender.Male;
            client.Account.Birthday = DateTime.UtcNow;
            client.Account.Creation = DateTime.UtcNow;
            client.Account.MaxCharacters = ServerConfig.Instance.DefaultCreationSlots;
            client.Account.IsMaster = true;
            client.Account.Create();
        }
    }
}
