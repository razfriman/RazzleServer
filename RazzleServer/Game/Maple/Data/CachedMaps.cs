using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedMaps
    {
        public Dictionary<int, Map> Data { get; set; } = new Dictionary<int, Map>();
    }
}
