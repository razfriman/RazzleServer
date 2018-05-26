using RazzleServer.Game.Maple.Interaction;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapPlayerShops : MapObjects<PlayerShop>
    {
        public MapPlayerShops(Map map) : base(map) { }

        public override void OnItemAdded(PlayerShop item)
        {
            lock (this)
            {
                Map.Broadcast(item.GetCreatePacket());
            }
        }

        public override void OnItemRemoved(PlayerShop item)
        {
            lock (this)
            {
                Map.Broadcast(item.GetDestroyPacket());
            }
        }
    }
}
