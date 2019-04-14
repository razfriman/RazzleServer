using System.Collections.Generic;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    public class CachedQuests
    {
        public Dictionary<int, QuestReference> Data { get; set; } = new Dictionary<int, QuestReference>();
    }
}
