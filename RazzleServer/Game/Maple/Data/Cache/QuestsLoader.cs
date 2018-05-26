using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class QuestsLoader : ACachedDataLoader<CachedQuests>
    {
        public override string CacheName => "Quests";

        private readonly ILogger Log = LogManager.Log;

        public override void LoadFromWz()
        {
           
        }
    }
}