namespace RazzleServer.Game.Maple
{
    public class Rectangle
    {
        public Point LT { get; set; }
        public Point RB { get; set; }

        public Rectangle(Point lt, Point rb)
        {
            LT = lt;
            RB = rb;
        }
    }
}
