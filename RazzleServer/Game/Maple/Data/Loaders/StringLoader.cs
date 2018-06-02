using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Data.Cache;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class StringLoader : ACachedDataLoader<CachedStrings>
    {
        public override string CacheName => "Strings";

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Strings");

            using (var file = GetWzFile("String.wz"))
            {
                file.ParseWzFile();

                file.Export("/Users/razfriman/Desktop/a.json");


                ProcessItems(file.WzDirectory.GetImageByName("Ins.img"));
                ProcessItems(file.WzDirectory.GetImageByName("Etc.img"));
                ProcessItems(file.WzDirectory.GetImageByName("Cash.img"));
                ProcessEquips(file.WzDirectory.GetImageByName("Eqp.img"));
                ProcessItems(file.WzDirectory.GetImageByName("Consume.img"));
                ProcessMaps(file.WzDirectory.GetImageByName("Map.img"));
                ProcessMobs(file.WzDirectory.GetImageByName("Mob.img"));
                ProcessNpcs(file.WzDirectory.GetImageByName("Npc.img"));
                ProcessPets(file.WzDirectory.GetImageByName("Pet.img"));
                ProcessSkills(file.WzDirectory.GetImageByName("Skill.img"));
            }
        }

        private void ProcessSkills(WzImage wzImage)
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
                           Data.Skills[id] = name;
                       }
                   }
               });
        }

        private void ProcessPets(WzImage wzImage)
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
                           Data.Pets[id] = name;
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
                       var name = x["name"]?.GetString() ?? null;
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
            wzImage
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

        private void ProcessEquips(WzImage wzImage)
        {
            wzImage
                .WzProperties
                .SelectMany(x => x.WzProperties)
                .SelectMany(x => x.WzProperties)
                .ToList()
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