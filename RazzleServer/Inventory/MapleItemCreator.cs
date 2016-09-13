using RazzleServer.Constants;
using RazzleServer.Data;
using RazzleServer.Data.WZ;

namespace RazzleServer.Inventory
{
    public static class MapleItemCreator
    {
        public static MapleItem CreateItem(int itemId, string source, short quantity = 1, bool randomStats = false)
        {
            MapleInventoryType type = ItemConstants.GetInventoryType(itemId);
            if (type == MapleInventoryType.Equip)
            {
                WzEquip wzInfo = DataBuffer.GetEquipById(itemId);
                if (wzInfo != null)
                {
                    MapleEquip equip = new MapleEquip(itemId, source);
                    equip.SetDefaultStats(wzInfo, randomStats);
                    return equip;
                }
            }
            else if (type != MapleInventoryType.Undefined)
            {
                WzItem wzInfo = DataBuffer.GetItemById(itemId);
                if (wzInfo != null)
                {
                    if (wzInfo.SlotMax > 0 && quantity > wzInfo.SlotMax)
                        quantity = (short)wzInfo.SlotMax;
                    MapleItem item = new MapleItem(itemId, source, quantity);
                    return item;
                }
            }
            return null;
        }
    }
}
