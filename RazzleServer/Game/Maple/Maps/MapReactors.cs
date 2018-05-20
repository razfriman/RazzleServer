using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapReactors : MapObjects<Reactor>
    {
        public MapReactors(Map map) : base(map) { }

        protected override void InsertItem(int index, Reactor item)
        {
            lock (this)
            {
                base.InsertItem(index, item);

                if (DataProvider.IsInitialized)
                {
                    var oPacket = item.GetCreatePacket();
                    Map.Broadcast(oPacket);
                }
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (this)
            {
                var item = Items[index];

                if (DataProvider.IsInitialized)
                {
                    Map.Broadcast(item.GetDestroyPacket());
                }

                base.RemoveItem(index);

                if (item.SpawnPoint != null)
                {
                    Delay.Execute(() => item.SpawnPoint.Spawn(), (item.SpawnPoint.RespawnTime <= 0 ? 30 : item.SpawnPoint.RespawnTime) * 100);
                }
            }
        }
    }
}
