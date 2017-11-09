using System.Collections.Generic;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapFootholds : List<Foothold>
    {
        public Map Map { get; private set; }

        public MapFootholds(Map map)
        {
            this.Map = map;
        }

        // TODO: Beautify.
        public Point FindFloor(Point position)
        {
            short x = position.X;
            short y = position.Y;
            short maxy = short.MaxValue;

            foreach (Foothold foothold in this)
            {
                short x1 = foothold.Line.Start.X;
                short y1 = foothold.Line.Start.Y;
                short x2 = foothold.Line.End.X;
                short y2 = foothold.Line.End.Y;

                if ((x >= x1 && x <= x2) || (x <= x1 && x >= x2))
                {
                    short fhy = (short)((float)(y2 - y1) / (x2 - x1) * (x - x1) + y1);

                    if ((y - 100) < fhy)
                    {
                        if (fhy < maxy)
                        {
                            maxy = (short)fhy;
                        }
                    }
                }
            }

            return new Point(x, maxy);
        }
    }
}
