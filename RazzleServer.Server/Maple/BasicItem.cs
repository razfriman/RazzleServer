using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Maple;
using RazzleServer.Data;
using RazzleServer.DataProvider;
using RazzleServer.DataProvider.References;
using RazzleServer.Net.Packet;

namespace RazzleServer.Server.Maple
{
    public class BasicItem
    {
        public static ItemType GetType(int mapleId) => (ItemType)(mapleId / 1000000);

        public BasicCharacterItems Parent { get; set; }
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
        public short Thaw { get; }
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
        public long CashId { get; set; }


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

        public ItemReference CachedReference => CachedData.Items.Data[MapleId];

        public ICharacter Character => Parent?.Parent;

        public short MaxPerStack => CachedReference.MaxPerStack;

        public short Quantity
        {
            get => _quantity;
            set => _quantity = Math.Min(value, MaxPerStack);
        }

        public bool IsSealed => CachedData.Items.WizetItemIds.Contains(MapleId);

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

        public BasicItem(int mapleId, short quantity = 1, DateTime? expiration = null, bool equipped = false)
        {
            MapleId = mapleId;

            if (!CachedData.Items.Data.ContainsKey(MapleId))
            {
                throw new KeyNotFoundException($"Item does not exist with ID={MapleId}");
            }

            Quantity = Type == ItemType.Equipment ? (short)1 : quantity;
            if (equipped)
            {
                Slot = (short)GetEquippedSlot();
            }

            Expiration = expiration ?? DateConstants.Permanent;
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
                Thaw = CachedReference.Thaw;
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
                CAvoid = CachedReference.CAvoidability;
                CSpeed = CachedReference.CSpeed;
                CJump = CachedReference.CJump;
            }

            Summons = CachedReference.Summons;
        }

        public BasicItem(ItemEntity entity)
        {
            Id = entity.Id;
            Assigned = true;
            AccountId = entity.AccountId;
            MapleId = entity.MapleId;
            Quantity = entity.Quantity;
            Slot = entity.Slot;
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
                CAvoid = CachedReference.CAvoidability;
                CSpeed = CachedReference.CSpeed;
                CJump = CachedReference.CJump;
            }

            Summons = CachedReference.Summons;
        }

        public void Save()
        {
            using var dbContext = new MapleDbContext();
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

        public void Delete()
        {
            using var dbContext = new MapleDbContext();
            var item = dbContext.Items.Find(Id);
            if (item != null)
            {
                dbContext.Remove(item);
                dbContext.SaveChanges();
            }

            Assigned = false;
        }

        public byte[] ToByteArray(bool zeroPosition = false, bool leaveOut = false)
        {
            using var pw = new PacketWriter();
            if (!zeroPosition && !leaveOut)
            {
                var slot = ComputedSlot;
                pw.WriteByte(slot);
            }

            pw.WriteInt(MapleId);
            pw.WriteBool(IsCash);

            if (IsCash)
            {
                pw.WriteLong(1); // TODO: Unique Id for cash items. CashId
            }

            pw.WriteDateTime(Expiration);

            if (PetId != null)
            {
                pw.WriteString("Pet Name", 13);
                pw.WriteByte(1); // Level
                pw.WriteShort(0); // Closeness
                pw.WriteByte(0); // Fullness
                pw.WriteDateTime(DateConstants.Permanent); // Expiration
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
            }
            else
            {
                pw.WriteShort(Quantity);

                if (IsRechargeable)
                {
                    pw.WriteLong(0); // TODO: Unique Id.
                }
            }

            return pw.ToArray();
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
    }
}
