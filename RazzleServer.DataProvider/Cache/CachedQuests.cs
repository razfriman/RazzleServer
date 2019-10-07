using System.Collections.Generic;
using ProtoBuf;
using RazzleServer.DataProvider.References;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CachedQuests
    {
        public Dictionary<int, QuestReference> Data { get; set; } = new Dictionary<int, QuestReference>();
    }
}
