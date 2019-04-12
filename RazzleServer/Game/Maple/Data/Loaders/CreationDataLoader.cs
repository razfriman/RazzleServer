using Serilog;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class CreationDataLoader : ACachedDataLoader<CachedCreationData>
    {
        public override string CacheName => "CreationData";

        public override ILogger Logger => Log.ForContext<CreationDataLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Character Creation Data");

            using var file = GetWzFile("Data.wz");
            file.ParseWzFile();
            var dir = file.WzDirectory.GetDirectoryByName("Etc");
            var makeCharInfo = dir.GetImageByName("MakeCharInfo.img")["Info"];
            var forbiddenNames = dir.GetImageByName("ForbiddenName.img");

            LoadCreationData(makeCharInfo, true);
            LoadCreationData(makeCharInfo, false);
            LoadForbiddenNames(forbiddenNames);
        }

        private void LoadForbiddenNames(WzImage forbiddenNames)
        {
            foreach (var p in forbiddenNames.WzProperties)
            {
                var name = p.GetString();
                Data.ForbiddenNames.Add(name);
            }
        }

        private void LoadCreationData(WzImageProperty img, bool isMale)
        {
            var gender = isMale ? "CharMale" : "CharFemale";

            foreach (var p in img[gender]["0"].WzProperties)
            {
                var collection = isMale ? Data.MaleFaces : Data.FemaleFaces;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["1"].WzProperties)
            {
                var collection = isMale ? Data.MaleHairs : Data.FemaleHairs;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["2"].WzProperties)
            {
                var collection = isMale ? Data.MaleHairColors : Data.FemaleHairColors;
                collection.Add((byte)p.GetInt());
            }

            foreach (var p in img[gender]["3"].WzProperties)
            {
                var collection = isMale ? Data.MaleSkins : Data.FemaleSkins;
                collection.Add((byte)p.GetInt());
            }

            foreach (var p in img[gender]["4"].WzProperties)
            {
                var collection = isMale ? Data.MaleTops : Data.FemaleTops;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["5"].WzProperties)
            {
                var collection = isMale ? Data.MaleBottoms : Data.FemaleBottoms;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["6"].WzProperties)
            {
                var collection = isMale ? Data.MaleShoes : Data.FemaleShoes;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["7"].WzProperties)
            {
                var collection = isMale ? Data.MaleWeapons : Data.FemaleWeapons;
                collection.Add(p.GetInt());
            }
        }
    }
}
