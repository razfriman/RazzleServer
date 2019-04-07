﻿using System;
using System.Linq;
using System.Net.Sockets;
using RazzleServer.Common;
using RazzleServer.Login.Maple;
using RazzleServer.Net;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Login
{
    public class LoginClient : AClient
    {
        public byte World { get; internal set; }
        public byte Channel { get; internal set; }
        public LoginAccount Account { get; internal set; }
        public string LastUsername { get; internal set; }
        public string LastPassword { get; internal set; }
        public LoginServer Server { get; internal set; }
        
        public bool ThrowOnExceptions { get; protected internal set; }

        public override ILogger Logger => Log.ForContext<LoginClient>();

        public LoginClient(Socket socket, LoginServer server)
            : base(socket, ServerConfig.Instance.Version, ServerConfig.Instance.SubVersion,
                ServerConfig.Instance.ServerType, ServerConfig.Instance.AesKey, ServerConfig.Instance.UseAesEncryption,
                ServerConfig.Instance.PrintPackets, true)
        {
            Server = server;
        }

        public override void Receive(PacketReader packet)
        {
            var header = ClientOperationCode.Unknown;
            try
            {
                if (packet.Available < 1)
                {
                    Logger.Warning("Not enough packet data");
                    return;
                }

                header = (ClientOperationCode)packet.ReadByte();

                if (Server.PacketHandlers.ContainsKey(header))
                {
                    if (ServerConfig.Instance.PrintPackets && !Server.IgnorePacketPrintSet.Contains(header))
                    {
                        Logger.Information($"Received [{header.ToString()}] {packet.ToPacketString()}");
                    }

                    foreach (var handler in Server.PacketHandlers[header])
                    {
                        handler.HandlePacket(packet, this);
                    }
                }
                else
                {
                    Logger.Warning($"Unhandled Packet [{header.ToString()}] {packet.ToPacketString()}");
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Packet Processing Error [{header.ToString()}] - {e.Message} - {e.StackTrace}");

                if (ThrowOnExceptions)
                {
                    throw;
                }
            }
        }

        public override void Disconnected()
        {
            try
            {
                base.Disconnected();
                SetOnline(false);
                Server.RemoveClient(this);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error while disconnecting. Account [{Account?.Username}]");
            }
        }

        public void SetOnline(bool isOnline)
        {
            using (var context = new MapleDbContext())
            {
                var account = context.Accounts.FirstOrDefault(x => x.Id == Account.Id);
                if (account == null)
                {
                    return;
                }

                account.IsOnline = isOnline;
                context.SaveChanges();
            }
        }
    }
}
