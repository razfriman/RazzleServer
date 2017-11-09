using System;
using System.Net.Sockets;
using RazzleServer.Common.Network;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game
{
    public sealed class GameClient : AClient
    {
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
            ClientOperationCode header = ClientOperationCode.Unknown;
            try
            {
                if (packet.Available >= 2)
                {
                    header = (ClientOperationCode)packet.ReadUShort();

                    if (GameServer.PacketHandlers.ContainsKey(header))
                    {
                        Log.LogInformation($"Received [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");

                        foreach (var handler in GameServer.PacketHandlers[header])
                        {
                            handler.HandlePacket(packet, this);
                        }
                    }
                    else
                    {
                        Log.LogWarning($"Unhandled Packet [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");
                        Character?.Release();
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Packet Processing Error [{header.ToString()}] - {e.Message} - {e.StackTrace}");
            }
        }

        public override void Disconnected()
        {
            var save = Character;
            try
            {
                Character?.Save();
                Character?.Map?.Characters?.Remove(Character);
                Connected = false;
                Server.RemoveClient(this);
                Socket?.Dispose();
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Error while disconnecting. Account [{Account?.Username}] Character [{save?.Name}]");
            }
        }

        public void ChangeChannel(byte channelID)
        {
            Server.Manager.Migrate(Host, Account.ID, Character.ID);

            var outPacket = new PacketWriter(ServerOperationCode.ChangeChannel);
            outPacket.WriteBool(true);
            outPacket.WriteBytes(Socket.HostBytes);
            outPacket.WriteUShort(Server.World[channelID].Port);
            Send(outPacket);
        }

        public void Ping() => Send(new PacketWriter(ServerOperationCode.Ping));
    }
}