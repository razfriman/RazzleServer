using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Common;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Game
{
    public class GameServer : MapleServer<GameClient, GamePacketHandler>
    {
        public byte ChannelId { get; set; }
        public int Population { get; set; }
        public World World { get; set; }
        public Dictionary<int, Map> Maps { get; set; } = new Dictionary<int, Map>();

        public Map this[int id]
        {
            get
            {
                if (Maps.ContainsKey(id))
                {
                    return Maps[id];
                }

                var map = new Map(this, id);
                return Maps[id] = map;
            }
        }

        public override ILogger Logger => Log.ForContext<GameServer>();


        public GameServer(ServerManager manager, World world, ushort port, byte channelId) : base(manager)
        {
            World = world;
            Port = port;
            ChannelId = channelId;
            Start(new IPAddress(new byte[] {0, 0, 0, 0}), Port);
        }

        public override void Dispose() => Shutdown();

        public void Send(PacketWriter pw, GameClient except = null) =>
            Clients
                .Values
                .Where(x => x.Key != except?.Key)
                .ToList()
                .ForEach(x => x.Send(pw));

        public Character GetCharacterById(int id) => Clients
            .Values
            .Select(x => x.Character)
            .FirstOrDefault(x => x.Id == id);

        public Character GetCharacterByName(string name) => Clients
            .Values
            .Select(x => x.Character)
            .FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public bool CharacterExists(string name)
        {
            using (var dbContext = new MapleDbContext())
            {
                return dbContext.Characters
                    .Where(x => x.WorldId == World.Id)
                    .Any(x => x.Name == name);
            }
        }
    }
}
