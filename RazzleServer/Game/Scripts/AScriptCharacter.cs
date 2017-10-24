using RazzleServer.Map;
using System.Collections.Generic;
using System.Drawing;

namespace RazzleServer.Scripts
{
    /// <summary>
    /// Wrapper for the main MapleCharacter
    /// Scripters, do not modify this file.
    /// </summary>
    public abstract class AScriptCharacter
    {
        public abstract int Id { get; }
        public abstract string Name { get; }
        public abstract byte Level { get; }
        public abstract short Job { get; }
        public abstract short Str { get; }
        public abstract short Dex { get; }
        public abstract short Luk { get; }
        public abstract short Int { get; }
        public abstract int Hp { get; }
        public abstract int Mp { get; }
        public abstract int MaxHp { get; }
        public abstract int MaxMp { get; }
        public abstract int Fame { get; }
        public abstract long Exp { get; }
        public abstract short Ap { get; }
        public abstract short Sp { get; }
        public abstract int Hair { get; }
        public abstract int Face { get; }
        public abstract byte Gender { get; }
        public abstract byte Skin { get; }
        public abstract Point Position { get; }
        public abstract byte Stance { get; }
        public abstract short BuddyCapacity { get; }
        public abstract int CurrentLevelSkillBook { get; }
        public abstract int MapId { get; }
        public abstract long Mesos { get; }
        public abstract bool HasGuild { get; }
        public abstract bool IsGuildLeader { get; }
        public abstract int GuildCapacity { get; }
        public abstract void SetGuildCapacity(int newCapacity);
        public abstract void EnableClientActions();
        public abstract void DisbandGuild();
        public abstract void ChangeJob(short newJob);
        public abstract void GainExp(int exp, bool show = false, bool fromMonster = false);
        public abstract void LevelUp();
        public abstract bool HasSkill(int skillId, int skillLevel = -1);
        public abstract void IncreaseSkillLevel(int skillId, byte amount = 1);
        public abstract void SetSkillLevel(int skillId, byte level, byte masterLevel = 0);
        public abstract void OpenNpc(int npcId);
        public abstract void SendBlueMessage(string message);
        public abstract void SendPopUpMessage(string message);
        public abstract void ChangeMap(int mapId, string toPortal = "");
        public abstract void SetHair(int newHair);
        public abstract void SetFace(int newFace);
        public abstract void SetSkin(byte newSkin);

        #region inventory stuff

        public abstract byte BuyItem(ShopItem item, int quantity);

        /// <summary>
        /// Adds item(s) to the character's inventory using an itemId.
        /// </summary>
        /// <param name="itemId">The ID of the item to be added</param> 
        /// <param name="quantity">The amount of items to be added</param>
        /// <returns></returns>
        public abstract bool AddItemById(int itemId, short quantity = 1);
        //return Character.Inventory.AddItemById(itemId, "Script by player " + Name, quantity);
        public abstract void GainMesos(int gain, bool showInChat = true, bool fromMonster = false);
        public abstract void RemoveMesos(int loss, bool showInChat = true, bool fromMonster = false);
        public abstract bool HasItem(int itemId, int quantity = 1);
        public abstract bool RemoveItems(int itemId, int amount);
        public abstract List<dynamic> GetEquipsWithRevealedPotential();
        //return Character.Inventory.GetEquipsWithRevealedPotential();

        #endregion
        #region quest stuff

        public abstract int? GetSavedLocation(string script);
        public abstract void SaveLocation(string script, int value);
        public abstract void SetQuestData(ushort questId, string data);
        public abstract string GetQuestData(ushort questId);
        #endregion

        public abstract bool CreateGuild(string name);

        public abstract bool CubeAndRevealEquip(dynamic dEquip, int cubeIndex);

        public abstract bool IsBeginnerJob { get; }
        public bool IsExplorer => Job < 600;
        public bool IsWarrior => Job / 100 == 1;
        public bool IsFighter => Job / 10 == 11;
        public bool IsPage => Job / 10 == 12;
        public bool IsSpearman => Job / 10 == 13;
        public bool IsMagician => Job / 100 == 2;
        public bool IsFirePoisonMage => Job / 10 == 21;
        public bool IsIceLightningMage => Job / 10 == 22;
        public bool IsCleric => Job / 10 == 23;
        public bool IsArcher => Job / 100 == 3;
        public bool IsHunter => Job / 10 == 31;
        public bool IsCrossbowman => Job / 10 == 32;
        public bool IsThief => Job / 100 == 4;
        public bool IsAssassin => Job / 10 == 41;
        public bool IsBandit => Job / 10 == 42;
        public bool IsPirate => Job / 100 == 5;
        public bool IsBrawler => Job / 10 == 51;
        public bool IsGunslinger => Job / 10 == 52;
        public bool IsJett => Job == 508 || Job / 10 == 57;
        public bool IsCygnus => Job / 1000 == 1;
        public bool IsDawnWarrior => Job / 10 == 11;
        public bool IsBlazeWizard => Job / 10 == 12;
        public bool IsWindArcher => Job / 10 == 13;
        public bool IsNightWalker => Job / 10 == 14;
        public bool IsThunderBreaker => Job / 10 == 15;

        public bool IsHero => Job / 1000 == 2;
        public bool IsAran => Job == 2000 || Job / 100 == 21;
        public bool IsEvan => Job == 2001 || Job / 100 == 22;
        public bool IsMercedes => Job == 2002 || Job / 100 == 23;
        public bool IsPhantom => Job == 2003 || Job / 100 == 24;
        public bool IsLuminous => Job == 2004 || Job / 100 == 27;
        public bool IsShade => Job == 2005 || Job / 100 == 26;
        public bool IsResistance => Job / 1000 == 3;
        public bool IsDemon => Job == 3001 || Job / 100 == 31;
        public bool IsDemonSlayer => Job == 3100 || Job / 10 == 311;
        public bool IsDemonAvenger => Job == 3101 || Job / 10 == 312;
        public bool IsBattleMage => Job / 100 == 32;
        public bool IsWildHunter => Job / 100 == 33;
        public bool IsMechanic => Job / 100 == 35;
        public bool IsXenon => Job == 3002 || Job / 100 == 36;

        public bool IsSengoku => Job / 1000 == 4;
        public bool IsHayato => Job == 4001 || Job / 100 == 41;
        public bool IsKanna => Job == 4002 || Job / 100 == 42;

        public bool IsMihile => Job == 5000 || Job / 100 == 51;

        public bool IsNova => Job / 1000 == 6;
        public bool IsKaiser => Job == 6000 || Job / 100 == 61;
        public bool IsAngelicBuster => Job == 6001 || Job / 100 == 65;

        public bool IsZero => Job == 10000 || Job / 100 == 101;
        public bool IsBeastTamer => Job == 11000 || Job / 100 == 112;
    }
}
