using System.Collections.ObjectModel;
using RazzleServer.Game.Maple.Life;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedMobs : KeyedCollection<int, Mob>
    {
        private readonly ILogger Log = LogManager.Log;

        public CachedMobs()
        {
            Log.LogInformation("Loading Mobs");
        }

        protected override int GetKeyForItem(Mob item) => item.MapleID;
    }
}
