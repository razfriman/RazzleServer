using System;

namespace RazzleServer.Player
{
    [Flags]
    public enum MapleCharacterStat : long
    {
        Skin = 0x1, // byte
        Face = 0x2, // int
        Hair = 0x4, // int
        Level = 0x10, // byte
        Job = 0x20, // short
        Str = 0x40, // short
        Dex = 0x80, // short
        Int = 0x100, // short
        Luk = 0x200, // short
        Hp = 0x400, // int
        MaxHp = 0x800, // int
        Mp = 0x1000, // int
        MaxMp = 0x2000, // int
        Ap = 0x4000, // short
        Sp = 0x8000, // short (depends)
        Exp = 0x10000, // long
        Fame = 0x20000, // int
        Meso = 0x40000, // long
        Pet = 0x180008, // Pets: 0x8 + 0x80000 + 0x100000  [3 longs]
        GachaponExp = 0x200000, // int
        Fatigue = 0x400000, // byte
        Charisma = 0x800000, // ambition int
        Insight = 0x1000000,
        Will = 0x2000000, // int
        Craft = 0x4000000, // dilligence, int
        Sense = 0x8000000, // empathy, int
        Charm = 0x10000000, // int
        TraitLimit = 0x20000000, // 12 bytes
        BattleExp = 0x40000000, // byte, int, int
        BattleRank = 0x80000000, // byte
        BattlePoints = 0x100000000,
        IceGage = 0x200000000,
        Virtue = 0x400000000
    }
}