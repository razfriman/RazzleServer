using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using MapleLib.PacketLib;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Player;
using RazzleServer.Scripts;
using RazzleServer.Server;
using RazzleServer.Util;

namespace RazzleServer.Game
{
    public sealed class GameClient : AClient
    {
        public static Dictionary<ClientOperationCode, List<GamePacketHandler>> PacketHandlers = new Dictionary<ClientOperationCode, List<GamePacketHandler>>();
        public MapleAccount Account { get; set; }
        public byte Channel { get; set; }
        public ChannelServer Server { get; set; }
        public NpcEngine NpcEngine { get; set; }

        private static ILogger Log = LogManager.Log;

        public GameClient(Socket session, ChannelServer server) : base(session)
        {
            Socket = new ClientSocket(session, this, ServerConfig.Instance.Version, ServerConfig.Instance.AESKey);
            Server = server;
            Host = Socket.Host;
            Port = Socket.Port;
            Channel = 0;
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
            var save = Account?.Character; ;
            try
            {
                Account?.Release();
                Connected = false;
                Server.RemoveClient(this);
                save?.LoggedOut();
                NpcEngine?.Dispose();
                Socket?.Dispose();
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Error while disconnecting. Account [{Account?.Name}] Character [{save?.Name}]");
            }
        }


        public long ID { get; private set; }

        public Account Account { get; set; }
        public Character Character { get; set; }

        public GameClient(Socket socket)
            : base(socket)
        {
            this.ID = Application.Random.Next();
        }

        private void Initialize(PacketReader inPacket)
        {
            int accountID;
            int characterID = inPacket.ReadInt();

            if ((accountID = WvsGame.CenterConnection.ValidateMigration(this.RemoteEndPoint.Address.ToString(), characterID)) == 0)
            {
                this.Stop();

                return;
            }

            this.Character = new Character(characterID, this);
            this.Character.Load();
            this.Character.Initialize();

            this.Title = this.Character.Name;
        }

        protected override void Register()
        {
            WvsGame.Clients.Add(this);
        }

        protected override void Terminate()
        {
            if (this.Character != null)
            {
                this.Character.Save();
                this.Character.LastNpc = null;
                this.Character.Map.Characters.Remove(this.Character);
            }
        }

        protected override void Unregister()
        {
            WvsGame.Clients.Remove(this);
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
}
