using RazzleServer.Data.WZ;
using RazzleServer.Map.Monster;
using RazzleServer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Inventory;
using RazzleServer.Util;
using RazzleServer.Constants;

namespace RazzleServer.Data
{
    public static class DataBuffer
    {
        public static Dictionary<int, WzEquip> EquipBuffer = new Dictionary<int, WzEquip>();
        public static Dictionary<int, WzItem> ItemBuffer = new Dictionary<int, WzItem>();
        public static Dictionary<int, WzMob> MobBuffer = new Dictionary<int, WzMob>();
        public static Dictionary<int, List<MobDrop>> MobDropBuffer = new Dictionary<int, List<MobDrop>>();
        public static Dictionary<int, WzMap> MapBuffer = new Dictionary<int, WzMap>();
        public static Dictionary<int, WzCharacterSkill> CharacterSkillBuffer = new Dictionary<int, WzCharacterSkill>();
        public static Dictionary<int, WzFamiliarSkill> FamiliarSkillBuffer = new Dictionary<int, WzFamiliarSkill>();
        public static Dictionary<int, WzRecipe> CraftRecipeBuffer = new Dictionary<int, WzRecipe>();
        public static Dictionary<ushort, WzQuest> QuestBuffer = new Dictionary<ushort, WzQuest>();
        public static Dictionary<int, WzItemOption> PotentialBuffer = new Dictionary<int, WzItemOption>();
        public static Dictionary<JobType, WzMakeCharInfo> CharCreationInfo = new Dictionary<JobType, WzMakeCharInfo>();

        public static List<MobDrop> GlobalDropBuffer = new List<MobDrop>();

        public static Dictionary<int, string> JobNames = new Dictionary<int, string>();
        public static Dictionary<int, string> NpcNames = new Dictionary<int, string>();

        public static Dictionary<int, Type> NpcScripts = new Dictionary<int, Type>();
        public static Dictionary<string, Type> PortalScripts = new Dictionary<string, Type>();
        public static Dictionary<string, Type> EventScripts = new Dictionary<string, Type>();

        public static WzEquip GetEquipById(int itemId) => EquipBuffer.TryGetValue(itemId, out WzEquip ret) ? ret : null;

        public static WzItem GetItemById(int itemId) => ItemBuffer.TryGetValue(itemId, out WzItem ret) ? ret : null;

        public static List<Tuple<int, string>> GetItemsByName(string name)
        {
            name = name.ToLower();
            List<Tuple<int, string>> idNamePairs = new List<Tuple<int, string>>();
            foreach (var kvp in EquipBuffer.Where(x => x.Value.Name != null && x.Value.Name.ToLower().Contains(name)))
            {
                idNamePairs.Add(new Tuple<int, string>(kvp.Key, kvp.Value.Name));
            }
            foreach (var kvp in ItemBuffer.Where(x => x.Value.Name != null && x.Value.Name.ToLower().Contains(name)))
            {
                idNamePairs.Add(new Tuple<int, string>(kvp.Key, kvp.Value.Name));
            }
            return idNamePairs;
        }

        public static WzMob GetMobById(int mobId) => MobBuffer.TryGetValue(mobId, out WzMob ret) ? ret : null;

        public static List<Tuple<int, string>> GetMobsByName(string name)
        {
            name = name.ToLower();
            var idNamePairs = new List<Tuple<int, string>>();
            foreach (var kvp in MobBuffer.Where(x => x.Value.Name != null && x.Value.Name.ToLower().Contains(name)))
            {
                idNamePairs.Add(new Tuple<int, string>(kvp.Key, kvp.Value.Name));
            }
            return idNamePairs;
        }

        public static List<MobDrop> GetMobDropsById(int mobId) => MobDropBuffer.TryGetValue(mobId, out List<MobDrop> ret) ? ret : new List<MobDrop>();

        public static WzMap GetMapById(int mapId) => MapBuffer.TryGetValue(mapId, out WzMap ret) ? ret : null;

