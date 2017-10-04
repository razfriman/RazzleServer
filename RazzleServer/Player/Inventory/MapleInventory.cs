using RazzleServer.Constants;
using RazzleServer.Data;
using RazzleServer.Data.WZ;
using RazzleServer.DB.Models;
using RazzleServer.Map;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MapleLib.PacketLib;

namespace RazzleServer.Inventory
{
    public class MapleInventory
    {
        private readonly Dictionary<short, MapleItem> EquippedInventory = new Dictionary<short, MapleItem>();
        private readonly Dictionary<short, MapleItem> EquipInventory = new Dictionary<short, MapleItem>();
        private readonly Dictionary<short, MapleItem> UseInventory = new Dictionary<short, MapleItem>();
        private readonly Dictionary<short, MapleItem> SetupInventory = new Dictionary<short, MapleItem>();
        private readonly Dictionary<short, MapleItem> EtcInventory = new Dictionary<short, MapleItem>();
        private readonly Dictionary<short, MapleItem> CashInventory = new Dictionary<short, MapleItem>();

        public byte EquipSlots { get; set; }
        public byte UseSlots { get; set; }
        public byte SetupSlots { get; set; }
        public byte EtcSlots { get; set; }
        public byte CashSlots { get; set; }

        private MapleCharacter Owner;

        public int Mesos
        {
            get
            {
                return Owner.Mesos;
            }
            private set
            {
                Owner.Mesos = value;
            }
        }

        public MapleInventory(MapleCharacter owner)
        {
            Owner = owner;

            EquipSlots = 24;
            UseSlots = 24;
            SetupSlots = 24;
            EtcSlots = 24;
            CashSlots = 96;
        }

        public MapleItem GetFirstItemFromInventory(MapleInventoryType type, Func<MapleItem, bool> condition)
        {
            var inventory = GetInventory(type);
            lock (inventory)
            {
                var items = inventory.Where(x => condition(x.Value));
                if (items.Any())
                    return items.OrderBy(x => x.Key).FirstOrDefault().Value;
                return null;
            }
        }

        public void Release()
        {
            Owner = null;
        }

        public void Bind(MapleCharacter character)
        {
            Owner = character;
        }

        public void GainMesos(int gain, bool fromMonster = true, bool showInChat = false)
        {
            if (gain < 0) gain *= -1;
            AddMeso(gain, fromMonster, showInChat);
        }

        public void RemoveMesos(int loss, bool fromMonster = true, bool showInChat = false)
        {
            if (loss == 0) return;
            if (loss > 0) loss *= -1;
            AddMeso(loss, fromMonster, showInChat);
        }

        private void AddMeso(int amount, bool fromMonster = true, bool showInChat = false)
        {
            if (amount > 0 && Mesos == int.MaxValue)
                return;

            if (Mesos + amount < 0)
                Mesos = 0;
            else if (Mesos + amount > int.MaxValue)
                Mesos = int.MaxValue;
            else
            {
                Mesos += amount;
            }

            MapleCharacter.UpdateSingleStat(Owner.Client, MapleCharacterStat.Meso, Mesos, fromMonster);

            if (fromMonster)
                Owner.Client.SendPacket(Packets.ShowMesoGain(amount, false));
            if (showInChat)
                Owner.Client.SendPacket(Packets.ShowMesoGain(amount, true));
        }

        //Sets an item to the given inventory in the given slot
        //Returns false if the slot is occupied or the given item's position does not match the given inventory type
        public bool SetItem(MapleItem item, MapleInventoryType inventoryType, short position, bool updateToClient = true)
        {
            if (position == 0)
                return false;
            if (position > 0 && position > GetInventorySize(inventoryType))
                return false;
            if (position < 0)
            {
                if (item.InventoryType != MapleInventoryType.Equip)
                    return false;
                inventoryType = MapleInventoryType.Equipped;
            }

            Dictionary<short, MapleItem> inventory = GetInventory(inventoryType);
            lock (inventory)
            {
                if (inventory.ContainsKey(position)) //slot is occupied
                {
                    return false;
                }
                inventory.Add(position, item);
                item.Position = position;
                if (updateToClient)
                    Owner.Client.SendPacket(Packets.AddItem(item, inventoryType, position));

                item.SaveToDatabase(Owner);
                return true;
            }
        }

        public bool AddItemById(int itemId, string source, short quantity = 1)
        {
            if (quantity < 0)
            {
                return RemoveItemsById(itemId, quantity);
            }
            if (quantity > 0)
            {
                MapleItem item = MapleItemCreator.CreateItem(itemId, source, quantity);
                if (item != null)
                    return AddItem(item, item.InventoryType, true);
            }
            return false;
        }

