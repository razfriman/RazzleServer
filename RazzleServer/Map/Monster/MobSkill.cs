using System.Collections.Generic;
using System.Drawing;

namespace RazzleServer.Map.Monster
{
    public class MobSkill
    {
        public readonly int Skill;
        public readonly int Level;
        public int mpCon;
        public int summonEffect;
        public int hp;
        public int x;
        public int y;
        public int time;
        public int interval;
        public int prop;
        public short limit;
        public Point lt;
        public Point rb;
        public bool summonOnce;
        private static Dictionary<int, MobSkill> Skills = new Dictionary<int, MobSkill>();
        public MobSkill(int skill, int level)
        {
            Skill = skill;
            Level = level;
        }
        private static int GetRealSkillID(int skill, int level)
        {
            return (skill * 1000) + level;
        }
        public static MobSkill GetSkill(int skill, int level)
        {
            MobSkill sk = null;
            if (Skills.TryGetValue(GetRealSkillID(skill, level), out sk))
            {
                return sk;
            }
            return null;
        }
        public static void SetSkill(MobSkill skill)
        {
            Skills[GetRealSkillID(skill.Skill, skill.Level)] = skill;
        }
    }
}
