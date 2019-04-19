using RazzleServer.DataProvider.Cache;
using RazzleServer.DataProvider.References;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class NpcsLoader : ACachedDataLoader<CachedNpcs>
    {
        public override string CacheName => "Npcs";

        public override ILogger Logger => Log.ForContext<NpcsLoader>();

        public override void LoadFromWz(WzFile file)
        {
            Logger.Information("Loading Npcs");

            var dir = file.WzDirectory.GetDirectoryByName("Npc");

            dir.WzImages.ForEach(x =>
            {
                var npc = new NpcReference(x);
                Data.Data.Add(npc.MapleId, npc);
            });
        }
    }
}
