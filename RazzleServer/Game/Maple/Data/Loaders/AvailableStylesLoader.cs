using Serilog;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class AvailableStylesDataLoader : ACachedDataLoader<CachedAvailableStyles>
    {
        public override string CacheName => "AvailableStyles";

        public override ILogger Logger => Log.ForContext<AvailableStylesDataLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Character Creation Data");

            using var file = GetWzFile("Data.wz");
            file.ParseWzFile();
            var dir = file.WzDirectory.GetDirectoryByName("Character");
            LoadSkins();
            LoadHairs(dir.GetDirectoryByName("Hair"));
            LoadFaces(dir.GetDirectoryByName("Face"));
        }

        private void LoadHairs(WzDirectory wzDirectory)
        {
            foreach (var i in wzDirectory.WzImages)
            {
                var name = i.Name.Remove(8);

                if (!int.TryParse(name, out var id))
                {
                    continue;
                }

                if (id / 1000 % 10 == 0)
                {
                    Data.MaleHairs.Add(id);
                }
                else
                {
                    Data.FemaleHairs.Add(id);
                }
            }
        }

        private void LoadFaces(WzDirectory wzDirectory)
        {
            foreach (var i in wzDirectory.WzImages)
            {
                var name = i.Name.Remove(8);
                if (!int.TryParse(name, out var id))
                {
                    continue;
                }

                if (id / 1000 % 10 == 0)
                {
                    Data.MaleFaces.Add(id);
                }
                else
                {
                    Data.FemaleFaces.Add(id);
                }
            }
        }

        private void LoadSkins()
        {
            Data.Skins.Add(0);
            Data.Skins.Add(1);
            Data.Skins.Add(2);
            Data.Skins.Add(3);
            Data.Skins.Add(4);
            Data.Skins.Add(5);
            Data.Skins.Add(9);
            Data.Skins.Add(10);
            Data.Skins.Add(11);
        }
    }
}
