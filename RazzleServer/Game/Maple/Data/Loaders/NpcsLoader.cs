using Serilog;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class NpcsLoader : ACachedDataLoader<CachedNpcs>
    {
        public override string CacheName => "Npcs";

        public override ILogger Logger => Log.ForContext<NpcsLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Npcs");

            using (var file = GetWzFile("Data.wz"))
            {
                file.ParseWzFile();
                var dir = file.WzDirectory.GetDirectoryByName("Npc");

                dir.WzImages.ForEach(x =>
                {
                    var npc = new NpcReference(x);
                    Data.Data.Add(npc.MapleId, npc);
                });
            }
        }
    }
}
