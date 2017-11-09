using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.WzLib;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Map
    {
        private int mObjectIDs;

        public int MapleID { get; private set; }
        public int ReturnMapID { get; private set; }
        public int ForcedReturnMapID { get; private set; }
        public sbyte RegenerationRate { get; private set; }
        public byte DecreaseHP { get; private set; }
        public ushort DamagePerSecond { get; private set; }
        public int ProtectorItemID { get; private set; }
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

        public Map(WzImage img)
        {
            var name = img.Name.Remove(9);
            if (!int.TryParse(name, out var id))
            {
                return;
            }
            MapleID = id;
            Characters = new MapCharacters(this);
            Drops = new MapDrops(this);
            Mobs = new MapMobs(this);
            Npcs = new MapNpcs(this);
            Footholds = new MapFootholds(this);
            Seats = new MapSeats(this);
            Reactors = new MapReactors(this);
            Portals = new MapPortals(this);
            SpawnPoints = new MapSpawnPoints(this);
            PlayerShops = new MapPlayerShops(this);

            var info = img["info"];
            IsTown = (info["town"]?.GetInt() ?? 0) > 0;
            SpawnRate = info["mobRate"]?.GetDouble() ?? 1.0;
            ReturnMapID = info["returnMap"]?.GetInt() ?? 0;
            ForcedReturnMapID = info["forcedReturn"]?.GetInt() ?? 0;

            img["portal"]?.WzProperties?.ForEach(x => Portals.Add(new Portal(x)));
            img["seat"]?.WzProperties?.ForEach(x => Seats.Add(new Seat(x)));
            img["footholds"]?.WzProperties?.ForEach(x => Footholds.Add(new Foothold(x)));
            img["reactor"]?.WzProperties?.ForEach(x => SpawnPoints.Add(new SpawnPoint(x, LifeObjectType.Reactor)));
            img["seat"]?.WzProperties?.ForEach(x => Seats.Add(new Seat(x)));
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

        public void Broadcast(PacketWriter oPacket, Character ignored = null)
        {
            foreach (Character character in Characters)
            {
                if (character != ignored)
                {
                    character.Client.Send(oPacket);
                }
            }
        }

        public void Notify(string text, NoticeType type = NoticeType.Popup) => Characters.ToList().ForEach(x => x.Notify(text, type));

        public int AssignObjectID() => ++mObjectIDs;
    }
}
