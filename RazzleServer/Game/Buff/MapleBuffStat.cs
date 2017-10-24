using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Player
{
    public class MapleBuffStat
    {
        public static readonly BuffStat STACKING_WDEF = new BuffStat(2, false, true);
        public static readonly BuffStat STACKING_MDEF = new BuffStat(3, false, true);
        public static readonly BuffStat STACKING_MAXHP = new BuffStat(4, false, true);
        public static readonly BuffStat STACKING_MAXHP_R = new BuffStat(5, false, true);
        public static readonly BuffStat STACKING_MAXMP = new BuffStat(6, false, true);
        public static readonly BuffStat STACKING_MAXMP_R = new BuffStat(6, false, true);
        public static readonly BuffStat STACKING_ACC = new BuffStat(8, false, true);
        public static readonly BuffStat STACKING_AVOID = new BuffStat(9, false, true);
        public static readonly BuffStat STACKING_JUMP = new BuffStat(10, false, true);
        public static readonly BuffStat STACKING_SPEED = new BuffStat(11, false, true);
        public static readonly BuffStat STACKING_STATS = new BuffStat(12, false, true);

        public static readonly BuffStat STACKING_BOOSTER = new BuffStat(15, false, true);

        public static readonly BuffStat STACKING_ATK = new BuffStat(20, false, true);

        public static readonly BuffStat STACKING_STR = new BuffStat(22, false, true);
        public static readonly BuffStat STACKING_DEX = new BuffStat(23, false, true);
        public static readonly BuffStat STACKING_INT = new BuffStat(24, false, true);
        public static readonly BuffStat STACKING_LUK = new BuffStat(25, false, true);
        public static readonly BuffStat STACKING_DMG_R = new BuffStat(26, false, true);

        public static readonly BuffStat STACKING_ASRB = new BuffStat(30, false, true);
        public static readonly BuffStat STACKING_CRIT = new BuffStat(32, false, true);
        public static readonly BuffStat STACKING_WDEF_R = new BuffStat(33, false, true);
        public static readonly BuffStat STACKING_MAX_CRIT = new BuffStat(34, false, true);
        public static readonly BuffStat STACKING_BOSS = new BuffStat(35, false, true);
        public static readonly BuffStat STACKING_STATS_R = new BuffStat(36, false, true);
        public static readonly BuffStat STACKING_STANCE = new BuffStat(37, false, true);
        public static readonly BuffStat STACKING_IGNORE_DEF = new BuffStat(38, false, true);

        public static readonly BuffStat STACKING_MAX_CRIT_R = new BuffStat(42, false, true); //e.g. 100% will make 25% -> 50%, raises min crit towards max crit if min crit is 50% ???
        public static readonly BuffStat STACKING_AVOID_R = new BuffStat(43, false, true);
        public static readonly BuffStat STACKING_MDEF_R = new BuffStat(44, false, true);

        public static readonly BuffStat STACKING_MIN_CRIT = new BuffStat(47, false, true); //increases min crit to 50% then raises max crit after

        public static readonly BuffStat WATK = new BuffStat(50);
        public static readonly BuffStat WDEF = new BuffStat(51);
        public static readonly BuffStat MATK = new BuffStat(52);
        public static readonly BuffStat MDEF = new BuffStat(53);
        public static readonly BuffStat ACC = new BuffStat(54);
        public static readonly BuffStat AVOID = new BuffStat(55);
        public static readonly BuffStat HANDS = new BuffStat(56);
        public static readonly BuffStat SPEED = new BuffStat(57);
        public static readonly BuffStat JUMP = new BuffStat(58);
        public static readonly BuffStat MAGIC_GUARD = new BuffStat(59);
        public static readonly BuffStat DARK_SIGHT = new BuffStat(60);
        public static readonly BuffStat BOOSTER = new BuffStat(61);
        public static readonly BuffStat POWERGUARD = new BuffStat(62);
        public static readonly BuffStat MAXHP_R = new BuffStat(63);
        public static readonly BuffStat MAXMP_R = new BuffStat(64);

        public static readonly BuffStat SPAWNMASK1 = new BuffStat(416);
    }
}
