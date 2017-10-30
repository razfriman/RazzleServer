using System.Collections.Generic;

namespace RazzleServer.Data.WZ
{
    public class WzItemOption
    {
        public int Id { get; set; }
        public byte ReqLevel { get; set; }
        public int OptionType { get; set; }
        public string Text { get; set; }        
        public Dictionary<byte, Dictionary<string, int>> LevelStats { get; set; }
        public int SubCategory => (Id % 10000) / 1000;

        public Dictionary<string, int> GetAttributes(byte level)
        {
            Dictionary<string, int> attributes;
            return LevelStats.TryGetValue(level, out attributes) ? attributes : null;
        }

        public string GetPotentialText(byte level)
        {
            Dictionary<string, int> attributes;
            if (LevelStats.TryGetValue(level, out attributes))
            {
                string textCopy = Text;
                foreach (var kvp in attributes)
                {
                    textCopy = textCopy.Replace("#" + kvp.Key, kvp.Value.ToString());
                }
                return textCopy;
            }
            return "";
        }

        public bool FitsItem(MapleItemType itemType)
        {
            if (OptionType == 0) return true;
            switch (OptionType)
            {
                case 10:
                    return ItemConstants.IsWeapon(itemType);
                case 11:
                    return !ItemConstants.IsWeapon(itemType);
                case 20:
                    return !ItemConstants.IsWeapon(itemType) && !ItemConstants.IsAccessory(itemType);
                case 40:
                    return ItemConstants.IsAccessory(itemType);
                case 51:
                    return itemType == MapleItemType.Cap;
                case 52:
                    return itemType == MapleItemType.Top || itemType == MapleItemType.Overall;
                case 53:
                    return itemType == MapleItemType.Legs || itemType == MapleItemType.Overall;
                case 54:
                    return itemType == MapleItemType.Glove;
                case 55:
                    return itemType == MapleItemType.Shoes;
            }
            return false;
        }
        public bool FitsItem(int itemId)
        {
            if (OptionType == 0) return true;
            return FitsItem(ItemConstants.GetMapleItemType(itemId));
        }
    }
}
