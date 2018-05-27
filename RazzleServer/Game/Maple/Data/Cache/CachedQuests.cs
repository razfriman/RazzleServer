using System.Collections.Generic;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedQuests
    {
        public Dictionary<int, QuestReference> Data { get; set; } = new Dictionary<int, QuestReference>();
    }
}
