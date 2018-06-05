using System;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Server;
using RazzleServer.Common.Network;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game
{
    public sealed class GameClient : AClient
    {
        public static int PingDelay = 5000;

        public Account Account { get; set; }
        public GameServer Server { get; set; }
        public Character Character { get; set; }


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
                            Log.LogInformation($"Received [{header.ToString()}] {packet.ToPacketString()}");
                        }

                        foreach (var handler in Server.PacketHandlers[header])
                        {
                            handler.HandlePacket(packet, this);
                        }
                    }
                    else
                    {
                        Log.LogWarning($"Unhandled Packet [{header.ToString()}] {packet.ToPacketString()}");
                        Character?.Release();
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Packet Processing Error [{header.ToString()}] {packet.ToPacketString()} - {e.Message} - {e.StackTrace}");
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
                Log.LogError(e, $"Error while disconnecting. Account [{Account?.Username}] Character [{save?.Name}]");
            }
        }

        public void ChangeChannel(byte channelId)
        {
            Character.Save();
            Server.Manager.Migrate(Host, Account.Id, Character.Id);

            var outPacket = new PacketWriter(ServerOperationCode.ChangeChannel);
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