using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Data;
using RazzleServer.Game.Server;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Game
{
    public class GameServer : MapleServer<GameClient, GamePacketHandler>, IGameServer
    {
        public byte ChannelId { get; set; }
        public int Population { get; set; }
        public World World { get; set; }
        
        private readonly Dictionary<int, Map> _maps = new Dictionary<int, Map>();

        public Map this[int id]
        {
            get
            {
                if (_maps.ContainsKey(id))
                {
                    return _maps[id];
                }

                var map = new Map(this, id);
                return _maps[id] = map;
            }
        }

        public override ILogger Logger => Log.ForContext<GameServer>();


        public GameServer(IServerManager manager, World world, ushort port, byte channelId) : base(manager)
        {
            World = world;
            Port = port;
            ChannelId = channelId;
        }

        public void Send(PacketWriter pw, GameClient except = null) =>
            Clients
                .Values
                .Where(x => x.Key != except?.Key)
                .ToList()
                .ForEach(x => x.Send(pw));

        public GameCharacter GetCharacterById(int id) => Clients
            .Values
            .Cast<GameClient>()
            .Select(x => x.GameCharacter)
            .FirstOrDefault(x => x.Id == id);

        public GameCharacter GetCharacterByName(string name) => Clients
            .Values
            .Cast<GameClient>()
            .Select(x => x.GameCharacter)
            .FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public bool CharacterExists(string name)
        {
            using var dbContext = new MapleDbContext();
            return dbContext.Characters
                .Where(x => x.WorldId == World.Id)
                .Any(x => x.Name == name);
        }
    }
}
