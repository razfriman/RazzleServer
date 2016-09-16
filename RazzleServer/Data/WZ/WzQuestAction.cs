using RazzleServer.Constants;
using RazzleServer.Inventory;
using RazzleServer.Player;
using System.Collections.Generic;

namespace RazzleServer.Data.WZ
{
    public class WzQuestAction
    {
        protected QuestActionType Type { get; set; }

        public WzQuestAction(QuestActionType type) 
        {
            Type = type;
        }

        public virtual void Act(MapleCharacter chr, int questId) 
        {

        }
    }

    public class WzQuestIntegerAction : WzQuestAction
    {
        public int Data { get; private set; }

        public WzQuestIntegerAction(QuestActionType type, int data) : base(type)           
        {
            Data = data;
        }

        public override void Act(MapleCharacter chr, int questId)
        {
            switch (Type)
            {
                case QuestActionType.exp: //int: give exp
                    chr.GainExp(Data, true, false);
                    break;
                case QuestActionType.money: //int: mesars
                    chr.Inventory.GainMesos(Data, false, true);
                    break;
                case QuestActionType.buffItemID: //int: apply item effect to player
                    //todo data = item id
                    break;                    
                case QuestActionType.pop: //int: fame
                    chr.AddFame(Data);
                    break;                
                case QuestActionType.nextQuest: //int: next quest in chain? check moopledev/v118
                    //todo
                    break;
            }
        }
    }

    public class WzQuestIntegerPairAction : WzQuestAction
    {
        public Dictionary<int, int> Data { get; private set; }

        public WzQuestIntegerPairAction(QuestActionType type, Dictionary<int, int> data) : base(type) 
        {
            Data = data;
        }

        public override void Act(MapleCharacter chr, int questId)
        {
            switch (Type)
            {
                case QuestActionType.quest: //<int:int> dictionary <questid:state> update quest status

                    break;
                case QuestActionType.sp: //<int:int> dictionary <sp_value:job>

                    break;
            }
        }
    }

    public class WzQuestSkillAction : WzQuestAction
    {
        private List<WzQuestSkillreward> rewards;
        

        public WzQuestSkillAction(List<WzQuestSkillreward> data) 
            : base(QuestActionType.skill)
        {
            rewards = data;
        }

        public override void Act(MapleCharacter chr, int questId)
        {

        }
    }
    public struct WzQuestSkillreward
    {
        public int SkillId;
        public byte Level;
        public byte MasterLevel;
        public List<int> Jobs;
    }

    public class WzQuestItemAction : WzQuestAction
    {
        private List<WzQuestItemReward> Rewards;
        //private List<int> Jobs;

        public WzQuestItemAction(List<WzQuestItemReward> data) 
            : base(QuestActionType.item)
        {
            Rewards = data;           
        }

        public override void Act(MapleCharacter chr, int questId)
        {
            foreach (WzQuestItemReward reward in Rewards)
            {
                if ((reward.Gender == 2 || reward.Gender == chr.Gender)) //todo: check job mask
                {
                     chr.Inventory.AddItemById(reward.ItemId, "Quest " + questId, (short)reward.Count);
                }
            }
        }

        public static List<int> GetJobsFromMask(int mask, int itemId, int questId)
        {         
            List<int> ret = new List<int>();
            if ((mask & 0x1) != 0)
                ret.Add(JobConstants.EXPLORER);            
            if ((mask & 0x2) != 0)
                ret.Add(JobConstants.SWORDMAN);            
            if ((mask & 0x4) != 0) 
                ret.Add(JobConstants.MAGICIAN);            
            if ((mask & 0x8) != 0) 
                ret.Add(JobConstants.ARCHER);            
            if ((mask & 0x10) != 0) 
                ret.Add(JobConstants.THIEF);            
            if ((mask & 0x20) != 0) 
                ret.Add(JobConstants.PIRATE);
            
            if ((mask & 0x400) != 0) 
                ret.Add(JobConstants.CYGNUS);            
            if ((mask & 0x800) != 0) 
                ret.Add(JobConstants.DAWNWARRIOR1);            
            if ((mask & 0x1000) != 0) 
                ret.Add(JobConstants.BLAZEWIZARD1);            
            if ((mask & 0x2000) != 0) 
                ret.Add(JobConstants.WINDARCHER1);            
            if ((mask & 0x4000) != 0) 
                ret.Add(JobConstants.NIGHTWALKER1);            
            if ((mask & 0x8000) != 0) 
                ret.Add(JobConstants.THUNDERBREAKER1);
            
            if ((mask & 0x20000) != 0) 
            {
                ret.Add(JobConstants.EVANBASICS);
                ret.Add(JobConstants.EVAN1);
            }
            if ((mask & 0x100000) != 0) 
            {
                ret.Add(JobConstants.EVANBASICS);
            }
            if ((mask & 0x400000) != 0) 
            {
                ret.Add(JobConstants.EVANBASICS);
                ret.Add(JobConstants.EVAN1);
            }
            if ((mask & 0x800000) != 0) 
            {
                //int i = 0;   
            }
            if ((mask & 0x1000000) != 0) 
            {
                //int i = 0;   
            }
            if ((mask & 0x2000000) != 0) 
            {
                //int i = 0;
            }
            if ((mask & 0x4000000) != 0)
            {
                //int i = 0;
            }
            if ((mask & 0x8000000) != 0)
            {
                //int i = 0;
            }
            if ((mask & 0x10000000) != 0)
            {
                //int i = 0;
            }
            if ((mask & 0x20000000) != 0)
            {
                //int i = 0;
            }            
            if ((mask & 0x40000000) != 0) 
            { 
                ret.Add(JobConstants.RESISTANCE);
                ret.Add(JobConstants.BATTLEMAGE1);
                ret.Add(JobConstants.WILDHUNTER1);
                ret.Add(JobConstants.MECHANIC1);
            }           
            if ((mask & 0x80000000) != 0) 
            { 
               // int i = 0;
            }
            return ret;            
        }
    }

    public struct WzQuestItemReward
    {
        public int ItemId;
        public int Count;
        public byte Gender;
        public MaplePotentialState Potential;
        public List<int> Jobs;
    }

    public class WzQuestSPAction : WzQuestAction
    {
        private List<WzQuestSPReward> rewards;

        public WzQuestSPAction(List<WzQuestSPReward> data)
            : base(QuestActionType.sp)
        {
            rewards = data;
        }

        public override void Act(MapleCharacter chr, int questId)
        {
            //We don't use this
        }
    }
    public struct WzQuestSPReward
    {
        public int Sp;
        public List<int> Jobs;        
    }

    public enum QuestActionType
    {
        undefined, //default
        item, //id - count - potentialgrade(string)       
        exp, //int, give exp
        money, //int, mesars
        buffItemID, //int, apply item effect to player
        pop, //int, fame
        quest, //<int,int> dictionary <questid,state> update quest status
        nextQuest, //int, next quest in chain? check moopledev/v118
        skill, //custom struct
        sp, //<int,int> dictionary <sp_value,job>
        willEXP, //int
        senseEXP, //int
        insightEXP, //int
        craftEXP, //int
        charismaEXP, //int
        charmEXP //int
    }
}
