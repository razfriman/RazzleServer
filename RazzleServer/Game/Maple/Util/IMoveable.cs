using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Util
{
    public interface IMoveable
    {
        byte Stance { get; set; }
        short Foothold { get; set; }
        Point Position { get; set; }
    }
}
