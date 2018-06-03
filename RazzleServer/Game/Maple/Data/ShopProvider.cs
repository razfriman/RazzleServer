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
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Shops;

namespace RazzleServer.Game.Maple.Data
{
    public class ShopProvider
    {
        private static readonly ILogger Log = LogManager.Log;
        private const string ShopsDataFile = "InitialData/shops.json";
        private const string ShopItemsDataFile = "InitialData/shopItems.json";
        private const string RechargeTiersDataFile = "InitialData/rechargeTiers.json";

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
                    await LoadShopItemsFromJson();
                }

                if (!context.ShopRecharges.Any())
                {
                    Log.LogInformation("Cannot find any shop recharges in the database, attempting to load from JSON");
                    await LoadShopRechargesFromJson();
                }

                var sw = Stopwatch.StartNew();

                await LoadFromDatabase(context);

                Log.LogInformation("Data loaded in {0}ms.", sw.ElapsedMilliseconds);
            }
        }

        private static async Task LoadShopRechargesFromJson()
        {
            if (!File.Exists(RechargeTiersDataFile))
            {
                Log.LogWarning($"Cannot find {RechargeTiersDataFile}");
                return;
            }

            using (var s = File.OpenRead(RechargeTiersDataFile))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            using (var context = new MapleDbContext())
            {
                try
                {
                    var sw = Stopwatch.StartNew();

                    var serializer = new JsonSerializer();
                    var data = serializer.Deserialize<List<ShopRechargeEntity>>(reader);

                    foreach (var item in data)
                    {
                        context.ShopRecharges.Add(new ShopRechargeEntity
                        {
                            TierId = item.TierId,
                            ItemId = item.ItemId,
                            Price = item.Price,
                        });
                    }

                    await context.SaveChangesAsync();
                    Log.LogInformation("Populated database in {0}ms.", sw.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    Log.LogError(e, "Error while loading changes from JSON");
                }
            }        }

        private static async Task LoadShopsFromJson()
        {
            if (!File.Exists(ShopsDataFile))
            {
                Log.LogWarning($"Cannot find {ShopsDataFile}");
                return;
            }

            using (var s = File.OpenRead(ShopsDataFile))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            using (var context = new MapleDbContext())
            {
                try
                {
                    var sw = Stopwatch.StartNew();

                    var serializer = new JsonSerializer();
                    var data = serializer.Deserialize<List<ShopEntity>>(reader);

                    foreach (var item in data)
                    {
                        context.Shops.Add(new ShopEntity
                        {
                            ShopId = item.ShopId,
                            NpcId = item.NpcId,
                            RechargeTier = item.RechargeTier,
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

        private static async Task LoadShopItemsFromJson()
        {
            if (!File.Exists(ShopItemsDataFile))
            {
                Log.LogWarning($"Cannot find {ShopItemsDataFile}");
                return;
            }

            using (var s = File.OpenRead(ShopsDataFile))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            using (var context = new MapleDbContext())
            {
                try
                {
                    var sw = Stopwatch.StartNew();

                    var serializer = new JsonSerializer();
                    var data = serializer.Deserialize<List<ShopItemEntity>>(reader);

                    foreach (var item in data)
                    {
                        context.ShopItems.Add(new ShopItemEntity
                        {
                            ItemId = item.ItemId,
                            Price = item.Price,
                            Quantity = item.Quantity,
                            ShopId = item.ShopId,
                            Sort = item.Sort
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

        private static async Task LoadFromDatabase(MapleDbContext context)
        {
            Log.LogInformation("Loading Shops from database");

            var rechargeTiers = await context
                .ShopRecharges
                .GroupBy(x => x.TierId)
                .ToListAsync();

            rechargeTiers.ForEach(x =>
            {
                var dict = new Dictionary<int, double>();
                x.ToList().ForEach(item => dict[item.ItemId] = item.Price);
                DataProvider.RechargeTiers.Data[x.Key] = dict;
            });

            var shops = await context
                .Shops
                .Include(x => x.ShopItems)
                .ToListAsync();

            shops
                .ForEach(x =>
                {
                    if (!DataProvider.Npcs?.Data?.ContainsKey(x.NpcId) ?? true)
                    {
                    Log.LogWarning($"Skipping shop - Cannot find Npc with ID={x.NpcId} in DataProvider");
                        return;
                    }

                    DataProvider.Shops.Data[x.NpcId] = new Shop(x);
                });
        }

    }
}
