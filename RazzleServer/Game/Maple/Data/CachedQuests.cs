using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedQuests : KeyedCollection<ushort, Quest>
    {
        private readonly ILogger Log = LogManager.Log;

        public CachedQuests()
            : base()
        {
            Log.LogInformation("Loading Quests");
        }

        protected override ushort GetKeyForItem(Quest item)
        {
            return item.MapleID;
        }
    }
}
