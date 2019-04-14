﻿using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapSpawnPoints : MapObjects<SpawnPoint>
    {
        public MapSpawnPoints(Map map) : base(map) { }

        public MapSpawnPoints() { }

        public void Spawn()
        {
            foreach (var spawnPoint in Values)
            {
                spawnPoint.Spawn();
            }
        }
    }
}
