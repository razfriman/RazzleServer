using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
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

        public Map(Datum datum)
        {
            MapleID = (int)datum["mapid"];
            ReturnMapID = (int)datum["return_map"];
            ForcedReturnMapID = (int)datum["forced_return_map"];

            RegenerationRate = (sbyte)datum["regen_rate"];
            DecreaseHP = (byte)datum["decrease_hp"];
            DamagePerSecond = (ushort)datum["damage_per_second"];
            ProtectorItemID = (int)datum["protect_item"];
            ShipKind = (sbyte)datum["ship_kind"];
            SpawnRate = (double)datum["mob_rate"];
            RequiredLevel = (byte)datum["min_level_limit"];
            TimeLimit = (int)datum["time_limit"];

            IsTown = ((string)datum["flags"]).Contains("town");
            HasClock = ((string)datum["flags"]).Contains("clock");
            IsEverlasting = ((string)datum["flags"]).Contains("everlast");
            DisablesTownScroll = ((string)datum["flags"]).Contains("scroll_disable");
            IsSwim = ((string)datum["flags"]).Contains("swim");
            ShufflesReactors = ((string)datum["flags"]).Contains("shuffle_reactors");
            UniqueShuffledReactor = (string)datum["shuffle_name"];
            IsShop = ((string)datum["flags"]).Contains("shop");
            NoPartyLeaderPass = ((string)datum["flags"]).Contains("no_party_leader_pass");

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

        public void Notify(string text, NoticeType type = NoticeType.Popup)
        {
            foreach (Character character in Characters)
            {
                character.Notify(text, type);
            }
        }

        // TODO: Refactor this.

        private int mObjectIDs = 0;

        public int AssignObjectID()
        {
            return ++mObjectIDs;
        }
    }
}
