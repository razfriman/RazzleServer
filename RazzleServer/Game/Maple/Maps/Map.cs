﻿using System;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Map
    {
        private int _mObjectIds;

        public int MapleId { get; }
        public GameServer Server { get; set; }
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
        public MapReference CachedReference => DataProvider.Maps.Data[MapleId];
        public FieldLimitFlags FieldLimit { get; }

        public Map(GameServer server, int id)
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
            reference.Footholds.ForEach(Footholds.Footholds.Add);
            reference.Npcs.ForEach(Npcs.Add);
            reference.SpawnPoints.ForEach(SpawnPoints.Add);
            reference.Portals.ForEach(Portals.Add);

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
