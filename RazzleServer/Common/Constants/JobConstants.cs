using System;
using System.Collections.Generic;

namespace RazzleServer.Constants
{
    public static class JobConstants
    {
        #region JobIds
        public const int EXPLORER = 0;
        public const int SWORDMAN = 100;
        public const int FIGHTER = 110;
        public const int CRUSADER = 111;
        public const int HERO = 112;
        public const int PAGE = 120;
        public const int WHITEKNIGHT = 121;
        public const int PALADIN = 122;
        public const int SPEARMAN = 130;
        public const int BERSERKER = 131;
        public const int DARKKNIGHT = 132;
        public const int MAGICIAN = 200;
        public const int FIREPOISON2 = 210;
        public const int FIREPOISON3 = 211;
        public const int FIREPOISON4 = 212;
        public const int ICELIGHTNING2 = 220;
        public const int ICELIGHTNING3 = 221;
        public const int ICELIGHTNING4 = 222;
        public const int CLERIC = 230;
        public const int PRIEST = 231;
        public const int BISHOP = 232;
        public const int ARCHER = 300;
        public const int HUNTER = 310;
        public const int RANGER = 311;
        public const int BOWMASTER = 312;
        public const int CROSSBOWMAN = 320;
        public const int SNIPER = 321;
        public const int MARKSMAN = 322;
        public const int THIEF = 400;
        public const int ASSASSIN = 410;
        public const int HERMIT = 411;
        public const int NIGHTLORD = 412;
        public const int BANDIT = 420;
        public const int CHIEFBANDIT = 421;
        public const int SHADOWER = 422;

        public const int GAMEMASTER = 900;
        public const int SUPERGAMEMASTER = 910;

        public static Dictionary<int, string> JobIdNamePairs = new Dictionary<int, string>()
        {
            {0,    "Explorer"},
            {100,  "Swordman"},
            {110,  "Fighter"},
            {111,  "Crusader"},
            {112,  "Hero"},
            {120,  "Page"},
            {121,  "WhiteKnight"},
            {122,  "Paladin"},
            {130,  "Spearman"},
            {131,  "Berserker"},
            {132,  "DarkKnight"},
            {200,  "Magician"},
            {210,  "FirePoison2"},
            {211,  "FirePoison3"},
            {212,  "FirePoison4"},
            {220,  "IceLightning2"},
            {221,  "IceLightning3"},
            {222,  "IceLightning4"},
            {230,  "Cleric"},
            {231,  "Priest"},
            {232,  "Bishop"},
            {300,  "Archer"},
            {310,  "Hunter"},
            {311,  "Ranger"},
            {312,  "Bowmaster"},
            {320,  "Crossbowman"},
            {321,  "Sniper"},
            {322,  "Marksman"},
            {400,  "Thief"},
            {410,  "Assassin"},
            {411,  "Hermit"},
            {412,  "NightLord"},
            {420,  "Bandit"},
            {421,  "ChiefBandit"},
            {422,  "Shadower"},
            {900,  "GameMaster"},
            {910,  "SuperGameMaster"}
        };

        #endregion

        public static int GetSkillBookForJob(int job)
        {
            int index = GetJobNumber(job) - 1;
            return Math.Max(index, 0);
        }

        public static int GetJobNumber(int job)
        {
            if (job / 100 == 0 || IsBeginnerJob(job))
            {
                return 0;
            }
            else if ((job / 10) % 10 == 0)
            {
                return 1;
            }
            else
            {
                return 2 + (job % 10);
            }
        }

        public static bool IsBeginnerJob(int job) => job == EXPLORER;

        public static bool JobCanLearnSkill(int skillId, short job)
        {
            short skillJobId = (short)(skillId / 10000);
            if (skillJobId == job)
                return true;
            if (skillJobId > job) //too low job
                return false;
            if (JobConstants.IsBeginnerJob(skillJobId))
            {
                return job / 1000 == skillJobId / 1000;
            }

            if (skillJobId % 100 >= 10) //e.g. 510, 511 or 512
            {
                if (skillJobId / 10 != job / 10) //first 2 job digits have to match the skill, e.g. for job 511 and skill 510, 51 == 51
                    return false;
            }

            int skillBaseId = skillJobId / 100;
            skillBaseId *= 100;
            int jobBaseId = job / 100;
            jobBaseId *= 100;
            if (skillBaseId != jobBaseId) //1st job id
            {
                return false;
            }
            return true;
        }
    }
}