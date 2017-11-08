namespace RazzleServer.DB.Models
{
    public class CharacterEntity
    {
        public int ID { get; set; }
        public byte WorldID { get; set; }
        public int AccountID { get; set; }
        public string Name { get; set; }
        public byte Level { get; set; }
        public short Job { get; set; }
        public short Strength { get; set; }
        public short Dexterity { get; set; }
        public short Luck { get; set; }
        public short Intelligence { get; set; }
        public short SkillPoints { get; set; }
        public short AbilityPoints { get; set; }
        public int GuildID { get; set; }
        public int Experience { get; set; }
        public int Meso { get; set; }
        public int MapID { get; set; }
        public byte SpawnPoint { get; set; }
        public short Health { get; set; }
        public short Mana { get; set; }
        public short MaxHealth { get; set; }
        public short MaxMana { get; set; }
        public short Fame { get; set; }
        public int Hair { get; set; }
        public int Face { get; set; }
        public int GuildContribution { get; set; }
        public byte Gender { get; set; }
        public byte Skin { get; set; }
        public int? GuildRank { get; set; }
        public byte EquipmentSlots { get; set; }
        public byte UsableSlots { get; set; }
        public byte SetupSlots { get; set; }
        public byte EtceteraSlots { get; set; }
        public byte CashSlots { get; set; }
        public int BuddyListSlots { get; set; }
        public int Rank { get; set; }
        public int RankMove { get; set; }
        public int JobRank { get; set; }
        public int JobRankMove { get; set; }

    }
}