using RazzleServer.Common.WzLib;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Seat : MapObject
    {
        public short ID { get; private set; }

        public Seat(WzImageProperty img)
        {
            ID = short.Parse(img.Name);
            Position = img.GetPoint();
        }
    }
}
