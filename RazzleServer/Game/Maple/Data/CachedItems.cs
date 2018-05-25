using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using RazzleServer.Center;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data
{
    public sealed class CachedItems : KeyedCollection<int, Item>
    {
        public List<int> WizetItemIds { get; private set; }

        private readonly ILogger Log = LogManager.Log;

        public void Load()
        {
            Log.LogInformation("Loading Items");

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
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Accessory"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Cap"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Cape"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Coat"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Glove"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Longcoat"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Pants"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("PetEquip"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Ring"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Shield"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Shoes"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("TamingMob"));
                LoadEquipment(file.WzDirectory.GetDirectoryByName("Weapon"));
            }

            LoadWizetItemIds();
        }

        private void LoadItems(WzDirectory dir, ItemType type)
        {
            dir.WzImages
               .SelectMany(x => x.WzProperties)
               .ToList()
               .ForEach(item => Add(new Item(item, type)));
        }

        private void LoadEquipment(WzDirectory dir)
        {
            dir.WzImages.ForEach(item =>
            {
                var i = new Item(item, ItemType.Equipment);

                if (!Contains(i.MapleId))
                {
                    Add(i);
                }
            });
        }

        private void LoadWizetItemIds()
        {
            WizetItemIds = new List<int>(4)
            {
                1002140,
                1322013,
                1042003,
                1062007
            };
        }

        protected override int GetKeyForItem(Item item) => item.MapleId;
    }
}