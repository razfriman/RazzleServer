using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterItems : IEnumerable<Item>
    {
        public Character Parent { get; }
        public Dictionary<ItemType, byte> MaxSlots { get; }
        private List<Item> Items { get; }

        public int Count => Items.Count;

        public CharacterItems(Character parent, byte equipmentSlots, byte usableSlots, byte setupSlots,
            byte etceteraSlots, byte cashSlots)
        {
            Parent = parent;

            MaxSlots = new Dictionary<ItemType, byte>(Enum.GetValues(typeof(ItemType)).Length)
            {
                {ItemType.Equipment, equipmentSlots},
                {ItemType.Usable, usableSlots},
                {ItemType.Setup, setupSlots},
                {ItemType.Etcetera, etceteraSlots},
                {ItemType.Pet, cashSlots}
            };


            Items = new List<Item>();
        }

        public void Load()
        {
            using var dbContext = new MapleDbContext();
            var itemsEntities = dbContext.Items
                .Where(x => x.CharacterId == Parent.Id)
                .Where(x => x.AccountId == Parent.AccountId)
                .ToArray();

            foreach (var itemEntity in itemsEntities)
            {
                var item = new Item(itemEntity);

                Add(item);

                if (item.PetId != null)
                {
                    Parent.Pets.Add(new Pet(item));
                }
            }
        }

        public void Save()
        {
            foreach (var item in this)
            {
                item.Save();
            }
        }

        public void Add(Item item, bool fromDrop = false, bool autoMerge = true, bool forceGetSlot = false)
        {
            if (autoMerge && Available(item.MapleId) % item.MaxPerStack != 0)
            {
                foreach (var loopItem in this.Where(x => x.MapleId == item.MapleId && x.Quantity < x.MaxPerStack))
                {
                    if (loopItem.Quantity + item.Quantity <= loopItem.MaxPerStack)
                    {
                        loopItem.Quantity += item.Quantity;
                        loopItem.Update();

                        item.Quantity = 0;

                        break;
                    }

                    item.Quantity -= (short)(loopItem.MaxPerStack - loopItem.Quantity);
                    item.Slot = GetNextFreeSlot(item.Type);

                    loopItem.Quantity = loopItem.MaxPerStack;

                    if (Parent.IsInitialized)
                    {
                        loopItem.Update();
                    }

                    break;
                }
            }

            if (item.Quantity > 0)
            {
                item.Parent = this;

                if (item.IsStored)
                {
                    item.Slot = (short)Items.Count;
                }
                else if (Parent.IsInitialized && item.Slot == 0 || forceGetSlot)
                {
                    item.Slot = GetNextFreeSlot(item.Type);
                }

                Items.Add(item);

                if (Parent.IsInitialized && !item.IsStored)
                {
                    using var pw = new PacketWriter(ServerOperationCode.InventoryOperation);
                    pw.WriteBool(fromDrop);
                    pw.WriteByte(1);
                    pw.WriteByte(InventoryOperationType.AddItem);
                    pw.WriteByte(item.Type);
                    pw.WriteShort(item.Slot);
                    pw.WriteBytes(item.ToByteArray(true));

                    Parent.Send(pw);
                }
            }
        }

        public void AddRange(IEnumerable<Item> items, bool fromDrop = false, bool autoMerge = true)
        {
            foreach (var loopItem in items)
            {
                Add(loopItem, fromDrop, autoMerge);
            }
        }

        public void Remove(int mapleId, short quantity)
        {
            var leftToRemove = quantity;

            var toRemove = new List<Item>();

            foreach (var loopItem in this)
            {
                if (loopItem.MapleId == mapleId)
                {
                    if (loopItem.Quantity > leftToRemove)
                    {
                        loopItem.Quantity -= leftToRemove;
                        loopItem.Update();

                        break;
                    }

                    leftToRemove -= loopItem.Quantity;
                    toRemove.Add(loopItem);
                }
            }

            foreach (var loopItem in toRemove)
            {
                Remove(loopItem, true);
            }
        }

        public void Remove(Item item, bool removeFromSlot, bool fromDrop = false)
        {
            if (removeFromSlot && item.IsEquipped)
            {
                throw new InvalidOperationException("Cannot remove equipped items from slot.");
            }

            if (removeFromSlot)
            {
                using var pw = new PacketWriter(ServerOperationCode.InventoryOperation);
                pw.WriteBool(fromDrop);
                pw.WriteByte(1);
                pw.WriteByte(InventoryOperationType.RemoveItem);
                pw.WriteByte(item.Type);
                pw.WriteShort(item.Slot);

                Parent.Send(pw);
            }

            if (item.Assigned)
            {
                item.Delete();
            }

            item.Parent = null;

            var wasEquipped = item.IsEquipped;

            Items.Remove(item);

            if (wasEquipped)
            {
                Parent.PrimaryStats.UpdateApperance();
            }

            Parent.PrimaryStats.EquipBonuses = CalculateStatBonus();
            Parent.PrimaryStats.Update();
        }

        public void Clear(bool removeFromSlot)
        {
            var toRemove = this.ToList();

            foreach (var loopItem in toRemove)
            {
                if (!(loopItem.IsEquipped && removeFromSlot))
                {
                    Remove(loopItem, removeFromSlot);
                }
            }
        }

        public bool Contains(int mapleId) => this.Any(loopItem => loopItem.MapleId == mapleId);

        public bool Contains(int mapleId, short quantity)
        {
            var count = this.Where(loopItem => loopItem.MapleId == mapleId)
                .Aggregate(0, (current, loopItem) => current + loopItem.Quantity);

            return count >= quantity;
        }

        public int Available(int mapleId) => this.Where(loopItem => loopItem.MapleId == mapleId)
            .Aggregate(0, (current, loopItem) => current + loopItem.Quantity);

        public sbyte GetNextFreeSlot(ItemType type)
        {
            for (sbyte i = 1; i <= MaxSlots[type]; i++)
            {
                if (this[type, i] == null)
                {
                    return i;
                }
            }

            throw new InventoryFullException();
        }

        public void NotifyFull()
        {
        }

        public bool IsFull(ItemType type)
        {
            short count = 0;

            foreach (var item in this)
            {
                if (item.Type == type)
                {
                    count++;
                }
            }

            return count == MaxSlots[type];
        }

        public int RemainingSlots(ItemType type)
        {
            short remaining = MaxSlots[type];

            foreach (var item in this)
            {
                if (item.Type == type)
                {
                    remaining--;
                }
            }

            return remaining;
        }

        public void Pickup(Drop drop)
        {
            if (drop.Picker != null)
            {
                // Someone already picked up this drop
                return;
            }

            drop.Picker = Parent;

            switch (drop)
            {
                case Meso meso when Parent.PrimaryStats.Meso != int.MaxValue:
                    Parent.PrimaryStats.Meso += meso.Amount;
                    break;
                case Item item when item.OnlyOne:
                    // TODO: Appropriate message.
                    return;
                case Item item when IsFull(item.Type):
                    NotifyFull();
                    break;
                case Item item when !IsFull(item.Type):
                    item.Slot = GetNextFreeSlot(item.Type);
                    Add(item, true);
                    break;
            }

            Parent.Map?.Drops.Remove(drop);
            drop.Picker.Send(drop.GetShowGainPacket());
        }

        public Item this[ItemType type, short slot]
        {
            get => this.FirstOrDefault(item => item.Type == type && item.Slot == slot);
        }

        public Item this[EquipmentSlot slot]
        {
            get => this.FirstOrDefault(item => item.Slot == (sbyte)slot);
        }

        public Item this[int mapleId, short slot]
        {
            get
            {
                return this.FirstOrDefault(item => item.Slot == slot && item.Type == Item.GetType(mapleId));
            }
        }

        public IEnumerable<Item> this[ItemType type]
        {
            get => Items.Where(loopItem => loopItem.Type == type && !loopItem.IsEquipped);
        }

        public IEnumerable<Item> GetStored() => Items.Where(loopItem => loopItem.IsStored);

        public IEnumerable<Item> GetEquipped(EquippedQueryMode mode = EquippedQueryMode.Any)
        {
            foreach (var loopItem in Items)
            {
                if (loopItem.IsEquipped)
                {
                    switch (mode)
                    {
                        case EquippedQueryMode.Any:
                            yield return loopItem;
                            break;

                        case EquippedQueryMode.Normal:
                            if (loopItem.Slot > -100)
                            {
                                yield return loopItem;
                            }

                            break;

                        case EquippedQueryMode.Cash:
                            if (loopItem.Slot < -100)
                            {
                                yield return loopItem;
                            }

                            break;
                    }
                }
            }
        }

        public int SpaceTakenBy(Item item, bool autoMerge = true)
        {
            if (item.Quantity < 0)
            {
                return 0;
            }

            if (Available(item.MapleId) % item.MaxPerStack != 0 && autoMerge)
            {
                foreach (var loopItem in this.Where(x => x.MapleId == item.MapleId && x.Quantity < x.MaxPerStack))
                {
                    return loopItem.Quantity + item.Quantity <= loopItem.MaxPerStack ? 0 : 1;
                }

                return 1;
            }

            return 1;
        }

        public bool CouldReceive(IEnumerable<Item> items, bool autoMerge = true)
        {
            var spaceCount = new Dictionary<ItemType, int>(5);
            {
                spaceCount.Add(ItemType.Equipment, 0);
                spaceCount.Add(ItemType.Usable, 0);
                spaceCount.Add(ItemType.Setup, 0);
                spaceCount.Add(ItemType.Etcetera, 0);
                spaceCount.Add(ItemType.Pet, 0);
            }

            foreach (var loopItem in items)
            {
                spaceCount[loopItem.Type] += SpaceTakenBy(loopItem, autoMerge);
            }

            foreach (var (slot, slotCount) in spaceCount)
            {
                if (RemainingSlots(slot) < slotCount)
                {
                    return false;
                }
            }

            return true;
        }

        public StatBonus CalculateStatBonus()
        {
            var stats = new StatBonus();

            foreach (var item in GetEquipped(EquippedQueryMode.Normal))
            {
                stats.MaxMana += item.Mana;
                stats.MaxHealth += item.Health;
                stats.Strength += item.Strength;
                stats.Dexterity += item.Dexterity;
                stats.Intelligence += item.Intelligence;
                stats.Luck += item.Luck;
                stats.Speed += item.Speed;
            }

            return stats;
        }

        public (Dictionary<byte, int> visibleLayer, Dictionary<byte, int> hiddenLayer) CalculateEquippedSlots()
        {
            var visibleLayer = new Dictionary<byte, int>();
            var hiddenLayer = new Dictionary<byte, int>();

            foreach (var item in GetEquipped())
            {
                var slot = item.AbsoluteSlot;

                if (slot < 100 && !visibleLayer.ContainsKey(slot))
                {
                    visibleLayer[slot] = item.MapleId;
                }
                else if (slot > 100 && slot != 111)
                {
                    slot -= 100;

                    if (visibleLayer.ContainsKey(slot))
                    {
                        hiddenLayer[slot] = visibleLayer[slot];
                    }

                    visibleLayer[slot] = item.MapleId;
                }
                else if (visibleLayer.ContainsKey(slot))
                {
                    hiddenLayer[slot] = item.MapleId;
                }
            }

            return (visibleLayer, hiddenLayer);
        }

        public byte[] ToByteArray(CharacterDataFlags flags = CharacterDataFlags.All)
        {
            using var pw = new PacketWriter();

            if (flags.HasFlag(CharacterDataFlags.Equipment))
            {
                foreach (var item in GetEquipped(EquippedQueryMode.Normal))
                {
                    pw.WriteBytes(item.ToByteArray());
                }

                pw.WriteByte(0);

                foreach (var item in GetEquipped(EquippedQueryMode.Cash))
                {
                    pw.WriteBytes(item.ToByteArray());
                }

                pw.WriteByte(0);
            }

            if (flags.HasFlag(CharacterDataFlags.Equipment))
            {
                pw.WriteByte(MaxSlots[ItemType.Equipment]);
                foreach (var item in this[ItemType.Equipment])
                {
                    pw.WriteBytes(item.ToByteArray());
                }

                pw.WriteByte(0);
            }

            if (flags.HasFlag(CharacterDataFlags.Usable))
            {
                pw.WriteByte(MaxSlots[ItemType.Usable]);
                foreach (var item in this[ItemType.Usable])
                {
                    pw.WriteBytes(item.ToByteArray());
                }

                pw.WriteByte(0);
            }

            if (flags.HasFlag(CharacterDataFlags.Setup))
            {
                pw.WriteByte(MaxSlots[ItemType.Setup]);
                foreach (var item in this[ItemType.Setup])
                {
                    pw.WriteBytes(item.ToByteArray());
                }

                pw.WriteByte(0);
            }

            if (flags.HasFlag(CharacterDataFlags.Etcetera))
            {
                pw.WriteByte(MaxSlots[ItemType.Etcetera]);
                foreach (var item in this[ItemType.Etcetera])
                {
                    pw.WriteBytes(item.ToByteArray());
                }

                pw.WriteByte(0);
            }

            if (flags.HasFlag(CharacterDataFlags.Pet))
            {
                pw.WriteByte(MaxSlots[ItemType.Pet]);
                foreach (var item in this[ItemType.Pet])
                {
                    pw.WriteBytes(item.ToByteArray());
                }

                pw.WriteByte(0);
            }


            return pw.ToArray();
        }

        public IEnumerator<Item> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Items).GetEnumerator();
    }
}
