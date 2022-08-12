using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using RazzleServer.Data;
using RazzleServer.DataProvider.References;
using Serilog;

namespace RazzleServer.DataProvider
{
    public class LootProvider
    {
        private static readonly ILogger Logger = Log.ForContext<LootProvider>();
        private const string InitialDataFile = "InitialData/loot.json";

        public static async Task Initialize()
        {
            await using var context = new MapleDbContext();
            if (!context.Loots.Any())
            {
                Logger.Information("Cannot find any loot in the database, attempting to load from JSON");
                await LoadFromJson();
            }

            var sw = Stopwatch.StartNew();

            await LoadFromDatabase(context);

            Logger.Information("Data loaded in {0}ms.", sw.ElapsedMilliseconds);
        }

        private static async Task LoadFromDatabase(MapleDbContext context)
        {
            Logger.Information("Loading Loot from database");

            var entities = context
                .Loots
                .ToList()
                .GroupBy(x => x.MobId)
                .ToList();

            entities
                .ForEach(x =>
                {
                    if (!CachedData.Mobs?.Data?.ContainsKey(x.Key) ?? true)
                    {
                        Logger.Warning($"Removing loot - Cannot find Mob with ID={x.Key} in DataProvider");
                        context.Loots.RemoveRange(x);
                        return;
                    }

                    var loots = CachedData.Mobs.Data[x.Key].Loots;
                    loots.Clear();

                    x
                        .ToList()
                        .ForEach(item =>
                        {
                            if (!item.IsMeso && !CachedData.Items.Data.ContainsKey(item.ItemId))
                            {
                                Logger.Warning(
                                    $"Removing loot - Cannot find Item with ID={item.ItemId} in DataProvider");
                                context.Loots.Remove(item);
                                return;
                            }

                            loots.Add(new LootReference
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
                Logger.Warning($"Cannot find {Path.GetFullPath(InitialDataFile)}");
                return;
            }

            await using var s = File.OpenRead(InitialDataFile);
            await using var context = new MapleDbContext();
            try
            {
                var sw = Stopwatch.StartNew();

                var data = JsonSerializer.Deserialize<Dictionary<int, List<LootReference>>>(s);

                foreach (var item in data.Values.SelectMany(x => x))
                {
                    if (!CachedData.Mobs.Data.ContainsKey(item.MobId))
                    {
                        Logger.Warning($"Skipping loot - Cannot find Mob with ID={item.MobId} in DataProvider");
                        continue;
                    }

                    if (!item.IsMeso && !CachedData.Items.Data.ContainsKey(item.ItemId))
                    {
                        Logger.Warning($"Skipping loot - Cannot find Item with ID={item.ItemId} in DataProvider");
                        continue;
                    }

                    await context.Loots.AddAsync(new LootEntity
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
                Logger.Information("Populated database in {0}ms.", sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Error while loading changes from JSON");
            }
        }
    }
}
