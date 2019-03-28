using Newtonsoft.Json;

namespace RazzleServer.Common.Util
{
    public struct Line
    {
        public Point Start { get; }
        public Point End { get; }

        [JsonConstructor]
        public Line(Point start, Point end)
        {
            Start = start;
            End = end;
        }
    }
}
