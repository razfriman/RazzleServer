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

        private readonly ILogger _log = LogManager.Log;

        public override void LoadFromWz()
        {
            _log.LogInformation("Loading Items from WZ");

            _log.LogInformation("Loading Regular Items");

            using (var file = GetWzFile("Item.wz"))
            {
                file.ParseWzFile();

                LoadItems(file.WzDirectory.GetDirectoryByName("Cash"), ItemType.Cash);
                LoadItems(file.WzDirectory.GetDirectoryByName("Consume"), ItemType.Usable);
                LoadItems(file.WzDirectory.GetDirectoryByName("Etc"), ItemType.Etcetera);
                LoadItems(file.WzDirectory.GetDirectoryByName("Install"), ItemType.Setup);
            }

            _log.LogInformation("Loading Equip Items");
            using (var file = GetWzFile("Character.wz"))
            {
                file.ParseWzFile();

                _log.LogInformation("Loading Equip Items: Accessory");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Accessory"));

                _log.LogInformation("Loading Equip Items: Cap");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Cap"));

                _log.LogInformation("Loading Equip Items: Cape");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Cape"));

                _log.LogInformation("Loading Equip Items: Coat");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Coat"));

                _log.LogInformation("Loading Equip Items: Glove");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Glove"));

                _log.LogInformation("Loading Equip Items: Longcoat");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Longcoat"));

                _log.LogInformation("Loading Equip Items: Pants");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Pants"));

                _log.LogInformation("Loading Equip Items: PetEquip");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("PetEquip"));

                _log.LogInformation("Loading Equip Items: Ring");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Ring"));

                _log.LogInformation("Loading Equip Items: Shield");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Shield"));

                _log.LogInformation("Loading Equip Items: Shoes");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Shoes"));

                _log.LogInformation("Loading Equip Items: TamingMob");
                LoadEquipment(file.WzDirectory.GetDirectoryByName("TamingMob"));

                _log.LogInformation("Loading Equip Items: Weapon");
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
                var mapleItem = new ItemReference(item, ItemType.Equipment);

                if (!Data.Data.ContainsKey(mapleItem.MapleId))
                {
                    Data.Data.Add(mapleItem.MapleId, mapleItem);
                }
            });
        }
    }
}