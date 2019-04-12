using System;

namespace RazzleServer.Common.Constants
{
    [Flags]
    public enum BuffValueTypes : uint
    {
        WeaponAttack = 0x01,
        WeaponDefense = 0x02,
        MagicAttack = 0x04,
        MagicDefense = 0x08,

        Accuracy = 0x10,
        Avoidability = 0x20,
        Hands = 0x40, // Yes, this has a modifier too.
        Speed = 0x80,

        Jump = 0x100,
        MagicGuard = 0x200,
        DarkSight = 0x400,
        Booster = 0x800,

        PowerGuard = 0x1000,
        MaxHealth = 0x2000,
        MaxMana = 0x4000,
        Invincible = 0x8000,

        SoulArrow = 0x10000,
        Stun = 0x020000, // Mob Skill: Stun and Dragon Roar
        Poison = 0x40000, // Mob Skill: Poison
        Seal = 0x80000, // Mob Skill: Seal

        Darkness = 0x100000, // Mob Skill: Darkness
        ComboAttack = 0x200000,
        Charges = 0x400000,
        DragonBlood = 0x800000,

        HolySymbol = 0x1000000,
        MesoUp = 0x2000000,
        ShadowPartner = 0x4000000,
        PickPocketMesoUp = 0x8000000,

        MesoGuard = 0x10000000,
        Thaw = 0x20000000,
        Weakness = 0x40000000, // Mob Skill: Weakness
        Curse = 0x80000000, // Mob Skill: Curse

        All = 0xFFFFFFFF,
        SpeedBuffElement = Speed | Jump | Stun | Weakness
    }
}
