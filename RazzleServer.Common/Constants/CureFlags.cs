using System;

namespace RazzleServer.Common.Constants
{
    [Flags]
    public enum CureFlags
    {
        Poison = 0x1,
        Weakness = 0x2,
        Curse = 0x4,
        Darkness = 0x8,
        Seal = 0x10,
    }
}
