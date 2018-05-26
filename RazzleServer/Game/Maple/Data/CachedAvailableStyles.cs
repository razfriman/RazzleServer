using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data
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
