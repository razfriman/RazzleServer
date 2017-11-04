using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Data;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedItems : KeyedCollection<int, Item>
    {
        public List<int> WizetItemIDs { get; private set; }

        private readonly ILogger Log = LogManager.Log;

        public CachedItems()
        {
            Log.LogInformation("Loading Items");
            WizetItemIDs = new List<int>(4)
            {
                1002140,
                1322013,
                1042003,
                1062007
            };
        }

        protected override int GetKeyForItem(Item item) => item.MapleID;
    }
}