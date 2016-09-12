using System.Collections.Generic;

namespace RazzleServer.Player
{
    public class AttackInfo
    {
        public int SkillID { get; set; }
        public byte SkillLevel { get; set; }
        public List<AttackPair> TargetDamageList { get; set; }
        public Point Position { get; set; }
        public int Charge { get; set; }
        public short Display { get; set; }
        public int Attacks { get; set; }
        public int Targets { get; set; }
        public byte AttacksByte { get; set; }
        public byte Speed { get; set; }
        public byte Unk { get; set; }

        public AttackInfo()
        {
            TargetDamageList = new List<AttackPair>();
        }
    }
}
