using RazzleServer.Data.WZ;
using RazzleServer.Map.Monster;
using RazzleServer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RazzleServer.Inventory;
using RazzleServer.Util;
using RazzleServer.Constants;

namespace RazzleServer.Data
{
    public class DataBuffer
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

        public static WzEquip GetEquipById(int itemId)
        {
            WzEquip ret;
            return EquipBuffer.TryGetValue(itemId, out ret) ? ret : null;
        }

        public static WzItem GetItemById(int itemId)
        {
            WzItem ret;
            return ItemBuffer.TryGetValue(itemId, out ret) ? ret : null;
        }

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

        public static WzMob GetMobById(int mobId)
        {
            WzMob ret;
            return MobBuffer.TryGetValue(mobId, out ret) ? ret : null;
        }

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

        public static List<MobDrop> GetMobDropsById(int mobId)
        {
            List<MobDrop> ret;
            return MobDropBuffer.TryGetValue(mobId, out ret) ? ret : new List<MobDrop>();
        }

        public static WzMap GetMapById(int mapId)
        {
            WzMap ret;
            return MapBuffer.TryGetValue(mapId, out ret) ? ret : null;
        }

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

        public static WzCharacterSkill GetCharacterSkillById(int skillId)
        {
            WzCharacterSkill ret;
            return CharacterSkillBuffer.TryGetValue(skillId, out ret) ? ret : null;
        }

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

        public static string GetSkillNameById(int skillId)
        {
            WzCharacterSkill ret;
            return CharacterSkillBuffer.TryGetValue(skillId, out ret) ? ret.Name : string.Empty;
        }

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

        public static WzFamiliarSkill GetFamiliarSkillById(int skillId)
        {
            WzFamiliarSkill ret;
            return FamiliarSkillBuffer.TryGetValue(skillId, out ret) ? ret : null;
        }

        public static Dictionary<int, string> GetAllJobNameIds()
        {
            return JobNames.ToDictionary(x => x.Key, x => x.Value);
        }

        public static List<int> GetAllJobIds()
        {
            return JobConstants.JobIdNamePairs.Keys.ToList();
        }

        public static List<KeyValuePair<int, string>> GetJobsByName(string name)
        {
            name = name.ToLower();
            return JobConstants.JobIdNamePairs.Where(x => x.Value.ToLower().Contains(name)).ToList();
        }

        public static string GetJobNameById(int id)
        {
            string ret;
            return JobConstants.JobIdNamePairs.TryGetValue(id, out ret) ? ret : string.Empty;
        }

        public static WzQuest GetQuestById(ushort id)
        {
            WzQuest ret;
            return QuestBuffer.TryGetValue(id, out ret) ? ret : null;
        }

        public static List<KeyValuePair<int, string>> GetNPCsByName(string name)
        {
            name = name.ToLower();
            return NpcNames.Where(x => x.Value.ToLower().Contains(name)).ToList();
        }

        public static string GetNPCNameById(int id)
        {
            string ret;
            return NpcNames.TryGetValue(id, out ret) ? ret : string.Empty;
        }

        public static WzRecipe GetCraftRecipeById(int Id)
        {
            WzRecipe ret;
            return CraftRecipeBuffer.TryGetValue(Id, out ret) ? ret : null;
        }

        public static WzMakeCharInfo GetCharCreationInfo(JobType jobType)
        {
            WzMakeCharInfo info;
            if (jobType == JobType.Cannonneer)
                jobType = JobType.Explorer;
            return CharCreationInfo.TryGetValue(jobType, out info) ? info : null;
        }

        public static WzItemOption GetPotential(int id)
        {
            WzItemOption ret;
            return PotentialBuffer.TryGetValue(id, out ret) ? ret : null;
        }

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