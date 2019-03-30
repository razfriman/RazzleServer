using System;

namespace RazzleServer.Common.Constants
{
    [Flags]
    public enum AdminLevel : byte
    {
        None = 0,
        LevelOne = 0x10,
        LevelTwo = 0x20,
        LevelThree = 0x40,
        LevelFour = 0x80
    }
}
