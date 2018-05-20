using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.WzLib;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple
{
    public class Quest
    {
        public ushort MapleId { get; private set; }
        public ushort NextQuestId { get; private set; }
        public sbyte Area { get; private set; }
        public byte MinimumLevel { get; private set; }
        public byte MaximumLevel { get; private set; }
        public short PetCloseness { get; private set; }
        public sbyte TamingMobLevel { get; private set; }
        public int RepeatWait { get; private set; }
        public short Fame { get; private set; }
        public int TimeLimit { get; private set; }
        public bool AutoStart { get; private set; }
        public bool SelectedMob { get; private set; }

        public List<ushort> PreRequiredQuests { get; private set; }
        public List<ushort> PostRequiredQuests { get; private set; }
        public Dictionary<int, short> PreRequiredItems { get; private set; }
        public Dictionary<int, short> PostRequiredItems { get; private set; }
        public Dictionary<int, short> PostRequiredKills { get; private set; }
        public List<Job> ValidJobs { get; private set; }

        // Rewards (Start, End)
        public int[] ExperienceReward { get; set; }
        public int[] MesoReward { get; set; }
        public int[] PetClosenessReward { get; set; }
        public bool[] PetSpeedReward { get; set; }
        public int[] FameReward { get; set; }
        public int[] PetSkillReward { get; set; }
        public Dictionary<int, short> PreItemRewards { get; private set; }
        public Dictionary<int, short> PostItemRewards { get; private set; }
        public Dictionary<Skill, Job> PreSkillRewards { get; set; }
        public Dictionary<Skill, Job> PostSkillRewards { get; set; }

        public Quest NextQuest => NextQuestId > 0 ? DataProvider.Quests[NextQuestId] : null;

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

        public Quest(WzImageProperty img)
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
