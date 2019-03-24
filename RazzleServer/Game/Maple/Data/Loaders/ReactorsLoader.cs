using Serilog;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class ReactorsLoader : ACachedDataLoader<CachedReactors>
    {
        public override string CacheName => "Reactors";

        public override ILogger Logger => Log.ForContext<ReactorsLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Reactors");

            using (var file = GetWzFile("Data.wz"))
            {
                file.ParseWzFile();
                var dir = file.WzDirectory.GetDirectoryByName("Reactor");

                dir.WzImages.ForEach(img =>
                {
                    var name = img.Name.Remove(7);
                    if (!int.TryParse(name, out var id))
                    {
                        return;
                    }

                    Load(file, name, id);
                });
            }
        }

        private ReactorReference Load(WzFile file, string name, int id)
        {
            if (Data.Data.ContainsKey(id))
            {
                return Data.Data[id];
            }

            var img = file.WzDirectory.GetImageByName($"{name}.img");
            ReactorReference linkedStats = null;
            var link = img["info"]?["link"]?.GetString();
            if (link != null)
            {
                if (int.TryParse(link, out var linkId))
                {
                    linkedStats = Load(file, link, linkId);
                }
            }

            var reactor = new ReactorReference(file.WzDirectory.GetImageByName($"{name}.img"), linkedStats);

            if (!Data.Data.ContainsKey(reactor.MapleId))
            {
                Data.Data.Add(reactor.MapleId, reactor);
            }

            return reactor;
        }
    }
}
