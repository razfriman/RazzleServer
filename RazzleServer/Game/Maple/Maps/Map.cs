using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.WzLib;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Map
    {
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

            //foreach (Datum datum in new Datums("map_footholds", Database.SchemaMCDB).Populate("mapid = {0}", key))
            //{
            //    this[key].Footholds.Add(new Foothold(datum));
            //}

            //foreach (Datum datum in new Datums("map_seats", Database.SchemaMCDB).Populate("mapid = {0}", key))
            //{
            //    this[key].Seats.Add(new Seat(datum));
            //}

            //foreach (Datum datum in new Datums("map_portals", Database.SchemaMCDB).Populate("mapid = {0}", key))
            //{
            //    this[key].Portals.Add(new Portal(datum));
            //}

            //foreach (Datum datum in new Datums("map_life", Database.SchemaMCDB).Populate("mapid = {0}", key))
            //{
            //    switch ((string)datum["life_type"])
            //    {
            //        case "npc":
            //            {
            //                this[key].Npcs.Add(new Npc(datum));
            //            }
            //            break;

            //        case "mob":
            //            this[key].SpawnPoints.Add(new SpawnPoint(datum, true));
            //            break;

            //        case "reactor":
            //            this[key].SpawnPoints.Add(new SpawnPoint(datum, false));
            //            break;
            //    }
            //}

            //this[key].SpawnPoints.Spawn();
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

        private int mObjectIDs;

        public int AssignObjectID() => ++mObjectIDs;
    }
}
