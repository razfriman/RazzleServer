using System;
using System.Net.Sockets;
using System.Threading;
using RazzleServer.Common;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Net;
using RazzleServer.Net.Packet;
using RazzleServer.Shop.Maple;
using Serilog;

namespace RazzleServer.Shop
{
    public class ShopClient : AClient
    {
        public const int PingDelay = 5000;

        public ShopAccount Account { get; set; }
        public ShopServer Server { get; set; }
        public Character Character { get; set; }
        public CancellationTokenSource PingToken { get; set; }
        public override ILogger Logger => Log.ForContext<ShopClient>();

        public ShopClient(Socket session, ShopServer server) : base(session, ServerConfig.Instance.Version,
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
                PingToken?.Cancel();
                PingToken?.Dispose();
                base.Disconnected();
                Server.RemoveClient(this);
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
            Server.Manager.Migrate(Host, Account.Id, Character.Id);

            using var outPacket = new PacketWriter(ServerOperationCode.ClientConnectToServer);
            outPacket.WriteBool(true);
            outPacket.WriteBytes(Socket.HostBytes);
            outPacket.WriteUShort(Server.Manager.Worlds[Character.WorldId][channelId].Port);
            Send(outPacket);
        }

        public void StartPingCheck() => PingToken = TaskRunner.RunRepeated(Ping, TimeSpan.FromMilliseconds(PingDelay));

        public void Ping() => Send(new PacketWriter(ServerOperationCode.Ping));
    }
}
