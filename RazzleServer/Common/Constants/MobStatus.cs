using System;

namespace RazzleServer.Common.Constants
{
    [Flags]
    public enum MobStatus : int
    {
        None,

        WeaponAttackIcon = 0x01,
        WeaponDefenceIcon = 0x02,
        MagicAttackIcon = 0x04,
        MagicDefenceIcon = 0x08,
        AccuracyIcon = 0x10,
        AvoidabilityIcon = 0x20,
        SpeedIcon = 0x40,

        Stunned = 0x80,
        Frozen = 0x100,
        Poisoned = 0x200,
        Sealed = 0x400,

        Unknown1 = 0x800,

        WeaponAttackUp = 0x1000,
        WeaponDefenseUp = 0x2000,
        MagicAttackUp = 0x4000,
        MagicDefenseUp = 0x8000,

        Doom = 0x10000,
        ShadowWeb = 0x20000,

        WeaponImmunity = 0x40000,
        MagicImmunity = 0x80000,

        Unknown2 = 0x100000,
        Unknown3 = 0x200000,
        NinjaAmbush = 0x400000,
        Unknown4 = 0x800000,
        VenomousWeapon = 0x1000000,
        Unknown5 = 0x2000000,
        Unknown6 = 0x4000000,
        Empty = 0x8000000,
        Hypnotized = 0x10000000,
        WeaponDamageReflect = 0x20000000,
        MagicDamageReflect = 0x40000000
    }
}
