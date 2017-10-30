using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Util;
using RazzleServer.Game.Maple;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Data.WZ
{
    public class WzQuestRequirement
    {
        public QuestRequirementType Type { get; set; }

        protected WzQuestRequirement(QuestRequirementType type)
        {
            Type = type;
        }

        public virtual bool Check(Character chr, int npcId, Quest quest) { return false; }
    }

    public class WzQuestStringRequirement : WzQuestRequirement
    {
        public string Data { get; private set; }

        public WzQuestStringRequirement(QuestRequirementType type, string data)
            : base(type)
        {
            Data = data;
        }

        public override bool Check(Character chr, int npcId, Quest quest)
        {
            //TODO: run quest script
            return true;
        }
    }

    public class WzQuestIntegerRequirement : WzQuestRequirement
    {
        public int Data { get; private set; }

        private static readonly ILogger Log = LogManager.Log;

        public WzQuestIntegerRequirement(QuestRequirementType type, int data)
            : base(type)
        {
            Data = data;
        }

        public override bool Check(Character chr, int npcId, Quest quest)
        {
            switch (Type)
            {
                case QuestRequirementType.lvmin:
                    return chr.Level >= Data;
                case QuestRequirementType.lvmax:
                    return chr.Level <= Data;
                case QuestRequirementType.npc:
                    return npcId == Data;
                case QuestRequirementType.interval:
                    return true; //todo: quest is repeatable
                case QuestRequirementType.pettamenessmin:
                    return true; //todo: chr.GetPet(0).Tameness >= Data;
                case QuestRequirementType.questComplete: //amount of quests completed
                    return chr.Quests.Completed.Count() >= Data;
                case QuestRequirementType.pop: //fame
                    return chr.Fame >= Data;
                default:
                    Log.LogWarning($"No check handler for {Type.ToString()}");
                    break;
            }
            return false;
        }
    }

    public class WzQuestIntegerPairRequirement : WzQuestRequirement
    {
        public Dictionary<int, int> Data { get; private set; }

        private static readonly ILogger Log = LogManager.Log;


        public WzQuestIntegerPairRequirement(QuestRequirementType type, Dictionary<int, int> data)
            : base(type)
        {
            Data = data;
        }

        public override bool Check(Character chr, int npcId, Quest quest)
        {
            switch (Type)
            {
                case QuestRequirementType.item:
                    foreach (var itemPair in Data)
                    {
                        if (!chr.Inventory.HasItem(itemPair.Key, itemPair.Value))
                            return false;
                    }
                    return true;
                case QuestRequirementType.mob:
                    foreach (var mobPair in Data)
                    {
                        if (!quest.PostRequiredKills.ContainsKey(mobPair.Key) || quest.PostRequiredKills[mobPair.Key] < mobPair.Value)
                            return false;
                    }
                    return true;
                case QuestRequirementType.quest:
                    foreach (var questPair in Data)
                    {
                        if (!chr.Quests.Completed.ContainsKey(questPair.Key))
                            return false;
                    }
                    return true;
                default:
                    Log.LogWarning($"No check handler for {Type.ToString()}");
                    break;
            }
            return false;
        }
    }

    public class WzQuestIntegerListRequirement : WzQuestRequirement
    {
        public List<int> Data { get; private set; }

        private static readonly ILogger Log = LogManager.Log;


        public WzQuestIntegerListRequirement(QuestRequirementType type, List<int> data)
            : base(type)
        {
            Data = data;
        }

        public override bool Check(Character chr, int npcId, Quest quest)
        {
            switch (Type)
            {
                case QuestRequirementType.job:
                    return Data.Any(i => i == chr.Job);
                case QuestRequirementType.fieldEnter:
                    return Data.Any(i => i == chr.map);
                case QuestRequirementType.pet:
                    return true; //todo: 
                                 /*
                                  return Data.Where(x => x == chr.GetPet(0).ItemId).Any();
                                  */
                default:
                    Log.LogWarning($"No check handler for {Type.ToString()}");
                    break;
            }
            return false;
        }
    }

    public class WzQuestDateRequirement : WzQuestRequirement
    {
        public DateTime Date { get; private set; }

        private static readonly ILogger Log = LogManager.Log;

        public WzQuestDateRequirement(QuestRequirementType type, DateTime date)
            : base(type)
        {
            Date = date;
        }

        public override bool Check(Character chr, int npcId, MapleQuest quest)
        {
            switch (Type)
            {
                case QuestRequirementType.end:
                    return DateTime.UtcNow <= Date;
                default:
                    Log.LogWarning($"No check handler for {Type}");
                    break;
            }
            return false;
        }
    }

    public enum QuestRequirementType
    {
        undefined,
        job,
        item,
        quest,
        lvmin,
        lvmax,
        end,
        mob,
        npc,
        fieldEnter,
        interval,
        startscript,
        endscript,
        pet,
        pettamenessmin,
        questComplete,
        pop,
        subJobFlags,
        dayByDay,
        normalAutoStart,
        partyQuest_S,
        charmMin,
        senseMin,
        craftMin,
        willMin,
        charismaMin,
        insightMin
    }
}
