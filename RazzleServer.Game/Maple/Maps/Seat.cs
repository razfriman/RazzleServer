using RazzleServer.Common.Util;
using RazzleServer.DataProvider.References;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Seat : IMapObject
    {
        public short Id { get; }
        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }

        public Seat() { }

        public Seat(SeatReference reference)
        {
            Id = reference.Id;
            Position = reference.Position;
        }
    }
}
