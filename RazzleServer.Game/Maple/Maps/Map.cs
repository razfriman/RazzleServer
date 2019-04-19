using System;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.DataProvider;
using RazzleServer.DataProvider.References;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Server;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Map
    {
        private int _mObjectIds;

        public int MapleId { get; }
        public IGameServer Server { get; set; }
        public MapCharacters Characters { get; }
        public MapDrops Drops { get; }
        public MapMobs Mobs { get; }
        public MapNpcs Npcs { get; }
        public MapFootholds Footholds { get; }
        public MapSeats Seats { get; }
        public MapPortals Portals { get; }
        public MapSpawnPoints SpawnPoints { get; }
        public MapPlayerShops PlayerShops { get; }
        public MapMists Mists { get; }
        public MapSummons Summons { get; }
        public MapReference CachedReference => CachedData.Maps.Data[MapleId];
        public FieldLimitFlags FieldLimit { get; }

        public Map(IGameServer server, int id)
        {
            Server = server;
            MapleId = id;

            Characters = new MapCharacters(this);
            Drops = new MapDrops(this);
            Mobs = new MapMobs(this);
            Npcs = new MapNpcs(this);
            Footholds = new MapFootholds();
            Seats = new MapSeats(this);
            Portals = new MapPortals(this);
            SpawnPoints = new MapSpawnPoints(this);
            PlayerShops = new MapPlayerShops(this);
            Summons = new MapSummons(this);
            Mists = new MapMists(this);

            var reference = CachedReference;
            reference.Footholds.ForEach(x => Footholds.Footholds.Add(new Foothold(x)));
            reference.Npcs.ForEach(x => Npcs.Add(new Npc(x)));
            reference.SpawnPoints.ForEach(x => SpawnPoints.Add(new SpawnPoint(x)));
            reference.Portals.ForEach(x => Portals.Add(new Portal(x)));

            Footholds.CalculateBounds();
            SpawnPoints.Spawn();
            FieldLimit = reference.FieldLimit;
        }

        public Map(int id) => MapleId = id;

        public void Send(PacketWriter pw, Character except = null) =>
            Characters.Values
                .Where(x => x.Id != except?.Id)
                .ToList()
                .ForEach(x => x.Send(pw));

        public int AssignObjectId() => ++_mObjectIds;

        public void SendWeatherEffect(int mapleId, string message, bool isAdmin = false, int delay = 30000)
        {
            using var pw = new PacketWriter(ServerOperationCode.WeatherEffect);
            pw.WriteBool(isAdmin);
            pw.WriteInt(mapleId);
            if (!isAdmin)
            {
                pw.WriteString(message);
            }

            Send(pw);

            if (mapleId != 0)
            {
                TaskRunner.Run(() =>
                {
                    SendWeatherEffect(0, message, isAdmin);
                }, TimeSpan.FromMilliseconds(delay));
            }
        }

        public void SendJukeboxSong(int mapleId, string characterName, int delay = 30000)
        {
            using var pw = new PacketWriter(ServerOperationCode.JukeboxEffect);
            pw.WriteInt(mapleId);
            if (mapleId != 0)
            {
                pw.WriteString(characterName);
            }

            Send(pw);

            if (mapleId != 0)
            {
                TaskRunner.Run(() =>
                {
                    SendJukeboxSong(0, characterName);
                }, TimeSpan.FromMilliseconds(delay));
            }
        }
    }
}
