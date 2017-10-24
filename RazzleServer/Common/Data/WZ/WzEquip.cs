using System;

namespace RazzleServer.Data.WZ
{
    public class WzEquip : WzItem
    {
        public byte ReqJob { get; set; }
        public byte ReqLevel { get; set; }
        public int ReqFame { get; set; }
        public short ReqStr { get; set; }
        public short ReqDex { get; set; }
        public short ReqInt { get; set; }
        public short ReqLuk { get; set; }
        public short IncStr { get; set; }
        public short IncDex { get; set; }
        public short IncInt { get; set; }
        public short IncLuk { get; set; }
        public short IncPad { get; set; }
        public short IncMad { get; set; }
        public short IncPdd { get; set; }
        public short IncMdd { get; set; }
        public short IncMhp { get; set; }
        public short IncMmp { get; set; }
        public short IncAcc { get; set; }
        public short IncEva { get; set; }
        public short IncSpeed { get; set; }
        public short IncJump { get; set; }
        public byte TotalUpgradeCount { get; set; }
        public bool EquipTradeBlock { get; set; }
        public int SetBonusId { get; set; }
        public int SpecialId { get; set; }

        public int MaxStarEnhance
        {
            get
            {
                if (TotalUpgradeCount == 0) //Items without upgrades cannot be enhanced
                    return 0;
                if (ReqLevel < 95)
                    return 5;
                if (ReqLevel < 108)
                    return 8;
                if (ReqLevel < 118)
                    return 10;
                if (ReqLevel < 138)
                    return 12;
                return 15; //Everything level 138 and above
            }
        }

        public long RevealPotentialCost // Yes I shamelessly took this decompiled mess from a java src :/
        {
            get
            {
                int level = ReqLevel;
                int multiplyer;
                if (level > 120)
                {
                    multiplyer = 40;
                }
                else if (level > 70)
                {
                    multiplyer = 5;
                }
                else if (level > 30)
                {
                    multiplyer = 1;
                }
                else
                {
                    return 0;
                }
                int basePrice = ((level * level)* multiplyer);
                int floored = (int) (0.5*basePrice);
                int ceiled = (int)Math.Ceiling(0.5*basePrice);
                return floored;
            }
        }

        public byte PotentialLevel => (byte)(Math.Round(ReqLevel / 10.0));
    }
}
