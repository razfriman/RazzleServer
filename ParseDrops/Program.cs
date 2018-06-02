using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Shops;

namespace ParseDrops
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            await ParseDrops();
            //await ParseShops();
        }

        private static async Task ParseShops()
        {
            var results = new Dictionary<int, List<Loot>>();

            using (var s = File.OpenRead("shop.csv"))
            using (var reader = new StreamReader(s))
            {
                while (!reader.EndOfStream)
                {
                    //INSERT INTO `shop_data` (`shopid`,`npcid`,`recharge_tier`) VALUES 
                    //INSERT INTO `shop_items` (`shopid`,`itemid`,`quantity`,`price`,`sort`) VALUES 

                    var line = await reader.ReadLineAsync();
                    var splitted = line.Split(',');

                    if (splitted.Length < 6)
                    {
                        continue;
                    }

                    Console.WriteLine(line);
                    var mobId = int.Parse(splitted[0]);

                    if (!results.ContainsKey(mobId))
                    {
                        results[mobId] = new List<Loot>();
                    }

                    //results[mobId].Add(new Shop
                    //{
                    //});
                }
            }

            Export("drops.json", results);
            Console.WriteLine("Finished processing drops");
        }

        private static async Task ParseDrops()
        {
            var results = new Dictionary<int, List<Loot>>();

            using (var s = File.OpenRead("drop.csv"))
            using (var reader = new StreamReader(s))
            {
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    var splitted = line.Split(',');

                    if (splitted.Length < 6)
                    {
                        continue;
                    }

                    Console.WriteLine(line);
                    var mobId = int.Parse(splitted[0]);

                    if (!results.ContainsKey(mobId))
                    {
                        results[mobId] = new List<Loot>();
                    }

                    results[mobId].Add(new Loot
                    {
                        MobId = mobId,
                        IsMeso = splitted[1] == "'is_mesos'",
                        ItemId = int.Parse(splitted[2]),
                        MinimumQuantity = int.Parse(splitted[3]),
                        MaximumQuantity = int.Parse(splitted[4]),
                        QuestId = int.Parse(splitted[5]),
                        Chance = int.Parse(splitted[6])
                    });
                }
            }

            Export("drops.json", results);
            Console.WriteLine("Finished processing drops");
        }

        public static void Export(string path, object obj)
        {
            using (var s = File.OpenWrite(path))
            using (var sr = new StreamWriter(s))
            using (var writer = new JsonTextWriter(sr))
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented
                };
                serializer.Serialize(writer, obj);
            }
        }
    }
}
