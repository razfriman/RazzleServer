using Newtonsoft.Json;

namespace RazzleServer.Common.Util
{
    public struct Rectangle
    {
        public Point Lt { get; set; }
        public Point Rb { get; set; }

        [JsonConstructor]
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
