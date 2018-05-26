using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Center;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedItems
    {
        public List<int> WizetItemIds { get; private set; } = new List<int>
        {
            1002140,
            1322013,
            1042003,
            1062007
        };

        public Dictionary<int, Item> Data { get; set; } = new Dictionary<int, Item>();
    }
}