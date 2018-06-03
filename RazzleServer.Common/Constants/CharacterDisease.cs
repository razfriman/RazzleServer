using System;

namespace RazzleServer.Common.Constants
{
    [Flags]
    public enum CharacterDisease : ulong
    {
        None = 0x0L,
        Slow = 0x1L,
        Seduce = 0x80L,
        Fishable = 0x100L,
        Confuse = 0x80000L,
        Stun = 0x2000000000000L,
        Poison = 0x4000000000000L,
        Sealed = 0x8000000000000L,
        Darkness = 0x10000000000000L,
        Weaken = 0x4000000000000000L,
        Curse = 0x8000000000000000L
    }
}
