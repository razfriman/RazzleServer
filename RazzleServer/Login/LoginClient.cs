﻿using System;
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
        public static Dictionary<ClientOperationCode, List<LoginPacketHandler>> PacketHandlers = new Dictionary<ClientOperationCode, List<LoginPacketHandler>>();

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

        public static void RegisterPacketHandlers()
        {

            var types = Assembly.GetEntryAssembly()
                                .GetTypes()
                                .Where(x => x.IsSubclassOf(typeof(LoginPacketHandler)));

            var handlerCount = 0;

            foreach (var type in types)
            {
                var attributes = type.GetTypeInfo()
                                     .GetCustomAttributes()
                                     .OfType<PacketHandlerAttribute>()
                                     .ToList();

                foreach (var attribute in attributes)
                {
                    var header = attribute.Header;

                    if (!PacketHandlers.ContainsKey(header))
                    {
                        PacketHandlers[header] = new List<LoginPacketHandler>();
                    }

                    handlerCount++;
                    var handler = (LoginPacketHandler)Activator.CreateInstance(type);
                    PacketHandlers[header].Add(handler);
                    Log.LogDebug($"Registered Packet Handler [{attribute.Header}] to [{type.Name}]");
                }
            }

            Log.LogInformation($"Registered {handlerCount} packet handlers");
        }

        public override void Receive(PacketReader packet)
        {
            ClientOperationCode header = ClientOperationCode.UNKNOWN;
            try
            {
                if (packet.Available >= 2)
                {
                    header = (ClientOperationCode)packet.ReadUShort();

                    if (PacketHandlers.ContainsKey(header))
                    {
                        Log.LogInformation($"Recevied [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");

                        foreach (var handler in PacketHandlers[header])
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
            this.Account = new Account(this);

            try
            {
                this.Account.Load(username);

                if (Functions.GetSha512(password + this.Account.Salt) != this.Account.Password)
                {
                    return LoginResult.InvalidPassword;
                }
                else if (this.Account.IsBanned)
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
                if (ServerConfig.Instance.LoginCreatesNewAccount && username == this.LastUsername && password == this.LastPassword)
                {
                    this.Account.Username = username;
                    this.Account.Salt = Functions.RandomString();
                    this.Account.Password = Functions.GetSha512(password + Account.Salt);
                    this.Account.Gender = Gender.Unset;
                    this.Account.Pin = string.Empty;
                    this.Account.IsBanned = false;
                    this.Account.IsMaster = false;
                    this.Account.Birthday = DateTime.UtcNow;
                    this.Account.Creation = DateTime.UtcNow;
                    this.Account.MaxCharacters = this.Server.Worlds[World].DefaultCreationSlots;
                    this.Account.Save();
                }
                else
                {
                    this.LastUsername = username;
                    this.LastPassword = password;
                    return LoginResult.InvalidUsername;
                }
            }

            return LoginResult.Valid;
        }
    }
}
