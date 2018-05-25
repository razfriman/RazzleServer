using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using RazzleServer.Center;
using RazzleServer.Center.Maple;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Server;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game
{
    public class GameServer : MapleServer<GameClient>
    {
        public byte ChannelId { get; set; }
        public int Population { get; set; }
        public World World { get; set; }

        public static Dictionary<ClientOperationCode, List<GamePacketHandler>> PacketHandlers { get; private set; } = new Dictionary<ClientOperationCode, List<GamePacketHandler>>();

        public GameServer(ServerManager manager, World world, ushort port, byte channelId) : base(manager)
        {
            World = world;
            Port = port;
            ChannelId = channelId;
            Start(new IPAddress(new byte[] { 0, 0, 0, 0 }), Port);
        }

        public override void Dispose() => ShutDown();

        public static void RegisterPacketHandlers()
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
                }
            }
        }

        public Character GetCharacterById(int id) => Clients.Values.Select(x => x.Character).FirstOrDefault(x => x.Id == id);
        public Character GetCharacterByName(string name) => Clients.Values.Select(x => x.Character).FirstOrDefault(x => x.Name == name);
    }
}
