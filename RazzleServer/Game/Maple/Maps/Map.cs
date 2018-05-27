using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Map
    {
        private int mObjectIds;

        public int MapleId { get; private set; }
        public GameServer Server { get; set; }
        public MapCharacters Characters { get; private set; }
        public MapDrops Drops { get; private set; }
        public MapMobs Mobs { get; private set; }
        public MapNpcs Npcs { get; private set; }
        public MapReactors Reactors { get; private set; }
        public MapFootholds Footholds { get; private set; }
        public MapSeats Seats { get; private set; }
        public MapPortals Portals { get; private set; }
        public MapSpawnPoints SpawnPoints { get; private set; }
        public MapPlayerShops PlayerShops { get; private set; }
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
            reference.Npcs.ForEach(x => Npcs.Add(x));
            reference.Footholds.ForEach(x => Footholds.Footholds.Add(x));
            reference.Npcs.ForEach(x => Npcs.Add(x));
            reference.SpawnPoints.ForEach(x => SpawnPoints.Add(x));
            reference.Portals.ForEach(x => Portals.Add(x));

            SpawnPoints.Spawn();
        }

        public void Send(PacketWriter pw, Character except = null)
        {
            Characters.Values
            .Where(x => x.Id != except?.Id)
            .ToList()
            .ForEach(x => x.Client.Send(pw));
        }

        public int AssignObjectId() => ++mObjectIds;
    }
}
