using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Wz;

namespace RazzleServer.Game.Maple.Data.References
{
    public class QuestReference
    {
        public ushort MapleId { get; set; }
        public ushort NextQuestId { get; set; }
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

        public List<ushort> PreRequiredQuests { get; set; }
        public List<ushort> PostRequiredQuests { get; set; }
        public Dictionary<int, short> PreRequiredItems { get; set; }
        public Dictionary<int, short> PostRequiredItems { get; set; }
        public Dictionary<int, short> PostRequiredKills { get; set; }
        public List<Job> ValidJobs { get; set; }

        public int[] ExperienceReward { get; set; }
        public int[] MesoReward { get; set; }
        public int[] PetClosenessReward { get; set; }
        public bool[] PetSpeedReward { get; set; }
        public int[] FameReward { get; set; }
        public int[] PetSkillReward { get; set; }
        public Dictionary<int, short> PreItemRewards { get; set; }
        public Dictionary<int, short> PostItemRewards { get; set; }
        public Dictionary<Skill, Job> PreSkillRewards { get; set; }
        public Dictionary<Skill, Job> PostSkillRewards { get; set; }

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

            PreRequiredQuests = new List<ushort>();
            PostRequiredQuests = new List<ushort>();
            PreRequiredItems = new Dictionary<int, short>();
            PostRequiredItems = new Dictionary<int, short>();
            PostRequiredKills = new Dictionary<int, short>();

            ExperienceReward = new int[2];
            MesoReward = new int[2];
            PetClosenessReward = new int[2];
            PetSpeedReward = new bool[2];
            FameReward = new int[2];
            PetSkillReward = new int[2];

            PreItemRewards = new Dictionary<int, short>();
            PostItemRewards = new Dictionary<int, short>();
            PreSkillRewards = new Dictionary<Skill, Job>();
            PostSkillRewards = new Dictionary<Skill, Job>();

            ValidJobs = new List<Job>();
        }
    }
}
