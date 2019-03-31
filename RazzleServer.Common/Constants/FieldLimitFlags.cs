using System;

namespace RazzleServer.Common.Constants
{
    [Flags]
    public enum FieldLimitFlags
    {
        MoveLimit = 0x01,
        SkillLimit = 0x02,
        SummonLimit = 0x04,
        MysticDoorLimit = 0x08,
        
        MigrateLimit = 0x10,
        PortalScrollLimit = 0x20,
        TeleportItemLimit = 0x40,
        MinigameLimit = 0x80,
        SpecificPortalScrollLimit = 0x100,
        TamingMobLimit = 0x200,
        StatChangeItemConsumeLimit = 0x400,
        PartyBossChangeLimit = 0x800,
        NoMobCapacityLimit = 0x1000,
        WeddingInvitationLimit = 0x2000,
        CashWeatherConsumeLimit = 0x4000,
        NoPet = 0x8000,
        AntiMacroLimit = 0x10000,
        FallDownLimit = 0x20000,
        SummonNpcLimit = 0x40000,
        NoExpDecrease = 0x80000,
        NoDamageOnFalling = 0x100000,
        ParcelOpenLimit = 0x200000,
        DropLimit = 0x400000
    }
}
