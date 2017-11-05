using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.WzLib;
using RazzleServer.Server;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class AvailableStyles
    {
        public List<byte> Skins { get; private set; }
        public List<int> MaleHairs { get; private set; }
        public List<int> FemaleHairs { get; private set; }
        public List<int> MaleFaces { get; private set; }
        public List<int> FemaleFaces { get; private set; }

        private readonly ILogger Log = LogManager.Log;

        public AvailableStyles()
        {
            Log.LogInformation("Loading Character Creation Data");

            Skins = new List<byte>();
            MaleHairs = new List<int>();
            FemaleHairs = new List<int>();
            MaleFaces = new List<int>();
            FemaleFaces = new List<int>();

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Character.wz"), WzMapleVersion.CLASSIC))
            {
                file.ParseWzFile();

                LoadSkins();
                LoadHairs(file.WzDirectory.GetDirectoryByName("Hair"));
                LoadFaces(file.WzDirectory.GetDirectoryByName("Face"));
            }
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

                if ((id / 1000) % 10 == 0)
                {
                    MaleHairs.Add(id);
                }
                else
                {
                    FemaleHairs.Add(id);
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

                if ((id / 1000) % 10 == 0)
                {
                    MaleHairs.Add(id);
                }
                else
                {
                    FemaleHairs.Add(id);
                }
            }
        }

        private void LoadSkins()
        {
            Skins.Add(0);
            Skins.Add(1);
            Skins.Add(2);
            Skins.Add(3);
            Skins.Add(4);
            Skins.Add(5);
            Skins.Add(9);
            Skins.Add(10);
            Skins.Add(11);
        }
    }
}
