using System.Linq;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Map
    {
        private int _mObjectIds;

        public int MapleId { get; }
        public GameServer Server { get; set; }
        public MapCharacters Characters { get; }
        public MapDrops Drops { get; }
        public MapMobs Mobs { get; }
        public MapNpcs Npcs { get; }
        public MapReactors Reactors { get; }
        public MapFootholds Footholds { get; }
        public MapSeats Seats { get; }
        public MapPortals Portals { get; }
        public MapSpawnPoints SpawnPoints { get; }
        public MapPlayerShops PlayerShops { get; }
        public MapReference CachedReference => DataProvider.Maps.Data[MapleId];

        public Map(GameServer server, int id)
        {
            Server = server;
            MapleId = id;

            Characters = new MapCharacters(this);
            Drops = new MapDrops(this);
            Mobs = new MapMobs(this);
            Npcs = new MapNpcs(this);
            Reactors = new MapReactors(this);
            Footholds = new MapFootholds();
            Seats = new MapSeats(this);
            Portals = new MapPortals(this);
            SpawnPoints = new MapSpawnPoints(this);
            PlayerShops = new MapPlayerShops(this);

            var reference = CachedReference;
            reference.Footholds.ForEach(x => Footholds.Footholds.Add(x));
            reference.Npcs.ForEach(x => Npcs.Add(x));
            reference.SpawnPoints.ForEach(x => SpawnPoints.Add(x));
            reference.Portals.ForEach(x => Portals.Add(x));

            SpawnPoints.Spawn();
        }

        public Map(int id)
        {
            MapleId = id;
        }

        public void Send(PacketWriter pw, Character except = null)
        {
            Characters.Values
            .Where(x => x.Id != except?.Id)
            .ToList()
            .ForEach(x => x.Client.Send(pw));
        }

        public int AssignObjectId() => ++_mObjectIds;
    }
}
