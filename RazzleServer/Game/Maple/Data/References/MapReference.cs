using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Data.References
{
    public class MapReference
    {
        public int MapleId { get; set; }
        public int ReturnMapId { get; set; }
        public int ForcedReturnMapId { get; set; }
        public sbyte RegenerationRate { get; set; }
        public byte DecreaseHp { get; set; }
        public ushort DamagePerSecond { get; set; }
        public int ProtectorItemId { get; set; }
        public sbyte ShipKind { get; set; }
        public byte RequiredLevel { get; set; }
        public int TimeLimit { get; set; }
        public double SpawnRate { get; set; }
        public bool IsTown { get; set; }
        public bool HasClock { get; set; }
        public bool IsEverlasting { get; set; }
        public bool DisablesTownScroll { get; set; }
        public bool IsSwim { get; set; }
        public bool ShufflesReactors { get; set; }
        public string UniqueShuffledReactor { get; set; }
        public bool IsShop { get; set; }
        public bool NoPartyLeaderPass { get; set; }
        public List<Foothold> Footholds { get; set; } = new List<Foothold>();
        public List<Portal> Portals { get; set; } = new List<Portal>();
        public List<Seat> Seats { get; set; } = new List<Seat>();
        public List<SpawnPoint> SpawnPoints { get; set; } = new List<SpawnPoint>();
        public List<Npc> Npcs { get; set; } = new List<Npc>();

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
