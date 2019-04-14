using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Net.Packet;

namespace RazzleServer.Common
{
    public interface ICharacter : IMapObject
    {
        int Id { get; set; }

        int AccountId { get; set; }
        bool IsMaster { get; }
        byte WorldId { get; set; }
        string Name { get; set; }
        bool IsInitialized { get; set; }
        byte SpawnPoint { get; set; }
        byte Stance { get; set; }
        int MapId { get; set; }
        short Foothold { get; set; }
        byte Portals { get; set; }
        int Chair { get; set; }
        int Rank { get; set; }
        int RankMove { get; set; }
        int JobRank { get; set; }
        int JobRankMove { get; set; }
        CharacterItems Items { get; }
        CharacterStats PrimaryStats { get; }
        CharacterTeleportRocks TeleportRocks { get; }
        CharacterSkills Skills { get; }
        CharacterQuests Quests { get; }
        CharacterRings Rings { get; }
        CharacterBuffs Buffs { get; }
        CharacterSummons Summons { get; set; }
        CharacterStorage Storage { get; }
        CharacterPets Pets { get; set; }

        void Send(PacketWriter packet);

        void ChangeMap(int mapId, string portalLabel);

        void ChangeMap(int mapId, byte? portalId = null);
        
        void Notify(string message, NoticeType type = NoticeType.PinkText);
        void Revive();
        void Attack(PacketReader packet, AttackType type);
        void Talk(string text, bool show = true);
        void PerformFacialExpression(int expressionId);
        void ShowLocalUserEffect(UserEffect effect);
        void ShowRemoteUserEffect(UserEffect effect, bool skipSelf = false);
        void Converse(Npc npc);
        void Save();
        void Load();
        void Hide(bool isHidden);
        void LogCheatWarning(CheatType type);
        byte[] DataToByteArray();
        byte[] AppearanceToByteArray();

    }
}