        //Adds an item to an existing stack or the first available slot, returns true if succeeded
        public bool AddItem(MapleItem newItem, MapleInventoryType inventoryType, bool updateToClient = true)
        {
            if ((byte)inventoryType < 1 || newItem.Quantity < 1) //can't automatically add items to Equipped inventory
                return false;

            if (newItem.InventoryType != MapleInventoryType.Equip)
            {
                var inventory = GetInventory(inventoryType);
                WzItem wzInfo = DataBuffer.GetItemById(newItem.ItemId);
                if (wzInfo == null)
                    return false;
                if (wzInfo.SlotMax > 1) //item can be stacked
                {
                    lock (inventory)
                    {
                        foreach (var kvp in inventory.Where(x => newItem.CanStackWith(x.Value)))
                        {
                            int freeQuantity = (wzInfo.SlotMax - kvp.Value.Quantity);
                            if (freeQuantity > 0)
                            {
                                if (newItem.Quantity <= freeQuantity)
                                {
                                    kvp.Value.Quantity += newItem.Quantity;
                                    if (updateToClient)
                                        Owner.Client.SendPacket(Packets.UpdateItemQuantity(kvp.Value.InventoryType, kvp.Value.Position, kvp.Value.Quantity));
                                    newItem.SaveToDatabase(null);
                                    kvp.Value.SaveToDatabase(Owner);
                                    return true;
                                }
                                if (GetFreeSlotCount(inventoryType) > 0) // The quantity of newItem doesnt fit in the existing one and a part will need to be set in another item or new slot
                                {
                                    kvp.Value.Quantity += (short)freeQuantity; // Add till the slotMax
                                    newItem.Quantity -= (short)freeQuantity; // Remove them from the new item
                                    if (updateToClient)
                                        Owner.Client.SendPacket(Packets.UpdateItemQuantity(inventoryType, kvp.Value.Position, kvp.Value.Quantity));
                                }
                            }
                        }
                    }
                }
            }
            if (newItem.Quantity <= 0)
                return true;
            short slot = GetFirstFreeSlot(inventoryType);
            if (slot > 0)
            {
                return SetItem(newItem, inventoryType, slot, updateToClient);
            }
            if (updateToClient)
                Owner.Client.SendPacket(Packets.ShowInventoryFull());
            return false; //inventory full
        }

        //moves an item from its current slot to a new position
        public bool MoveItem(MapleInventoryType type, short oldPosition, short newPosition, bool updateToClient = true)
        {
            if (newPosition == 0) return false; //Can't drop, use DropItem()
            if (oldPosition < 0) //equipped
            {
                if (newPosition >= 0) return false; //Can't move from equipped to equip, use UnEquipItem()
            }
            else if (newPosition < 0)
            {
                if (oldPosition >= 0) return false; //Can't move from equip to equipped, use EquipItem()
            }
            var inventory = GetInventory(type);
            lock (inventory)
            {
                MapleItem item;
                if (!inventory.TryGetValue(oldPosition, out item)) //item doesnt exist
                    return false;
                if (inventory.ContainsKey(newPosition)) //new slot is occupied, then we swap          
                    return SwapItems(type, oldPosition, newPosition, updateToClient);

                inventory.Remove(oldPosition);
                inventory.Add(newPosition, item);
                item.Position = newPosition;
                item.SaveToDatabase(Owner);
                if (updateToClient)
                    Owner.Client.SendPacket(Packets.MoveItem(type, oldPosition, newPosition));
            }
            return true;
        }

        public bool EquipItem(short oldPosition, short newPosition, bool updateToClient = true)
        {
            lock (EquipInventory)
            {
                MapleItem item;
                if (EquipInventory.TryGetValue(oldPosition, out item))
                {
                    MapleEquip toEquip = item as MapleEquip;
                    WzEquip equipInfo;
                    if (toEquip == null || !CanEquip(Owner, toEquip, newPosition, out equipInfo))
                    {
                        if (updateToClient) Owner.SendPopUpMessage("You cannot equip this");
                        Owner.EnableActions();
                        return false;
                    }
                    lock (EquippedInventory)
                    {
                        EquipInventory.Remove(oldPosition);
                        MapleItem toUnEquip;
                        if (EquippedInventory.TryGetValue(newPosition, out toUnEquip)) // New position is occupied -> they are swapped
                        {
                            EquippedInventory.Remove(newPosition);
                            toUnEquip.Position = oldPosition;
                            EquipInventory.Add(oldPosition, toUnEquip);
                            toUnEquip.SaveToDatabase(Owner);
                        }
                        toEquip.Position = newPosition;
                        EquippedInventory.Add(newPosition, toEquip);
                        toEquip.SaveToDatabase(Owner);
                        List<InventoryOperation> operations = new List<InventoryOperation>();
                        if (equipInfo.EquipTradeBlock && !toEquip.Flags.HasFlag(MapleItemFlags.Untradeable))
                        {
                            toEquip.Flags |= MapleItemFlags.Untradeable;
                            operations.Add(new InventoryOperation(MapleInventoryOperationType.Add, MapleInventoryType.Equip, oldPosition) { Item = toEquip });
                        }
                        operations.Add(new InventoryOperation(MapleInventoryOperationType.Move, MapleInventoryType.Equip, oldPosition) { NewPosition = newPosition });
                        if (updateToClient)
                            Owner.Client.SendPacket(Packets.ShowOperations(operations));
                    }
                }
            }
            Owner.Map.BroadcastPacket(Packets.UpdateCharacterLook(Owner), Owner);
            //Owner.Stats.Recalculate(Owner);
            return true;
        }

        private static bool CanEquip(MapleCharacter chr, MapleEquip equip, short slot, out WzEquip equipInfo)
        {
            equipInfo = DataBuffer.GetEquipById(equip.ItemId);
            if (equipInfo == null) return false;
            if (chr.Level < equipInfo.ReqLevel) return false;
            if (chr.Stats.Str < equipInfo.ReqStr) return false;
            if (chr.Stats.Dex < equipInfo.ReqDex) return false;
            if (chr.Stats.Int < equipInfo.ReqInt) return false;
            if (chr.Stats.Luk < equipInfo.ReqLuk) return false;
            if (chr.Fame < equipInfo.ReqFame) return false;
            if (equipInfo.ReqJob > 0 && (chr.Job % 1000) / 100 != equipInfo.ReqJob) return false;
            MapleItemType itemType = equip.ItemType;
            if (itemType == MapleItemType.DualBowGun)
                if (!chr.IsMercedes) return false;
            if (itemType == MapleItemType.Katana)
                if (!chr.IsHayato) return false;
                else if (itemType == MapleItemType.Fan)
                    if (!chr.IsKanna) return false;
                    else if (itemType == MapleItemType.BigSword || itemType == MapleItemType.LongSword)
                        if (!chr.IsZero) return false;


            //todo: correct slot
            return true;
        }

