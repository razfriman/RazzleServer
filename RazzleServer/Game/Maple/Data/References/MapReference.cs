using System.Collections.Generic;
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
        public byte DecreaseHp { get; private set; }
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

        public MapReference()
        {
        }

        public List<Foothold> Footholds { get; private set; } = new List<Foothold>();
        public List<Portal> Portals { get; set; } = new List<Portal>();
        public List<Seat> Seats { get; set; } = new List<Seat>();
        public List<SpawnPoint> SpawnPoints { get; set; } = new List<SpawnPoint>();
        public List<Npc> Npcs { get; set; } = new List<Npc>();

        public MapReference(WzImage img)
        {
            var name = img.Name.Remove(9);
            if (!int.TryParse(name, out var id))
            {
                return;
            }
            MapleId = id;

            var info = img["info"];
            IsTown = (info["town"]?.GetInt() ?? 0) > 0;
            SpawnRate = info["mobRate"]?.GetDouble() ?? 1.0;
            ReturnMapId = info["returnMap"]?.GetInt() ?? 0;
            ForcedReturnMapId = info["forcedReturn"]?.GetInt() ?? 0;

            img["portal"]?.WzProperties?.ForEach(x => Portals.Add(new Portal(x)));
            img["seat"]?.WzProperties?.ForEach(x => Seats.Add(new Seat(x)));
            // img["foothold"]?.WzProperties?.ForEach(x => Footholds.Add(new Foothold(x)));
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
        }
    }
}
