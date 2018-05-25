using System;

namespace RazzleServer.Common.Constants
{
    [Flags]
    public enum FieldLimitFlags
    {
        MOVE_LIMIT = 0x01,
        SKILL_LIMIT = 0x02,
        SUMMON_LIMIT = 0x04,
        MYSTIC_DOOR_LIMIT = 0x08,
        MIGRATE_LIMIT = 0x10,
        PORTAL_SCROLL_LIMIT = 0x40,
        MINIGAME_LIMIT = 0x80,
        SPECIFIC_PORTAL_SCROLL_LIMIT = 0x100,
        TAMING_MOB_LIMIT = 0x200,
        STAT_CHANGE_ITEM_CONSUME_LIMIT = 0x400,
        PARTY_BOSS_CHANGE_LIMIT = 0x800,
        NO_MOB_CAPACITY_LIMIT = 0x1000,
        WEDDING_INVITATION_LIMIT = 0x2000,
        CASH_WEATHER_CONSUME_LIMIT = 0x4000,
        NO_PET = 0x8000,
        ANTI_MACRO_LIMIT = 0x10000,
        FALL_DOWN_LIMIT = 0x20000,
        SUMMON_NPC_LIMIT = 0x40000,
        NO_EXP_DECREASE = 0x80000,
        NO_DAMAGE_ON_FALLING = 0x100000,
        PARCEL_OPEN_LIMIT = 0x200000,
        DROP_LIMIT = 0x400000
    }
}
