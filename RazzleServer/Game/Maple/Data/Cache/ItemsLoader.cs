using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.Cache
{
    public sealed class ItemsLoader : ACachedDataLoader<CachedItems>
    {
        public override string CacheName => "Items";

        private readonly ILogger Log = LogManager.Log;

        public override void LoadFromWz()
        {
            Log.LogInformation("Loading Items from WZ");

            Log.LogInformation("Loading Regular Items");
            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Item.wz"), WzMapleVersion.CLASSIC))
            {
                file.ParseWzFile();

                LoadItems(file.WzDirectory.GetDirectoryByName("Cash"), ItemType.Cash);
                LoadItems(file.WzDirectory.GetDirectoryByName("Consume"), ItemType.Usable);
                LoadItems(file.WzDirectory.GetDirectoryByName("Etc"), ItemType.Etcetera);
                LoadItems(file.WzDirectory.GetDirectoryByName("Install"), ItemType.Setup);
            }

            Log.LogInformation("Loading Equip Items");
            using (var file = new WzFile(Path.Combine(ServerConfig.Instance.WzFilePath, "Character.wz"), WzMapleVersion.CLASSIC))
            {
                file.ParseWzFile();

                Log.LogInformation("Loading Equip Items: Accessory");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Accessory"));

                Log.LogInformation("Loading Equip Items: Cap");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Cap"));

                Log.LogInformation("Loading Equip Items: Cape");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Cape"));

                Log.LogInformation("Loading Equip Items: Coat");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Coat"));

                Log.LogInformation("Loading Equip Items: Glove");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Glove"));

                Log.LogInformation("Loading Equip Items: Longcoat");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Longcoat"));

                Log.LogInformation("Loading Equip Items: Pants");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Pants"));

                Log.LogInformation("Loading Equip Items: PetEquip");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("PetEquip"));

                Log.LogInformation("Loading Equip Items: Ring");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Ring"));

                Log.LogInformation("Loading Equip Items: Shield");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Shield"));

                Log.LogInformation("Loading Equip Items: Shoes");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Shoes"));

                Log.LogInformation("Loading Equip Items: TamingMob");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("TamingMob"));

                Log.LogInformation("Loading Equip Items: Weapon");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Weapon"));
            }
        }

        private void LoadItems(WzDirectory dir, ItemType type)
        {
            dir.WzImages
               .SelectMany(x => x.WzProperties)
               .ToList()
               .ForEach(item =>
               {
                   var mapleItem = new Item(item, type);
                   if (!Data.Data.ContainsKey(mapleItem.MapleId))
                   {
                       Data.Data.Add(mapleItem.MapleId, mapleItem);
                   }
               });
        }

        private void LoadEquipment(WzDirectory dir)
        {
            dir.WzImages
            .ForEach(item =>
            {
                var mapleItem = new Item(item, ItemType.Equipment);

                if (!Data.Data.ContainsKey(mapleItem.MapleId))
                {
                    Data.Data.Add(mapleItem.MapleId, mapleItem);
                }
            });
        }
    }
}