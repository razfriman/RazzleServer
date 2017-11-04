using System.Net;
using Microsoft.Extensions.Logging;
using RazzleServer.Server;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Server;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game
{
    public class GameServer : MapleServer<GameClient>
    {
        public GameCenterClient CenterConnection { get; set; }
        public byte ChannelID { get; set; }
        public WorldConfig World { get; set; }
        public Dictionary<ClientOperationCode, List<GamePacketHandler>> PacketHandlers { get; private set; } = new Dictionary<ClientOperationCode, List<GamePacketHandler>>();

        public static GameServer CurrentInstance { get; private set; }

        public GameServer(WorldConfig world)
        {
            World = world;
            Port = world.Port;
            StartCenterConnection(IPAddress.Loopback, ServerConfig.Instance.CenterPort);
            CurrentInstance = this;
        }

        public override void ServerRegistered() => Start(new IPAddress(new byte[] { 0, 0, 0, 0 }), Port);

        public override void CenterServerConnected()
        {
            Log.LogInformation("CENTER CONNECTED");
            CenterConnection = new GameCenterClient(this, _centerSocket);
            CenterConnection.Socket.Crypto.HandshakeFinished += (SIV, RIV) => SendRegistrationRequest();
        }

        public void SendRegistrationRequest()
        {
            Log.LogInformation("GAME SENDING REG REQUEST");
            var pw = new PacketWriter(InteroperabilityOperationCode.RegistrationRequest);
            pw.WriteByte((int)ServerType.Channel);
            CenterConnection?.Send(pw);
        }

        public override void Dispose()
        {
            CenterConnection?.Dispose();
            ShutDown();
        }

        public override void RegisterPacketHandlers()
        {
            var types = Assembly.GetEntryAssembly()
                                .GetTypes()
                                .Where(x => x.IsSubclassOf(typeof(GamePacketHandler)));

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

        public Character GetCharacterByName(string name) => Clients.Values.Select(x => x.Character).FirstOrDefault(x => x.Name == name);   
    }
}
