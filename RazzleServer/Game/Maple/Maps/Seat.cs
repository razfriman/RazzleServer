using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Seat : MapObject
    {
        public short Id { get; private set; }

        public Seat(WzImageProperty img)
        {
            Id = short.Parse(img.Name);
            Position = img.GetPoint();
        }
    }
}
