using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.WzLib;
using RazzleServer.Data;
using RazzleServer.DB.Models;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple
{
    public class Item : Drop
    {
        public static ItemType GetType(int mapleID) => (ItemType)(mapleID / 1000000);

        public CharacterItems Parent { get; set; }

        public int ID { get; private set; }
        public int AccountID { get; private set; }
        public int MapleID { get; private set; }
        public short Slot { get; set; }
        private short maxPerStack;
        private short quantity;
        public string Creator { get; set; }
        public DateTime Expiration { get; set; }
        public int? PetID { get; set; }

        public bool IsCash { get; private set; }
        public bool OnlyOne { get; private set; }
        public bool PreventsSlipping { get; private set; }
        public bool PreventsColdness { get; private set; }
        public bool IsTradeBlocked { get; private set; }
        public bool IsStored { get; set; }
        public int SalePrice { get; private set; }
        public int Meso { get; private set; }

        public byte UpgradesAvailable { get; private set; }
        public byte UpgradesApplied { get; private set; }
        public short Strength { get; private set; }
        public short Dexterity { get; private set; }
        public short Intelligence { get; private set; }
        public short Luck { get; private set; }
        public short Health { get; private set; }
        public short Mana { get; private set; }
        public short WeaponAttack { get; private set; }
        public short MagicAttack { get; private set; }
        public short WeaponDefense { get; private set; }
        public short MagicDefense { get; private set; }
        public short Accuracy { get; private set; }
        public short Avoidability { get; private set; }
        public short Agility { get; private set; }
        public short Speed { get; private set; }
        public short Jump { get; private set; }

        public byte AttackSpeed { get; private set; }
        public short RecoveryRate { get; private set; }
        public short KnockBackChance { get; private set; }

        public short RequiredLevel { get; private set; }
        public short RequiredStrength { get; private set; }
        public short RequiredDexterity { get; private set; }
        public short RequiredIntelligence { get; private set; }
        public short RequiredLuck { get; private set; }
        public short RequiredFame { get; private set; }
        public Job RequiredJob { get; private set; }

        // Consume data properties are prefixed with 'C'.
        public int CItemId { get; private set; }
        public string CFlags { get; private set; }
        public string CCureAilments { get; private set; }
        public short CEffect { get; private set; }
        public short CHealth { get; private set; }
        public short CMana { get; private set; }
        public short CHealthPercentage { get; private set; }
        public short CManaPercentage { get; private set; }
        public int CMoveTo { get; private set; }
        public short CProb { get; private set; }
        public int CBuffTime { get; private set; }
        public short CWeaponAttack { get; private set; }
        public short CMagicAttack { get; private set; }
        public short CWeaponDefense { get; private set; }
        public short CMagicDefense { get; private set; }
        public short CAccuracy { get; private set; }
        public short CAvoid { get; private set; }
        public short CSpeed { get; private set; }
        public short CJump { get; private set; }
        public short CMorph { get; private set; }

        public List<Tuple<int, short>> Summons { get; private set; }

        public ItemType Type => GetType(MapleID);

        public WeaponType WeaponType
        {
            get
            {
                switch (MapleID / 10000 % 100)
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

        public Item CachedReference => DataProvider.Items[MapleID];

        public Character Character => Parent.Parent;

        public short MaxPerStack
        {
            get
            {
                if (IsRechargeable && Parent != null)
                {
                    return maxPerStack;
                }
                else
                {
                    return maxPerStack;
                }
            }
            set
            {
                maxPerStack = value;
            }
        }

        public short Quantity
        {
            get => quantity;
            set
            {
                if (value > MaxPerStack)
                {
                    throw new ArgumentException("Quantity too high.");
                }
                else
                {
                    quantity = value;
                }
            }
        }

        public bool IsSealed => DataProvider.Items.WizetItemIDs.Contains(MapleID);

        public byte Flags
        {
            get
            {
                byte flags = 0;

                if (IsSealed) flags |= (byte)ItemFlags.Sealed;
                if (PreventsSlipping) flags |= (byte)ItemFlags.AddPreventSlipping;
                if (PreventsColdness) flags |= (byte)ItemFlags.AddPreventColdness;
                if (IsTradeBlocked) flags |= (byte)ItemFlags.Untradeable;

                return flags;
            }
        }

        public bool IsEquipped => Slot < 0;

        public bool IsEquippedCash => Slot < -100;

        public bool IsConsumable => MapleID / 10000 >= 200 && MapleID / 10000 < 204;

        public bool IsRechargeable => IsThrowingStar || IsBullet;

        public bool IsThrowingStar => MapleID / 10000 == 207;

        public bool IsBullet => MapleID / 10000 == 233;

        public bool IsArrow => IsArrowForBow || IsArrowForCrossbow;

        public bool IsArrowForBow => MapleID >= 2060000 && MapleID < 2061000;

        public bool IsArrowForCrossbow => MapleID >= 2061000 && MapleID < 2062000;

        public bool IsOverall => MapleID / 10000 == 105;

        public bool IsWeapon => WeaponType != WeaponType.NotAWeapon;

        public bool IsShield => MapleID / 10000 % 100 == 9;

        public bool IsPet => MapleID >= 5000000 && MapleID <= 5000100;

        public bool IsTownScroll => MapleID >= 2030000 && MapleID < 2030020;

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
                else
                {
                    throw new InvalidOperationException("Attempting to retrieve absolute slot for non-equipped item.");
                }
            }
        }

        public byte ComputedSlot
        {
            get
            {
                if (IsEquippedCash)
                {
                    return ((byte)(AbsoluteSlot - 100));
                }
                else if (IsEquipped)
                {
                    return AbsoluteSlot;
                }
                else
                {
                    return (byte)Slot;
                }
            }
        }

        public bool Assigned { get; set; }

        public Item(int mapleID, short quantity = 1, DateTime? expiration = null, bool equipped = false)
        {
            MapleID = mapleID;
            MaxPerStack = CachedReference.MaxPerStack;
            Quantity = (Type == ItemType.Equipment) ? (short)1 : quantity;
            if (equipped) Slot = (short)GetEquippedSlot();

            if (!expiration.HasValue)
            {
                expiration = new DateTime(2079, 1, 1, 12, 0, 0); // NOTE: Default expiration time (permanent).
            }

            Expiration = (DateTime)expiration;

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

        public Item(WzImage img, ItemType type)
        {
            var name = img.Name.Remove(8);
            var info = img["info"];

            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleID = id;
            Summons = new List<Tuple<int, short>>();
            MaxPerStack = info["slotMax"]?.GetShort() ?? 1;
            IsCash = (info["cash"]?.GetInt() ?? 0) > 0;
            Mana = info["recoveryMP"]?.GetShort() ?? 0;
            Health = info["recoveryHP"]?.GetShort() ?? 0;
            IsTradeBlocked = (info["tradeBlock"]?.GetInt() ?? 0) > 0;
            SalePrice = info["price"]?.GetInt() ?? 0;
            OnlyOne = (info["only"]?.GetInt() ?? 0) > 0;
            RequiredJob = Job.Beginner;
            RequiredLevel = info["reqLevel"]?.GetShort() ?? 0;
            RequiredStrength = info["reqSTR"]?.GetShort() ?? 0;
            RequiredDexterity = info["reqDEX"]?.GetShort() ?? 0;
            RequiredIntelligence = info["reqINT"]?.GetShort() ?? 0;
            RequiredLuck = info["reqLUK"]?.GetShort() ?? 0;
            RequiredFame = info["reqLevel"]?.GetShort() ?? 0;
            AttackSpeed = (byte)(info["reqLevel"]?.GetInt() ?? 0);
            //<string name="islot" value="Wp"/>
            //<string name="vslot" value="Wp"/>
            //<int name="walk" value="1"/>
            //<int name="stand" value="1"/>
            //<short name="attack" value="1"/>
            //<string name="afterImage" value="swordOL"/>
            //<string name="sfx" value="swordL"/>
            //<int name="incPAD" value="17"/>
            //<int name="tuc" value="7"/>
        }
        public Item(WzImageProperty img, ItemType type)
        {
            var name = img.Name;
            var info = img["info"];
            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleID = id;
            Summons = new List<Tuple<int, short>>();
            MaxPerStack = info["slotMax"]?.GetShort() ?? 1;
            IsCash = (info["cash"]?.GetInt() ?? 0) > 0;
            Mana = info["recoveryMP"]?.GetShort() ?? 0;
            Health = info["recoveryHP"]?.GetShort() ?? 0;
            RequiredLevel = info["reqLevel"]?.GetShort() ?? 0;
            IsTradeBlocked = (info["tradeBlock"]?.GetInt() ?? 0) > 0;
            SalePrice = info["price"]?.GetInt() ?? 0;
            OnlyOne = (info["only"]?.GetInt() ?? 0) > 0;
        }

        public Item(ItemEntity datum)
        {
            ID = datum.ID;
            Assigned = true;
            AccountID = datum.AccountID;
            MapleID = datum.MapleID;
            MaxPerStack = CachedReference.MaxPerStack;
            Quantity = datum.Quantity;
            Slot = datum.Slot;
            Creator = datum.Creator;
            Expiration = datum.Expiration;
            PetID = datum.PetID;

            IsCash = CachedReference.IsCash;
            OnlyOne = CachedReference.OnlyOne;
            IsTradeBlocked = CachedReference.IsTradeBlocked;
            IsStored = datum.IsStored;
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

                UpgradesAvailable = datum.UpgradesAvailable;
                UpgradesApplied = datum.UpgradesApplied;
                Strength = datum.Strength;
                Dexterity = datum.Dexterity;
                Intelligence = datum.Intelligence;
                Luck = datum.Luck;
                Health = datum.Health;
                Mana = datum.Mana;
                WeaponAttack = datum.WeaponAttack;
                MagicAttack = datum.MagicAttack;
                WeaponDefense = datum.WeaponDefense;
                MagicDefense = datum.MagicDefense;
                Accuracy = datum.Accuracy;
                Avoidability = datum.Avoidability;
                Agility = datum.Agility;
                Speed = datum.Speed;
                Jump = datum.Jump;
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

        public void LoadConsumeData(Datum datum)
        {
            //this.CFlags = datum["flags"];
            //this.CCureAilments = datum["cure_ailments"];
            CEffect = (byte)datum["effect"];
            CHealth = (short)datum["hp"];
            CMana = (short)datum["mp"];
            CHealthPercentage = (short)datum["hp_percentage"];
            CManaPercentage = (short)datum["mp_percentage"];
            CMoveTo = (int)datum["move_to"];
            CProb = (byte)datum["prob"];
            CBuffTime = (int)datum["buff_time"];
            CWeaponAttack = (short)datum["weapon_attack"];
            CMagicAttack = (short)datum["magic_attack"];
            CWeaponDefense = (short)datum["weapon_defense"];
            CMagicAttack = (short)datum["magic_attack"];
            CAccuracy = (short)datum["accuracy"];
            CAvoid = (short)datum["avoid"];
            CSpeed = (short)datum["speed"];
            CJump = (short)datum["jump"];
            CMorph = (short)datum["morph"];
        }

        public void LoadEquipmentData(Datum datum)
        {
            RequiredStrength = (short)datum["req_str"];
            RequiredDexterity = (short)datum["req_dex"];
            RequiredIntelligence = (short)datum["req_int"];
            RequiredLuck = (short)datum["req_luk"];
            RequiredFame = (short)datum["req_fame"];

            UpgradesAvailable = (byte)(ushort)datum["scroll_slots"];
            UpgradesApplied = 0;

            Health = (short)datum["hp"];
            Mana = (short)datum["mp"];
            Strength = (short)datum["strength"];
            Dexterity = (short)datum["dexterity"];
            Intelligence = (short)datum["intelligence"];
            Luck = (short)datum["luck"];
            WeaponAttack = (short)datum["weapon_attack"];
            WeaponDefense = (short)datum["weapon_defense"];
            MagicAttack = (short)datum["magic_attack"];
            MagicDefense = (short)datum["magic_defense"];
            Accuracy = (short)datum["accuracy"];
            Avoidability = (short)datum["avoid"];
            Speed = (short)datum["speed"];
            Jump = (short)datum["jump"];
            Agility = (short)datum["hands"];
        }

        public void Save()
        {
            Datum datum = new Datum("items");

            datum["AccountID"] = Character.AccountID;
            datum["CharacterID"] = Character.ID;
            datum["MapleID"] = MapleID;
            datum["Quantity"] = Quantity;
            datum["Slot"] = Slot;
            datum["Creator"] = Creator;
            datum["UpgradesAvailable"] = UpgradesAvailable;
            datum["UpgradesApplied"] = UpgradesApplied;
            datum["Strength"] = Strength;
            datum["Dexterity"] = Dexterity;
            datum["Intelligence"] = Intelligence;
            datum["Luck"] = Luck;
            datum["Health"] = Health;
            datum["Mana"] = Mana;
            datum["WeaponAttack"] = WeaponAttack;
            datum["MagicAttack"] = MagicAttack;
            datum["WeaponDefense"] = WeaponDefense;
            datum["MagicDefense"] = MagicDefense;
            datum["Accuracy"] = Accuracy;
            datum["Avoidability"] = Avoidability;
            datum["Agility"] = Agility;
            datum["Speed"] = Speed;
            datum["Jump"] = Jump;
            datum["IsStored"] = IsStored;
            datum["PreventsSlipping"] = PreventsSlipping;
            datum["PreventsColdness"] = PreventsColdness;

            if (Assigned)
            {
                datum.Update("ID = {0}", ID);
            }
            else
            {
                ID = datum.InsertAndReturnID();
                Assigned = true;
            }
        }

        public void Delete()
        {
            using (var dbContext = new MapleDbContext())
            {
                var item = dbContext.Items.Find(ID);
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

            short sourceSlot = Slot;
            EquipmentSlot destinationSlot = GetEquippedSlot();

            Item top = Parent[EquipmentSlot.Top];
            Item bottom = Parent[EquipmentSlot.Bottom];
            Item weapon = Parent[EquipmentSlot.Weapon];
            Item shield = Parent[EquipmentSlot.Shield];

            Item destination = Parent[destinationSlot];

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
                        if (IsOverall && bottom != null)
                        {
                            bottom.Unequip();
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
                        if (IsTwoHanded && shield != null)
                        {
                            shield.Unequip();
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

            short sourceSlot = Slot;

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

                Item dropped = new Item(MapleID, quantity)
                {
                    Dropper = Character,
                    Owner = null
                };

                Character.Map.Drops.Add(dropped);
            }
        }

        public void Move(short destinationSlot)
        {
            short sourceSlot = Slot;

            Item destination = Parent[Type, destinationSlot];

            if (destination != null &&
                Type != ItemType.Equipment &&
                MapleID == destination.MapleID &&
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
            using (var oPacket = new PacketWriter())
            {
                if (!zeroPosition && !leaveOut)
                {
                    byte slot = ComputedSlot;

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
                        oPacket.WriteShort(slot);
                    }
                    else
                    {
                        oPacket.WriteByte(slot);
                    }
                }

                oPacket.WriteByte((byte)(PetID != null ? 3 : Type == ItemType.Equipment ? 1 : 2));
                oPacket.WriteInt(MapleID);
                oPacket.WriteBool(IsCash);

                if (IsCash)
                {
                    oPacket.WriteLong(1); // TODO: Unique ID for cash items.
                }

                oPacket.WriteDateTime(Expiration);

                if (PetID != null)
                {

                }
                else if (Type == ItemType.Equipment)
                {
                    oPacket.WriteByte(UpgradesAvailable);
                    oPacket.WriteByte(UpgradesApplied);
                    oPacket.WriteShort(Strength);
                    oPacket.WriteShort(Dexterity);
                    oPacket.WriteShort(Intelligence);
                    oPacket.WriteShort(Luck);
                    oPacket.WriteShort(Health);
                    oPacket.WriteShort(Mana);
                    oPacket.WriteShort(WeaponAttack);
                    oPacket.WriteShort(MagicAttack);
                    oPacket.WriteShort(WeaponDefense);
                    oPacket.WriteShort(MagicDefense);
                    oPacket.WriteShort(Accuracy);
                    oPacket.WriteShort(Avoidability);
                    oPacket.WriteShort(Agility);
                    oPacket.WriteShort(Speed);
                    oPacket.WriteShort(Jump);
                    oPacket.WriteString(Creator);
                    oPacket.WriteByte(Flags);
                    oPacket.WriteByte(0);

                    if (!IsEquippedCash)
                    {
                        oPacket.WriteByte(0);
                        oPacket.WriteByte(0);
                        oPacket.WriteShort(0);
                        oPacket.WriteShort(0);
                        oPacket.WriteInt(0);
                        oPacket.WriteLong(0);
                        oPacket.WriteLong(0);
                        oPacket.WriteInt(-1);
                    }
                }
                else
                {
                    oPacket.WriteShort(Quantity);
                    oPacket.WriteString(Creator);
                    oPacket.WriteByte(Flags);
                    oPacket.WriteByte(0);

                    if (IsRechargeable)
                    {
                        oPacket.WriteLong(0); // TODO: Unique ID.
                    }
                }

                return oPacket.ToArray();
            }
        }

        private EquipmentSlot GetEquippedSlot()
        {
            short slot = 0;

            if (MapleID >= 1000000 && MapleID < 1010000)
            {
                slot -= 1;
            }
            else if (MapleID >= 1010000 && MapleID < 1020000)
            {
                slot -= 2;
            }
            else if (MapleID >= 1020000 && MapleID < 1030000)
            {
                slot -= 3;
            }
            else if (MapleID >= 1030000 && MapleID < 1040000)
            {
                slot -= 4;
            }
            else if (MapleID >= 1040000 && MapleID < 1060000)
            {
                slot -= 5;
            }
            else if (MapleID >= 1060000 && MapleID < 1070000)
            {
                slot -= 6;
            }
            else if (MapleID >= 1070000 && MapleID < 1080000)
            {
                slot -= 7;
            }
            else if (MapleID >= 1080000 && MapleID < 1090000)
            {
                slot -= 8;
            }
            else if (MapleID >= 1102000 && MapleID < 1103000)
            {
                slot -= 9;
            }
            else if (MapleID >= 1092000 && MapleID < 1100000)
            {
                slot -= 10;
            }
            else if (MapleID >= 1300000 && MapleID < 1800000)
            {
                slot -= 11;
            }
            else if (MapleID >= 1112000 && MapleID < 1120000)
            {
                slot -= 12;
            }
            else if (MapleID >= 1122000 && MapleID < 1123000)
            {
                slot -= 17;
            }
            else if (MapleID >= 1900000 && MapleID < 2000000)
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
            var oPacket = new PacketWriter(ServerOperationCode.Message);

            oPacket.WriteByte((byte)MessageType.DropPickup);
            oPacket.WriteBool(false);
            oPacket.WriteInt(MapleID);
            oPacket.WriteInt(Quantity);
            oPacket.WriteInt(0);
            oPacket.WriteInt(0);

            return oPacket;
        }
    }
}
