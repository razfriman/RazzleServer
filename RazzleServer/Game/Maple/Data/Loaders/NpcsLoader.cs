using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class NpcsLoader : ACachedDataLoader<CachedNpcs>
    {
        public override string CacheName => "Npcs";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Npcs");

            using (var file = GetWzFile("Npc.wz"))
            {
                file.ParseWzFile();
                file.WzDirectory.WzImages.ForEach(x =>
                {
                    var npc = new NpcReference(x);
                    Data.Data.Add(npc.MapleId, npc);
                });
            }
        }
    }
}