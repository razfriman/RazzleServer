using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using RazzleServer.Common.Constants;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.References
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class MapReference
    {
        private readonly ILogger _log = Log.ForContext<MapReference>();

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
        public MapFieldType FieldType { get; set; }
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
        public List<FootholdReference> Footholds { get; set; } = new List<FootholdReference>();
        public List<PortalReference> Portals { get; set; } = new List<PortalReference>();
        public List<SeatReference> Seats { get; set; } = new List<SeatReference>();
        public List<SpawnPointReference> SpawnPoints { get; set; } = new List<SpawnPointReference>();
        public List<MapNpcReference> Npcs { get; set; } = new List<MapNpcReference>();

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

            foreach (var node in info.WzPropertiesList)
            {
                switch (node.Name)
                {
                    case "mapMark":
                    case "cloud":
                    case "snow":
                    case "rain":
                    case "fs":
                    case "bgm":
                    case "version":
                    case "mapDesc":
                    case "mapName":
                    case "help":
                    case "streetName":
                    case "moveLimit":
                    case "hideMinimap":
                        break;
                    case "town":
                        IsTown = node.GetInt() > 0;
                        break;
                    case "mobRate":
                        SpawnRate = node.GetDouble();
                        break;
                    case "returnMap":
                        ReturnMapId = node.GetInt();
                        break;
                    case "forcedReturn":
                        ForcedReturnMapId = node.GetInt();
                        break;
                    case "fieldLimit":
                        FieldLimit = (FieldLimitFlags)node.GetInt();
                        break;
                    case "bUnableToChangeChannel":
                        IsUnableToChangeChannel = node.GetInt() > 0;
                        break;
                    case "bUnableToShop":
                        IsUnableToShop = node.GetInt() > 0;
                        break;
                    case "everlast":
                        IsEverlastDrops = node.GetInt() > 0;
                        break;
                    case "personalShop":
                        IsPersonalShop = node.GetInt() > 0;
                        break;
                    case "recovery":
                        RecoveryHp = (byte)node.GetInt();
                        break;
                    case "decHP":
                        DecreaseHp = (byte)node.GetInt();
                        break;
                    case "scrollDisable":
                        IsScrollDisable = node.GetInt() > 0;
                        break;
                    case "timeLimit":
                        TimeLimit = node.GetInt();
                        break;
                    case "VRTop":
                        VrTop = node.GetInt();
                        break;
                    case "VRLeft":
                        VrLeft = node.GetInt();
                        break;
                    case "VRBottom":
                        VrBottom = node.GetInt();
                        break;
                    case "VRRight":
                        VrRight = node.GetInt();
                        break;
                    case "fieldType":
                        FieldType = (MapFieldType)node.GetInt();
                        break;
                    default:
                        _log.Warning(
                            $"Unknown map node Skill={MapleId} Name={node.Name} Value={node.WzValue}");
                        break;
                }
            }

            HasClock = img["clock"] != null;
            HasShip = img["shipObj"] != null;

            img["portal"]?.WzPropertiesList?.ToList().ForEach(x => Portals.Add(new PortalReference(x)));
            img["seat"]?.WzPropertiesList?.ToList().ForEach(x => Seats.Add(new SeatReference(x)));
            img["foothold"]?.WzPropertiesList.SelectMany(x => x.WzPropertiesList).SelectMany(x => x.WzPropertiesList)
                .ToList()
                .ForEach(x => Footholds.Add(new FootholdReference(x)));
            img["seat"]?.WzPropertiesList?.ToList().ForEach(x => Seats.Add(new SeatReference(x)));
            img["life"]?.WzPropertiesList?.ToList().ForEach(life =>
            {
                var type = life["type"].GetString();

                switch (type)
                {
                    case "n":
                        Npcs.Add(new MapNpcReference(life));
                        break;
                    case "m":
                        SpawnPoints.Add(new SpawnPointReference(life, LifeObjectType.Mob));
                        break;
                }
            });
        }
    }
}
