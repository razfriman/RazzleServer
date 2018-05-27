using System.Collections.Generic;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class CachedCreationData
    {
        public List<string> ForbiddenNames { get; } = new List<string>();
        public List<byte> MaleSkins { get; } = new List<byte>();
        public List<byte> FemaleSkins { get; } = new List<byte>();
        public List<int> MaleFaces { get; } = new List<int>();
        public List<int> FemaleFaces { get; } = new List<int>();
        public List<int> MaleHairs { get; } = new List<int>();
        public List<int> FemaleHairs { get; } = new List<int>();
        public List<byte> MaleHairColors { get; } = new List<byte>();
        public List<byte> FemaleHairColors { get; } = new List<byte>();
        public List<int> MaleTops { get; } = new List<int>();
        public List<int> FemaleTops { get; } = new List<int>();
        public List<int> MaleBottoms { get; } = new List<int>();
        public List<int> FemaleBottoms { get; } = new List<int>();
        public List<int> MaleShoes { get; } = new List<int>();
        public List<int> FemaleShoes { get; } = new List<int>();
        public List<int> MaleWeapons { get; } = new List<int>();
        public List<int> FemaleWeapons { get; } = new List<int>();
    }
}