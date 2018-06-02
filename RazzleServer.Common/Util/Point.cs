using System;

namespace RazzleServer.Common.Util
{
    public class Point
    {
        public short X { get; set; }
        public short Y { get; set; }

        public Point() { }

        public Point(short x, short y)
        {
            X = x;
            Y = y;
        }

        public Point(int x, int y)
        {
            X = (short)x;
            Y = (short)y;
        }

        public double DistanceFrom(Point point) => Math.Sqrt(Math.Pow(X - point.X, 2) + Math.Pow(Y - point.Y, 2));

        public bool IsInRectangle(Rectangle rectangle) =>
                X >= rectangle.Lt.X &&
                Y >= rectangle.Lt.Y &&
                X <= rectangle.Rb.X &&
                Y <= rectangle.Rb.Y;

        public static Point operator +(Point p1, Point p2) => new Point(p1.X + p2.X, p1.Y + p2.Y);

        public static Point operator -(Point p1, Point p2) => new Point(p1.X - p2.X, p1.Y - p2.Y);
    }
}