        public static List<Tuple<int, string>> GetMapsByName(string name)
        {
            name = name.ToLower();
            List<Tuple<int, string>> idNamePairs = new List<Tuple<int, string>>();
            foreach (var kvp in MapBuffer.Where(x => x.Value.Name != null && x.Value.Name.ToLower().Contains(name)))
            {
                idNamePairs.Add(new Tuple<int, string>(kvp.Key, kvp.Value.Name));
            }
            return idNamePairs;
        }

        public static WzCharacterSkill GetCharacterSkillById(int skillId) => CharacterSkillBuffer.TryGetValue(skillId, out WzCharacterSkill ret) ? ret : null;

        public static List<WzCharacterSkill> GetCharacterSkillListByJob(int jobId)
        {
            List<WzCharacterSkill> list = new List<WzCharacterSkill>();
            var kvpList = CharacterSkillBuffer.Where(x => x.Key / 10000 == jobId);
            foreach (var kvp in kvpList)
            {
                list.Add(kvp.Value);
            }
            return list;
        }

        public static string GetSkillNameById(int skillId) => CharacterSkillBuffer.TryGetValue(skillId, out WzCharacterSkill ret) ? ret.Name : string.Empty;

        public static Dictionary<int, string> GetSkillsByName(string name)
        {
            name = name.ToLower();
            var idNamePairs = new Dictionary<int, string>();
            foreach (var kvp in CharacterSkillBuffer.Where(x => x.Value.Name != null && x.Value.Name.ToLower().Contains(name)))
            {
                idNamePairs.Add(kvp.Key, kvp.Value.Name);
            }
            return idNamePairs;
        }

        public static WzFamiliarSkill GetFamiliarSkillById(int skillId) => FamiliarSkillBuffer.TryGetValue(skillId, out WzFamiliarSkill ret) ? ret : null;

        public static Dictionary<int, string> GetAllJobNameIds() => JobNames.ToDictionary(x => x.Key, x => x.Value);

        public static List<int> GetAllJobIds() => JobConstants.JobIdNamePairs.Keys.ToList();

        public static List<KeyValuePair<int, string>> GetJobsByName(string name) => JobConstants.JobIdNamePairs.Where(x => x.Value.ToLower().Contains(name.ToLower())).ToList();

        public static string GetJobNameById(int id) => JobConstants.JobIdNamePairs.TryGetValue(id, out string ret) ? ret : string.Empty;

        public static WzQuest GetQuestById(ushort id) => QuestBuffer.TryGetValue(id, out WzQuest ret) ? ret : null;

        public static List<KeyValuePair<int, string>> GetNPCsByName(string name) => NpcNames.Where(x => x.Value.ToLower().Contains(name.ToLower())).ToList();

        public static string GetNPCNameById(int id) => NpcNames.TryGetValue(id, out string ret) ? ret : string.Empty;

        public static WzRecipe GetCraftRecipeById(int Id) => CraftRecipeBuffer.TryGetValue(Id, out WzRecipe ret) ? ret : null;

        public static WzMakeCharInfo GetCharCreationInfo(JobType jobType)
        {
            if (jobType == JobType.Cannonneer)
            {
                jobType = JobType.Explorer;
            }

            return CharCreationInfo.TryGetValue(jobType, out WzMakeCharInfo info) ? info : null;
        }

        public static WzItemOption GetPotential(int id) => PotentialBuffer.TryGetValue(id, out WzItemOption ret) ? ret : null;

        public static ushort GetRandomPotential(MaplePotentialState grade, byte reqLevel, int itemIdFor, bool bonusPotential = false)
        {
            int gradeBaseNum = (int)grade;
            if (grade >= MaplePotentialState.Rare)
                gradeBaseNum -= 16;
            if (gradeBaseNum < 1 || gradeBaseNum > 4) return 0;
            gradeBaseNum *= 10000;
            int limit = gradeBaseNum + 10000;
            int optionSubCategory = bonusPotential ? 2 : 0; //1 == skill pots but we don't use them for now
            var matchingPotentials = PotentialBuffer.Values.Where(x => x.Id >= gradeBaseNum && x.Id < limit && x.ReqLevel <= reqLevel && x.SubCategory == optionSubCategory && x.FitsItem(itemIdFor)).ToList();
            int index = Functions.Random(matchingPotentials.Count());
            return (ushort)matchingPotentials[index].Id;
        }
    }
}