        public bool UnEquip(short oldPosition, short newPosition, bool updateToClient = true)
        {
            if (newPosition <= 0)
                return false;
            lock (EquippedInventory)
            {
                MapleItem item;
                if (EquippedInventory.TryGetValue(oldPosition, out item))
                {
                    MapleEquip toUnEquip = (MapleEquip)item;
                    lock (EquipInventory)
                    {
                        if (EquipInventory.ContainsKey(newPosition)) //new position is occupied
                            return false;
                        EquippedInventory.Remove(oldPosition);
                        toUnEquip.Position = newPosition;
                        EquipInventory.Add(newPosition, toUnEquip);
                    }
                    item.SaveToDatabase(Owner);
                    if (updateToClient)
                        Owner.Client.SendPacket(Packets.MoveItem(MapleInventoryType.Equip, oldPosition, newPosition));
                }
            }
            Owner.Map.BroadcastPacket(Packets.UpdateCharacterLook(Owner), Owner);
            //Owner.Stats.Recalculate(Owner);
            return true;
        }

        public void ClearInventory(MapleInventoryType inventoryType, MapleClient c)
        {
            List<InventoryOperation> operations = new List<InventoryOperation>();
            var inventory = GetInventory(inventoryType);
            lock (inventory)
            {
                foreach (MapleItem item in inventory.Values.ToList())
                {
                    inventory.Remove(item.Position);
                    item.SaveToDatabase(null);
                    operations.Add(new InventoryOperation(MapleInventoryOperationType.Remove, inventoryType, item.Position));
                }
            }
            c.SendPacket(Packets.ShowOperations(operations));
        }

        public void MergeSlots(MapleInventoryType inventoryType, MapleClient c)
        {
            var inventory = GetInventory(inventoryType);
            List<InventoryOperation> operations = new List<InventoryOperation>();
            lock (inventory)
            {
                Dictionary<MapleItem, int> stackableItems = new Dictionary<MapleItem, int>();
                foreach (MapleItem item in inventory.Values)
                {
                    if (stackableItems.ContainsKey(item)) continue;
                    var wzInfo = DataBuffer.GetItemById(item.ItemId);
                    if (wzInfo != null && wzInfo.SlotMax > 1)
                        stackableItems.Add(item, wzInfo.SlotMax);
                }
                var itemsToStack = stackableItems.Where(x => stackableItems.Count(y => y.Key.CanStackWith(x.Key)) > 1).OrderByDescending(x => x.Key.Position).ToList(); //Loop through from back to front
                for (int i = 0; i < itemsToStack.Count(); i++)
                {
                    int slotMax = itemsToStack[i].Value;
                    MapleItem itemToStack = itemsToStack[i].Key;
                    for (int j = i + 1; j < itemsToStack.Count && itemToStack.Quantity > 0; j++) //Loop through all other stackable items that are in a lower position in the inventory
                    {
                        MapleItem destItem = itemsToStack[j].Key;
                        if (itemToStack.CanStackWith(destItem) && destItem.Quantity < slotMax)
                        {
                            int canAdd = slotMax - destItem.Quantity; //Max quantity that can be added to the destination item
                            int move = Math.Min(itemToStack.Quantity, canAdd); //Move the remaining quantity to reach slotmax or everything if it's less than slotmax
                            itemToStack.Quantity -= (short)move;
                            destItem.Quantity += (short)move;
                            if (itemToStack.Quantity > 0) //Item still has quantity remaining
                            {
                                itemToStack.SaveToDatabase(c.Account.Character);
                                operations.Add(new InventoryOperation(MapleInventoryOperationType.UpdateQuantity, inventoryType, itemToStack.Position) { Quantity = itemToStack.Quantity });
                            }
                            else
                            {
                                RemoveItem(inventoryType, itemToStack.Position, false);
                                operations.Add(new InventoryOperation(MapleInventoryOperationType.Remove, inventoryType, itemToStack.Position));
                            }
                            operations.Add(new InventoryOperation(MapleInventoryOperationType.UpdateQuantity, inventoryType, destItem.Position) { Quantity = destItem.Quantity });
                        }
                    }
                }
            }
            c.SendPacket(Packets.ShowOperations((operations)));
        }

        public void SortItems(MapleInventoryType inventoryType, MapleClient c)
        {
            var inventory = GetInventory(inventoryType);
            List<InventoryOperation> moves = new List<InventoryOperation>();
            lock (inventory)
            {
                var sortedInventory = inventory.Values.OrderBy(x => x.ItemId).ThenByDescending(x => x.Quantity).ToList();
                for (int i = 0; i < sortedInventory.Count; i++)
                {
                    short newPosition = (short)(i + 1); //Position is 1-based
                    MapleItem sortedItem = sortedInventory[i];
                    short oldPosition = sortedItem.Position;
                    if (oldPosition == newPosition) continue;
                    moves.Add(new InventoryOperation(MapleInventoryOperationType.Move, inventoryType, oldPosition) { NewPosition = newPosition });
                    inventory.Remove(oldPosition); //Remove the item from the old position
                    MapleItem itemAtNewPosition;
                    if (inventory.TryGetValue(newPosition, out itemAtNewPosition))
                    {
                        inventory.Remove(newPosition);
                        itemAtNewPosition.Position = oldPosition;
                        inventory.Add(oldPosition, itemAtNewPosition);
                    }
                    sortedItem.Position = newPosition;
                    inventory.Add(newPosition, sortedItem);
                }
            }
            c.SendPacket(Packets.ShowOperations(moves));
        }

