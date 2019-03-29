using RazzleServer.Common.Util;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class Foothold
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<Foothold>();
        public short Id { get; }
        public Line Line { get; set; }
        public short DragForce { get; }
        public bool ForbidDownwardJump { get; }
        public bool IsWall => Line.Start.X == Line.End.X;

        public Foothold() { }

        public Foothold(WzImageProperty img)
        {
            if (!short.TryParse(img.Name, out var id))
            {
                Log.Warning($"Cannot parse foothold: {id}");
                return;
            }

            Id = id;
            Line = new Line(new Point(img["x1"].GetShort(), img["y1"].GetShort()),
                new Point(img["x2"].GetShort(), img["y2"].GetShort()));
            DragForce = img["force"]?.GetShort() ?? 0;
            ForbidDownwardJump = (img["forbidFallDown"]?.GetInt() ?? 0) > 0;
            // prev
            // next
        }
    }
}
