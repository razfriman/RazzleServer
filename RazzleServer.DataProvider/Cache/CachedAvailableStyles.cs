using System.Collections.Generic;
using ProtoBuf;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public sealed class CachedAvailableStyles
    {
        public List<byte> Skins { get; } = new List<byte>();
        public List<int> MaleHairs { get; } = new List<int>();
        public List<int> FemaleHairs { get; } = new List<int>();
        public List<int> MaleFaces { get; } = new List<int>();
        public List<int> FemaleFaces { get; } = new List<int>();
    }
}
