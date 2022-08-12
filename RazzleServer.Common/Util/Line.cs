using System.Text.Json.Serialization;

namespace RazzleServer.Common.Util
{
    public readonly struct Line
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
