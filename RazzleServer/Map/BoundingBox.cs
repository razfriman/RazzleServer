using RazzleServer.Map;
using System.Drawing;

namespace RazzleServer
{
    public struct BoundingBox
    {
        public Point LeftTop;
        public Point RightBottom;

        public BoundingBox(Point lt, Point rb)
        {
            LeftTop = lt;
            RightBottom = rb;
        }

        public BoundingBox(int ltX, int ltY, int rbX, int rbY)
        {
            LeftTop = new Point(ltX, ltY);
            RightBottom = new Point(rbX, rbY);
        }

        public BoundingBox(Point origin, Point ltDelta, Point rbDelta)
        {
            LeftTop = new Point(origin.X + ltDelta.X, origin.Y + ltDelta.Y);
            RightBottom = new Point(origin.X + rbDelta.X, origin.Y + rbDelta.Y);
        }

        public BoundingBox(Point topleft, Size size)
        {
            LeftTop = new Point(topleft.X, topleft.Y);
            RightBottom = new Point(topleft.X + size.Width, topleft.Y + size.Height);
        }

        public bool Contains(Point position)
        {
            if (position.X >= LeftTop.X && position.X <= RightBottom.X &&
                    position.Y >= LeftTop.Y && position.Y <= RightBottom.Y)
                return true;
            else
                return false;
        }
    }
}