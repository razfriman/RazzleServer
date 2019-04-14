using RazzleServer.Common.Constants;
using RazzleServer.DataProvider.References;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class SpawnPoint : LifeObject
    {
        public SpawnPoint() { }

        public SpawnPoint(SpawnPointReference reference)
            : base(reference)
        {
        }

        public void Spawn()
        {
            if (Type == LifeObjectType.Mob)
            {
                Map.Mobs.Add(new Mob(this));
            }
        }
    }
}
