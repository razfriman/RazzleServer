using Newtonsoft.Json;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class MapObject
    {
        [JsonIgnore] public Map Map { get; set; }

        [JsonIgnore] public int ObjectId { get; set; }

        public Point Position { get; set; }
    }
}
