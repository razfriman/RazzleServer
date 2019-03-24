using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RazzleServer.Common;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Shops;
using Serilog;

namespace RazzleServer.Game.Maple.Data
{
    public class ShopProvider
    {
        private static readonly ILogger Logger = Log.ForContext<ShopProvider>();
        private const string ShopsDataFile = "InitialData/shops.json";
        private const string ShopItemsDataFile = "InitialData/shopItems.json";
        private const string RechargeTiersDataFile = "InitialData/rechargeTiers.json";

        internal static async Task Initialize()
        {
            using (var context = new MapleDbContext())
            {
                if (!context.Shops.Any())
                {
                    Logger.Information("Cannot find any shops in the database, attempting to load from JSON");
                    await LoadShopsFromJson();
                }

                if (!context.ShopItems.Any())
                {
                    Logger.Information("Cannot find any shop items in the database, attempting to load from JSON");
                    await LoadShopItemsFromJson();
                }

                if (!context.ShopRecharges.Any())
                {
                    Logger.Information("Cannot find any shop recharges in the database, attempting to load from JSON");
                    await LoadShopRechargesFromJson();
                }

                var sw = Stopwatch.StartNew();

                await LoadFromDatabase(context);

                Logger.Information("Data loaded in {0}ms.", sw.ElapsedMilliseconds);
            }
        }

        private static async Task LoadShopRechargesFromJson()
        {
            if (!File.Exists(RechargeTiersDataFile))
            {
                Logger.Warning($"Cannot find {RechargeTiersDataFile}");
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
                            TierId = item.TierId, ItemId = item.ItemId, Price = item.Price
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

        private static async Task LoadShopsFromJson()
        {
            if (!File.Exists(ShopsDataFile))
            {
                Logger.Warning($"Cannot find {ShopsDataFile}");
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
                        if (!DataProvider.Npcs?.Data?.ContainsKey(item.NpcId) ?? true)
                        {
                            Logger.Warning($"Skipping shop - Cannot find Npc with ID={item.NpcId} in DataProvider");
                            continue;
                        }

                        context.Shops.Add(new ShopEntity
                        {
                            ShopId = item.ShopId, NpcId = item.NpcId, RechargeTier = item.RechargeTier
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

        private static async Task LoadShopItemsFromJson()
        {
            if (!File.Exists(ShopItemsDataFile))
            {
                Logger.Warning($"Cannot find {ShopItemsDataFile}");
                return;
            }

            using (var s = File.OpenRead(ShopItemsDataFile))
            using (var sr = new StreamReader(s))
            using (var reader = new JsonTextReader(sr))
            using (var context = new MapleDbContext())
            {
                try
                {
                    var sw = Stopwatch.StartNew();
                    var serializer = new JsonSerializer();
                    var data = serializer.Deserialize<List<ShopItemEntity>>(reader);
                    var shops = context.Shops.Select(x => x.ShopId).ToHashSet();
                    
                    foreach (var item in data)
                    {
                        if (!shops.Contains(item.ShopId))
                        {
                            Logger.Warning(
                                $"Skipping shop item - Cannot find Shop with ID={item.ShopId} in DataProvider");
                            continue;
                        }

                        if (!DataProvider.Items.Data.ContainsKey(item.ItemId))
                        {
                            Logger.Warning(
                                $"Skipping shop item - Cannot find Item with ID={item.ItemId} in DataProvider");
                            continue;
                        }

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
                    Logger.Information("Populated database in {0}ms.", sw.ElapsedMilliseconds);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Error while loading changes from JSON");
                }
            }
        }

        private static async Task LoadFromDatabase(MapleDbContext context)
        {
            Logger.Information("Loading Shops from database");

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
                        Logger.Warning($"Skipping shop - Cannot find Npc with ID={x.NpcId} in DataProvider");
                        return;
                    }

                    DataProvider.Shops.Data[x.NpcId] = new Shop(x);
                });
        }
    }
}
