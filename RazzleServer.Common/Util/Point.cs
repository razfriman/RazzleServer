using System;

namespace RazzleServer.Common.Util
{
    public struct Point
    {
        public short X { get; }
        public short Y { get; }

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

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var p = (Point)obj;
            return X == p.X && Y == p.Y;
        }

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public static Point operator +(Point p1, Point p2) => new Point(p1.X + p2.X, p1.Y + p2.Y);

        public static Point operator -(Point p1, Point p2) => new Point(p1.X - p2.X, p1.Y - p2.Y);

        public static bool operator ==(Point p1, Point p2) => p1.Equals(p2);

        public static bool operator !=(Point p1, Point p2) => !p1.Equals(p2);

    }
}