        public bool DropItem(MapleInventoryType type, short position, short quantity, bool updateToClient = true)
        {
            if (quantity < -1 || position <= 0) //impossible, must be a cheater
                return false;
            var inventory = GetInventory(type);
            MapleItem item;
            lock (inventory)
            {
                if (!inventory.TryGetValue(position, out item)) return false;
                if (quantity == -1) //drop everything, non stackable item
                {
                    RemoveItem(type, position, updateToClient);
                }
                else if (quantity > 0) //is a stackable item
                {
                    if (item.InventoryType == MapleInventoryType.Equip) //failsafe
                    {
                        RemoveItem(type, position, updateToClient);
                    }
                    else
                    {
                        int totalItemCount = GetItemCount(item.ItemId);
                        if (quantity > totalItemCount)
                            return false;
                        if (totalItemCount == quantity)
                        {
                            RemoveItem(type, position, updateToClient);
                        }
                        else
                        {
                            RemoveItemsFromSlot(type, position, quantity);
                            item = new MapleItem(item, item.Source) { Quantity = quantity };
                        }
                    }
                }
                item.Position = 0;
            }
            WzItem itemInfo = item.Type == 1 ? DataBuffer.GetEquipById(item.ItemId) : DataBuffer.GetItemById(item.ItemId);
            if (itemInfo != null && itemInfo.Tradeable && !item.Flags.HasFlag(MapleItemFlags.Untradeable))
            {
                Owner.Map.SpawnMapItem(item, Owner.Position, Owner.Map.GetDropPositionBelow(new Point(Owner.Position.X, Owner.Position.Y - 50), Owner.Position), true, MapleDropType.FreeForAll, Owner);
            }
            else
            {
                MapleMapItem fakeMapItem = new MapleMapItem(-1, item, Owner.Map.GetDropPositionBelow(new Point(Owner.Position.X, Owner.Position.Y - 50), Owner.Position), Owner.ID, MapleDropType.FreeForAll, true);
                Owner.Map.BroadcastPacket(MapleMapItem.Packets.SpawnMapItem(fakeMapItem, Owner.Position, 3));
            }
            return true;
        }

        //Swaps the positions of two items, make sure both items are checked for existance beforehand! Don't use for equipped items!
        private bool SwapItems(MapleInventoryType type, short pos1, short pos2, bool updateToClient = true)
        {
            var inventory = GetInventory(type);
            lock (inventory)
            {
                MapleItem item1, item2;
                if (!inventory.TryGetValue(pos1, out item1) || !inventory.TryGetValue(pos2, out item2))
                    return false;
                inventory.Remove(pos1);
                inventory.Remove(pos2);
                item1.Position = pos2;
                item2.Position = pos1;
                inventory.Add(pos2, item1);
                inventory.Add(pos1, item2);
                item1.SaveToDatabase(Owner);
                item2.SaveToDatabase(Owner);
            }
            if (updateToClient)
                Owner.Client.SendPacket(Packets.MoveItem(type, pos1, pos2));
            return true;
        }

        //Removes the whole item from inventory, even if it is an item which stacks, so be careful with that!
        //Returns the item if removal was succesful
        public MapleItem RemoveItem(MapleInventoryType type, short pos, bool updateToClient = true)
        {
            if (pos < 0)
                type = MapleInventoryType.Equipped;
            var inventory = GetInventory(type);
            MapleItem item;
            lock (inventory)
            {
                if (inventory.TryGetValue(pos, out item))
                {
                    inventory.Remove(pos);
                    if (updateToClient)
                        Owner.Client.SendPacket(Packets.RemoveItem(type == MapleInventoryType.Equipped ? MapleInventoryType.Equip : type, pos));
                    item.SaveToDatabase(null);
                }
                else
                    return null;
            }
            if (type == MapleInventoryType.Equipped)
            {
                //Owner.Stats.Recalculate(Owner);
            }
            return item;
        }

