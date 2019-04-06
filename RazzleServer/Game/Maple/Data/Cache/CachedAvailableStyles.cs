using System.Collections.Generic;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedAvailableStyles
    {
        public List<byte> Skins { get; } = new List<byte>();
        public List<int> MaleHairs { get; } = new List<int>();
        public List<int> FemaleHairs { get; } = new List<int>();
        public List<int> MaleFaces { get; } = new List<int>();
        public List<int> FemaleFaces { get; } = new List<int>();
    }
}
