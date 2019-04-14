﻿using System.Linq;
using RazzleServer.DataProvider.Cache;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class StringLoader : ACachedDataLoader<CachedStrings>
    {
        public override string CacheName => "Strings";

        public override ILogger Logger => Log.ForContext<StringLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Strings");

            using var file = GetWzFile("Data.wz");
            file.ParseWzFile();
            var dir = file.WzDirectory.GetDirectoryByName("String");
            ProcessItems(dir.GetImageByName("Item.img"));
            ProcessMaps(dir.GetImageByName("Map.img"));
            ProcessMobs(dir.GetImageByName("Mob.img"));
            ProcessNpcs(dir.GetImageByName("Npc.img"));
            ProcessSkills(dir.GetImageByName("Skill.img"));
        }

        private void ProcessSkills(WzImage wzImage)
        {
            wzImage
                .WzProperties
                .ForEach(x =>
                {
                    if (int.TryParse(x.Name, out var id))
                    {
                        var name = x["name"]?.GetString();
                        if (name != null)
                        {
                            Data.Skills[id] = name;
                        }
                    }
                });
        }

        private void ProcessNpcs(WzImage wzImage)
        {
            wzImage
                .WzProperties
                .ForEach(x =>
                {
                    if (int.TryParse(x.Name, out var id))
                    {
                        var name = x["name"]?.GetString();
                        if (name != null)
                        {
                            Data.Npcs[id] = name;
                        }
                    }
                });
        }

        private void ProcessMobs(WzImage wzImage)
        {
            wzImage
                .WzProperties
                .ForEach(x =>
                {
                    if (int.TryParse(x.Name, out var id))
                    {
                        var name = x["name"]?.GetString() ?? null;
                        if (name != null)
                        {
                            Data.Mobs[id] = name;
                        }
                    }
                });
        }

        private void ProcessMaps(WzImage wzImage)
        {
            wzImage
                .WzProperties
                .SelectMany(x => x.WzProperties)
                .ToList()
                .ForEach(x =>
                {
                    if (int.TryParse(x.Name, out var id))
                    {
                        var streetName = x["streetName"]?.GetString() ?? null;
                        var mapName = x["mapName"]?.GetString() ?? null;
                        if (mapName != null)
                        {
                            Data.Maps[id] = $"{streetName} - {mapName}";
                        }
                    }
                });
        }

        private void ProcessItems(WzImage wzImage)
        {
            wzImage["Eqp"].WzProperties.ForEach(ProcessItemSection);
            ProcessItemSection(wzImage["Con"]);
            ProcessItemSection(wzImage["Ins"]);
            ProcessItemSection(wzImage["Etc"]);
            ProcessItemSection(wzImage["Pet"]);
        }

        private void ProcessItemSection(WzImageProperty itemProperty)
        {
            itemProperty
                .WzProperties
                .ForEach(x =>
                {
                    if (int.TryParse(x.Name, out var id))
                    {
                        var name = x["name"]?.GetString();
                        if (name != null)
                        {
                            Data.Items[id] = name;
                        }
                    }
                });
        }
    }
}
