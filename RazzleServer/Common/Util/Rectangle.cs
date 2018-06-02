namespace RazzleServer.Common.Util
{
    public class Rectangle
    {
        public Point Lt { get; set; }
        public Point Rb { get; set; }

        public Rectangle() { }

        public Rectangle(Point lt, Point rb)
        {
            Lt = lt;
            Rb = rb;
        }

        public Rectangle(int ltX, int ltY, int rbX, int rbY)
        {
            Lt = new Point(ltX, ltY);
            Rb = new Point(rbX, rbY);
        }
    }
}
