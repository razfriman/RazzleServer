using RazzleServer.Inventory;
using RazzleServer.Map;
using RazzleServer.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Scripts
{
    public class ScriptCharacter : AScriptCharacter
    {
        public string AssignedScriptName { get; }
        WeakReference<MapleCharacter> CharacterReference { get; }
        public ScriptCharacter(MapleCharacter chr, string scriptName)
        {
            CharacterReference = new WeakReference<MapleCharacter>(chr);
            AssignedScriptName = scriptName;
        }

        private MapleCharacter Character
        {
            get
            {
                MapleCharacter ret;
                return CharacterReference.TryGetTarget(out ret) ? ret : null;
            }
        }

        public override int Id => Character.ID;
        public override string Name => Character.Name;
        public override byte Level => Character.Level;
        public override short Job => Character.Job;
        public override short Str => Character.Str;
        public override short Dex => Character.Dex;
        public override short Luk => Character.Luk;
        public override short Int => Character.Int;
        public override int Hp => Character.HP;
        public override int Mp => Character.MP;
        public override int MaxHp => Character.MaxHP;
        public override int MaxMp => Character.MaxMP;
        public override int Fame => Character.Fame;
        public override long Exp => Character.Exp;
        public override short Ap => Character.AP;
        public override short Sp => Character.SP;
        public override int Hair => Character.Hair;
        public override int Face => Character.Hair;
        public override byte Gender => Character.Gender;
        public override byte Skin => Character.Skin;
        public override Point Position => Character.Position;
        public override byte Stance => Character.Stance;
        public override short BuddyCapacity => Character.BuddyCapacity;
        public override int CurrentLevelSkillBook => Character.CurrentLevelSkillBook;
        public override int MapId => Character.MapID;
        public override long Mesos => Character.Inventory.Mesos;
        public override bool HasGuild => Character.Guild != null;
        public override bool IsGuildLeader => Character.Guild?.LeaderID == Character.ID;
        public override int GuildCapacity => Character.Guild.Capacity;
        public override void SetGuildCapacity(int newCapacity) => Character.Guild.Capacity = newCapacity;
        public override bool CreateGuild(string name) => Character.CreateGuild(name);
        public override void DisbandGuild() => Character.Guild?.Disband();
        public override void EnableClientActions() => Character.EnableActions();
        public override void ChangeJob(short newJob) => Character.ChangeJob(newJob);
        public override void GainExp(int exp, bool show = false, bool fromMonster = false) => Character.GainExp(exp, show, fromMonster);
        public override void LevelUp() => Character.LevelUp();
        public override bool HasSkill(int skillId, int skillLevel = -1) => Character.HasSkill(skillId, skillLevel);
        public override void IncreaseSkillLevel(int skillId, byte amount = 1) => Character.IncreaseSkillLevel(skillId, amount);
        public override void SetSkillLevel(int skillId, byte level, byte masterLevel = 0) => Character.SetSkillLevel(skillId, level, masterLevel);
        public override void OpenNpc(int npcId) => Character.OpenNpc(npcId);
        public override void SendBlueMessage(string message) => Character.SendBlueMessage(message);
        public override void SendPopUpMessage(string message) => Character.SendPopUpMessage(message);
        public override void ChangeMap(int mapId, string toPortal = "") => Character.ChangeMap(mapId, toPortal);
        public override bool IsBeginnerJob => Character.IsBeginnerJob;
        public override void SetHair(int newHair) => Character.Hair = newHair; //Todo: update info to client
        public override void SetFace(int newFace) => Character.Face = newFace; //Todo: update info to client
        public override void SetSkin(byte newSkin) => Character.Skin = newSkin; //Todo: update info to client

        #region Inventory
        public override byte BuyItem(ShopItem item, int quantity)
        {
            //SendBlueMessage($"quantity: {item.}");
            if (quantity <= 0 || quantity > item.MaximumPurchase) return 1; //you do not have enough in stock
            if (item.ReqItemId == 0) // Mesos
            {
                //SendBlueMessage("D:");
                int price = item.Price * quantity;
                if (Character.Mesos < price) return 2; // You do not have enough mesos
                if (!AddItemById(item.Id, (short)(quantity))) return 4; // Inventory full
                RemoveMesos(price, false);
                return 0; //success
            }
            else
            {
                int cost = quantity * item.ReqItemQuantity;
                if (!Character.Inventory.HasItem(item.ReqItemId, cost)) return 0x10; // You need more items
                if (!AddItemById(item.Id, (short)quantity)) return 4; // Inventory full
                RemoveItems(item.ReqItemId, cost);
                return 0;
            }
            //3 = that item cannot be purchased right now
        }

        public override bool AddItemById(int itemId, short quantity = 1) => Character.Inventory.AddItemById(itemId, string.Format("Script '{0}' character {1}", AssignedScriptName, Character.Name), quantity);
        public override void GainMesos(int amount, bool showInChat = true, bool fromMonster = false) => Character.Inventory.GainMesos(amount, fromMonster, showInChat);
        public override void RemoveMesos(int amount, bool showInChat = true, bool fromMonster = false) => Character.Inventory.RemoveMesos(amount, fromMonster, showInChat);
        public override bool HasItem(int itemId, int quantity = 1) => Character.Inventory.HasItem(itemId, quantity);
        public override bool RemoveItems(int itemId, int amount) => Character.Inventory.RemoveItemsById(itemId, amount);

        public override List<dynamic> GetEquipsWithRevealedPotential()
        {
            List<MapleEquip> equips = Character.Inventory.GetEquipsWithRevealedPotential();
            return new List<dynamic>(equips.Select(x => x as dynamic));
        }

        #endregion
        #region Quest
        public override int? GetSavedLocation(string scriptName) => Character.GetSavedLocation(scriptName);
        public override void SaveLocation(string scriptName, int value) => Character.SaveLocation(scriptName, value);
        public override void SetQuestData(ushort questId, string data) => Character.SetQuestData(questId, data);
        public override string GetQuestData(ushort questId) => Character.GetQuestData(questId);
        #endregion

        public override bool CubeAndRevealEquip(dynamic dEquip, int cubeIndex)
        {
            MapleEquip equip = dEquip as MapleEquip;
            if (equip == null) return false;
            CubeType cubeType = (CubeType)cubeIndex;
            return MapleEquipEnhancer.CubeItem(equip, cubeType, Character, true);
        }
    }
}