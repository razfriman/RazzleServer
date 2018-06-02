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
using RazzleServer.Game.Maple.Shops;

namespace RazzleServer.Game.Maple.Data
{
    public class ShopProvider
    {
        private static readonly ILogger Log = LogManager.Log;
        private const string InitialDataFile = "InitialData/loot.json";

        internal static async Task Initialize()
        {
            using (var context = new MapleDbContext())
            {
                if (!context.Shops.Any())
                {
                    Log.LogInformation("Cannot find any shops in the database, attempting to load from JSON");
                    await LoadShopsFromJson();
                }

                if (!context.ShopItems.Any())
                {
                    Log.LogInformation("Cannot find any shop items in the database, attempting to load from JSON");
                    //await LoadShopItemsFromJson();
                }

                if (!context.ShopRecharges.Any())
                {
                    Log.LogInformation("Cannot find any shop recharges in the database, attempting to load from JSON");
                    //await LoadShopRechargesFromJson();
                }

                var sw = Stopwatch.StartNew();

                await LoadFromDatabase(context);

                Log.LogInformation("Data loaded in {0}ms.", sw.ElapsedMilliseconds);
            }
        }

        private static Task LoadShopsFromJson()
        {
            throw new NotImplementedException();
        }

        private static async Task LoadFromDatabase(MapleDbContext context)
        {
            Log.LogInformation("Loading Shops from database");

            var shops = await context
                .Shops
                .Include(x => x.ShopItems)
                .ToListAsync();

            shops
                .ForEach(x =>
                {
                    if (!DataProvider.Npcs?.Data?.ContainsKey(x.NpcId) ?? true)
                    {
                        Log.LogWarning($"Skipping shop - Cannot find Npc with ID={x.Id} in DataProvider");
                        return;
                    }

                    DataProvider.Shops.Data[x.NpcId] = new Shop(x);

                });
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
                    //RechargeTiers = new Dictionary<byte, Dictionary<int, double>>();
                    //
                    //var data = serializer.Deserialize<Dictionary<int, List<Loot>>>(reader);

                    //foreach (var item in data.Values.SelectMany(x => x))
                    //{
                    //    context.Loots.Add(new LootEntity
                    //    {
                    //        Chance = item.Chance,
                    //        IsMeso = item.IsMeso,
                    //        ItemId = item.ItemId,
                    //        MaximumQuantity = item.MaximumQuantity,
                    //        MinimumQuantity = item.MinimumQuantity,
                    //        MobId = item.MobId,
                    //        QuestId = item.QuestId
                    //    });
                    //}

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
