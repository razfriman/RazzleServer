using System.Linq;
using Serilog;
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

        public override ILogger Logger => Log.ForContext<ItemsLoader>();

        public override void LoadFromWz()
        {
            Logger.Information("Loading Items from WZ");

            Logger.Information("Loading Regular Items");

            using (var file = GetWzFile("Data.wz"))
            {
                file.ParseWzFile();
                var itemDir = file.WzDirectory.GetDirectoryByName("Item");
                var characterDir = file.WzDirectory.GetDirectoryByName("Character");

                //LoadItems(file.WzDirectory.GetDirectoryByName("Cash"), ItemType.Cash);
                LoadItems(itemDir.GetDirectoryByName("Consume"), ItemType.Usable);
                LoadItems(itemDir.GetDirectoryByName("Etc"), ItemType.Etcetera);
                LoadItems(itemDir.GetDirectoryByName("Install"), ItemType.Setup);

                Logger.Information("Loading Equip Items");

                Logger.Information("Loading Equip Items: Accessory");
                LoadEquipment(characterDir.GetDirectoryByName("Accessory"));

                Logger.Information("Loading Equip Items: Cap");
                LoadEquipment(characterDir.GetDirectoryByName("Cap"));

                Logger.Information("Loading Equip Items: Cape");
                LoadEquipment(characterDir.GetDirectoryByName("Cape"));

                Logger.Information("Loading Equip Items: Coat");
                LoadEquipment(characterDir.GetDirectoryByName("Coat"));

                Logger.Information("Loading Equip Items: Glove");
                LoadEquipment(characterDir.GetDirectoryByName("Glove"));

                Logger.Information("Loading Equip Items: Longcoat");
                LoadEquipment(characterDir.GetDirectoryByName("Longcoat"));

                Logger.Information("Loading Equip Items: Pants");
                LoadEquipment(characterDir.GetDirectoryByName("Pants"));

                Logger.Information("Loading Equip Items: PetEquip");
                LoadEquipment(characterDir.GetDirectoryByName("PetEquip"));

                Logger.Information("Loading Equip Items: Ring");
                LoadEquipment(characterDir.GetDirectoryByName("Ring"));

                Logger.Information("Loading Equip Items: Shield");
                LoadEquipment(characterDir.GetDirectoryByName("Shield"));

                Logger.Information("Loading Equip Items: Shoes");
                LoadEquipment(characterDir.GetDirectoryByName("Shoes"));

                Logger.Information("Loading Equip Items: Weapon");
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
