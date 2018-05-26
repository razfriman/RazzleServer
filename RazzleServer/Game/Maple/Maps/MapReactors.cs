using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapReactors : MapObjects<Reactor>
    {
        public MapReactors(Map map) : base(map) { }


        public override void OnItemAdded(Reactor item)
        {
            lock (this)
            {
                if (DataProvider.IsInitialized)
                {
                    var oPacket = item.GetCreatePacket();
                    Map.Broadcast(oPacket);
                }
            }
        }

        public override void OnItemRemoved(Reactor item)
        {
            lock (this)
            {
                if (DataProvider.IsInitialized)
                {
                    Map.Broadcast(item.GetDestroyPacket());
                }

                if (item.SpawnPoint != null)
                {
                    Delay.Execute(() => item.SpawnPoint.Spawn(), (item.SpawnPoint.RespawnTime <= 0 ? 30 : item.SpawnPoint.RespawnTime) * 100);
                }
            }
        }
    }
}
