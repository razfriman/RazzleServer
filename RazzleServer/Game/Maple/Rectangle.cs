namespace RazzleServer.Game.Maple
{
    public class Rectangle
    {
        public Point Lt { get; set; }
        public Point Rb { get; set; }

        public Rectangle(Point lt, Point rb)
        {
            Lt = lt;
            Rb = rb;
        }
    }
}