        //Removes several items at once, can both completely remove an item and also take a certain quantity
        //Be sure to check the total item quantity before calling this otherwise it might not remove the full desired amount
        public bool RemoveItemsById(int itemId, int amount, bool removeWhenQuantityZero = true, bool updateToClient = true)
        {
            MapleInventoryType type = ItemConstants.GetInventoryType(itemId);
            var inventory = GetInventory(type);
            int amountLeft = amount;

            lock (inventory)
            {
                foreach (var kvp in inventory.Where(x => x.Value.ItemId == itemId).OrderBy(kvp => kvp.Key))
                {
                    if (amountLeft > 0)
                    {
                        if (kvp.Value.Quantity > amountLeft || !removeWhenQuantityZero) //some of the item's stack is left after removing
                        {
                            kvp.Value.Quantity -= (short)amountLeft;
                            if (kvp.Value.Quantity < 0)
                                kvp.Value.Quantity = 0;
                            if (updateToClient)
                                Owner.Client.SendPacket(Packets.UpdateItemQuantity(type, kvp.Key, kvp.Value.Quantity));
                            kvp.Value.SaveToDatabase(Owner);
                            return true;
                        }
                        if (kvp.Value.Quantity < amountLeft) //the whole stack is removed and needs more removal from another stack
                        {
                            amountLeft -= kvp.Value.Quantity;
                            RemoveItem(type, kvp.Key, updateToClient);
                        }
                        else //only other possibility is amountLeft == quantity, so remove the whole item
                        {
                            kvp.Value.Quantity = 0;
                            RemoveItem(type, kvp.Key, updateToClient);
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //removes a number of items from the character's inventory, but only from the specified slot.
        //returns false if it fails.
        public bool RemoveItemsFromSlot(MapleInventoryType type, short pos, short count, bool updateToClient = true)
        {
            var inventory = GetInventory(type);

            lock (inventory)
            {
                MapleItem item;
                if (!inventory.TryGetValue(pos, out item)) return false;
                if (item.Quantity < count) return false;
                if (item.Quantity > count) //Some of the item's stack is left
                {
                    item.Quantity -= count;
                    if (updateToClient)
                        Owner.Client.SendPacket(Packets.UpdateItemQuantity(type, pos, item.Quantity));
                    item.SaveToDatabase(Owner);
                    return true;
                }
                //Item.Quantity == count:
                item.Quantity = 0;
                RemoveItem(type, pos, updateToClient);
                return true;
            }
        }
       
        public int GetFreeSlotCount(MapleInventoryType type)
        {
            if (type == MapleInventoryType.Equipped || type == MapleInventoryType.Undefined)
                return 0;
            short size = GetInventorySize(type);
            var inventory = GetInventory(type);
            lock (inventory)
            {
                return Math.Max(0, size - inventory.Count);
            }
        }

        public short GetFirstFreeSlot(MapleInventoryType type)
        {
            var inventory = GetInventory(type);
            lock (inventory)
            {
                for (int i = 1; i <= GetInventorySize(type); i++)
                {
                    if (!inventory.ContainsKey((short)i))
                    {
                        return (short)i;
                    }
                }
            }
            return -1;
        }

        public int GetItemCount(int itemId)
        {
            int count = 0;
            MapleInventoryType type = ItemConstants.GetInventoryType(itemId);
            var inventory = GetInventory(type);
            lock (inventory)
            {
                foreach (var kvp in inventory.Values.Where(x => x.ItemId == itemId))
                {
                    count += kvp.Quantity;
                }
            }
            return count;
        }

        public Dictionary<short, MapleItem> GetInventoryThreadSafe(MapleInventoryType inventoryType) //this should be used as less as possible
        {
            switch (inventoryType)
            {
                case MapleInventoryType.Equipped:
                    lock (EquippedInventory)
                    {
                        return EquippedInventory.ToDictionary(x => x.Key, x => x.Value);
                    }
                case MapleInventoryType.Equip:
                    lock (EquipInventory)
                    {
                        return EquipInventory.ToDictionary(x => x.Key, x => x.Value);
                    }
                case MapleInventoryType.Use:
                    lock (UseInventory)
                    {
                        return UseInventory.ToDictionary(x => x.Key, x => x.Value);
                    }
                case MapleInventoryType.Setup:
                    lock (SetupInventory)
                    {
                        return SetupInventory.ToDictionary(x => x.Key, x => x.Value);
                    }
                case MapleInventoryType.Etc:
                    lock (EtcInventory)
                    {
                        return EtcInventory.ToDictionary(x => x.Key, x => x.Value);
                    }
                case MapleInventoryType.Cash:
                    lock (CashInventory)
                    {
                        return CashInventory.ToDictionary(x => x.Key, x => x.Value);
                    }
                default:
                    return new Dictionary<short, MapleItem>();
            }
        }

        public short GetInventorySize(MapleInventoryType inventoryType)
        {
            switch (inventoryType)
            {
                case MapleInventoryType.Equip:
                    return EquipSlots;
                case MapleInventoryType.Use:
                    return UseSlots;
                case MapleInventoryType.Setup:
                    return SetupSlots;
                case MapleInventoryType.Etc:
                    return EtcSlots;
                case MapleInventoryType.Cash:
                    return CashSlots;
                default:
                    return 0;
            }
        }

        public List<MapleItem> GetAllItems()
        {
            List<MapleItem> allItems = new List<MapleItem>();
            lock (EquippedInventory)
            {
                allItems.AddRange(EquippedInventory.Values);
            }
            lock (EquipInventory)
            {
                allItems.AddRange(EquipInventory.Values);
            }
            lock (UseInventory)
            {
                allItems.AddRange(UseInventory.Values);
            }
            lock (SetupInventory)
            {
                allItems.AddRange(SetupInventory.Values);
            }
            lock (EtcInventory)
            {
                allItems.AddRange(EtcInventory.Values);
            }
            lock (CashInventory)
            {
                allItems.AddRange(CashInventory.Values);
            }
            return allItems;
        }

        public bool HasItem(int itemId, int quantity = 1)
        {
            return GetItemCount(itemId) >= quantity;
        }
        public MapleItem GetEquippedItem(short position)
        {
            MapleItem item;
            lock (EquippedInventory)
            {
                if (EquippedInventory.TryGetValue(position, out item))
                    return item;
                else
                    return null;
            }
        }
        public MapleItem GetItemSlotFromInventory(MapleInventoryType type, short itemSlot)
        {
            if (type == MapleInventoryType.Equip && itemSlot < 0)
                type = MapleInventoryType.Equipped; ;
            var inventory = GetInventory(type);
            lock (inventory)
            {
                MapleItem item;
                if (inventory.TryGetValue(itemSlot, out item))
                    return item;
            }
            return null;
        }
        public List<MapleItem> GetItemsFromInventory(MapleInventoryType inventoryType)
        {
            var inventory = GetInventory(inventoryType);
            lock (inventory)
            {
                return inventory.Values.ToList();
            }
        }
        public List<MapleItem> GetItemsFromInventory(MapleInventoryType type, Func<MapleItem, bool> condition)
        {
            var inventory = GetInventory(type);
            lock (inventory)
            {
                return inventory.Values.Where(condition).ToList();
            }
        }
        public List<MapleEquip> GetEquipsWithRevealedPotential()
        {
            List<MapleEquip> ret = new List<MapleEquip>();
            lock (EquippedInventory)
            {
                ret.AddRange(EquippedInventory.Values.Select(x => x as MapleEquip).Where(x => x.PotentialState >= MaplePotentialState.Rare));
            }
            lock (EquipInventory)
            {
                ret.AddRange(EquipInventory.Values.Select(x => x as MapleEquip).Where(x => x.PotentialState >= MaplePotentialState.Rare));
            }
            return ret;
        }

        #region Database
        public static MapleInventory LoadFromDatabase(MapleCharacter chr)
        {
            MapleInventory inventory = new MapleInventory(chr);
            int characterId = chr.ID;

            #region Items
            List<InventoryItem> dbInventoryItems;
            using (var dbContext = new MapleDbContext())
            {
                dbInventoryItems = dbContext.InventoryItems.Where(x => x.CharacterID == characterId).ToList();
            }
            foreach (InventoryItem dbInventoryItem in dbInventoryItems)
            {
                MapleInventoryType type = ItemConstants.GetInventoryType(dbInventoryItem.ItemID);
                MapleItem item;
                if (type == MapleInventoryType.Equip)
                {
                    MapleEquip equip = new MapleEquip(dbInventoryItem.ItemID, dbInventoryItem.Source, dbInventoryItem.Creator, (MapleItemFlags)dbInventoryItem.Flags, dbInventoryItem.Position, dbInventoryItem.ID); InventoryEquip dbInventoryEquip;
                    using (var dbContext = new MapleDbContext())
                    {
                        dbInventoryEquip = dbContext.InventoryEquips.FirstOrDefault(x => x.InventoryItemID == dbInventoryItem.ID);
                    }
                    if (dbInventoryEquip != null)
                    {
                        equip.DbId = dbInventoryItem.ID;
                        equip.RemainingUpgradeCount = dbInventoryEquip.RemainingUpgradeCount;
                        equip.UpgradeCount = dbInventoryEquip.UpgradeCount;
                        equip.Str = dbInventoryEquip.Str;
                        equip.Dex = dbInventoryEquip.Dex;
                        equip.Int = dbInventoryEquip.Int;
                        equip.Luk = dbInventoryEquip.Luk;
                        equip.IncMhp = dbInventoryEquip.IncMaxHP;
                        equip.IncMmp = dbInventoryEquip.IncMaxMP;
                        equip.Pad = dbInventoryEquip.Pad;
                        equip.Mad = dbInventoryEquip.Mad;
                        equip.Pdd = dbInventoryEquip.Pdd;
                        equip.Mdd = dbInventoryEquip.Mdd;
                        equip.Acc = dbInventoryEquip.Acc;
                        equip.Eva = dbInventoryEquip.Eva;
                        equip.Speed = dbInventoryEquip.Speed;
                        equip.Jump = dbInventoryEquip.Jump;
                        equip.Diligence = dbInventoryEquip.Diligence;
                        equip.Durability = dbInventoryEquip.Durability;
                        equip.Enhancements = dbInventoryEquip.Enhancements;
                        equip.PotentialState = (MaplePotentialState)dbInventoryEquip.PotentialState;
                        equip.Potential1 = (ushort)dbInventoryEquip.Potential1;
                        equip.Potential2 = (ushort)dbInventoryEquip.Potential2;
                        equip.Potential3 = (ushort)dbInventoryEquip.Potential3;
                        equip.BonusPotential1 = (ushort)dbInventoryEquip.BonusPotential1;
                        equip.BonusPotential2 = (ushort)dbInventoryEquip.BonusPotential2;
                        equip.Socket1 = dbInventoryEquip.Socket1;
                        equip.Socket2 = dbInventoryEquip.Socket2;
                        equip.Socket3 = dbInventoryEquip.Socket3;
                        equip.CustomLevel = dbInventoryEquip.CustomLevel;
                        equip.CustomExp = dbInventoryEquip.CustomExp;
                        equip.HammersApplied = dbInventoryEquip.HammerApplied;
                    }
                    item = equip;
                }
                else
                {
                    item = new MapleItem(dbInventoryItem.ItemID, dbInventoryItem.Source, dbInventoryItem.Quantity, dbInventoryItem.Creator, (MapleItemFlags)dbInventoryItem.Flags, dbInventoryItem.Position, dbInventoryItem.ID);
                }
                inventory.SetItem(item, item.InventoryType, item.Position, false);
            }
            #endregion

            #region Slots
            InventorySlot dbInventorySlot;
            using (var dbContext = new MapleDbContext())
            {
                dbInventorySlot = dbContext.InventorySlots.SingleOrDefault(x => x.CharacterID == characterId);
            }
            if (dbInventorySlot != null)
            {
                inventory.EquipSlots = dbInventorySlot.EquipSlots;
                inventory.UseSlots = dbInventorySlot.UseSlots;
                inventory.SetupSlots = dbInventorySlot.SetupSlots;
                inventory.EtcSlots = dbInventorySlot.EtcSlots;
                inventory.CashSlots = dbInventorySlot.CashSlots;
            }
            #endregion

            return inventory;
        }

        public void SaveToDatabase(bool insert = false)
        {
            List<MapleItem> inventoryItems = GetAllItems();
            foreach (MapleItem item in inventoryItems)
            {
                item.SaveToDatabase(Owner);
            }
            #region Slots
            using (var dbContext = new MapleDbContext())
            {
                InventorySlot updateSlot = dbContext.InventorySlots.FirstOrDefault(x => x.CharacterID == Owner.ID);
                if (updateSlot != null && !insert)
                {
                    updateSlot.EquipSlots = EquipSlots;
                    updateSlot.UseSlots = UseSlots;
                    updateSlot.SetupSlots = SetupSlots;
                    updateSlot.EtcSlots = EtcSlots;
                    updateSlot.CashSlots = CashSlots;
                }
                else
                {
                    InventorySlot insertSlot = new InventorySlot();
                    insertSlot.CharacterID = Owner.ID;
                    insertSlot.EquipSlots = EquipSlots;
                    insertSlot.UseSlots = UseSlots;
                    insertSlot.SetupSlots = SetupSlots;
                    insertSlot.EtcSlots = EtcSlots;
                    insertSlot.CashSlots = CashSlots;
                    dbContext.InventorySlots.Add(insertSlot);
                }

                dbContext.SaveChanges();
            }

            #endregion
        }
        #endregion

        public static void UpdateMesos(MapleClient c, int mesos)
        {
            MapleCharacter.UpdateSingleStat(c, MapleCharacterStat.Meso, mesos);
        }

        public class InventoryOperation
        {
            public MapleInventoryOperationType OperationType;
            public MapleInventoryType InventoryType;
            public short ItemPosition;
            public short NewPosition;
            public short Quantity;
            public MapleItem Item;

            public InventoryOperation(MapleInventoryOperationType operationType, MapleInventoryType inventoryType, short position)
            {
                OperationType = operationType;
                InventoryType = inventoryType;
                ItemPosition = position;
            }
        }


        public static class Packets
        {
            public static PacketWriter UpdateItemQuantity(MapleInventoryType inventoryType, short position, short quantity)
            {
                return ShowOperations(new List<InventoryOperation> { new InventoryOperation(MapleInventoryOperationType.UpdateQuantity, inventoryType, position) { Quantity = quantity } });
            }

            //Completely removes the item from inventory
            public static PacketWriter RemoveItem(MapleInventoryType inventoryType, short position)
            {
                return ShowOperations(new List<InventoryOperation> { new InventoryOperation(MapleInventoryOperationType.Remove, inventoryType, position) });
            }

            public static PacketWriter MoveItem(MapleInventoryType type, short oldPosition, short newPosition)
            {
                return ShowOperations(new List<InventoryOperation> { new InventoryOperation(MapleInventoryOperationType.Move, type, oldPosition) { NewPosition = newPosition } });
            }

            public static PacketWriter AddItem(MapleItem item, MapleInventoryType inventoryType, short position)
            {
                return ShowOperations(new List<InventoryOperation> { new InventoryOperation(MapleInventoryOperationType.Add, inventoryType, position) { Item = item } });
            }

            //Client automatically swaps the items if the new position has an item in it
            public static PacketWriter ShowOperations(List<InventoryOperation> operations)
            {
                
                var pw = new PacketWriter((ushort)SMSGHeader.INVENTORY_OPERATION);
                pw.WriteBool(true); //enable actions
                pw.WriteShort((short)operations.Count);

                foreach (InventoryOperation operation in operations)
                {
                    pw.WriteByte((byte)operation.OperationType);
                    pw.WriteByte((byte)operation.InventoryType);
                    switch (operation.OperationType)
                    {
                        case MapleInventoryOperationType.Add:
                            pw.WriteShort(operation.ItemPosition);
                            MapleItem.AddItemInfo(pw, operation.Item);
                            break;
                        case MapleInventoryOperationType.UpdateQuantity:
                            pw.WriteShort(operation.ItemPosition);
                            pw.WriteShort(operation.Quantity);
                            break;
                        case MapleInventoryOperationType.Move:
                            pw.WriteShort(operation.ItemPosition);
                            pw.WriteShort(operation.NewPosition);
                            if (operation.ItemPosition < 0 || operation.NewPosition < 0) //from or to equipped inventory
                                pw.WriteByte(0); //increments every time an equipped inventory operation is done in gMS
                            break;
                        case MapleInventoryOperationType.Remove:
                            pw.WriteShort(operation.ItemPosition);
                            if (operation.ItemPosition < 0) //from equipped inventory
                                pw.WriteByte(0); //increments every time an equipped inventory operation is done in gMS
                            break;
                    }
                }
                return pw;
            }

            public static PacketWriter ShowMesoGain(int amount, bool inChat)
            {
                
                var pw = new PacketWriter((ushort)SMSGHeader.SHOW_STATUS_INFO);
                if (inChat)
                {
                    pw.WriteByte(6);
                    pw.WriteInt(amount);
                    pw.WriteInt(-1);
                }
                else
                {
                    pw.WriteByte(0);
                    pw.WriteByte(1);
                    pw.WriteByte(0);
                    pw.WriteInt(amount);
                    pw.WriteShort(0);
                    pw.WriteShort(0); //new v158
                }
                return pw;
            }

            public static PacketWriter ShowItemGain(int itemId, int quantity, bool inChat = false)
            {
                
                var pw = new PacketWriter((ushort)SMSGHeader.SHOW_STATUS_INFO);
                pw.WriteByte(0);
                pw.WriteByte(0);
                pw.WriteInt(itemId);
                pw.WriteInt(quantity);
                return pw;
            }

            public static PacketWriter ShowInventoryFull()
            {
                
                var pw = new PacketWriter((ushort)SMSGHeader.INVENTORY_OPERATION);
                pw.WriteByte(1);
                pw.WriteByte(0);
                pw.WriteByte(0);
                return pw;
            }

            public static void AddInventoryInfo(PacketWriter pw, MapleInventory inventory)
            {
                pw.WriteLong(inventory.Mesos); //long in v137 //pw.WriteLong(long.MaxValue); trolololo will go outside the meso box coz too many digits (max is 9,999,999,999)          
                pw.WriteInt(0);
                pw.WriteInt(inventory.Owner.ID);

                for (int i = 0; i < 7; i++)
                {
                    pw.WriteInt(0); //new v137, probably bit inventory
                }
                pw.WriteZeroBytes(3);

                pw.WriteByte(inventory.EquipSlots);
                pw.WriteByte(inventory.UseSlots);
                pw.WriteByte(inventory.SetupSlots);
                pw.WriteByte(inventory.EtcSlots);
                pw.WriteByte(inventory.CashSlots);

                pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(-2L)); //TODO quest 122700 timestamp

                pw.WriteByte(0); // new v148

                //Equipped items
                Dictionary<short, MapleItem> equipped = inventory.GetInventory(MapleInventoryType.Equipped);
                foreach (var kvp in equipped.Where(kvp => kvp.Value.Position > -100 && kvp.Value.Position < 0))
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteShort(0); //1

                //Masked equipped items, Cash Shop stuff I think
                foreach (var kvp in equipped.Where(kvp => kvp.Value.Position > -1000 && kvp.Value.Position < -100))
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteShort(0); //2

                //Equip inventory
                Dictionary<short, MapleItem> equipInventory = inventory.GetInventory(MapleInventoryType.Equip);
                foreach (KeyValuePair<short, MapleItem> kvp in equipInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteShort(0); //3

                // Masked equipped pos <= -1000 && > -1100
                pw.WriteShort(0); //4

                // Masked equipped pos <= -1100 && > -1200
                pw.WriteShort(0); //5

                // Masked equipped pos <= 1200
                pw.WriteShort(0); //6

                // ?
                pw.WriteShort(0); //7

                // ?
                pw.WriteShort(0); //8

                //Totem slots, pos 5000
                pw.WriteShort(0); //9

                pw.WriteShort(0); //10 added v137

                pw.WriteShort(0); //11 added v137

                pw.WriteShort(0); //12 added v137

                pw.WriteShort(0); //13 added v142

                pw.WriteShort(0); //14 added v143

                pw.WriteShort(0); //15 added v143

                Dictionary<short, MapleItem> useInventory = inventory.GetInventory(MapleInventoryType.Use);
                foreach (KeyValuePair<short, MapleItem> kvp in useInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteByte(0); //16  

                Dictionary<short, MapleItem> setupInventory = inventory.GetInventory(MapleInventoryType.Setup);
                foreach (KeyValuePair<short, MapleItem> kvp in setupInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteByte(0); //17           

                Dictionary<short, MapleItem> etcInventory = inventory.GetInventory(MapleInventoryType.Etc);
                foreach (KeyValuePair<short, MapleItem> kvp in etcInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteByte(0); //18

                Dictionary<short, MapleItem> cashInventory = inventory.GetInventory(MapleInventoryType.Cash);
                foreach (KeyValuePair<short, MapleItem> kvp in cashInventory)
                {
                    MapleItem.AddItemPosition(pw, kvp.Value);
                    MapleItem.AddItemInfo(pw, kvp.Value);
                }
                pw.WriteByte(0); //19       

                //TODO: extended slots
                pw.WriteInt(0);
                pw.WriteInt(0);
                pw.WriteInt(0);
                pw.WriteInt(0);
                pw.WriteByte(0);
            }

            public static PacketWriter UpdateCharacterLook(MapleCharacter chr)
            {
                
                var pw = new PacketWriter((ushort)SMSGHeader.UPDATE_CHAR_LOOK);
                pw.WriteInt(chr.ID);
                pw.WriteByte(1);
                MapleCharacter.AddCharLook(pw, chr, false);
                pw.WriteShort(0); //TODO: RINGS
                pw.WriteShort(0); //TODO: RINGS
                pw.WriteShort(0); //TODO: MRINGINFO
                pw.WriteInt(0);
                pw.WriteByte(0);
                return pw;
            }
        }

        public Dictionary<short, MapleItem> GetInventory(MapleInventoryType inventoryType)
        {
            switch (inventoryType)
            {
                case MapleInventoryType.Equipped:
                    return EquippedInventory;
                case MapleInventoryType.Equip:
                    return EquipInventory;
                case MapleInventoryType.Use:
                    return UseInventory;
                case MapleInventoryType.Setup:
                    return SetupInventory;
                case MapleInventoryType.Etc:
                    return EtcInventory;
                case MapleInventoryType.Cash:
                    return CashInventory;
                default:
                    return new Dictionary<short, MapleItem>();
            }
        }
    }



    public enum MapleInventoryType : sbyte
    {
        Equipped = -1,
        Undefined = 0,
        Equip = 1,
        Use = 2,
        Setup = 3,
        Etc = 4,
        Cash = 5
    }

    public enum MapleInventoryOperationType : byte
    {
        Add = 0,
        UpdateQuantity = 1,
        Move = 2,
        Remove = 3
    }

    public enum MapleEquipPosition : short
    {
        Hat = -1,
        FaceAcc = -2,
        EyeAcc = -3,
        Earings = -4,
        Top = -5,
        Bottom = -6,
        Shoes = -7,
        Gloves = -8,
        Cape = -9,
        SecondWeapon = -10,
        Weapon = -11
    }
}