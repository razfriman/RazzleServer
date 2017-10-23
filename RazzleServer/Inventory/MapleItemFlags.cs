using System;
namespace RazzleServer.Inventory
{
    [Flags]
    public enum MapleItemFlags : short
    {
        None = 0x0,
        Lock = 0x1,
        NoSlip = 0x2,
        ColdResistance = 0x4,
        Untradeable = 0x8,
        Karma = 0x10,
        Charm = 0x20,
        AndroidActivated = 0x40,
        Crafted = 0x80,
        CurseProtection = 0x100,
        LuckyDay = 0x200,
        KarmaAccUse = 0x400,
        KarmaAcc = 0x1000,
        UpgradeCountProtection = 0x2000, // Protects upgrade count
        ScrollProtection = 0x4000,

        KarmaUse = 0x2,
    }
}
