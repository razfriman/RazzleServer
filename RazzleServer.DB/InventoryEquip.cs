using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.DB
{
    public class InventoryEquip
    {
        public long ID { get; set; }
        public long InventoryItemID { get; set; }
        public byte RemainingUpgradeCount { get; set; }
        public byte UpgradeCount { get; set; }
        public short Str { get; set; }
        public short Dex { get; set; }
        public short Luk { get; set; }
        public short Int { get; set; }
        public short IncMaxHP { get; set; }
        public short IncMaxMP { get; set; }
        public short Pad { get; set; }
        public short Mad { get; set; }
        public short Pdd { get; set; }
        public short Mdd { get; set; }
        public short Acc { get; set; }
        public short Eva { get; set; }
        public short Speed { get; set; }
        public short Jump { get; set; }
        public short Diligence { get; set; }
        public byte PotentialState { get; set; }
        public short Potential1 { get; set; }
        public short Potential2 { get; set; }
        public short Potential3 { get; set; }
        public short BonusPotential1 { get; set; }
        public short BonusPotential2 { get; set; }
        public short Socket1 { get; set; }
        public short Socket2 { get; set; }
        public short Socket3 { get; set; }
        public int Durability { get; set; }
        public short CustomExp { get; set; }
        public byte HammerApplied { get; set; }
        public byte Enhancements { get; set; }
        public byte CustomLevel { get; set; }
    }
}
