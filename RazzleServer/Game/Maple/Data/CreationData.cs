using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Data;
using RazzleServer.Common.Util;
using RazzleServer.Common.WzLib;
using RazzleServer.Server;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CreationData
    {
        private readonly ILogger Log = LogManager.Log;

        public List<string> ForbiddenNames { get; private set; } = new List<string>();
        public List<byte> MaleSkins { get; private set; } = new List<byte>();
        public List<byte> FemaleSkins { get; private set; } = new List<byte>();
        public List<int> MaleFaces { get; private set; } = new List<int>();
        public List<int> FemaleFaces { get; private set; } = new List<int>();
        public List<int> MaleHairs { get; private set; } = new List<int>();
        public List<int> FemaleHairs { get; private set; } = new List<int>();
        public List<byte> MaleHairColors { get; private set; } = new List<byte>();
        public List<byte> FemaleHairColors { get; private set; } = new List<byte>();
        public List<int> MaleTops { get; private set; } = new List<int>();
        public List<int> FemaleTops { get; private set; } = new List<int>();
        public List<int> MaleBottoms { get; private set; } = new List<int>();
        public List<int> FemaleBottoms { get; private set; } = new List<int>();
        public List<int> MaleShoes { get; private set; } = new List<int>();
        public List<int> FemaleShoes { get; private set; } = new List<int>();
        public List<int> MaleWeapons { get; private set; } = new List<int>();
        public List<int> FemaleWeapons { get; private set; } = new List<int>();

        public void Load()
        {
            Log.LogInformation("Loading Character Creation Data");

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Etc.wz"), WzMapleVersion.CLASSIC))
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
                ForbiddenNames.Add(name);
            }
        }

        private void LoadCreationData(WzImageProperty img, bool isMale)
        {
            var gender = isMale ? "CharMale" : "CharFemale";

            foreach (var p in img[gender]["0"].WzProperties)
            {
                var collection = isMale ? MaleFaces : FemaleFaces;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["1"].WzProperties)
            {
                var collection = isMale ? MaleHairs : FemaleHairs;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["2"].WzProperties)
            {
                var collection = isMale ? MaleHairColors : FemaleHairColors;
                collection.Add((byte)p.GetInt());
            }

            foreach (var p in img[gender]["3"].WzProperties)
            {
                var collection = isMale ? MaleSkins : FemaleSkins;
                collection.Add((byte)p.GetInt());
            }

            foreach (var p in img[gender]["4"].WzProperties)
            {
                var collection = isMale ? MaleTops : FemaleTops;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["5"].WzProperties)
            {
                var collection = isMale ? MaleBottoms : FemaleBottoms;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["6"].WzProperties)
            {
                var collection = isMale ? MaleShoes : FemaleShoes;
                collection.Add(p.GetInt());
            }

            foreach (var p in img[gender]["7"].WzProperties)
            {
                var collection = isMale ? MaleWeapons : FemaleWeapons;
                collection.Add(p.GetInt());
            }
        }
    }
}