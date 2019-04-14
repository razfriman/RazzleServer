using System.Collections.Generic;

namespace RazzleServer.DataProvider.Cache
{
    public class CachedRechargeTiers
    {
        public readonly Dictionary<byte, Dictionary<int, double>>
            Data = new Dictionary<byte, Dictionary<int, double>>();
    }
}
