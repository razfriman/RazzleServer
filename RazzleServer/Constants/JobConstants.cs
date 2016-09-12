using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public const int DUALBLADE2 = 430;
        public const int DUALBLADE2P = 431;
        public const int DUALBLADE3 = 432;
        public const int DUALBLADE3P = 433;
        public const int DUALBLADE4 = 434;
        public const int PIRATE = 500;
        public const int BRAWLER = 510;
        public const int MARAUDER = 511;
        public const int BUCCANEER = 512;
        public const int GUNSLINGER = 520;
        public const int OUTLAW = 521;
        public const int CORSAIR = 522;
        public const int CANNONEER1 = 501;
        public const int CANNONEER2 = 530;
        public const int CANNONEER3 = 531;
        public const int CANNONEER4 = 532;
        public const int JETT1 = 508;
        public const int JETT2 = 570;
        public const int JETT3 = 571;
        public const int JETT4 = 572;
        public const int CYGNUS = 1000;
        public const int DAWNWARRIOR1 = 1100;
        public const int DAWNWARRIOR2 = 1110;
        public const int DAWNWARRIOR3 = 1111;
        public const int DAWNWARRIOR4 = 1112;
        public const int BLAZEWIZARD1 = 1200;
        public const int BLAZEWIZARD2 = 1210;
        public const int BLAZEWIZARD3 = 1211;
        public const int BLAZEWIZARD4 = 1212;
        public const int WINDARCHER1 = 1300;
        public const int WINDARCHER2 = 1310;
        public const int WINDARCHER3 = 1311;
        public const int WINDARCHER4 = 1312;
        public const int NIGHTWALKER1 = 1400;
        public const int NIGHTWALKER2 = 1410;
        public const int NIGHTWALKER3 = 1411;
        public const int NIGHTWALKER4 = 1412;
        public const int THUNDERBREAKER1 = 1500;
        public const int THUNDERBREAKER2 = 1510;
        public const int THUNDERBREAKER3 = 1511;
        public const int THUNDERBREAKER4 = 1512;
        public const int EVANBASICS = 2001;
        public const int EVAN1 = 2200;
        public const int EVAN2 = 2210;
        public const int EVAN3 = 2211;
        public const int EVAN4 = 2212;
        public const int EVAN5 = 2213;
        public const int EVAN6 = 2214;
        public const int EVAN7 = 2215;
        public const int EVAN8 = 2216;
        public const int EVAN9 = 2217;
        public const int EVAN10 = 2218;
        public const int MERCEDESBASICS = 2002;
        public const int MERCEDES1 = 2300;
        public const int MERCEDES2 = 2310;
        public const int MERCEDES3 = 2311;
        public const int MERCEDES4 = 2312;
        public const int PHANTOMBASICS = 2003;
        public const int PHANTOM1 = 2400;
        public const int PHANTOM2 = 2410;
        public const int PHANTOM3 = 2411;
        public const int PHANTOM4 = 2412;
        public const int RESISTANCE = 3000;
        public const int DEMONBASICS = 3001;
        public const int DEMONSLAYER1 = 3100;
        public const int DEMONSLAYER2 = 3110;
        public const int DEMONSLAYER3 = 3111;
        public const int DEMONSLAYER4 = 3112;
        public const int DEMONAVENGER1 = 3101;
        public const int DEMONAVENGER2 = 3120;
        public const int DEMONAVENGER3 = 3121;
        public const int DEMONAVENGER4 = 3122;
        public const int BATTLEMAGE1 = 3200;
        public const int BATTLEMAGE2 = 3210;
        public const int BATTLEMAGE3 = 3211;
        public const int BATTLEMAGE4 = 3212;
        public const int WILDHUNTER1 = 3300;
        public const int WILDHUNTER2 = 3310;
        public const int WILDHUNTER3 = 3311;
        public const int WILDHUNTER4 = 3312;
        public const int MECHANIC1 = 3500;
        public const int MECHANIC2 = 3510;
        public const int MECHANIC3 = 3511;
        public const int MECHANIC4 = 3512;
        public const int XENONBASICS = 3002;
        public const int XENON1 = 3600;
        public const int XENON2 = 3610;
        public const int XENON3 = 3611;
        public const int XENON4 = 3612;
        public const int HAYATOBASICS = 4001;
        public const int HAYATO1 = 4100;
        public const int HAYATO2 = 4110;
        public const int HAYATO3 = 4111;
        public const int HAYATO4 = 4112;
        public const int KANNABASICS = 4002;
        public const int KANNA1 = 4200;
        public const int KANNA2 = 4210;
        public const int KANNA3 = 4211;
        public const int KANNA4 = 4212;
        public const int MIHILEBASICS = 5000;
        public const int MIHILE1 = 5100;
        public const int MIHILE2 = 5110;
        public const int MIHILE3 = 5111;
        public const int MIHILE4 = 5112;
        public const int KAISERBASICS = 6000;
        public const int KAISER1 = 6100;
        public const int KAISER2 = 6110;
        public const int KAISER3 = 6111;
        public const int KAISER4 = 6112;
        public const int ANGELICBUSTERBASICS = 6001;
        public const int ANGELICBUSTER1 = 6500;
        public const int ANGELICBUSTER2 = 6510;
        public const int ANGELICBUSTER3 = 6511;
        public const int ANGELICBUSTER4 = 6512;
        public const int ZEROBASICS = 10000;
        public const int ZERO1 = 10100;
        public const int ZERO2 = 10110;
        public const int ZERO3 = 10111;
        public const int ZERO4 = 10112;
        public const int BEASTTAMERBASICS = 11000;
        public const int BEASTTAMER1 = 11200;
        public const int BEASTTAMER2 = 11210;
        public const int BEASTTAMER3 = 11211;
        public const int BEASTTAMER4 = 11212;

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
            {430,  "DualBlade2"},
            {431,  "DualBlade2p"},
            {432,  "DualBlade3"},
            {433,  "DualBlade3p"},
            {434,  "DualBlade4"},
            {500,  "Pirate"},
            {510,  "Brawler"},
            {511,  "Marauder"},
            {512,  "Buccaneer"},
            {520,  "Gunslinger"},
            {521,  "Outlaw"},
            {522,  "Corsair"},
            {501,  "Cannoneer1"},
            {530,  "Cannoneer2"},
            {531,  "Cannoneer3"},
            {532,  "Cannoneer4"},
            {508,  "Jett1"},
            {570,  "Jett2"},
            {571,  "Jett3"},
            {572,  "Jett4"},
            {1000, "Cygnus"},
            {1100, "DawnWarrior1"},
            {1110, "DawnWarrior2"},
            {1111, "DawnWarrior3"},
            {1112, "DawnWarrior4"},
            {1200, "BlazeWizard1"},
            {1210, "BlazeWizard2"},
            {1211, "BlazeWizard3"},
            {1212, "BlazeWizard4"},
            {1300, "WindArcher1"},
            {1310, "WindArcher2"},
            {1311, "WindArcher3"},
            {1312, "WindArcher4"},
            {1400, "NightWalker1"},
            {1410, "NightWalker2"},
            {1411, "NightWalker3"},
            {1412, "NightWalker4"},
            {1500, "ThunderBreaker1"},
            {1510, "ThunderBreaker2"},
            {1511, "ThunderBreaker3"},
            {1512, "ThunderBreaker4"},
            {2001, "EvanBasics"},
            {2200, "Evan1"},
            {2210, "Evan2"},
            {2211, "Evan3"},
            {2212, "Evan4"},
            {2213, "Evan5"},
            {2214, "Evan6"},
            {2215, "Evan7"},
            {2216, "Evan8"},
            {2217, "Evan9"},
            {2218, "Evan10"},
            {2002, "MercedesBasics"},
            {2300, "Mercedes1"},
            {2310, "Mercedes2"},
            {2311, "Mercedes3"},
            {2312, "Mercedes4"},
            {2003, "PhantomBasics"},
            {2400, "Phantom1"},
            {2410, "Phantom2"},
            {2411, "Phantom3"},
            {2412, "Phantom4"},
            {2005, "ShadeBasics"},
            {2500, "Shade1"},
            {2510, "Shade2"},
            {2511, "Shade3"},
            {2512, "Shade4"},
            {3000, "Resistance"},
            {3001, "DemonBasics"},
            {3100, "DemonSlayer1"},
            {3110, "DemonSlayer2"},
            {3111, "DemonSlayer3"},
            {3112, "DemonSlayer4"},
            {3101, "DemonAvenger1"},
            {3120, "DemonAvenger2"},
            {3121, "DemonAvenger3"},
            {3122, "DemonAvenger4"},
            {3200, "BattleMage1"},
            {3210, "BattleMage2"},
            {3211, "BattleMage3"},
            {3212, "BattleMage4"},
            {3300, "WildHunter1"},
            {3310, "WildHunter2"},
            {3311, "WildHunter3"},
            {3312, "WildHunter4"},
            {3500, "Mechanic1"},
            {3510, "Mechanic2"},
            {3511, "Mechanic3"},
            {3512, "Mechanic4"},
            {3002, "XenonBasics"},
            {3600, "Xenon1"},
            {3610, "Xenon2"},
            {3611, "Xenon3"},
            {3612, "Xenon4"},
            {4001, "HayatoBasics"},
            {4100, "Hayato1"},
            {4110, "Hayato2"},
            {4111, "Hayato3"},
            {4112, "Hayato4"},
            {4002, "KannaBasics"},
            {4200, "Kanna1"},
            {4210, "Kanna2"},
            {4211, "Kanna3"},
            {4212, "Kanna4"},
            {5000, "MihileBasics"},
            {5100, "Mihile1"},
            {5110, "Mihile2"},
            {5111, "Mihile3"},
            {5112, "Mihile4"},
            {6000, "KaiserBasics"},
            {6100, "Kaiser1"},
            {6110, "Kaiser2"},
            {6111, "Kaiser3"},
            {6112, "Kaiser4"},
            {6001, "AngelicBusterBasics"},
            {6500, "AngelicBuster1"},
            {6510, "AngelicBuster2"},
            {6511, "AngelicBuster3"},
            {6512, "AngelicBuster4"},
            {10000,"ZeroBasics"},
            {10100,"Zero1"},
            {10110,"Zero2"},
            {10111,"Zero3"},
            {10112,"Zero4"},
            {11000,"BeastTamerBasics"},
            {11200,"BeastTamer1"},
            {11210,"BeastTamer2"},
            {11211,"BeastTamer3"},
            {11212,"BeastTamer4"},

            {900,  "GameMaster"},
            {910,  "SuperGameMaster"}
        };

        public static bool IsDualBlade(int jobId)
        {
            return jobId / 10 == 43;
        }
        public static bool IsCannonneer(int jobId)
        {
            return jobId == 501 || jobId / 10 == 53;
        }
        public static bool IsJett(int jobId)
        {
            return jobId == 508 || jobId / 10 == 57;
        }
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
            else if ((job / 10) % 10 == 0 || job == JobConstants.CANNONEER1 || job == JobConstants.DEMONAVENGER1)
            {
                return 1;
            }
            else
            {
                return 2 + (job % 10);
            }
        }

        public static bool IsBeginnerJob(int job)
        {
            return job == EXPLORER ||
                    job == CYGNUS ||
                    job == RESISTANCE || job == DEMONBASICS || job == XENONBASICS ||
                    job == HAYATOBASICS || job == KANNABASICS ||
                    job == MIHILEBASICS ||
                    job == KAISERBASICS || job == ANGELICBUSTERBASICS ||
                    job == ZEROBASICS ||
                    (job >= 7000 && job < 10000);
        }

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
            if (skillJobId >= 2210 && skillJobId <= 2218) //evan                
            {
                if (job >= 2210 && job <= 2218)
                    return true;
                return false;
            }
            else if (skillJobId == 508 || (skillJobId >= 570 && skillJobId <= 572)) //jett
            {
                if (job == 508 || (job >= 570 && job <= 572))
                    return true;
                return false;
            }
            else if (skillJobId == 501 || (skillJobId >= 530 && skillJobId <= 532)) //cannoneer
            {
                if (job == 501 || (job >= 530 && job <= 532))
                    return true;
                return false;
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

        public static short GetSubJobByJob(int jobId)
        {
            switch (jobId)
            {
                case 430:
                case 431:
                case 432:
                case 433:
                case 434:
                    return 1;
                case 501:
                case 530:
                case 531:
                case 532:
                    return 2;
                case 508:
                case 570:
                case 571:
                case 572:
                    return 10;
                default:
                    return 0;
            }
        }
    }
}