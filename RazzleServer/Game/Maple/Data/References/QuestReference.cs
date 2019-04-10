using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class QuestReference
    {
        public int MapleId { get; set; }
        public string Name { get; set; }
        public int NextQuestId { get; set; }
        public sbyte Area { get; set; }
        public byte MinimumLevel { get; set; }
        public byte MaximumLevel { get; set; }
        public short PetCloseness { get; set; }
        public sbyte TamingMobLevel { get; set; }
        public int RepeatWait { get; set; }
        public short Fame { get; set; }
        public int TimeLimit { get; set; }
        public bool AutoStart { get; set; }
        public bool SelectedMob { get; set; }
        public List<ushort> PreRequiredQuests { get; set; } = new List<ushort>();
        public List<ushort> PostRequiredQuests { get; set; } = new List<ushort>();
        public Dictionary<int, short> PreRequiredItems { get; set; } = new Dictionary<int, short>();
        public Dictionary<int, short> PostRequiredItems { get; set; } = new Dictionary<int, short>();
        public Dictionary<int, short> PostRequiredKills { get; set; } = new Dictionary<int, short>();
        public List<Job> ValidJobs { get; set; }
        public List<int> ExperienceReward { get; set; } = new List<int>();
        public List<int> MesoReward { get; set; } = new List<int>();
        public List<int> PetClosenessReward { get; set; } = new List<int>();
        public List<int> PetSpeedReward { get; set; } = new List<int>();
        public List<int> FameReward { get; set; } = new List<int>();
        public List<int> PetSkillReward { get; set; } = new List<int>();
        public Dictionary<int, short> PreItemRewards { get; set; } = new Dictionary<int, short>();
        public Dictionary<int, short> PostItemRewards { get; set; } = new Dictionary<int, short>();
        public Dictionary<Skill, Job> PreSkillRewards { get; set; } = new Dictionary<Skill, Job>();
        public Dictionary<Skill, Job> PostSkillRewards { get; set; } = new Dictionary<Skill, Job>();

        public byte Flags
        {
            get
            {
                byte flags = 0;

                if (AutoStart)
                {
                    flags |= (byte)QuestFlags.AutoStart;
                }

                if (SelectedMob)
                {
                    flags |= (byte)QuestFlags.SelectedMob;
                }

                return flags;
            }
        }


        public QuestReference()
        {
        }

        public QuestReference(WzImageProperty img)
        {
            //this.MapleId = (ushort)img["questid"];
            //this.NextQuestId = (ushort)img["next_quest"];
            //this.Area = (sbyte)img["quest_area"];
            //this.MinimumLevel = (byte)img["min_level"];
            //this.MaximumLevel = (byte)img["max_level"];
            //this.PetCloseness = (short)img["pet_closeness"];
            //this.TamingMobLevel = (sbyte)img["taming_mob_level"];
            //this.RepeatWait = (int)img["repeat_wait"];
            //this.Fame = (short)img["fame"];
            //this.TimeLimit = (int)img["time_limit"];
            //this.AutoStart = img["flags"].ToString().Contains("auto_start");
            //this.SelectedMob = img["flags"].ToString().Contains("selected_mob");
        }
    }
}
