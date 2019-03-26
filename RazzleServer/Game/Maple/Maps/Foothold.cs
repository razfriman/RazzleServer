using RazzleServer.Common.Util;
using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Foothold
    {
        public short Id { get; set; }
        public Line Line { get; set; }
        public short DragForce { get; set; }
        public bool ForbidDownwardJump { get; set; }
        public bool IsWall => Line.Start.X == Line.End.X;

        public Foothold() { }

        public Foothold(WzImageProperty img)
        {
            if (short.TryParse(img.Name, out var id))
            {
                Id = id;
                Line = new Line(new Point(img["x1"].GetShort(), img["y1"].GetShort()),
                    new Point(img["x2"].GetShort(), img["y2"].GetShort()));
                // prev
                // next
                DragForce = img["force"]?.GetShort() ?? 0;
                ForbidDownwardJump = (img["forbidFallDown"]?.GetInt() ?? 0) > 0;
            }
        }
    }
}
