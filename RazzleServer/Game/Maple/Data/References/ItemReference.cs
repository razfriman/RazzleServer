using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class ItemReference
    {
        public int MapleId { get; set; }
        public short Slot { get; set; }
        public string Creator { get; set; }
        public DateTime Expiration { get; set; }
        public int? PetId { get; set; }
        public bool IsCash { get; set; }
        public bool OnlyOne { get; set; }
        public bool PreventsSlipping { get; set; }
        public bool PreventsColdness { get; set; }
        public bool IsTradeBlocked { get; set; }
        public bool IsStored { get; set; }
        public int SalePrice { get; set; }
        public int Meso { get; set; }
        public byte UpgradesAvailable { get; set; }
        public byte UpgradesApplied { get; set; }
        public short Strength { get; set; }
        public short Dexterity { get; set; }
        public short Intelligence { get; set; }
        public short Luck { get; set; }
        public short Health { get; set; }
        public short Mana { get; set; }
        public short WeaponAttack { get; set; }
        public short MagicAttack { get; set; }
        public short WeaponDefense { get; set; }
        public short MagicDefense { get; set; }
        public short Accuracy { get; set; }
        public short Avoidability { get; set; }
        public short Agility { get; set; }
        public short Speed { get; set; }
        public short Jump { get; set; }
        public byte AttackSpeed { get; set; }
        public short RecoveryRate { get; set; }
        public short KnockBackChance { get; set; }
        public short RequiredLevel { get; set; }
        public short RequiredStrength { get; set; }
        public short RequiredDexterity { get; set; }
        public short RequiredIntelligence { get; set; }
        public short RequiredLuck { get; set; }
        public short RequiredFame { get; set; }
        public Job RequiredJob { get; set; }

        // Consume data properties are prefixed with 'C'.
        public int CItemId { get; set; }
        public string CFlags { get; set; }
        public string CCureAilments { get; set; }
        public short CEffect { get; set; }
        public short CHealth { get; set; }
        public short CMana { get; set; }
        public short CHealthPercentage { get; set; }
        public short CManaPercentage { get; set; }
        public int CMoveTo { get; set; }
        public short CProb { get; set; }
        public int CBuffTime { get; set; }
        public short CWeaponAttack { get; set; }
        public short CMagicAttack { get; set; }
        public short CWeaponDefense { get; set; }
        public short CMagicDefense { get; set; }
        public short CAccuracy { get; set; }
        public short CAvoid { get; set; }
        public short CSpeed { get; set; }
        public short CJump { get; set; }
        public short CMorph { get; set; }
        public short MaxPerStack { get; set; }
        public List<Tuple<int, short>> Summons { get; set; }

        public ItemReference()
        {
        }

        public ItemReference(WzImage img, ItemType type)
        {
            var name = img.Name.Remove(8);
            var info = img["info"];

            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleId = id;
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
            AttackSpeed = (byte)(info["attackSpeed"]?.GetInt() ?? 0);
            UpgradesAvailable = (byte)(info["tuc"]?.GetInt() ?? 0);
            WeaponAttack = info["incPAD"]?.GetShort() ?? 0;
            Accuracy = info["incACC"]?.GetShort() ?? 0;
            Jump = info["incJump"]?.GetShort() ?? 0;
            Speed = info["incSpeed"]?.GetShort() ?? 0;
            Avoidability = info["incEVA"]?.GetShort() ?? 0;
            MagicDefense = info["incMDD"]?.GetShort() ?? 0;
            WeaponDefense = info["incPDD"]?.GetShort() ?? 0;
            MagicAttack = info["incMAD"]?.GetShort() ?? 0;
            Strength = info["incSTR"]?.GetShort() ?? 0;
            Dexterity = info["incJump"]?.GetShort() ?? 0;
            Intelligence = info["incJump"]?.GetShort() ?? 0;;
            Luck = info["incJump"]?.GetShort() ?? 0;;
            Health = info["incJump"]?.GetShort() ?? 0;;
            Mana = info["incJump"]?.GetShort() ?? 0;;
        }
        public ItemReference(WzImageProperty img, ItemType type)
        {
            var name = img.Name;
            var info = img["info"];
            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleId = id;
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

        [JsonIgnore]
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

    }
}
