using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Exceptions;
using RazzleServer.Common.Maple;
using RazzleServer.Data;
using RazzleServer.Net.Packet;

namespace RazzleServer.Server.Maple
{
    public class BasicCharacterItems
    {
        public ICharacter Parent { get; }
        public Dictionary<ItemType, byte> MaxSlots { get; }
        private List<BasicItem> Items { get; } = new List<BasicItem>();

        public int Count => Items.Count;

        public BasicCharacterItems(ICharacter parent, byte equipmentSlots, byte usableSlots, byte setupSlots,
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
                var item = new BasicItem(itemEntity);
                Items.Add(item);
            }
        }

        public void Save()
        {
            foreach (var item in Items)
            {
                item.Save();
            }
        }

        public void Add(BasicItem item, bool fromDrop = false, bool autoMerge = true, bool forceGetSlot = false)
        {
            if (item.Quantity > 0)
            {
                item.Parent = this;

                if (item.IsStored)
                {
                    item.Slot = (short)Items.Count;
                }
                else if (item.Slot == 0 || forceGetSlot)
                {
                    item.Slot = GetNextFreeSlot(item.Type);
                }

                Items.Add(item);
            }
        }

        public void AddRange(IEnumerable<BasicItem> items, bool fromDrop = false, bool autoMerge = true)
        {
            foreach (var loopItem in items)
            {
                Add(loopItem, fromDrop, autoMerge);
            }
        }

        public void Remove(int mapleId, short quantity)
        {
            var leftToRemove = quantity;

            var toRemove = new List<BasicItem>();

            foreach (var loopItem in Items)
            {
                if (loopItem.MapleId == mapleId)
                {
                    if (loopItem.Quantity > leftToRemove)
                    {
                        loopItem.Quantity -= leftToRemove;
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

        public void Remove(BasicItem loginItem, bool removeFromSlot, bool fromDrop = false)
        {
            if (removeFromSlot && loginItem.IsEquipped)
            {
                throw new InvalidOperationException("Cannot remove equipped items from slot.");
            }

            if (loginItem.Assigned)
            {
                loginItem.Delete();
            }

            loginItem.Parent = null;

            Items.Remove(loginItem);
        }

        public void Clear(bool removeFromSlot)
        {
            var toRemove = new List<BasicItem>();

            foreach (var loopItem in Items)
            {
                toRemove.Add(loopItem);
            }

            foreach (var loopItem in toRemove)
            {
                if (!(loopItem.IsEquipped && removeFromSlot))
                {
                    Remove(loopItem, removeFromSlot);
                }
            }
        }

        public bool Contains(int mapleId)
        {
            foreach (var loopItem in Items)
            {
                if (loopItem.MapleId == mapleId)
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(int mapleId, short quantity)
        {
            var count = 0;

            foreach (var loopItem in Items)
            {
                if (loopItem.MapleId == mapleId)
                {
                    count += loopItem.Quantity;
                }
            }

            return count >= quantity;
        }

        public int Available(int mapleId)
        {
            var count = 0;

            foreach (var loopItem in Items)
            {
                if (loopItem.MapleId == mapleId)
                {
                    count += loopItem.Quantity;
                }
            }

            return count;
        }

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

            foreach (var item in Items)
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

            foreach (var item in Items)
            {
                if (item.Type == type)
                {
                    remaining--;
                }
            }

            return remaining;
        }

        public BasicItem this[ItemType type, short slot]
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.Type == type && item.Slot == slot)
                    {
                        return item;
                    }
                }

                return null;
            }
        }

        public BasicItem this[EquipmentSlot slot]
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.Slot == (sbyte)slot)
                    {
                        return item;
                    }
                }

                return null;
            }
        }

        public BasicItem this[int mapleId, short slot]
        {
            get
            {
                foreach (var item in Items)
                {
                    if (item.Slot == slot && item.Type == BasicItem.GetType(mapleId))
                    {
                        return item;
                    }
                }

                return null;
            }
        }

        public IEnumerable<BasicItem> this[ItemType type]
        {
            get
            {
                foreach (var loopItem in Items)
                {
                    if (loopItem.Type == type && !loopItem.IsEquipped)
                    {
                        yield return loopItem;
                    }
                }
            }
        }

        public IEnumerable<BasicItem> GetStored()
        {
            foreach (var loopItem in Items)
            {
                if (loopItem.IsStored)
                {
                    yield return loopItem;
                }
            }
        }

        public IEnumerable<BasicItem> GetEquipped(EquippedQueryMode mode = EquippedQueryMode.Any)
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

        public int SpaceTakenBy(BasicItem loginItem, bool autoMerge = true)
        {
            if (loginItem.Quantity < 0)
            {
                return 0;
            }

            if (Available(loginItem.MapleId) % loginItem.MaxPerStack != 0 && autoMerge)
            {
                foreach (var loopItem in Items.Where(x => x.MapleId == loginItem.MapleId && x.Quantity < x.MaxPerStack))
                {
                    return loopItem.Quantity + loginItem.Quantity <= loopItem.MaxPerStack ? 0 : 1;
                }

                return 1;
            }

            return 1;
        }

        public bool CouldReceive(IEnumerable<BasicItem> items, bool autoMerge = true)
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

            foreach (var loopSpaceCount in spaceCount)
            {
                if (RemainingSlots(loopSpaceCount.Key) < loopSpaceCount.Value)
                {
                    return false;
                }
            }

            return true;
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
    }
}
