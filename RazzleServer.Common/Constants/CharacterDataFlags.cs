using System;

namespace RazzleServer.Common.Constants
{
    [Flags]
    public enum CharacterDataFlags : ushort
    {
        None = 0x00,

        Stats = 0x01,
        Money = 0x02,
        Equipment = 0x04,
        Usable = 0x08,

        Setup = 0x10,
        Etcetera = 0x20,
        Pet = 0x40,
        Skills = 0x80,

        Quests = 0x100,
        MinigameStats = 0x200,
        FriendRing = 0x400,
        TeleportRock = 0x800,

        All = 0xFFFF,
        Items = Equipment | Usable | Setup | Etcetera | Pet,
        CashShop = Stats | Money | Items | Skills
    }
}
