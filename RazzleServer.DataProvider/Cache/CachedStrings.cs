using System.Collections.Generic;
using ProtoBuf;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class CachedStrings
    {
        public Dictionary<int, string> Items { get; private set; } = new Dictionary<int, string>();

        public Dictionary<int, string> Maps { get; private set; } = new Dictionary<int, string>();
        public Dictionary<int, string> Mobs { get; private set; } = new Dictionary<int, string>();
        public Dictionary<int, string> Npcs { get; private set; } = new Dictionary<int, string>();
        public Dictionary<int, string> Skills { get; private set; } = new Dictionary<int, string>();
    }
}
