using RazzleServer.Common.Constants;

namespace RazzleServer.Game.Maple.Data.References
{
    public class ReactorStateReference
    {
        public sbyte State { get; set; }
        public ReactorEventType Type { get; set; }
        public int ItemId { get; set; }
        public int ItemCount { get; set; }
        public sbyte NextState { get; set; }
    }
}
