using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using RazzleServer.Common.Network;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Login.Maple;
using RazzleServer.Common.Util;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Server;

namespace RazzleServer.Login
{
    public sealed class LoginClient : AClient
    {
        public byte World { get; internal set; }
        public byte Channel { get; internal set; }
        public Account Account { get; internal set; }
        public string LastUsername { get; internal set; }
        public string LastPassword { get; internal set; }
        public string[] MacAddresses { get; internal set; }
        public LoginServer Server { get; internal set; }

        public LoginClient(Socket socket, LoginServer server)
            : base(socket)
        {
            Server = server;
        }



        public override void Receive(PacketReader packet)
        {
            ClientOperationCode header = ClientOperationCode.Unknown;
            try
            {
                if (packet.Available >= 2)
                {
                    header = (ClientOperationCode)packet.ReadUShort();

                    if (LoginServer.PacketHandlers.ContainsKey(header))
                    {
                        Log.LogInformation($"Received [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");

                        foreach (var handler in LoginServer.PacketHandlers[header])
                        {
                            handler.HandlePacket(packet, this);
                        }
                    }
                    else
                    {
                        Log.LogWarning($"Unhandled Packet [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");
                    }

                }
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Packet Processing Error [{header.ToString()}] - {e.Message} - {e.StackTrace}");
            }
        }

        public LoginResult Login(string username, string password)
        {
            Account = new Account(this);

            try
            {
                Account.Username = username;
                Account.Load();

                if (Functions.GetSha512(password + Account.Salt) != Account.Password)
                {
                    return LoginResult.InvalidPassword;
                }
                else if (Account.IsBanned)
                {
                    return LoginResult.Banned;
                }
                else
                {
                    return LoginResult.Valid;
                }
            }
            catch (NoAccountException)
            {
                if (ServerConfig.Instance.EnableAutoRegister && username == LastUsername && password == LastPassword)
                {
                    Account.Username = username;
                    Account.Salt = Functions.RandomString();
                    Account.Password = Functions.GetSha512(password + Account.Salt);
                    Account.Gender = Gender.Unset;
                    Account.Pin = string.Empty;
                    Account.IsBanned = false;
                    Account.IsMaster = false;
                    Account.Birthday = DateTime.UtcNow;
                    Account.Creation = DateTime.UtcNow;
                    Account.MaxCharacters = ServerConfig.Instance.DefaultCreationSlots;
                    Account.Create();
                }
                else
                {
                    LastUsername = username;
                    LastPassword = password;
                    return LoginResult.InvalidUsername;
                }
            }

            return LoginResult.Valid;
        }
    }
}
