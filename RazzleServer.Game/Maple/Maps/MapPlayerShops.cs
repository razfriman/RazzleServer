using RazzleServer.Game.Maple.Interaction;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapPlayerShops : MapObjects<PlayerShop>
    {
        public MapPlayerShops(Map map) : base(map) { }

        public override void Add(PlayerShop item)
        {
            lock (this)
            {
                base.Add(item);
                Map.Send(item.GetCreatePacket());
            }
        }

        public override void Remove(PlayerShop item)
        {
            lock (this)
            {
                Map.Send(item.GetDestroyPacket());
                base.Remove(item);
            }
        }
    }
}
