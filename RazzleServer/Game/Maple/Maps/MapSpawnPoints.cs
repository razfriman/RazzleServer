namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapSpawnPoints : MapObjects<SpawnPoint>
    {
        public MapSpawnPoints(Map map) : base(map) { }

        public void Spawn()
        {
            foreach (SpawnPoint spawnPoint in this)
            {
                spawnPoint.Spawn();
            }
        }
    }
}
