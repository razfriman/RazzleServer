using System.Collections.Generic;
using ProtoBuf;

namespace RazzleServer.DataProvider.Cache
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class CachedRechargeTiers
    {
        public readonly Dictionary<byte, Dictionary<int, double>>
            Data = new Dictionary<byte, Dictionary<int, double>>();
    }
}
