using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Exceptions;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Common.Util;
using RazzleServer.Common.Data;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterItems : IEnumerable<Item>
    {
        public Character Parent { get; private set; }
        public Dictionary<ItemType, byte> MaxSlots { get; private set; }
        private List<Item> Items { get; set; }

        public CharacterItems(Character parent, byte equipmentSlots, byte usableSlots, byte setupSlots, byte etceteraSlots, byte cashSlots)
            : base()
        {
            Parent = parent;

            MaxSlots = new Dictionary<ItemType, byte>(Enum.GetValues(typeof(ItemType)).Length);

            MaxSlots.Add(ItemType.Equipment, equipmentSlots);
            MaxSlots.Add(ItemType.Usable, usableSlots);
            MaxSlots.Add(ItemType.Setup, setupSlots);
            MaxSlots.Add(ItemType.Etcetera, etceteraSlots);
            MaxSlots.Add(ItemType.Cash, cashSlots);

            Items = new List<Item>();
        }

        public void Load()
        {
            //// TODO: Use JOIN with the pets table.
            //foreach (Datum datum in new Datums("items").Populate("CharacterId = {0} AND IsStored = 0", Parent.Id))
            //{
            //    Item item = new Item(datum);

            //    Add(item);

            //    if (item.PetId != null)
            //    {
            //        //this.Parent.Pets.Add(new Pet(item));
            //    }
            //}
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
            if (Available(item.MapleId) % item.MaxPerStack != 0 && autoMerge)
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
                    else
                    {
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
            }

            if (item.Quantity > 0)
            {
                item.Parent = this;

                if ((Parent.IsInitialized && item.Slot == 0) || forceGetSlot)
                {
                    item.Slot = GetNextFreeSlot(item.Type);
                }

                Items.Add(item);

                if (Parent.IsInitialized)
                {
                    using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
                    {
                        oPacket.WriteBool(fromDrop);
                        oPacket.WriteByte(1);
                        oPacket.WriteByte((byte)InventoryOperationType.AddItem);
                        oPacket.WriteByte((byte)item.Type);
                        oPacket.WriteShort(item.Slot);
                        oPacket.WriteBytes(item.ToByteArray(true));

                        Parent.Client.Send(oPacket);
                    }
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
                    else
                    {
                        leftToRemove -= loopItem.Quantity;
                        toRemove.Add(loopItem);
                    }
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
                using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
                {
                    oPacket.WriteBool(fromDrop);
                    oPacket.WriteByte(1);
                    oPacket.WriteByte((byte)InventoryOperationType.RemoveItem);
                    oPacket.WriteByte((byte)item.Type);
                    oPacket.WriteShort(item.Slot);

                    Parent.Client.Send(oPacket);
                }
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
                Parent.UpdateApperance();
            }
        }

        public void Clear(bool removeFromSlot)
        {
            var toRemove = new List<Item>();

            foreach (var loopItem in this)
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
            foreach (var loopItem in this)
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

            foreach (var loopItem in this)
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

            foreach (var loopItem in this)
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

            foreach (var item in this)
            {
                if (item.Type == type)
                {
                    count++;
                }
            }

            return (count == MaxSlots[type]);
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

        public void Sort(PacketReader iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            var type = (ItemType)iPacket.ReadByte();
        }

        public void Gather(PacketReader iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            var type = (ItemType)iPacket.ReadByte();
        }

        public void Handle(PacketReader iPacket)
        {
            iPacket.ReadInt();

            var type = (ItemType)iPacket.ReadByte();
            var source = iPacket.ReadShort();
            var destination = iPacket.ReadShort();
            var quantity = iPacket.ReadShort();

            try
            {
                var item = this[type, source];

                if (destination < 0)
                {
                    item.Equip();
                }
                else if (source < 0 && destination > 0)
                {
                    item.Unequip(destination);
                }
                else if (destination == 0)
                {
                    item.Drop(quantity);
                }
                else
                {
                    item.Move(destination);
                }
            }
            catch (InventoryFullException)
            {
                NotifyFull();
            }
        }

        public void UseItem(PacketReader iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            var slot = iPacket.ReadShort();
            var itemId = iPacket.ReadInt();

            var item = this[ItemType.Usable, slot];

            if (item == null || itemId != item.MapleId)
            {
                return;
            }

            Remove(itemId, 1);

            if (item.CHealth > 0)
            {
                Parent.Health += item.CHealth;
            }

            if (item.CMana > 0)
            {
                Parent.Mana += item.CMana;
            }

            if (item.CHealthPercentage != 0)
            {
                Parent.Health += (short)((item.CHealthPercentage * Parent.MaxHealth) / 100);
            }

            if (item.CManaPercentage != 0)
            {
                Parent.Mana += (short)((item.CManaPercentage * Parent.MaxMana) / 100);
            }

            if (item.CBuffTime > 0 && item.CProb == 0)
            {
                // TODO: Add buff.
            }

            if (false)
            {
                // TODO: Add Monster Book card.
            }
        }

        public void UseSummonBag(PacketReader iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            var slot = iPacket.ReadShort();
            var itemId = iPacket.ReadInt();

            var item = this[ItemType.Usable, slot];

            if (item == null || itemId != item.MapleId)
            {
                return;
            }

            Remove(itemId, 1);

            foreach (var summon in item.Summons)
            {
                if (Functions.Random(0, 100) < summon.Item2)
                {
                    if (DataProvider.Mobs.Contains(summon.Item1))
                    {
                        Parent.Map.Mobs.Add(new Mob(summon.Item1, Parent.Position));
                    }
                }
            }
        }

        public void UseCashItem(PacketReader iPacket)
        {
            var slot = iPacket.ReadShort();
            var itemId = iPacket.ReadInt();

            var item = this[ItemType.Cash, slot];

            if (item == null || itemId != item.MapleId)
            {
                return;
            }

            var used = false;

            switch (item.MapleId) // TODO: Enum for these.
            {
                case 5040000: // NOTE: Teleport Rock.
                case 5040001: // NOTE: Coke Teleport Rock.
                case 5041000: // NOTE: VIP Teleport Rock.
                    {
                        used = Parent.Trocks.Use(itemId, iPacket);
                    }
                    break;

                case 5050001: // NOTE: 1st Job SP Reset.
                case 5050002: // NOTE: 2nd Job SP Reset.
                case 5050003: // NOTE: 3rd Job SP Reset.
                case 5050004: // NOTE: 4th Job SP Reset.
                    {

                    }
                    break;

                case 5050000: // NOTE: AP Reset.
                    {
                        var statDestination = (StatisticType)iPacket.ReadInt();
                        var statSource = (StatisticType)iPacket.ReadInt();

                        Parent.AddAbility(statDestination, 1, true);
                        Parent.AddAbility(statSource, -1, true);

                        used = true;
                    }
                    break;

                case 5071000: // NOTE: Megaphone.
                    {
                        if (Parent.Level <= 10)
                        {
                            // NOTE: You can't use a megaphone unless you're over level 10.

                            return;
                        }

                        var text = iPacket.ReadString();

                        var message = string.Format($"{Parent.Name} : {text}"); // TODO: Include medal name.

                        // NOTE: In GMS, this sends to everyone on the current channel, not the map (despite the item's description).
                        using (var oPacket = new PacketWriter(ServerOperationCode.BroadcastMsg))
                        {
                            oPacket.WriteByte((byte)NoticeType.Megaphone);
                            oPacket.WriteString(message);

                            //this.Parent.Client.Channel.Broadcast(oPacket);
                        }

                        used = true;
                    }
                    break;

                case 5072000: // NOTE: Super Megaphone.
                    {
                        if (Parent.Level <= 10)
                        {
                            // NOTE: You can't use a megaphone unless you're over level 10.

                            return;
                        }

                        var text = iPacket.ReadString();
                        var whisper = iPacket.ReadBool();

                        var message = string.Format($"{Parent.Name} : {text}"); // TODO: Include medal name.

                        using (var oPacket = new PacketWriter(ServerOperationCode.BroadcastMsg))
                        {
                            oPacket.WriteByte((byte)NoticeType.SuperMegaphone);
                            oPacket.WriteString(message);
                            oPacket.WriteByte(Parent.Client.Server.ChannelId);
                            oPacket.WriteBool(whisper);

                            //this.Parent.Client.World.Broadcast(oPacket);
                        }

                        used = true;
                    }
                    break;

                case 5390000: // NOTE: Diablo Messenger.
                case 5390001: // NOTE: Cloud 9 Messenger.
                case 5390002: // NOTE: Loveholic Messenger.
                    {
                        if (Parent.Level <= 10)
                        {
                            // NOTE: You can't use a megaphone unless you're over level 10.

                            return;
                        }

                        var text1 = iPacket.ReadString();
                        var text2 = iPacket.ReadString();
                        var text3 = iPacket.ReadString();
                        var text4 = iPacket.ReadString();
                        var whisper = iPacket.ReadBool();

                        using (var oPacket = new PacketWriter(ServerOperationCode.SetAvatarMegaphone))
                        {
                            oPacket.WriteInt(itemId);
                            oPacket.WriteString(Parent.Name);
                            oPacket.WriteString(text1);
                            oPacket.WriteString(text2);
                            oPacket.WriteString(text3);
                            oPacket.WriteString(text4);
                            oPacket.WriteInt(Parent.Client.Server.ChannelId);
                            oPacket.WriteBool(whisper);
                            oPacket.WriteBytes(Parent.AppearanceToByteArray());

                            //this.Parent.Client.World.Broadcast(oPacket);
                        }

                        used = true;
                    }
                    break;

                case 5076000: // NOTE: Item Megaphone.
                    {
                        var text = iPacket.ReadString();
                        var whisper = iPacket.ReadBool();
                        var includeItem = iPacket.ReadBool();

                        Item targetItem = null;

                        if (includeItem)
                        {
                            var type = (ItemType)iPacket.ReadInt();
                            var targetSlot = iPacket.ReadShort();

                            targetItem = this[type, targetSlot];

                            if (targetItem == null)
                            {
                                return;
                            }
                        }

                        var message = string.Format($"{Parent.Name} : {text}"); // TODO: Include medal name.

                        using (var oPacket = new PacketWriter(ServerOperationCode.BroadcastMsg))
                        {
                            oPacket.WriteByte((byte)NoticeType.ItemMegaphone);
                            oPacket.WriteString(message);
                            oPacket.WriteByte(Parent.Client.Server.ChannelId);
                            oPacket.WriteBool(whisper);
                            oPacket.WriteByte((byte)(targetItem != null ? targetItem.Slot : 0));

                            if (targetItem != null)
                            {
                                oPacket.WriteBytes(targetItem.ToByteArray(true));
                            }

                            //this.Parent.Client.World.Broadcast(oPacket);
                        }

                        used = true;
                    }
                    break;

                case 5077000: // NOTE: Art Megaphone.
                    {

                    }
                    break;

                case 5170000: // NOTE: Pet Name Tag.
                    {
                        //// TODO: Get the summoned pet.

                        //string name = iPacket.ReadString();

                        //using (var oPacket = new PacketWriter(ServerOperationCode.PetNameChanged))
                        //{
                        //    oPacket
                        //        oPacket.WriteInt(this.Parent.Id)
                        //        oPacket.WriteByte() // NOTE: Index.
                        //        oPacket.WriteString(name)
                        //        oPacket.WriteByte();

                        //    this.Parent.Map.Broadcast(oPacket);
                        //}
                    }
                    break;

                case 5060000: // NOTE: Item Name Tag.
                    {
                        var targetSlot = iPacket.ReadShort();

                        if (targetSlot == 0)
                        {
                            return;
                        }

                        var targetItem = this[ItemType.Equipment, targetSlot];

                        if (targetItem == null)
                        {
                            return;
                        }

                        targetItem.Creator = Parent.Name;
                        targetItem.Update(); // TODO: This does not seem to update the item's creator.

                        used = true;
                    }
                    break;

                case 5520000: // NOTE: Scissors of Karma.
                case 5060001: // NOTE: Item Lock. 
                    {

                    }
                    break;

                case 5075000: // NOTE: Maple TV Messenger.
                case 5075003: // NOTE: Megassenger.
                    {

                    }
                    break;

                case 5075001: // NOTE: Maple TV Star Messenger.
                case 5075004: // NOTE: Star Megassenger.
                    {

                    }
                    break;

                case 5075002: // NOTE: Maple TV Heart Messenger.
                case 5075005: // NOTE: Heart Megassenger.
                    {

                    }
                    break;

                case 5200000: // NOTE: Bronze Sack of Meso.
                case 5200001: // NOTE: Silver Sack of Meso.
                case 5200002: // NOTE: Gold Sack of Meso.
                    {
                        Parent.Meso += item.Meso;

                        // TODO: We definitely need a GainMeso method with inChat parameter.
                        using (var oPacket = new PacketWriter(ServerOperationCode.Message))
                        {
                            oPacket.WriteByte((byte)MessageType.IncreaseMeso);
                            oPacket.WriteInt(item.Meso);
                            oPacket.WriteShort(0);

                            Parent.Client.Send(oPacket);
                        }

                        used = true;
                    }
                    break;

                case 5370000: // NOTE: Chalkboard.
                case 5370001: // NOTE: Chalkboard 2.
                    {
                        var text = iPacket.ReadString();

                        Parent.Chalkboard = text;
                    }
                    break;

                case 5300000: // NOTE: Fungus Scrol.
                case 5300001: // NOTE: Oinker Delight.
                case 5300002: // NOTE: Zeta Nightmare.
                    {

                    }
                    break;

                case 5090000: // NOTE: Note (Memo).
                    {
                        //string targetName = iPacket.ReadString();
                        //string message = iPacket.ReadString();

                        //if (this.Parent.Client.World.IsCharacterOnline(targetName))
                        //{
                        //    using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
                        //    {
                        //        oPacket
                        //            oPacket.WriteByte((byte)MemoResult.Error)
                        //            oPacket.WriteByte((byte)MemoError.ReceiverOnline);

                        //        this.Parent.Client.Send(oPacket);
                        //    }
                        //}
                        //else if (!Database.Exists("characters", "Name = {0}", targetName))
                        //{
                        //    using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
                        //    {
                        //        oPacket
                        //            oPacket.WriteByte((byte)MemoResult.Error)
                        //            oPacket.WriteByte((byte)MemoError.ReceiverInvalidName);

                        //        this.Parent.Client.Send(oPacket);
                        //    }
                        //}
                        //else if (false) // TODO: Receiver's inbox is full. I believe the maximum amount is 5, but need to verify.
                        //{
                        //    using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
                        //    {
                        //        oPacket
                        //            oPacket.WriteByte((byte)MemoResult.Error)
                        //            oPacket.WriteByte((byte)MemoError.ReceiverInboxFull);

                        //        this.Parent.Client.Send(oPacket);
                        //    }
                        //}
                        //else
                        //{
                        //    Datum datum = new Datum("memos");

                        //    datum["CharacterId"] = Database.Fetch("characters", "Id", "Name = {0}", targetName);
                        //    datum["Sender"] = this.Parent.Name;
                        //    datum["Message"] = message;
                        //    datum["Received"] = DateTime.Now;

                        //    datum.Insert();

                        //    using (var oPacket = new PacketWriter(ServerOperationCode.MemoResult))
                        //    {
                        //        oPacket.WriteByte((byte)MemoResult.Sent);

                        //        this.Parent.Client.Send(oPacket);
                        //    }

                        //    used = true;
                        //}
                    }
                    break;

                case 5100000: // NOTE: Congratulatory Song.
                    {

                    }
                    break;
            }

            if (used)
            {
                Remove(itemId, 1);
            }
            else
            {
                Parent.Release(); // TODO: Blank inventory update.
            }
        }

        public void UseReturnScroll(PacketReader iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            var slot = iPacket.ReadShort();
            var itemId = iPacket.ReadInt();

            var item = this[itemId, slot];

            if (item == null)
            {
                return;
            }

            Remove(itemId, 1);

            Parent.ChangeMap(item.CMoveTo);
        }

        public void Pickup(Drop drop)
        {
            if (drop.Picker == null)
            {
                try
                {
                    drop.Picker = Parent;

                    if (drop is Meso)
                    {
                        Parent.Meso += ((Meso)drop).Amount; // TODO: Check for max meso.
                    }
                    else if (drop is Item)
                    {
                        if (((Item)drop).OnlyOne)
                        {
                            // TODO: Appropriate message.

                            return;
                        }

                        ((Item)drop).Slot = GetNextFreeSlot(((Item)drop).Type); // TODO: Check for inv. full. 
                        Add((Item)drop, true);
                    }

                    Parent.Map.Drops.Remove(drop);
                    drop.Picker.Client.Send(drop.GetShowGainPacket());
                }
                catch (InventoryFullException)
                {
                    NotifyFull();
                }
            }
        }

        public void Pickup(PacketReader iPacket)
        {
            iPacket.Skip(1);
            iPacket.Skip(4);
            var position = new Point(iPacket.ReadShort(), iPacket.ReadShort());

            // TODO: Validate position relative to the picker.

            var objectId = iPacket.ReadInt();

            lock (Parent.Map.Drops)
            {
                if (Parent.Map.Drops.Contains(objectId))
                {
                    Pickup(Parent.Map.Drops[objectId]);
                }
            }
        }

        public Item this[ItemType type, short slot]
        {
            get
            {
                foreach (var item in this)
                {
                    if (item.Type == type && item.Slot == slot)
                    {
                        return item;
                    }
                }

                return null;
            }
        }

        public Item this[EquipmentSlot slot]
        {
            get
            {
                foreach (var item in this)
                {
                    if (item.Slot == (sbyte)slot)
                    {
                        return item;
                    }
                }

                return null;
            }
        }

        public Item this[int mapleId, short slot]
        {
            get
            {
                foreach (var item in this)
                {
                    if (item.Slot == slot && item.Type == Item.GetType(mapleId))
                    {
                        return item;
                    }
                }

                return null;
            }
        }

        public IEnumerable<Item> this[ItemType type]
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

        public IEnumerable<Item> GetStored()
        {
            foreach (var loopItem in Items)
            {
                if (loopItem.IsStored)
                {
                    yield return loopItem;
                }
            }
        }

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
                    if (loopItem.Quantity + item.Quantity <= loopItem.MaxPerStack)
                    {
                        return 0;
                    }
                    else
                    {
                        return 1;
                    }
                }

                return 1;
            }
            else
            {
                return 1;
            }
        }

        public bool CouldReceive(IEnumerable<Item> items, bool autoMerge = true)
        {
            var spaceCount = new Dictionary<ItemType, int>(5);
            {
                spaceCount.Add(ItemType.Equipment, 0);
                spaceCount.Add(ItemType.Usable, 0);
                spaceCount.Add(ItemType.Setup, 0);
                spaceCount.Add(ItemType.Etcetera, 0);
                spaceCount.Add(ItemType.Cash, 0);
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

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteByte(MaxSlots[ItemType.Equipment]);
                oPacket.WriteByte(MaxSlots[ItemType.Usable]);
                oPacket.WriteByte(MaxSlots[ItemType.Setup]);
                oPacket.WriteByte(MaxSlots[ItemType.Etcetera]);
                oPacket.WriteByte(MaxSlots[ItemType.Cash]);


                foreach (var item in GetEquipped(EquippedQueryMode.Normal))
                {
                    oPacket.WriteBytes(item.ToByteArray());
                }

                oPacket.WriteByte(0);

                foreach (var item in GetEquipped(EquippedQueryMode.Cash))
                {
                    oPacket.WriteBytes(item.ToByteArray());
                }

                oPacket.WriteByte(0);

                foreach (var item in this[ItemType.Equipment])
                {
                    oPacket.WriteBytes(item.ToByteArray());
                }

                oPacket.WriteByte(0);

                foreach (var item in this[ItemType.Usable])
                {
                    oPacket.WriteBytes(item.ToByteArray());
                }

                oPacket.WriteByte(0);

                foreach (var item in this[ItemType.Setup])
                {
                    oPacket.WriteBytes(item.ToByteArray());
                }

                oPacket.WriteByte(0);

                foreach (var item in this[ItemType.Etcetera])
                {
                    oPacket.WriteBytes(item.ToByteArray());
                }

                oPacket.WriteByte(0);

                foreach (var item in this[ItemType.Cash])
                {
                    oPacket.WriteBytes(item.ToByteArray());
                }

                oPacket.WriteByte(0);

                return oPacket.ToArray();
            }
        }

        public IEnumerator<Item> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Items).GetEnumerator();
    }
}

