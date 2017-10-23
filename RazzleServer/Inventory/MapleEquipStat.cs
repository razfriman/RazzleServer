using System;
namespace RazzleServer.Inventory
{
    [Flags]
    public enum MapleEquipStat : int
    {
        None = 0x0,
        RemainingUpgrades = 0x1, //Byte
        Upgrades = 0x2, //Byte
        Str = 0x4, //Short
        Dex = 0x8, //Short
        Int = 0x10, //Short
        Luk = 0x20, //Short
        IncMhp = 0x40, //Short
        IncMmp = 0x80, //Short
        Pad = 0x100, //Short
        Mad = 0x200, //Short
        Pdd = 0x400, //Short
        Mdd = 0x800, //Short
        Acc = 0x1000, //Short
        Eva = 0x2000, //Short
        Diligence = 0x4000, //Short
        Speed = 0x8000, //Short
        Jump = 0x10000, //Short
        Flag = 0x20000, //short
                        //0x40000, //unk
                        //0x80000, //unk
                        //0x100000, //unk
                        //0x200000, //unk
        Hammer = 0x400000, //int
                           //0x800000, //unk
                           //0x1000000, //unk
        FeverTime = 0x02000000 //short 0x4 as value
    }
}
