using System.Text.Json.Serialization;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Maps
{
    public interface IMapObject
    {
        [JsonIgnore] Map Map { get; set; }

        [JsonIgnore] int ObjectId { get; set; }

        Point Position { get; set; }
    }
}
