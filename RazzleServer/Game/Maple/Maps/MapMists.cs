using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Maps
{
    public class MapMists : MapObjects<Mist>
    {
        public MapMists(Map map) : base(map) { }

        public override void Add(Mist item)
        {
            base.Add(item);
            Map.Send(item.GetCreatePacket());
        }

        public override void Remove(Mist item)
        {
            Map.Send(item.GetDestroyPacket());
            base.Remove(item);
        }
    }
}
