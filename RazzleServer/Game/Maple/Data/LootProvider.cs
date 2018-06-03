using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        private const string InitialDataFile = "InitialData/loot.json";

        internal static async Task Initialize()
        {
            using (var context = new MapleDbContext())
            {
                if (!context.Loots.Any())
                {
                    Log.LogInformation("Cannot find any loot in the database, attempting to load from JSON");
                    await LoadFromJson();
                }

                var sw = Stopwatch.StartNew();

                await LoadFromDatabase(context);

                Log.LogInformation("Data loaded in {0}ms.", sw.ElapsedMilliseconds);
            }
        }

        private static async Task LoadFromDatabase(MapleDbContext context)
        {
            Log.LogInformation("Loading Loot from database");

            var entities = await context
            .Loots
            .GroupBy(x => x.MobId)
            .ToListAsync();

            entities
            .ForEach(x =>
            {
                if (!DataProvider.Mobs?.Data?.ContainsKey(x.Key) ?? true)
                {
                    Log.LogWarning($"Removing loot - Cannot find Mob with ID={x.Key} in DataProvider");
                    context.Loots.RemoveRange(x);
                    return;
                }

                var loots = DataProvider.Mobs.Data[x.Key].Loots;
                loots.Clear();

                x
                .ToList()
                .ForEach(item =>
                {

                    if (!item.IsMeso && !DataProvider.Items.Data.ContainsKey(item.ItemId))
                    {
                        Log.LogWarning($"Removing loot - Cannot find Item with ID={item.ItemId} in DataProvider");
                        context.Loots.Remove(item);
                        return;
                    }

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

            await context.SaveChangesAsync();
        }

        private static async Task LoadFromJson()
        {
            if (!File.Exists(InitialDataFile))
            {
                Log.LogWarning($"Cannot find {InitialDataFile}");
                return;
            }

            using (var s = File.OpenRead(InitialDataFile))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            using (var context = new MapleDbContext())
            {
                try
                {
                    var sw = Stopwatch.StartNew();

                    var serializer = new JsonSerializer();
                    var data = serializer.Deserialize<Dictionary<int, List<Loot>>>(reader);

                    foreach (var item in data.Values.SelectMany(x => x))
                    {
                        if (!DataProvider.Mobs.Data.ContainsKey(item.MobId))
                        {
                            Log.LogWarning($"Skipping loot - Cannot find Mob with ID={item.MobId} in DataProvider");
                            continue;
                        }

                        if (!item.IsMeso && !DataProvider.Items.Data.ContainsKey(item.ItemId))
                        {
                            Log.LogWarning($"Skipping loot - Cannot find Item with ID={item.ItemId} in DataProvider");
                            continue;
                        }

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

                    await context.SaveChangesAsync();
                    Log.LogInformation("Populated database in {0}ms.", sw.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    Log.LogError(e, "Error while loading changes from JSON");
                }
            }
        }
    }
}
