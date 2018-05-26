using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedQuests
    {
        public Dictionary<int, Quest> Data { get; set; } = new Dictionary<int, Quest>();
    }
}
