using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapFootholds
    {
        public List<Foothold> Footholds { get; } = new List<Foothold>();

        public Rectangle Bounds { get; set; }

        public void CalculateBounds()
        {
            var allPoints = Footholds
                .Select(x => x.Line.Start)
                .Union(Footholds.Select(x => x.Line.End))
                .ToList();

            var xMin = allPoints.Min(p => p.X);
            var xMax = allPoints.Max(p => p.X);
            var yMin = allPoints.Min(p => p.Y);
            var yMax = allPoints.Max(p => p.Y);

            Bounds = new Rectangle(new Point(xMin, yMax), new Point(xMax, yMin));
        }

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

        public bool HasWallBetween(Point p1, Point p2)
        {
            if (p1.Y != p2.Y)
            {
                return false;
            }

            var exactMatch = Footholds
                .Where(x => x.IsWall)
                .Where(foothold => foothold.Line.Start.X >= p1.X)
                .Where(foothold => foothold.Line.Start.X <= p2.X)
                .Where(foothold => foothold.Line.Start.Y >= p1.Y)
                .Any(foothold => foothold.Line.End.Y <= p1.Y);

            if (exactMatch)
            {
                return true;
            }

            return p1.X - Bounds.Lt.X > 0 && p2.X - Bounds.Lt.X < 0 ||
                     p1.X - Bounds.Lt.X < 0 && p2.X - Bounds.Lt.X > 0 ||
                     p1.X - Bounds.Rb.X > 0 && p2.X - Bounds.Rb.X < 0 ||
                     p1.X - Bounds.Rb.X < 0 && p2.X - Bounds.Rb.X > 0;
        }
    }
}
