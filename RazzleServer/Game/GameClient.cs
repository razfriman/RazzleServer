using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using RazzleServer.Common.Network;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Server;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game
{
    public sealed class GameClient : AClient
    {
        public static Dictionary<ClientOperationCode, List<GamePacketHandler>> PacketHandlers = new Dictionary<ClientOperationCode, List<GamePacketHandler>>();
        public Account Account { get; set; }
        public ChannelServer Server { get; set; }
        public Character Character { get; set; }

        private static ILogger Log = LogManager.Log;

        public GameClient(Socket session, ChannelServer server) : base(session)
        {
            Socket = new ClientSocket(session, this, ServerConfig.Instance.Version, ServerConfig.Instance.AESKey);
            Server = server;
            Host = Socket.Host;
            Port = Socket.Port;
            Connected = true;
        }

        public static void RegisterPacketHandlers()
        {

            var types = Assembly.GetEntryAssembly().GetTypes();

            var handlerCount = 0;

            foreach (var type in types)
            {
                var attributes = type.GetTypeInfo().GetCustomAttributes()
                                     .OfType<PacketHandlerAttribute>()
                                     .ToList();

                foreach (var attribute in attributes)
                {
                    var header = attribute.Header;

                    if (!PacketHandlers.ContainsKey(header))
                    {
                        PacketHandlers[header] = new List<GamePacketHandler>();
                    }

                    handlerCount++;
                    var handler = (GamePacketHandler)Activator.CreateInstance(type);
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

        public override void Disconnected()
        {
            var save = Character;
            try
            {
                Connected = false;
                Server.RemoveClient(this);
                Socket?.Dispose();
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Error while disconnecting. Account [{Account?.Username}] Character [{save?.Name}]");
            }
        }



        private void Initialize(PacketReader inPacket)
        {
            int accountID;
            int characterID = inPacket.ReadInt();

            if ((accountID = WvsGame.CenterConnection.ValidateMigration(Socket.Host, characterID)) == 0)
            {
                Terminate("Invalid migration");
                return;
            }

            Character = new Character(characterID, this);
            Character.Load();
            Character.Initialize();
        }

        public override void Register()
        {
            //WvsGame.CenterConnection.UpdatePopulation(0);
            base.Register();
        }

        public override void Unregister()
        {
            //WvsGame.CenterConnection.UpdatePopulation(0);
            base.Unregister();
        }

        public override void Terminate(string message = null)
        {
            if (Character != null)
            {
                Character.Save();
                Character.LastNpc = null;
                Character.Map.Characters.Remove(Character);
            }
        }

        private void ChangeChannel(PacketReader inPacket)
        {
            byte channelID = inPacket.ReadByte();

            var outPacket = new PacketWriter(ServerOperationCode.MigrateCommand);
            outPacket.WriteBool(true);
            outPacket.WriteBytes(Socket.HostBytes);
            outPacket.WriteUShort(WvsGame.CenterConnection.GetChannelPort(channelID));
            Send(outPacket);
        }
    }
}