using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Data.Cache;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Data.Loaders
{
    public sealed class ItemsLoader : ACachedDataLoader<CachedItems>
    {
        public override string CacheName => "Items";

        public override ILogger Log => LogManager.CreateLogger<ItemsLoader>();

        public override void LoadFromWz()
        {
            Log.LogInformation("Loading Items from WZ");

            Log.LogInformation("Loading Regular Items");

            using (var file = GetWzFile("Data.wz"))
            {
                file.ParseWzFile();
                var itemDir = file.WzDirectory.GetDirectoryByName("Item");
                var characterDir = file.WzDirectory.GetDirectoryByName("Character");

                //LoadItems(file.WzDirectory.GetDirectoryByName("Cash"), ItemType.Cash);
                LoadItems(itemDir.GetDirectoryByName("Consume"), ItemType.Usable);
                LoadItems(itemDir.GetDirectoryByName("Etc"), ItemType.Etcetera);
                LoadItems(itemDir.GetDirectoryByName("Install"), ItemType.Setup);

                Log.LogInformation("Loading Equip Items");

                Log.LogInformation("Loading Equip Items: Accessory");
                LoadEquipment(characterDir.GetDirectoryByName("Accessory"));

                Log.LogInformation("Loading Equip Items: Cap");
                LoadEquipment(characterDir.GetDirectoryByName("Cap"));

                Log.LogInformation("Loading Equip Items: Cape");
                LoadEquipment(characterDir.GetDirectoryByName("Cape"));

                Log.LogInformation("Loading Equip Items: Coat");
                LoadEquipment(characterDir.GetDirectoryByName("Coat"));

                Log.LogInformation("Loading Equip Items: Glove");
                LoadEquipment(characterDir.GetDirectoryByName("Glove"));

                Log.LogInformation("Loading Equip Items: Longcoat");
                LoadEquipment(characterDir.GetDirectoryByName("Longcoat"));

                Log.LogInformation("Loading Equip Items: Pants");
                LoadEquipment(characterDir.GetDirectoryByName("Pants"));

                Log.LogInformation("Loading Equip Items: PetEquip");
                LoadEquipment(characterDir.GetDirectoryByName("PetEquip"));

                Log.LogInformation("Loading Equip Items: Ring");
                LoadEquipment(characterDir.GetDirectoryByName("Ring"));

                Log.LogInformation("Loading Equip Items: Shield");
                LoadEquipment(characterDir.GetDirectoryByName("Shield"));

                Log.LogInformation("Loading Equip Items: Shoes");
                LoadEquipment(characterDir.GetDirectoryByName("Shoes"));

                Log.LogInformation("Loading Equip Items: Weapon");
                LoadEquipment(characterDir.GetDirectoryByName("Weapon"));
            }
        }

        private void LoadItems(WzDirectory dir, ItemType type)
        {
            dir.WzImages
                .SelectMany(x => x.WzProperties)
                .ToList()
                .ForEach(item =>
                {
                    var mapleItem = new ItemReference(item, type);
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
                    var mapleItem = new ItemReference(item);

                    if (!Data.Data.ContainsKey(mapleItem.MapleId))
                    {
                        Data.Data.Add(mapleItem.MapleId, mapleItem);
                    }
                });
        }
    }
}
