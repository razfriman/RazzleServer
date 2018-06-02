using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Common;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Life;

namespace RazzleServer.Game.Maple.Data
{
    public static class LootProvider
    {
        private static readonly ILogger Log = LogManager.Log;
        private const string LootJsonFile = "loot.json";

        internal static void Initialize()
        {
            using (var context = new MapleDbContext())
            {
                if (!context.Loots.Any())
                {
                    Log.LogInformation("Cannot find any loot in the database, attempting to load from JSON");
                    LoadFromJson(true);
                }

                Log.LogInformation("Loading Loot from database");

                context
                    .Loots
                    .GroupBy(x => x.MobId)
                    .ToList()
                    .ForEach(x =>
                    {
                        var loots = DataProvider.Mobs.Data[x.Key].Loots;
                        loots.Clear();

                        x
                        .ToList()
                        .ForEach(item =>
                        {
                            loots.Add(new Loot
                            {
                                Chance = item.Chance,
                                IsMeso = item.IsMeso,
                                ItemId = item.ItemId,
                                MaximumQuantity = item.MaximumQuantity,
                                MinimumQuantity = item.MinimumQuantity,
                                MobId = item.MobId,
                                QuestId = item.QuestId
                            });
                        });
                    });

            }
        }

        private static void LoadFromJson(bool v)
        {
            if(!File.Exists(LootJsonFile))
            {
                Log.LogWarning($"Cannot find {LootJsonFile}");
                return;
            }

            using (var s = File.OpenRead(LootJsonFile))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            using (var context = new MapleDbContext())
            {
                try
                {
                    var serializer = new JsonSerializer();
                    var data = serializer.Deserialize<Dictionary<int, List<Loot>>>(reader);

                    foreach (var item in data.Values.SelectMany(x => x))
                    {
                        context.Loots.Add(new LootEntity
                        {
                            Chance = item.Chance,
                            IsMeso = item.IsMeso,
                            ItemId = item.ItemId,
                            MaximumQuantity = item.MaximumQuantity,
                            MinimumQuantity = item.MinimumQuantity,
                            MobId = item.MobId,
                            QuestId = item.QuestId
                        });
                    }

                    context.SaveChanges();

                }
                catch (Exception e)
                {
                    Log.LogError(e, "Error while loading changes from JSON");
                }
            }
        }
    }
}
