using RazzleServer.Common.Util;
using RazzleServer.Common.Constants;

namespace RazzleServer.Game.Maple.Util
{
    public sealed class Movement
    {
        public MovementType Type { get; set; }
        public Point Position { get; set; }
        public Point Velocity { get; set; }
        public short FallStart { get; set; }
        public short Foothold { get; set; }
        public short Duration { get; set; }
        public byte Stance { get; set; }
        public byte Statistic { get; set; }
    }
}
