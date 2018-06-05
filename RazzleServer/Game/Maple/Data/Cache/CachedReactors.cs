using System.Collections.Generic;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedReactors
    {
        public Dictionary<int, ReactorReference> Data { get; set; } = new Dictionary<int, ReactorReference>();
    }
}
