using System;
using System.Net.Sockets;
using RazzleServer.Common;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Net;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Game
{
    public sealed class GameClient : AClient
    {
        public const int PingDelay = 5000;

        public GameAccount Account { get; set; }
        public GameServer Server { get; set; }
        public Character Character { get; set; }
        public override ILogger Logger => Log.ForContext<GameClient>();

        public GameClient(Socket session, GameServer server) : base(session, ServerConfig.Instance.Version,
            ServerConfig.Instance.SubVersion, ServerConfig.Instance.ServerType, ServerConfig.Instance.AesKey,
            ServerConfig.Instance.UseAesEncryption, ServerConfig.Instance.PrintPackets, true)
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
                    Logger.Error("Invalid packet - no data available");
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
                    Character?.Release();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e,
                    $"Packet Processing Error [{header.ToString()}] {packet.ToPacketString()} - {e.Message} - {e.StackTrace}");
            }
        }

        public override void Disconnected()
        {
            var save = Character;
            try
            {
                base.Disconnected();
                Server.RemoveClient(this);
                Character?.Save();
                Character?.Map?.Characters?.Remove(Character.Id);
                SetOnline(false);
                Character = null;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error while disconnecting. Account [{Account?.Username}] Character [{save?.Name}]");
            }
        }
        
        public void SetOnline(bool isOnline)
        {
            using var context = new MapleDbContext();
            var account = context.Accounts.Find(Account.Id);
            account.IsOnline = isOnline;
            context.SaveChanges();
        }

        public void ChangeChannel(byte channelId)
        {
            Character.Save();
            Server.Manager.Migrate(Host, Account.Id, Character.Id);

            using var outPacket = new PacketWriter(ServerOperationCode.ClientConnectToServer);
            outPacket.WriteBool(true);
            outPacket.WriteBytes(Socket.HostBytes);
            outPacket.WriteUShort(Server.World[channelId].Port);
            Send(outPacket);
        }

        public void StartPingCheck()
        {
            Ping();

            if (Socket?.Connected ?? false)
            {
                Delay.Execute(StartPingCheck, PingDelay);
            }
        }

        public void Ping() => Send(new PacketWriter(ServerOperationCode.Ping));
    }
}
