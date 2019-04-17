using System;
using System.Net.Sockets;
using System.Threading;
using RazzleServer.Common;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Server;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Game
{
    public sealed class GameClient : AMapleClient
    {
        public const int PingDelay = 5000;
        public CancellationTokenSource PingToken { get; set; }
        public GameCharacter GameCharacter { get; set; }
        public GameServer Server { get; set; }

        public override ILoginServer LoginServer => throw new NotSupportedException(
            $"Cannot access Login Server from {GetType()}");

        public override IGameServer GameServer => Server;

        public override IShopServer ShopServer => throw new NotSupportedException(
            $"Cannot access Shop Server from {GetType()}");

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
                    GameCharacter?.Release();
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
            var save = GameCharacter;
            try
            {
                PingToken?.Cancel();
                PingToken?.Dispose();
                base.Disconnected();
                Server.RemoveClient(this);
                GameCharacter?.Save();
                GameCharacter?.Map?.Characters?.Remove(GameCharacter.Id);
                SetOnline(false);
                GameCharacter = null;
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
            GameCharacter.Save();
            Server.Manager.Migrate(Host, Account.Id, GameCharacter.Id);

            using var outPacket = new PacketWriter(ServerOperationCode.ClientConnectToServer);
            outPacket.WriteBool(true);
            outPacket.WriteBytes(Socket.HostBytes);
            outPacket.WriteUShort(Server.World[channelId].Port);
            Send(outPacket);
        }

        public void OpenCashShop()
        {
            GameCharacter.Save();
            Server.Manager.Migrate(Host, Account.Id, GameCharacter.Id);

            using var outPacket = new PacketWriter(ServerOperationCode.ClientConnectToServer);
            outPacket.WriteBool(true);
            outPacket.WriteBytes(Socket.HostBytes);
            outPacket.WriteUShort(Server.Manager.Shop.Port);
            Send(outPacket);
        }

        public void StartPingCheck() => PingToken = TaskRunner.RunRepeated(Ping, TimeSpan.FromMilliseconds(PingDelay));

        public void Ping()
        {
            if (DateTime.UtcNow.Subtract(LastPong).TotalSeconds > ServerConfig.Instance.PingTimeout)
            {
                Terminate("Ping timeout");
                return;
            }
            
            Send(new PacketWriter(ServerOperationCode.Ping));
        }
    }
}
