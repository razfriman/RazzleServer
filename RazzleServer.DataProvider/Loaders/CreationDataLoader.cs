﻿using RazzleServer.DataProvider.Cache;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class CreationDataLoader : ACachedDataLoader<CachedCreationData>
    {
        public override string CacheName => "CreationData";

        public override ILogger Logger => Log.ForContext<CreationDataLoader>();

        public override void LoadFromWz(WzFile file)
        {
            Logger.Information("Loading Character Creation Data");

            var dir = file.WzDirectory.GetDirectoryByName("Etc");
            var makeCharInfo = dir.GetImageByName("MakeCharInfo.img")["Info"];
            var forbiddenNames = dir.GetImageByName("ForbiddenName.img");

            LoadCreationData(makeCharInfo, true);
            LoadCreationData(makeCharInfo, false);
            LoadForbiddenNames(forbiddenNames);
        }

        private void LoadForbiddenNames(WzImage forbiddenNames)
        {
            foreach (var p in forbiddenNames.WzPropertiesList)
            {
                var name = p.GetString();
                Data.ForbiddenNames.Add(name);
            }
        }

        private void LoadCreationData(WzImageProperty img, bool isMale)
        {
            var gender = isMale ? "CharMale" : "CharFemale";

            foreach (var p in img[gender]["0"].WzPropertiesList)
            {
                var collection = isMale ? Data.MaleFaces : Data.FemaleFaces;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["1"].WzPropertiesList)
            {
                var collection = isMale ? Data.MaleHairs : Data.FemaleHairs;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["2"].WzPropertiesList)
            {
                var collection = isMale ? Data.MaleHairColors : Data.FemaleHairColors;
                collection.Add((byte)p.GetInt());
            }

            foreach (var p in img[gender]["3"].WzPropertiesList)
            {
                var collection = isMale ? Data.MaleSkins : Data.FemaleSkins;
                collection.Add((byte)p.GetInt());
            }

            foreach (var p in img[gender]["4"].WzPropertiesList)
            {
                var collection = isMale ? Data.MaleTops : Data.FemaleTops;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["5"].WzPropertiesList)
            {
                var collection = isMale ? Data.MaleBottoms : Data.FemaleBottoms;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["6"].WzPropertiesList)
            {
                var collection = isMale ? Data.MaleShoes : Data.FemaleShoes;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["7"].WzPropertiesList)
            {
                var collection = isMale ? Data.MaleWeapons : Data.FemaleWeapons;
                collection.Add(p.GetInt());
            }
        }
    }
}
