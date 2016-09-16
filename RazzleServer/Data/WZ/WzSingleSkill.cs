using System.Drawing;

namespace RazzleServer.Data.WZ
{
    //These skill doesn't have any levels like player skills

    public class WzFamiliarSkill
    {
        public int Prop { get; set; }
        public int Time { get; set; }
        public int AttackCount { get; set; }
        public int TargetCount { get; set; }
        public int Speed { get; set; }
        public bool Knockback { get; set; }
        Point LeftTop { get; set; }
        Point RightBottom { get; set; }
    }

    public class WzItemSkill
    {
       
    }
}
