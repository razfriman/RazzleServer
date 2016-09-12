using RazzleServer.Player;

namespace RazzleServer.Map.Monster
{
    public static class MonsterBuffStat
    {
        public static readonly BuffStat WATK = new BuffStat(0);
        public static readonly BuffStat WDEF = new BuffStat(1);
        public static readonly BuffStat MATK = new BuffStat(2);
        public static readonly BuffStat MDEF = new BuffStat(3);
        public static readonly BuffStat ACC = new BuffStat(4);
        public static readonly BuffStat AVOID = new BuffStat(5);
        public static readonly BuffStat FREEZE = new BuffStat(6, false, true);
        public static readonly BuffStat STUN = new BuffStat(7);
        public static readonly BuffStat IMMOBILIZE = new BuffStat(8);
        public static readonly BuffStat DAM_R_TAKEN = new BuffStat(45);
        public static readonly BuffStat POISON = new BuffStat(58);
    }
}