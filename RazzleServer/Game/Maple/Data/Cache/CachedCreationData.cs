using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedCreationData
    {
        public List<string> ForbiddenNames { get; private set; } = new List<string>();
        public List<byte> MaleSkins { get; private set; } = new List<byte>();
        public List<byte> FemaleSkins { get; private set; } = new List<byte>();
        public List<int> MaleFaces { get; private set; } = new List<int>();
        public List<int> FemaleFaces { get; private set; } = new List<int>();
        public List<int> MaleHairs { get; private set; } = new List<int>();
        public List<int> FemaleHairs { get; private set; } = new List<int>();
        public List<byte> MaleHairColors { get; private set; } = new List<byte>();
        public List<byte> FemaleHairColors { get; private set; } = new List<byte>();
        public List<int> MaleTops { get; private set; } = new List<int>();
        public List<int> FemaleTops { get; private set; } = new List<int>();
        public List<int> MaleBottoms { get; private set; } = new List<int>();
        public List<int> FemaleBottoms { get; private set; } = new List<int>();
        public List<int> MaleShoes { get; private set; } = new List<int>();
        public List<int> FemaleShoes { get; private set; } = new List<int>();
        public List<int> MaleWeapons { get; private set; } = new List<int>();
        public List<int> FemaleWeapons { get; private set; } = new List<int>();
    }
}