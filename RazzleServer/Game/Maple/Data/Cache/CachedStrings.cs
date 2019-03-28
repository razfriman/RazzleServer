using System.Collections.Generic;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedStrings
    {
        public readonly Dictionary<int, string> Items = new Dictionary<int, string>();
        public readonly Dictionary<int, string> Maps = new Dictionary<int, string>();
        public readonly Dictionary<int, string> Mobs = new Dictionary<int, string>();
        public readonly Dictionary<int, string> Npcs = new Dictionary<int, string>();
        public readonly Dictionary<int, string> Skills = new Dictionary<int, string>();
    }
}
