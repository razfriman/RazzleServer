using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class SpawnPoint : LifeObject
    {
        public SpawnPoint(WzImageProperty img, LifeObjectType type)
            : base(img, type)
        {
        }

        public void Spawn()
        {
            if (Type == LifeObjectType.Mob)
            {
                Map.Mobs.Add(new Mob(this));
            }
            else if (Type == LifeObjectType.Reactor)
            {
                Map.Reactors.Add(new Reactor(this));
            }
        }
    }
}
