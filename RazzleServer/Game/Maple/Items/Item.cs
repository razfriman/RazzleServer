using System;
using System.Collections.Generic;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Items
{
    public class Item : Drop
    {
        public static ItemType GetType(int mapleId) => (ItemType)(mapleId / 1000000);

        public CharacterItems Parent { get; set; }
        public int Id { get; private set; }
        public int AccountId { get; }
        public int MapleId { get; }
        public short Slot { get; set; }
        private short _quantity;
        public string Creator { get; set; }
        public DateTime Expiration { get; set; }
        public int? PetId { get; set; }
        public bool IsCash { get; }
        public bool OnlyOne { get; }
        public bool PreventsSlipping { get; }
        public bool PreventsColdness { get; }
        public bool IsTradeBlocked { get; }
        public bool IsStored { get; set; }
        public int SalePrice { get; }
        public int Meso { get; }
        public byte UpgradesAvailable { get; }
        public byte UpgradesApplied { get; }
        public short Strength { get; }
        public short Dexterity { get; }
        public short Intelligence { get; }
        public short Luck { get; }
        public short Health { get; }
        public short Mana { get; }
        public short WeaponAttack { get; }
        public short MagicAttack { get; }
        public short WeaponDefense { get; }
        public short MagicDefense { get; }
        public short Accuracy { get; }
        public short Avoidability { get; }
        public short Agility { get; }
        public short Speed { get; }
        public short Jump { get; }
        public byte AttackSpeed { get; }
        public short RecoveryRate { get; }
        public short KnockBackChance { get; }
        public short RequiredLevel { get; }
        public short RequiredStrength { get; }
        public short RequiredDexterity { get; }
        public short RequiredIntelligence { get; }
        public short RequiredLuck { get; }
        public short RequiredFame { get; }
        public Job RequiredJob { get; }

        // Consume data properties are prefixed with 'C'.
        public int CItemId { get; private set; }
        public string CFlags { get; }
        public string CCureAilments { get; }
        public short CEffect { get; }
        public short CHealth { get; }
        public short CMana { get; }
        public short CHealthPercentage { get; }
        public short CManaPercentage { get; }
        public int CMoveTo { get; }
        public short CProb { get; }
        public int CBuffTime { get; }
        public short CWeaponAttack { get; }
        public short CMagicAttack { get; }
        public short CWeaponDefense { get; }
        public short CMagicDefense { get; }
        public short CAccuracy { get; }
        public short CAvoid { get; }
        public short CSpeed { get; }
        public short CJump { get; }
        public short CMorph { get; }

        public List<Tuple<int, short>> Summons { get; }

        public ItemType Type => GetType(MapleId);

        public WeaponType WeaponType
        {
            get
            {
                switch (MapleId / 10000 % 100)
                {
                    case 30:
                        return WeaponType.Sword1H;

                    case 31:
                        return WeaponType.Axe1H;

                    case 32:
                        return WeaponType.Blunt1H;

                    case 33:
                        return WeaponType.Dagger;

                    case 37:
                        return WeaponType.Wand;

                    case 38:
                        return WeaponType.Staff;

                    case 40:
                        return WeaponType.Sword2H;

                    case 41:
                        return WeaponType.Axe2H;

                    case 42:
                        return WeaponType.Blunt2H;

                    case 43:
                        return WeaponType.Spear;

                    case 44:
                        return WeaponType.PoleArm;

                    case 45:
                        return WeaponType.Bow;

                    case 46:
                        return WeaponType.Crossbow;

                    case 47:
                        return WeaponType.Claw;

                    case 48:
                        return WeaponType.Knuckle;

                    default:
                        return WeaponType.NotAWeapon;
                }
            }
        }

        public ItemReference CachedReference => DataProvider.Items.Data[MapleId];

        public Character Character => Parent.Parent;

        public short MaxPerStack => CachedReference.MaxPerStack;

        public short Quantity
        {
            get => _quantity;
            set => _quantity = Math.Min(value, MaxPerStack);
        }

        public bool IsSealed => DataProvider.Items.WizetItemIds.Contains(MapleId);

        public byte Flags
        {
            get
            {
                byte flags = 0;

                if (IsSealed)
                {
                    flags |= (byte)ItemFlags.Sealed;
                }

                if (PreventsSlipping)
                {
                    flags |= (byte)ItemFlags.AddPreventSlipping;
                }

                if (PreventsColdness)
                {
                    flags |= (byte)ItemFlags.AddPreventColdness;
                }

                if (IsTradeBlocked)
                {
                    flags |= (byte)ItemFlags.Untradeable;
                }

                return flags;
            }
        }

        public bool IsEquipped => Slot < 0;

        public bool IsEquippedCash => Slot < -100;

        public bool IsConsumable => MapleId / 10000 >= 200 && MapleId / 10000 < 204;

        public bool IsRechargeable => IsThrowingStar || IsBullet;

        public bool IsThrowingStar => MapleId / 10000 == 207;

        public bool IsBullet => MapleId / 10000 == 233;

        public bool IsArrow => IsArrowForBow || IsArrowForCrossbow;

        public bool IsArrowForBow => MapleId >= 2060000 && MapleId < 2061000;

        public bool IsArrowForCrossbow => MapleId >= 2061000 && MapleId < 2062000;

        public bool IsOverall => MapleId / 10000 == 105;

        public bool IsWeapon => WeaponType != WeaponType.NotAWeapon;

        public bool IsShield => MapleId / 10000 % 100 == 9;

        public bool IsPet => MapleId >= 5000000 && MapleId <= 5000100;

        public bool IsTownScroll => MapleId >= 2030000 && MapleId < 2030020;

        public bool IsTwoHanded
        {
            get
            {
                switch (WeaponType)
                {
                    case WeaponType.Sword2H:
                    case WeaponType.Axe2H:
                    case WeaponType.Blunt2H:
                    case WeaponType.Spear:
                    case WeaponType.PoleArm:
                    case WeaponType.Bow:
                    case WeaponType.Crossbow:
                    case WeaponType.Claw:
                    case WeaponType.Knuckle:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public bool IsBlocked => IsCash || IsSealed || IsTradeBlocked;

        public byte AbsoluteSlot
        {
            get
            {
                if (IsEquipped)
                {
                    return (byte)(Slot * -1);
                }

                throw new InvalidOperationException("Attempting to retrieve absolute slot for non-equipped item.");
            }
        }

        public byte ComputedSlot
        {
            get
            {
                if (IsEquippedCash)
                {
                    return (byte)(AbsoluteSlot - 100);
                }

                if (IsEquipped)
                {
                    return AbsoluteSlot;
                }

                return (byte)Slot;
            }
        }

        public bool Assigned { get; set; }

        public Item(int mapleId, short quantity = 1, DateTime? expiration = null, bool equipped = false)
        {
            MapleId = mapleId;
            Quantity = Type == ItemType.Equipment ? (short)1 : quantity;
            if (equipped)
            {
                Slot = (short)GetEquippedSlot();
            }

            if (!expiration.HasValue)
            {
                expiration = DateConstants.Permanent;
            }

            Expiration = expiration.Value;

            IsCash = CachedReference.IsCash;
            OnlyOne = CachedReference.OnlyOne;
            IsTradeBlocked = CachedReference.IsTradeBlocked;
            SalePrice = CachedReference.SalePrice;
            RequiredLevel = CachedReference.RequiredLevel;
            Meso = CachedReference.Meso;

            if (Type == ItemType.Equipment)
            {
                PreventsSlipping = CachedReference.PreventsSlipping;
                PreventsColdness = CachedReference.PreventsColdness;

                AttackSpeed = CachedReference.AttackSpeed;
                RecoveryRate = CachedReference.RecoveryRate;
                KnockBackChance = CachedReference.KnockBackChance;

                RequiredStrength = CachedReference.RequiredStrength;
                RequiredDexterity = CachedReference.RequiredDexterity;
                RequiredIntelligence = CachedReference.RequiredIntelligence;
                RequiredLuck = CachedReference.RequiredLuck;
                RequiredFame = CachedReference.RequiredFame;
                RequiredJob = CachedReference.RequiredJob;

                UpgradesAvailable = CachedReference.UpgradesAvailable;
                UpgradesApplied = CachedReference.UpgradesApplied;
                Strength = CachedReference.Strength;
                Dexterity = CachedReference.Dexterity;
                Intelligence = CachedReference.Intelligence;
                Luck = CachedReference.Luck;
                Health = CachedReference.Health;
                Mana = CachedReference.Mana;
                WeaponAttack = CachedReference.WeaponAttack;
                MagicAttack = CachedReference.MagicAttack;
                WeaponDefense = CachedReference.WeaponDefense;
                MagicDefense = CachedReference.MagicDefense;
                Accuracy = CachedReference.Accuracy;
                Avoidability = CachedReference.Avoidability;
                Agility = CachedReference.Agility;
                Speed = CachedReference.Speed;
                Jump = CachedReference.Jump;
            }
            else if (IsConsumable)
            {

                CFlags = CachedReference.CFlags;
                CCureAilments = CachedReference.CCureAilments;
                CEffect = CachedReference.CEffect;
                CHealth = CachedReference.CHealth;
                CMana = CachedReference.CMana;
                CHealthPercentage = CachedReference.CHealthPercentage;
                CManaPercentage = CachedReference.CManaPercentage;
                CMoveTo = CachedReference.CMoveTo;
                CProb = CachedReference.CProb;
                CBuffTime = CachedReference.CBuffTime;
                CWeaponAttack = CachedReference.CWeaponAttack;
                CMagicAttack = CachedReference.CMagicAttack;
                CWeaponDefense = CachedReference.CWeaponDefense;
                CMagicDefense = CachedReference.CMagicDefense;
                CAccuracy = CachedReference.CAccuracy;
                CAvoid = CachedReference.CAvoid;
                CSpeed = CachedReference.CSpeed;
                CJump = CachedReference.CJump;
                CMorph = CachedReference.CMorph;
            }

            Summons = CachedReference.Summons;
        }

        public Item(ItemEntity entity)
        {
            Id = entity.Id;
            Assigned = true;
            AccountId = entity.AccountId;
            MapleId = entity.MapleId;
            Quantity = entity.Quantity;
            Slot = entity.Slot;
            Creator = entity.Creator;
            Expiration = entity.Expiration;
            PetId = entity.PetId;

            IsCash = CachedReference.IsCash;
            OnlyOne = CachedReference.OnlyOne;
            IsTradeBlocked = CachedReference.IsTradeBlocked;
            IsStored = entity.IsStored;
            SalePrice = CachedReference.SalePrice;
            RequiredLevel = CachedReference.RequiredLevel;
            Meso = CachedReference.Meso;

            if (Type == ItemType.Equipment)
            {
                AttackSpeed = CachedReference.AttackSpeed;
                RecoveryRate = CachedReference.RecoveryRate;
                KnockBackChance = CachedReference.KnockBackChance;

                RequiredStrength = CachedReference.RequiredStrength;
                RequiredDexterity = CachedReference.RequiredDexterity;
                RequiredIntelligence = CachedReference.RequiredIntelligence;
                RequiredLuck = CachedReference.RequiredLuck;
                RequiredFame = CachedReference.RequiredFame;
                RequiredJob = CachedReference.RequiredJob;

                UpgradesAvailable = entity.UpgradesAvailable;
                UpgradesApplied = entity.UpgradesApplied;
                Strength = entity.Strength;
                Dexterity = entity.Dexterity;
                Intelligence = entity.Intelligence;
                Luck = entity.Luck;
                Health = entity.Health;
                Mana = entity.Mana;
                WeaponAttack = entity.WeaponAttack;
                MagicAttack = entity.MagicAttack;
                WeaponDefense = entity.WeaponDefense;
                MagicDefense = entity.MagicDefense;
                Accuracy = entity.Accuracy;
                Avoidability = entity.Avoidability;
                Agility = entity.Agility;
                Speed = entity.Speed;
                Jump = entity.Jump;
            }
            else if (IsConsumable)
            {
                CFlags = CachedReference.CFlags;
                CCureAilments = CachedReference.CCureAilments;
                CEffect = CachedReference.CEffect;
                CHealth = CachedReference.CHealth;
                CMana = CachedReference.CMana;
                CHealthPercentage = CachedReference.CHealthPercentage;
                CManaPercentage = CachedReference.CManaPercentage;
                CMoveTo = CachedReference.CMoveTo;
                CProb = CachedReference.CProb;
                CBuffTime = CachedReference.CBuffTime;
                CWeaponAttack = CachedReference.CWeaponAttack;
                CMagicAttack = CachedReference.CMagicAttack;
                CWeaponDefense = CachedReference.CWeaponDefense;
                CMagicDefense = CachedReference.CMagicDefense;
                CAccuracy = CachedReference.CAccuracy;
                CAvoid = CachedReference.CAvoid;
                CSpeed = CachedReference.CSpeed;
                CJump = CachedReference.CJump;
                CMorph = CachedReference.CMorph;
            }

            Summons = CachedReference.Summons;
        }

        public void Save()
        {
            using (var dbContext = new MapleDbContext())
            {
                var item = dbContext.Items.Find(Id);
                var isNewItem = item == null;

                if (isNewItem)
                {
                    item = new ItemEntity();
                    dbContext.Items.Add(item);
                }

                item.AccountId = Character.AccountId;
                item.CharacterId = Character.Id;
                item.MapleId = MapleId;
                item.Accuracy = Accuracy;
                item.Agility = Agility;
                item.Avoidability = Avoidability;
                item.Creator = Creator;
                item.Dexterity = Dexterity;
                item.Expiration = Expiration;
                item.Flags = Flags;
                item.Health = Health;
                item.Intelligence = Intelligence;
                item.MagicAttack = MagicAttack;
                item.MagicDefense = MagicAttack;
                item.WeaponDefense = WeaponDefense;
                item.WeaponAttack = WeaponAttack;
                item.UpgradesAvailable = UpgradesAvailable;
                item.UpgradesApplied = UpgradesApplied;
                item.IsStored = IsStored;
                item.Jump = Jump;
                item.Luck = Luck;
                item.Mana = Mana;
                item.Quantity = Quantity;
                item.Slot = Slot;
                item.Speed = Speed;
                item.Strength = Strength;
                item.PetId = PetId;

                dbContext.SaveChanges();

                if (isNewItem)
                {
                    Id = item.Id;
                }
            }
        }

        public void Delete()
        {
            using (var dbContext = new MapleDbContext())
            {
                var item = dbContext.Items.Find(Id);
                if (item != null)
                {
                    dbContext.Remove(item);
                    dbContext.SaveChanges();
                }

                Assigned = false;
            }
        }

        public void Update()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
            {
                oPacket.WriteBool(true);
                oPacket.WriteByte(1);
                oPacket.WriteByte((byte)InventoryOperationType.ModifyQuantity);
                oPacket.WriteByte((byte)Type);
                oPacket.WriteShort(Slot);
                oPacket.WriteShort(Quantity);

                Character.Client.Send(oPacket);
            }
        }

        public void Equip()
        {
            if (Type != ItemType.Equipment)
            {
                throw new InvalidOperationException("Can only equip equipment items.");
            }

            if ((Character.Strength < RequiredStrength ||
                Character.Dexterity < RequiredDexterity ||
                Character.Intelligence < RequiredIntelligence ||
                Character.Luck < RequiredLuck) &&
                !Character.IsMaster)
            {
                return;
            }

            var sourceSlot = Slot;
            var destinationSlot = GetEquippedSlot();

            var top = Parent[EquipmentSlot.Top];
            var bottom = Parent[EquipmentSlot.Bottom];
            var weapon = Parent[EquipmentSlot.Weapon];
            var shield = Parent[EquipmentSlot.Shield];

            var destination = Parent[destinationSlot];

            if (destination != null)
            {
                destination.Slot = sourceSlot;
            }

            Slot = (short)destinationSlot;

            using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
            {

                oPacket.WriteBool(true);
                oPacket.WriteByte(1);
                oPacket.WriteByte((byte)InventoryOperationType.ModifySlot);
                oPacket.WriteByte((byte)Type);
                oPacket.WriteShort(sourceSlot);
                oPacket.WriteShort((short)destinationSlot);
                oPacket.WriteByte(1);

                Character.Client.Send(oPacket);
            }

            switch (destinationSlot)
            {
                case EquipmentSlot.Bottom:
                    {
                        if (top != null && top.IsOverall)
                        {
                            top.Unequip();
                        }
                    }
                    break;

                case EquipmentSlot.Top:
                    {
                        if (IsOverall)
                        {
                            bottom?.Unequip();
                        }
                    }
                    break;

                case EquipmentSlot.Shield:
                    {
                        if (weapon != null && weapon.IsTwoHanded)
                        {
                            weapon.Unequip();
                        }
                    }
                    break;

                case EquipmentSlot.Weapon:
                    {
                        if (IsTwoHanded)
                        {
                            shield?.Unequip();
                        }
                    }
                    break;
            }

            Character.UpdateApperance();
        }

        public void Unequip(short destinationSlot = 0)
        {
            if (Type != ItemType.Equipment)
            {
                throw new InvalidOperationException("Cna only unequip equipment items.");
            }

            var sourceSlot = Slot;

            if (destinationSlot == 0)
            {
                destinationSlot = Parent.GetNextFreeSlot(ItemType.Equipment);
            }

            Slot = destinationSlot;

            using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
            {

                oPacket.WriteBool(true);
                oPacket.WriteByte(1);
                oPacket.WriteByte((byte)InventoryOperationType.ModifySlot);
                oPacket.WriteByte((byte)Type);
                oPacket.WriteShort(sourceSlot);
                oPacket.WriteShort(destinationSlot);
                oPacket.WriteByte(1);

                Character.Client.Send(oPacket);
            }

            Character.UpdateApperance();
        }

        public void Drop(short quantity)
        {
            if (IsRechargeable)
            {
                quantity = Quantity;
            }

            if (IsBlocked)
            {
                return;
            }

            if (quantity > Quantity)
            {
                return;
            }

            if (quantity == Quantity)
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
                {
                    oPacket.WriteBool(true);
                    oPacket.WriteByte(1);
                    oPacket.WriteByte((byte)InventoryOperationType.RemoveItem);
                    oPacket.WriteByte((byte)Type);
                    oPacket.WriteShort(Slot);

                    if (IsEquipped)
                    {
                        oPacket.WriteByte(1);
                    }

                    Character.Client.Send(oPacket);
                }

                Dropper = Character;
                Owner = null;

                Character.Map.Drops.Add(this);

                Parent.Remove(this, false);
            }
            else if (quantity < Quantity)
            {
                Quantity -= quantity;

                using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
                {
                    oPacket.WriteBool(true);
                    oPacket.WriteByte(1);
                    oPacket.WriteByte((byte)InventoryOperationType.ModifyQuantity);
                    oPacket.WriteByte((byte)Type);
                    oPacket.WriteShort(Slot);
                    oPacket.WriteShort(Quantity);

                    Character.Client.Send(oPacket);
                }

                var dropped = new Item(MapleId, quantity)
                {
                    Dropper = Character,
                    Owner = null
                };

                Character.Map.Drops.Add(dropped);
            }
        }

        public void Move(short destinationSlot)
        {
            var sourceSlot = Slot;

            var destination = Parent[Type, destinationSlot];

            if (destination != null &&
                Type != ItemType.Equipment &&
                MapleId == destination.MapleId &&
                !IsRechargeable &&
                destination.Quantity < destination.MaxPerStack)
            {
                if (Quantity + destination.Quantity > destination.MaxPerStack)
                {
                    Quantity -= (short)(destination.MaxPerStack - destination.Quantity);

                    using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
                    {
                        oPacket.WriteBool(true);
                        oPacket.WriteByte(2);
                        oPacket.WriteByte((byte)InventoryOperationType.ModifyQuantity);
                        oPacket.WriteByte((byte)Type);
                        oPacket.WriteShort(sourceSlot);
                        oPacket.WriteShort(Quantity);
                        oPacket.WriteByte((byte)InventoryOperationType.ModifyQuantity);
                        oPacket.WriteByte((byte)destination.Type);
                        oPacket.WriteShort(destinationSlot);
                        oPacket.WriteShort(destination.Quantity);

                        Character.Client.Send(oPacket);
                    }
                }
                else
                {
                    destination.Quantity += Quantity;

                    using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
                    {
                        oPacket.WriteBool(true);
                        oPacket.WriteByte(2);
                        oPacket.WriteByte((byte)InventoryOperationType.RemoveItem);
                        oPacket.WriteByte((byte)Type);
                        oPacket.WriteShort(sourceSlot);
                        oPacket.WriteByte((byte)InventoryOperationType.ModifyQuantity);
                        oPacket.WriteByte((byte)destination.Type);
                        oPacket.WriteShort(destinationSlot);
                        oPacket.WriteShort(destination.Quantity);

                        Character.Client.Send(oPacket);
                    }
                }
            }
            else
            {
                if (destination != null)
                {
                    destination.Slot = sourceSlot;
                }

                Slot = destinationSlot;

                using (var oPacket = new PacketWriter(ServerOperationCode.InventoryOperation))
                {

                    oPacket.WriteBool(true);
                    oPacket.WriteByte(1);
                    oPacket.WriteByte((byte)InventoryOperationType.ModifySlot);
                    oPacket.WriteByte((byte)Type);
                    oPacket.WriteShort(sourceSlot);
                    oPacket.WriteShort(destinationSlot);

                    Character.Client.Send(oPacket);
                }
            }
        }

        public byte[] ToByteArray(bool zeroPosition = false, bool leaveOut = false)
        {
            //00 00 00 00 01 01 00 00 4E B3 D6 46 E9 60 7A 9B 20 A0 09 E4 FF FF FF FF FF FF FF FF A1 95 04 00 72 61 7A 7A 7A 7A 7A 6C 65 00 00 00 00 00 00 20 4E 00 00 47 75 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 06 00 04 00 06 00 09 00 32 00 32 00 05 00 05 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00 00 01 00 00 00
            //14 00 00 00 00 18 18 18 18 60 
            //      05      01 82 DE 0F 00   00 00 80 05 BB 46 E6 17 02 07 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 06 01 A2 2C 10 00 00 00 80 05 BB 46 E6 17 02 07 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 07 01 81 5B 10 00 00 00 80 05 BB 46 E6 17 02 05 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0B 01 F0 DD 13 00 00 00 80 05 BB 46 E6 17 02 07 00 00 00 00 00 00 00 00 00 00 00 00 00 11 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 02 97 AB 1E 00 00 00 80 05 BB 46 E6 17 02 01 00 00 00 00 00 00 00 01 02 E9 7D 3F 00 00 00 80 05 BB 46 E6 17 02 01 00 00 00 00 00 00 00 00 00 00 00 01 00 FD 03 00 00 00 00 00 00 00 00 00 00 00 00 FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B 00 00 00 00 70 23 55 B9 CC F7 D3 01
            //ITEM: 05 {00} 01 82 DE 0F 00 [00] 00 80 5E B2 E1 20 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
            //ITEM: 06 00 01 A2 2C 10 00 00 80 5E B2 E1 20 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
            //ITEM: 07 00 01 81 5B 10 00 00 80 5E B2 E1 20 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
            //ITEM: 0B 00 01 F0 DD 13 00 00 80 5E B2 E1 20 03 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00

            using (var pw = new PacketWriter())
            {
                if (!zeroPosition && !leaveOut)
                {
                    var slot = ComputedSlot;

                    if (slot < 0)
                    {
                        slot = (byte)(slot * -1);
                    }
                    else if (slot > 100)
                    {
                        slot -= 100;
                    }

                    if (Type == ItemType.Equipment)
                    {
                        pw.WriteByte(slot);
                    }
                    else
                    {
                        pw.WriteByte(slot);
                    }
                }

                //      05    01 [82 DE 0F 00] [00] 00 80 05 BB 46 E6 17 02 [07]  00  00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00  03 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00 
                //      05    01 [82 DE 0F 00] [00] 00 80 5E B2 E1 20 03 00 [00]  00  00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00 00 00
                //      06    01 [A2 2C 10 00] [00] 00 80 05 BB 46 E6 17 02 [07]  00  00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00  02 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00 
                //      07    01 [81 5B 10 00] [00] 00 80 05 BB 46 E6 17 02 [05]  00  00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00  02 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00  00 00
                //      0B    01 [F0 DD 13 00] [00] 00 80 05 BB 46 E6 17 02 [07]  00  00  00 00  00 00 00 00 00 00 00 00 00 11 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 02 97 AB 1E 00 00 00 80 05 BB 46 E6 17 02 01 00 00 00 00 00 00 00 01 02 E9 7D 3F 00 00 00 80 05 BB 46 E6 17 02 01 00 00 00 00 00 00 00 00 00 00 00 01 00 FD 03 00 00 00 00 00 00 00 00 00 00 00 00 FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B FF C9 9A 3B 00 00 00 00 70 23 55 B9 CC F7 D3 01

                pw.WriteByte((byte)(PetId != null ? 3 : Type == ItemType.Equipment ? 1 : 2));
                pw.WriteInt(MapleId);
                pw.WriteBool(IsCash);

                if (IsCash)
                {
                    pw.WriteLong(1); // TODO: Unique Id for cash items.
                }

                pw.WriteHexString("00 80 05 BB 46 E6 17 02");
                //pw.WriteDateTime(Expiration);

                if (PetId != null)
                {

                }
                else if (Type == ItemType.Equipment)
                {
                    pw.WriteByte(UpgradesAvailable);
                    pw.WriteByte(UpgradesApplied);
                    pw.WriteShort(Strength);
                    pw.WriteShort(Dexterity);
                    pw.WriteShort(Intelligence);
                    pw.WriteShort(Luck);
                    pw.WriteShort(Health);
                    pw.WriteShort(Mana);
                    pw.WriteShort(WeaponAttack);
                    pw.WriteShort(MagicAttack);
                    pw.WriteShort(WeaponDefense);
                    pw.WriteShort(MagicDefense);
                    pw.WriteShort(Accuracy);
                    pw.WriteShort(Avoidability);
                    pw.WriteShort(Agility);
                    pw.WriteShort(Speed);
                    pw.WriteShort(Jump);
                    pw.WriteString(Creator);
                    pw.WriteByte(Flags);

                    // if ring && !equipped
                    // pw.WriteByte(0);

                    if (!IsEquippedCash)
                    {
                        pw.WriteByte(0);
                        pw.WriteLong(0);
                    }
                }
                else
                {
                    pw.WriteShort(Quantity);
                    pw.WriteString(Creator);
                    pw.WriteByte(Flags);
                    pw.WriteByte(0);

                    if (IsRechargeable)
                    {
                        pw.WriteLong(0); // TODO: Unique Id.
                    }
                }

                return pw.ToArray();
            }
        }

        private EquipmentSlot GetEquippedSlot()
        {
            short slot = 0;

            if (MapleId >= 1000000 && MapleId < 1010000)
            {
                slot -= 1;
            }
            else if (MapleId >= 1010000 && MapleId < 1020000)
            {
                slot -= 2;
            }
            else if (MapleId >= 1020000 && MapleId < 1030000)
            {
                slot -= 3;
            }
            else if (MapleId >= 1030000 && MapleId < 1040000)
            {
                slot -= 4;
            }
            else if (MapleId >= 1040000 && MapleId < 1060000)
            {
                slot -= 5;
            }
            else if (MapleId >= 1060000 && MapleId < 1070000)
            {
                slot -= 6;
            }
            else if (MapleId >= 1070000 && MapleId < 1080000)
            {
                slot -= 7;
            }
            else if (MapleId >= 1080000 && MapleId < 1090000)
            {
                slot -= 8;
            }
            else if (MapleId >= 1102000 && MapleId < 1103000)
            {
                slot -= 9;
            }
            else if (MapleId >= 1092000 && MapleId < 1100000)
            {
                slot -= 10;
            }
            else if (MapleId >= 1300000 && MapleId < 1800000)
            {
                slot -= 11;
            }
            else if (MapleId >= 1112000 && MapleId < 1120000)
            {
                slot -= 12;
            }
            else if (MapleId >= 1122000 && MapleId < 1123000)
            {
                slot -= 17;
            }
            else if (MapleId >= 1900000 && MapleId < 2000000)
            {
                slot -= 18;
            }

            if (IsCash)
            {
                slot -= 100;
            }

            return (EquipmentSlot)slot;
        }

        public override PacketWriter GetShowGainPacket()
        {
            return GamePackets.ShowStatusInfo(MessageType.DropPickup, false, Quantity, false, true, MapleId);
        }
    }
}
