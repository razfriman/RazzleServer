using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Data
{
    public class MapReference
    {
        public int MapleId { get; private set; }
        public int ReturnMapId { get; private set; }
        public int ForcedReturnMapId { get; private set; }
        public sbyte RegenerationRate { get; private set; }
        public byte DecreaseHP { get; private set; }
        public ushort DamagePerSecond { get; private set; }
        public int ProtectorItemId { get; private set; }
        public sbyte ShipKind { get; private set; }
        public byte RequiredLevel { get; private set; }
        public int TimeLimit { get; private set; }
        public double SpawnRate { get; private set; }
        public bool IsTown { get; private set; }
        public bool HasClock { get; private set; }
        public bool IsEverlasting { get; private set; }
        public bool DisablesTownScroll { get; private set; }
        public bool IsSwim { get; private set; }
        public bool ShufflesReactors { get; private set; }
        public string UniqueShuffledReactor { get; private set; }
        public bool IsShop { get; private set; }
        public bool NoPartyLeaderPass { get; private set; }

        public MapMobs Mobs { get; private set; }
        public MapNpcs Npcs { get; private set; }
        public MapReactors Reactors { get; private set; }
        public MapFootholds Footholds { get; private set; }
        public MapSeats Seats { get; private set; }
        public MapPortals Portals { get; private set; }
        public MapSpawnPoints SpawnPoints { get; private set; }

        public MapReference()
        {
        }

        public MapReference(WzImage img)
        {
            var name = img.Name.Remove(9);
            if (!int.TryParse(name, out var id))
            {
                return;
            }
            MapleId = id;
            Mobs = new MapMobs(null);
            Npcs = new MapNpcs(null);
            Footholds = new MapFootholds();
            Seats = new MapSeats(null);
            Reactors = new MapReactors(null);
            Portals = new MapPortals(null);
            SpawnPoints = new MapSpawnPoints(null);

            var info = img["info"];
            IsTown = (info["town"]?.GetInt() ?? 0) > 0;
            SpawnRate = info["mobRate"]?.GetDouble() ?? 1.0;
            ReturnMapId = info["returnMap"]?.GetInt() ?? 0;
            ForcedReturnMapId = info["forcedReturn"]?.GetInt() ?? 0;

            img["portal"]?.WzProperties?.ForEach(x => Portals.Add(new Portal(x)));
            img["seat"]?.WzProperties?.ForEach(x => Seats.Add(new Seat(x)));
            // img["foothold"]?.WzProperties?.ForEach(x => Footholds.Footholds.Add(new Foothold(x)));
            img["reactor"]?.WzProperties?.ForEach(x => SpawnPoints.Add(new SpawnPoint(x, LifeObjectType.Reactor)));
            img["seat"]?.WzProperties?.ForEach(x => Seats.Add(new Seat(x)));
            img["life"]?.WzProperties?.ForEach(life =>
            {
                var type = life["type"].GetString();

                switch (type)
                {
                    case "n":
                        Npcs.Add(new Npc(life));
                        break;
                    case "m":
                        SpawnPoints.Add(new SpawnPoint(life, LifeObjectType.Mob));
                        break;
                }
            });

            SpawnPoints.Spawn();
        }
    }
}
