using RazzleServer.Constants;
using RazzleServer.Data;
using RazzleServer.DB.Models;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using System;
using System.Linq;
using MapleLib.PacketLib;

namespace RazzleServer.Inventory
{
    public class MapleItem
    {
        public long DbId { get; set; }
        public int ItemId { get; set; }
        public short Position { get; set; }
        public MapleItemFlags Flags { get; set; }
        public short Quantity { get; set; }
        public string Creator { get; set; }
        public string Source { get; set; }

        public MapleItem(int itemId, string source, short quantity = 1, string creator = "", MapleItemFlags flags = 0, short position = 0, long dbId = -1)
        {
            ItemId = itemId;
            Position = position;
            Quantity = quantity;
            Creator = creator;
            Flags = flags;
            Source = source;
            DbId = dbId;
        }

        public MapleItem(MapleItem item, string source)
        {
            ItemId = item.ItemId;
            Position = item.Position;
            Flags = item.Flags;
            Quantity = item.Quantity;
            Creator = item.Creator;
            DbId = -1;
            Source = source;
        }

        /// <param name="owner">if equal to null, deletes the item from the database</param>
        public virtual void SaveToDatabase(MapleCharacter owner)
        {
            using (var context = new MapleDbContext())
            {
                if (owner == null)
                {
                    if (InventoryType == MapleInventoryType.Equip)
                    {
                        InventoryEquip equipEntry = context.InventoryEquips.FirstOrDefault(x => x.ID == DbId);
                        if (equipEntry != null)
                            context.InventoryEquips.Remove(equipEntry);
                    }
                    InventoryItem entry = context.InventoryItems.FirstOrDefault(x => x.ID == DbId);
                    if (entry != null)
                        context.InventoryItems.Remove(entry);
                    DbId = -1;
                }
                else
                {
                    InventoryItem dbActionItem = null;
                    if (DbId != -1)
                        dbActionItem = context.InventoryItems.FirstOrDefault(x => x.ID == DbId);
                    if (dbActionItem == null)
                    {
                        dbActionItem = new InventoryItem();
                        context.InventoryItems.Add(dbActionItem);
                        context.SaveChanges();
                        DbId = dbActionItem.ID;
                    }
                    dbActionItem.CharacterID = owner.ID;
                    dbActionItem.ItemID = ItemId;
                    dbActionItem.Position = Position;
                    dbActionItem.Quantity = Quantity;
                    dbActionItem.Source = Source;
                    dbActionItem.Creator = Creator;
                    dbActionItem.Flags = (short)Flags;
                }
                context.SaveChanges();
            }
        }

        public bool CanStackWith(MapleItem otherItem) => ItemId == otherItem.ItemId && Creator == otherItem.Creator;

        #region Packets
        public static PacketWriter ShowItemGain(MapleItem item)
        {
            
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.SHOW_STATUS_INFO);
            pw.WriteShort(0);
            pw.WriteInt(item.ItemId);
            pw.WriteInt(item.Quantity);
            return pw;
        }

        public static void AddItemPosition(PacketWriter pw, MapleItem item)
        {
            short position = item.Position;
            if (position < 0)
            {
                position = Math.Abs(position);
                if (position > 100 && position < 1000)
                    position -= 100;
            }
            if (item.Type == 1) //equip
                pw.WriteShort(position);
            else
                pw.WriteByte((byte)position);
        }

        public bool CheckAndRemoveFlag(MapleItemFlags flag)
        {
            if (!Flags.HasFlag(flag)) return false;
            Flags &= ~flag;
            return true;
        }

        public static void AddItemInfo(PacketWriter pw, MapleItem item)
        {
            pw.WriteByte(item.Type); //TODO: pets = 3
            pw.WriteInt(item.ItemId);

            bool isCashShop = false;

            pw.WriteBool(isCashShop);
            if(isCashShop)
            {
                //var uniqueID = isPet ? item.getPetId() : isRing ? equip.getRingId() : item.getCashId());
                //pw.WriteLong(uniqueID); 
            }

            pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-1)); //TODO: item expiration

            if (item.Type == 1) //Equip
            {
                MapleEquip equip = (MapleEquip)item;
                MapleEquip.AddStats(equip, pw);

                pw.WriteByte(0);
                pw.WriteByte(equip.CustomLevel);
                pw.WriteShort(0);
                pw.WriteShort(equip.CustomExp);
                pw.WriteInt(0); // Vicious
                pw.WriteLong(0); // DbID
                pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-2)); //don't know
                pw.WriteInt(-1);
            }
            else
            {
                pw.WriteShort(item.Quantity);
                pw.WriteMapleString(item.Creator);
                pw.WriteShort((short)item.Flags);
                if (item.IsAmmo || item.IsFamiliarCard)
                    pw.WriteLong(-1);
            }
        }
        #endregion

        #region Constant & properties
        public virtual MapleInventoryType InventoryType => ItemConstants.GetInventoryType(ItemId);

        public bool Tradeable
        {
            get
            {
                MapleItemFlags noTradeFlags = Flags & (MapleItemFlags.Untradeable | MapleItemFlags.Lock);
                if (noTradeFlags > 0)
                    return false;
                var info = Type == 1 ? DataBuffer.GetEquipById(ItemId) : DataBuffer.GetItemById(ItemId);
                if (info == null || !info.Tradeable)
                    return false;
                return true;
            }
        }

        public virtual byte Type
        {
            get
            {
                if (InventoryType == MapleInventoryType.Equip)
                    return 1;
                else
                    return 2;
                //TODO: pet = 3
            }
        }

        public int ItemIdBase => ItemId / 10000;

        public bool IsAmmo => IsThrowingStar || IsBullet;
        public bool IsWeapon => ItemConstants.IsWeapon(ItemId);

        public bool IsBowArrow => ItemId >= 2060000 && ItemId < 2061000;
        public bool IsCrossbowArrow => ItemId >= 2061000 && ItemId < 2062000;
        public bool IsThrowingStar => ItemIdBase == 207;
        public bool IsSummonSack => ItemIdBase == 210;
        public bool IsBullet => ItemIdBase == 233;
        public bool IsMonsterCard => ItemIdBase == 238;

        public MapleItemType ItemType => ItemConstants.GetMapleItemType(ItemId);

        public bool IsFamiliarCard => ItemIdBase == 287;
        #endregion
    }


}
