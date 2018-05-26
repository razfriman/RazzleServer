using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedReactors
    {
        public Dictionary<int, Reactor> Data { get; set; } = new Dictionary<int, Reactor>();
    }
}
