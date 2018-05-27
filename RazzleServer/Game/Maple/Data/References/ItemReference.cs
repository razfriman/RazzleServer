using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data
{
    public class ItemReference
    {
        public int MapleId { get; private set; }
        public short Slot { get; set; }
        public string Creator { get; set; }
        public DateTime Expiration { get; set; }
        public int? PetId { get; set; }

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

        public short MaxPerStack { get; private set; }


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
