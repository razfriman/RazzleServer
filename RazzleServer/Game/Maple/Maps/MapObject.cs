using Newtonsoft.Json;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class MapObject
    {
        [JsonIgnore]
        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }
    }
}
