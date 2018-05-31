using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapReactors : MapObjects<Reactor>
    {
        public MapReactors(Map map) : base(map) { }


        public override void Add(Reactor item)
        {
            lock (this)
            {
                base.Add(item);
                Map.Send(item.GetCreatePacket());
            }
        }

        public override void Remove(Reactor item)
        {
            lock (this)
            {
                Map.Send(item.GetDestroyPacket());

                base.Remove(item);

                if (item.SpawnPoint != null)
                {
                    Delay.Execute(item.SpawnPoint.Spawn, (item.SpawnPoint.RespawnTime <= 0 ? 30 : item.SpawnPoint.RespawnTime) * 100);
                }
            }
        }
    }
}
