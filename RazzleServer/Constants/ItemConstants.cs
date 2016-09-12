using NLog;
using RazzleServer.Inventory;
using RazzleServer.Player;
using System;
using System.Collections.Generic;

namespace RazzleServer.Constants
{
    public static class ItemConstants
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();


        #region Weapon Damage Modifiers
        private static readonly WeaponInfo defaultWeaponModInfo = new WeaponInfo(1.3, 20);
        private static Dictionary<MapleItemType, WeaponInfo> WeaponInfo = new Dictionary<MapleItemType, WeaponInfo>()
        {
            { MapleItemType.SoulShooter, new WeaponInfo(1.375, 15) }, //Angelic Buster
            { MapleItemType.Desperado, new WeaponInfo(1.0, 20) }, //Demon Avenger
            { MapleItemType.WhipBlade, new WeaponInfo(1.315, 20) }, //Xenon
            { MapleItemType.Scepter, new WeaponInfo(1.338, 20) }, //BeastTamer
            { MapleItemType.OneHandedSword, new WeaponInfo(1.2, 20) },
            { MapleItemType.OneHandedAxe, new WeaponInfo(1.2, 20) },
            { MapleItemType.OneHandedMace, new WeaponInfo(1.2, 20) },
            { MapleItemType.Dagger, new WeaponInfo(1.56, 20) },
            { MapleItemType.Cane, new WeaponInfo(1.3, 20) },
            { MapleItemType.Wand, new WeaponInfo(1.2, 25) },
            { MapleItemType.Staff, new WeaponInfo(1.2, 25) },
            { MapleItemType.TwoHandedSword, new WeaponInfo(1.34, 20) },
            { MapleItemType.TwoHandedAxe, new WeaponInfo(1.34, 20) },
            { MapleItemType.TwoHandedMace, new WeaponInfo(1.34, 20) },
            { MapleItemType.Spear, new WeaponInfo(1.49, 20) },
            { MapleItemType.Polearm, new WeaponInfo(1.49, 20) },
            { MapleItemType.Bow, new WeaponInfo(1.3, 15) },
            { MapleItemType.Crossbow, new WeaponInfo(1.35, 15) },
            { MapleItemType.Claw, new WeaponInfo(1.75, 15) },
            { MapleItemType.Knuckle, new WeaponInfo(1.7, 20) },
            { MapleItemType.Gun, new WeaponInfo(1.5, 15) },
            { MapleItemType.DualBowGun, new WeaponInfo(1.3, 15) },
            { MapleItemType.Cannon, new WeaponInfo(1.5, 15) },
            { MapleItemType.Katana, new WeaponInfo(1.25, 20) }, //Hayato
            { MapleItemType.Fan, new WeaponInfo(1.35, 25) }, //Kanna
            { MapleItemType.BigSword, new WeaponInfo(1.3, 15) }, //Zero
            { MapleItemType.LongSword, new WeaponInfo(1.3, 15) } //Zero
        };
        #endregion

        public static WeaponInfo GetWeaponModifierInfo(MapleItemType weaponType)
        {
            WeaponInfo ret;
            if (WeaponInfo.TryGetValue(weaponType, out ret))
                return ret;
            Log.Warn($"Unhandled MapleItemType [{Enum.GetName(typeof(MapleItemType), weaponType)}] for getting Weapon Modifier Info in ItemConstants");
            return defaultWeaponModInfo;
        }

        public static MapleInventoryType GetInventoryType(int itemId)
        {
            byte type = (byte)(itemId / 1000000);
            if (type < 1 || type > 5)
            {
                return MapleInventoryType.Undefined;
            }
            return (MapleInventoryType)type;
        }

        public static bool IsWeapon(int itemId) => IsWeapon(GetMapleItemType(itemId));

        public static bool IsWeapon(MapleItemType itemType) =>
            (itemType > MapleItemType.Totem && itemType <= MapleItemType.CashShopEffectWeapon) &&
            itemType != MapleItemType.HerbalismTool && itemType != MapleItemType.MiningTool &&
            !IsOffhand(itemType);

        public static bool IsOffhand(int itemId) => IsOffhand(GetMapleItemType(itemId));

        public static bool IsOffhand(MapleItemType itemType) => itemType == MapleItemType.SecondaryWeapon || itemType == MapleItemType.Shield;

        public static bool IsAccessory(int itemId) => IsAccessory(GetMapleItemType(itemId));

        public static bool IsAccessory(MapleItemType itemType) =>
                (itemType >= MapleItemType.FaceAccessory && itemType <= MapleItemType.Top) ||
                (itemType >= MapleItemType.Ring && itemType <= MapleItemType.MonsterBook) ||
                (itemType >= MapleItemType.Badge && itemType <= MapleItemType.Emblem);

        public static MapleItemType GetMapleItemType(int itemId)
        {
            int itemBase = itemId / 10000;
            if (Enum.IsDefined(typeof(MapleItemType), itemBase))
                return (MapleItemType)itemBase;
            return MapleItemType.Undefined;
        }
    }
    public class WeaponInfo
    {
        public double DamageModifier;
        public int BaseMastery;

        public WeaponInfo(double mod, int baseMastery)
        {
            DamageModifier = mod;
            BaseMastery = baseMastery;
        }
    }
}
