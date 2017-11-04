using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class AvailableStyles
    {
        public List<byte> Skins { get; private set; }
        public List<int> MaleHairs { get; private set; }
        public List<int> FemaleHairs { get; private set; }
        public List<int> MaleFaces { get; private set; }
        public List<int> FemaleFaces { get; private set; }

        private readonly ILogger Log = LogManager.Log;

        public AvailableStyles()
        {
            Skins = new List<byte>();
            MaleHairs = new List<int>();
            FemaleHairs = new List<int>();
            MaleFaces = new List<int>();
            FemaleFaces = new List<int>();
        }
    }
}
