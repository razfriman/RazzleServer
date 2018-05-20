using RazzleServer.Game.Maple.Interaction;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapPlayerShops : MapObjects<PlayerShop>
    {
        public MapPlayerShops(Map map) : base(map) { }

        protected override void InsertItem(int index, PlayerShop item)
        {
            lock (this)
            {
                base.InsertItem(index, item);
                Map.Broadcast(item.GetCreatePacket());
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (this)
            {
                var item = Items[index];
                Map.Broadcast(item.GetDestroyPacket());
                base.RemoveItem(index);
            }
        }
    }
}
