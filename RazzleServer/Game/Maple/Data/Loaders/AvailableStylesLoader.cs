using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class AvailableStylesDataLoader : ACachedDataLoader<CachedAvailableStyles>
    {
        public override string CacheName => "AvailableStyles";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Character Creation Data");

            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Character.wz"), WzMapleVersion.Classic))
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
                    Data.MaleHairs.Add(id);
                }
                else
                {
                    Data.FemaleHairs.Add(id);
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