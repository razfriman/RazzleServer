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
    public sealed class ReactorsLoader : ACachedDataLoader<CachedReactors>
    {
        public override string CacheName => "Reactors";

        private readonly ILogger Log = LogManager.Log;

        public override void LoadFromWz()
        {
           
        }
    }
}