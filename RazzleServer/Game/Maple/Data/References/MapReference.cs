using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class MapReference
    {
        public int MapleId { get; set; }
        public int ReturnMapId { get; set; }
        public int ForcedReturnMapId { get; set; }
        public byte RecoveryHp { get; set; }
        public byte DecreaseHp { get; set; }
        public bool HasShip { get; set; }
        public int TimeLimit { get; set; }
        public double SpawnRate { get; set; }
        public bool IsTown { get; set; }
        public bool HasClock { get; set; }
        public bool IsEverlasting { get; set; }
        public bool DisablesTownScroll { get; set; }
        public bool IsSwim { get; set; }
        public bool IsShop { get; set; }
        public bool NoPartyLeaderPass { get; set; }
        public int VrRight { get; set; }

        public int VrBottom { get; set; }

        public int VrLeft { get; set; }

        public int VrTop { get; set; }

        public bool IsEverlastDrops { get; set; }

        public bool IsPersonalShop { get; set; }

        public int Recovery { get; set; }

        public bool IsScrollDisable { get; set; }

        public bool IsUnableToShop { get; set; }

        public bool IsUnableToChangeChannel { get; set; }
        public FieldLimitFlags FieldLimit { get; set; }
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
            FieldLimit = (FieldLimitFlags)(info["fieldLimit"]?.GetInt() ?? 0);
            IsUnableToChangeChannel = (info["bUnableToChangeChannel"]?.GetInt() ?? 0) > 0;
            IsUnableToShop = (info["bUnableToShop"]?.GetInt() ?? 0) > 0;
            IsEverlastDrops = (info["everlast"]?.GetInt() ?? 0) > 0;
            IsPersonalShop = (info["personalShop"]?.GetInt() ?? 0) > 0;
            RecoveryHp = (byte)(info["recovery"]?.GetInt() ?? 0);
            DecreaseHp = (byte)(info["decHP"]?.GetInt() ?? 0);
            IsScrollDisable = (info["scrollDisable"]?.GetInt() ?? 0) > 0;
            TimeLimit = info["timeLimit"]?.GetInt() ?? 0;
            VrTop = info["VRTop"]?.GetInt() ?? 0;
            VrLeft = info["VRLeft"]?.GetInt() ?? 0;
            VrBottom = info["VRBottom"]?.GetInt() ?? 0;
            VrRight = info["VRRight"]?.GetInt() ?? 0;
            //mapMark
            //fieldType
            //cloud
            //snow
            //rain
            //fs
            HasClock = img["clock"] != null;
            HasShip = img["shipObj"] != null;
            img["portal"]?.WzProperties?.ForEach(x => Portals.Add(new Portal(x)));
            img["seat"]?.WzProperties?.ForEach(x => Seats.Add(new Seat(x)));
            img["foothold"]?.WzProperties.SelectMany(x => x.WzProperties).SelectMany(x => x.WzProperties).ToList()
                .ForEach(x => Footholds.Add(new Foothold(x)));
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
