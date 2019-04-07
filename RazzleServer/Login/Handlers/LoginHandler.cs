using System;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Common.Util;
using RazzleServer.Login.Maple;
using RazzleServer.Net.Packet;

namespace RazzleServer.Login.Handlers
{
    [PacketHandler(ClientOperationCode.Login)]
    public class LoginHandler : LoginPacketHandler
    {
        public override void HandlePacket(PacketReader packet, LoginClient client)
        {
            var accountName = packet.ReadString();
            var accountPassword = packet.ReadString();
            packet.ReadInt(); // Start up
            var machineBytes = packet.ReadBytes(16);
            var result = Login(client, accountName, accountPassword, machineBytes);
            client.Send(LoginPackets.LoginResult(result, client.Account));


            if (result != LoginResult.Valid)
            {
                return;
            }

            client.SetOnline(true);
            client.Send(LoginPackets.ListWorlds(client.Server.Manager.Worlds));
            client.Send(LoginPackets.EndListWorlds());
        }


        public static LoginResult Login(LoginClient client, string username, string password, byte[] machineBytes)
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

                if (client.Account.BanReason != BanReasonType.None)
                {
                    return LoginResult.Banned;
                }

                if (client.Account.IsOnline)
                {
                    return LoginResult.LoggedIn;
                }

                return LoginResult.Valid;
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
            client.Account.IsMaster = true;
            client.Account.Create();
        }
    }
}
