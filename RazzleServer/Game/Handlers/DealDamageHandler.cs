using RazzleServer.Constants;
using RazzleServer.Inventory;
using RazzleServer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Handlers
{
    public class DealDamageHandler
    {
        public static bool HandleRangedAttackAmmoUsage(MapleCharacter chr, int bulletCon)
        {
            if (!chr.IsMechanic && !chr.IsMercedes) // Don't use ammo
            {
                MapleEquip weapon = chr.Inventory.GetEquippedItem((short)MapleEquipPosition.Weapon) as MapleEquip;
                MapleItemType weaponType = weapon.ItemType;
                int ammoItemId = 0;
                switch (weaponType)
                {
                    case MapleItemType.Bow:
                        if (!chr.HasBuff(Hunter.SOUL_ARROW_BOW) && !chr.HasBuff(WindArcher2.SOUL_ARROW))
                        {
                            MapleItem ammoItem = chr.Inventory.GetFirstItemFromInventory(MapleInventoryType.Use, item => item.IsBowArrow && item.Quantity > 0);
                            if (ammoItem == null) return false; //player has no bow arrows                                        
                            ammoItemId = ammoItem.ItemId;
                        }
                        break;
                    case MapleItemType.Crossbow:
                        if (!chr.HasBuff(Crossbowman.SOUL_ARROW_CROSSBOW) && !chr.HasBuff(WildHunter2.SOUL_ARROW_CROSSBOW))
                        {
                            MapleItem ammoItem = chr.Inventory.GetFirstItemFromInventory(MapleInventoryType.Use, item => item.IsCrossbowArrow && item.Quantity > 0);
                            if (ammoItem == null) return false; //player has no xbow arrows                                        
                            ammoItemId = ammoItem.ItemId;
                        }
                        break;
                    case MapleItemType.Claw:
                        if (!chr.HasBuff(Hermit.SHADOW_STARS) && !chr.HasBuff(NightWalker3.SHADOW_STARS))
                        {
                            MapleItem ammoItem = chr.Inventory.GetFirstItemFromInventory(MapleInventoryType.Use, item => item.IsThrowingStar && item.Quantity > 0);
                            if (ammoItem == null) return false; //player has no bullets                                        
                            ammoItemId = ammoItem.ItemId;
                        }
                        break;
                    case MapleItemType.Gun:
                        if (!chr.HasBuff(Gunslinger.INFINITY_BLAST))
                        {
                            MapleItem ammoItem = chr.Inventory.GetFirstItemFromInventory(MapleInventoryType.Use, item => item.IsBullet && item.Quantity > 0);
                            if (ammoItem == null) return false; //player has no bullets                                        
                            ammoItemId = ammoItem.ItemId;
                        }
                        break;
                }
                if (ammoItemId > 0)
                {
                    chr.Inventory.RemoveItemsById(ammoItemId, bulletCon, false); //Even if player only has 1 bullet left and bulletCon is > 1, we'll allow it since it removes the item or stack anyway
                }
            }
            return true;
        }
    }
}