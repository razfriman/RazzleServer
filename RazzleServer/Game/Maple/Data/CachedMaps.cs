using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Data;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedMaps : KeyedCollection<int, Map>
    {
        private readonly ILogger Log = LogManager.Log;

        public CachedMaps()
        {
            Log.LogInformation("Loading Maps");
        }

        public new Map this[int key]
        {
            get
            {
                if (!Contains(key))
                {
                    // Load map by key
                    Add(new Map(key));
                }

                //this[key].SpawnPoints.Spawn();

                return base[key];
            }
        }

        protected override int GetKeyForItem(Map item) => item.MapleID;
    }
}
