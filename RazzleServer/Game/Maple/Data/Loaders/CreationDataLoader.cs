using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Data.Cache;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class CreationDataLoader : ACachedDataLoader<CachedCreationData>
    {
        public override string CacheName => "CreationData";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Character Creation Data");

            using (var file = GetWzFile("Etc.wz"))
            {
                file.ParseWzFile();
                var makeCharInfo = file.WzDirectory.GetImageByName("MakeCharInfo.img")["Info"];
                var forbiddenNames = file.WzDirectory.GetImageByName("ForbiddenName.img");

                LoadCreationData(makeCharInfo, true);
                LoadCreationData(makeCharInfo, false);
                LoadForbiddenNames(forbiddenNames);
            }
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