namespace RazzleServer.Game.Maple
{
    public sealed class Line
    {
        public Point Start { get; private set; }
        public Point End { get; private set; }

        public Line(Point start, Point end)
        {
            Start = start;
            End = end;
        }
    }
}
