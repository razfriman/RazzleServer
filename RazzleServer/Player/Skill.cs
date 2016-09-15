using RazzleServer.Data;
using RazzleServer.Data.WZ;
using RazzleServer.Packet;
using RazzleServer.Util;
using System.Collections.Generic;

namespace RazzleServer.Player
{
    public class Skill
    {
        public int SkillID { get; set; }
        public short SkillExp { get; set; }
        public byte Level { get; set; }
        public byte MasterLevel { get; set; }
        public long Expiration { get; set; }

        public Skill(int skillID, byte masterLevel = 0, byte level = 0)
        {
            SkillID = skillID;
            MasterLevel = masterLevel;
            Expiration = -1;
            Level = level;
            SkillExp = 0;
        }

        #region Helpers        
        public bool HasMastery
        {
            get
            {
                WzCharacterSkill skillInfo = DataBuffer.GetCharacterSkillById(SkillID);
                if (skillInfo == null)
                    return false;
                else
                    return skillInfo.HasMastery;
            }
        }
        #endregion

        #region Packets
        public static PacketWriter UpdateSkills(List<Skill> skills)
        {
            var pw = new PacketWriter(SMSGHeader.UPDATE_SKILLS);
            pw.WriteBool(true); //enable actions
            pw.WriteByte(0);
            pw.WriteByte(0);

            var count = (short)skills.Count;
            pw.WriteShort(count);
            for (int i = 0; i < count; i++)
            {
                pw.WriteInt(skills[i].SkillID);
                pw.WriteInt(skills[i].Level);
                pw.WriteInt(skills[i].MasterLevel);
                pw.WriteLong(MapleFormatHelper.GetMapleTimeStamp(skills[i].Expiration));
            }
            pw.WriteByte(0x2F);
            return pw;
        }

        public static PacketWriter UpdateSingleSkill(Skill skill)
        {
            return UpdateSkills(new List<Skill>() { skill });
        }

        public static PacketWriter ShowCooldown(int skillId, uint time)
        {
            var pw = new PacketWriter(SMSGHeader.GIVE_COOLDOWN);
            pw.WriteInt(skillId);
            pw.WriteUInt(time);
            return pw;
        }

        public static PacketWriter ShowOwnSkillEffect(int skillId, byte skillLevel)
        {
            var pw = new PacketWriter(SMSGHeader.SHOW_SKILL_EFFECT);
            pw.WriteByte(2);
            pw.WriteInt(skillId);
            pw.WriteByte(skillLevel);
            return pw;
        }

        public static PacketWriter ShowBuffEffect(int skillId, byte characterLevel, byte? skillLevel, bool show) //remove if show = false
        {
            var pw = new PacketWriter(SMSGHeader.SHOW_SKILL_EFFECT);
            pw.WriteByte(1);
            pw.WriteInt(skillId);
            pw.WriteByte(characterLevel);
            if (skillLevel.HasValue)
            {
                pw.WriteByte(skillLevel.Value);
            }
            pw.WriteBool(show);
            return pw;
        }
        #endregion
    }
}
