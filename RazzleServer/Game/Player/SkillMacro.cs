using MapleLib.PacketLib;
using RazzleServer.Common.Packet;

namespace RazzleServer.Player
{
    public class SkillMacro
    {
        public string Name { get; private set; }
        public bool ShoutName { get; private set; }
        public int[] Skills { get; }
        public bool Changed { get; set; }

        public SkillMacro(string skillName, bool shoutSkillName, int skill1, int skill2, int skill3)
        {
            Name = skillName;
            ShoutName = shoutSkillName;
            Skills = new int[3];
            Skills[0] = skill1;
            Skills[1] = skill2;
            Skills[2] = skill3;
            Changed = true;
        }

        public void SetSkills(string skillName, bool shoutSkillName, int skill1, int skill2, int skill3)
        {
            if (Name != skillName)
            {
                Name = skillName;
                Changed = true;
            }
            if (ShoutName != shoutSkillName)
            {
                ShoutName = shoutSkillName;
                Changed = true;
            }
            if (Skills[0] != skill1)
            {
                Skills[0] = skill1;
                Changed = true;
            }
            if (Skills[1] != skill2)
            {
                Skills[1] = skill2;
                Changed = true;
            }
            if (Skills[2] != skill3)
            {
                Skills[2] = skill3;
                Changed = true;
            }
        }

        public static PacketWriter ShowSkillMacros(SkillMacro[] skillMacros)
        {
            var pw = new PacketWriter(); pw.WriteHeader(SMSGHeader.SKILL_MACRO);
            byte count = 0;
            for (int i = 0; i < 5; i++)
            {
                if (skillMacros[i] != null)
                    count = (byte)(i + 1);
            }
            pw.WriteByte(count);
            for (int i = 0; i < count; i++)
            {
                if (skillMacros[i] != null)
                {
                    SkillMacro macro = skillMacros[i];
                    pw.WriteMapleString(macro.Name);
                    pw.WriteBool(!macro.ShoutName);
                    for (int j = 0; j < 3; j++)
                        pw.WriteInt(macro.Skills[j]);
                }
                else
                {
                    pw.WriteShort(0);
                    pw.WriteBool(false);
                    pw.WriteZeroBytes(12);
                }
            }

            return pw;
        }
    }
}