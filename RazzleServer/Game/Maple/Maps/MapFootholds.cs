using System.Collections.Generic;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapFootholds
    {
        public List<Foothold> Footholds { get; private set; } = new List<Foothold>();

        public Point FindFloor(Point position)
        {
            var x = position.X;
            var y = position.Y;
            var maxy = short.MaxValue;

            foreach (var foothold in Footholds)
            {
                var x1 = foothold.Line.Start.X;
                var y1 = foothold.Line.Start.Y;
                var x2 = foothold.Line.End.X;
                var y2 = foothold.Line.End.Y;

                if (x >= x1 && x <= x2 || x <= x1 && x >= x2)
                {
                    var fhy = (short)((float)(y2 - y1) / (x2 - x1) * (x - x1) + y1);

                    if (y - 100 < fhy)
                    {
                        if (fhy < maxy)
                        {
                            maxy = fhy;
                        }
                    }
                }
            }

            return new Point(x, maxy);
        }
    }
}
