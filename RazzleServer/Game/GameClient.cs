using System;
using System.Net.Sockets;
using RazzleServer.Common.Server;
using RazzleServer.Common.Network;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;
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

        public GameClient(Socket session, GameServer server) : base(session)
        {
            Server = server;
            Host = Socket.Host;
            Port = Socket.Port;
            Connected = true;
        }

        public override void Receive(PacketReader packet)
        {
            var header = ClientOperationCode.Unknown;
            try
            {
                if (packet.Available >= 2)
                {
                    header = (ClientOperationCode)packet.ReadUShort();

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
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Packet Processing Error [{header.ToString()}] {packet.ToPacketString()} - {e.Message} - {e.StackTrace}");
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
                Character = null;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Error while disconnecting. Account [{Account?.Username}] Character [{save?.Name}]");
            }
        }

        public void ChangeChannel(byte channelId)
        {
            Character.Save();
            Server.Manager.Migrate(Host, Account.Id, Character.Id);

            using (var outPacket = new PacketWriter(ServerOperationCode.ClientConnectToServer))
            {
                outPacket.WriteBool(true);
                outPacket.WriteBytes(Socket.HostBytes);
                outPacket.WriteUShort(Server.World[channelId].Port);
                Send(outPacket);
            }
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
