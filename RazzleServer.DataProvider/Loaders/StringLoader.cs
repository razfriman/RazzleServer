using System.Linq;
using RazzleServer.DataProvider.Cache;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.Loaders
{
    public sealed class StringLoader : ACachedDataLoader<CachedStrings>
    {
        public override string CacheName => "Strings";

        public override ILogger Logger => Log.ForContext<StringLoader>();

        public override void LoadFromWz(WzFile file)
        {
            Logger.Information("Loading Strings");

            var dir = file.WzDirectory.GetDirectoryByName("String");
            ProcessItems(dir.GetImageByName("Item.img"));
            ProcessMaps(dir.GetImageByName("Map.img"));
            ProcessMobs(dir.GetImageByName("Mob.img"));
            ProcessNpcs(dir.GetImageByName("Npc.img"));
            ProcessSkills(dir.GetImageByName("Skill.img"));
        }

        private void ProcessSkills(WzImage wzImage)
        {
            foreach (var x in wzImage.WzPropertiesList)
            {
                if (!int.TryParse(x.Name, out var id))
                {
                    continue;
                }

                var name = x["name"]?.GetString();
                if (name != null)
                {
                    Data.Skills[id] = name;
                }
            }
        }

        private void ProcessNpcs(WzImage wzImage)
        {
            foreach (var x in wzImage.WzPropertiesList)
            {
                if (!int.TryParse(x.Name, out var id))
                {
                    continue;
                }

                var name = x["name"]?.GetString();
                if (name != null)
                {
                    Data.Npcs[id] = name;
                }
            }
        }

        private void ProcessMobs(WzImage wzImage)
        {
            foreach (var x in wzImage.WzPropertiesList)
            {
                if (!int.TryParse(x.Name, out var id))
                {
                    continue;
                }

                var name = x["name"]?.GetString() ?? null;
                if (name != null)
                {
                    Data.Mobs[id] = name;
                }
            }
        }

        private void ProcessMaps(WzImage wzImage)
        {
            wzImage
                .WzPropertiesList
                .SelectMany(x => x.WzPropertiesList)
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
            wzImage["Eqp"].WzPropertiesList.ToList().ForEach(ProcessItemSection);
            ProcessItemSection(wzImage["Con"]);
            ProcessItemSection(wzImage["Ins"]);
            ProcessItemSection(wzImage["Etc"]);
            ProcessItemSection(wzImage["Pet"]);
        }

        private void ProcessItemSection(WzImageProperty itemProperty)
        {
            foreach (var x in itemProperty.WzPropertiesList)
            {
                if (!int.TryParse(x.Name, out var id))
                {
                    continue;
                }

                var name = x["name"]?.GetString();
                if (name != null)
                {
                    Data.Items[id] = name;
                }
            }
        }
    }
}
