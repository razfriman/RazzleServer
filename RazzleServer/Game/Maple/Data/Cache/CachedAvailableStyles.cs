using System.Collections.Generic;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedAvailableStyles
    {
        public List<byte> Skins { get; private set; } = new List<byte>();
        public List<int> MaleHairs { get; private set; } = new List<int>();
        public List<int> FemaleHairs { get; private set; } = new List<int>();
        public List<int> MaleFaces { get; private set; } = new List<int>();
        public List<int> FemaleFaces { get; private set; } = new List<int>();
    }
}
