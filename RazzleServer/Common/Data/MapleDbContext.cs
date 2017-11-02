using Microsoft.EntityFrameworkCore;
using RazzleServer.DB.Models;
using RazzleServer.Server;

namespace RazzleServer.Data
{

    public class MapleDbContext : DbContext
    {
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<BuddyEntity> Buddies { get; set; }
        public DbSet<CharacterEntity> Characters { get; set; }
        public DbSet<GuildEntity> Guilds { get; set; }
        public DbSet<InventoryItemEntity> InventoryItems { get; set; }
        public DbSet<InventoryEquipEntity> InventoryEquips { get; set; }
        public DbSet<InventorySlotEntity> InventorySlots { get; set; }
        public DbSet<KeyMapEntity> KeyMaps { get; set; }
        public DbSet<QuickSlotKeyMapEntity> QuickSlotKeyMaps { get; set; }
        public DbSet<QuestStatusEntity> QuestStatus { get; set; }
        public DbSet<QuestMobStatusEntity> QuestStatusMobs { get; set; }
        public DbSet<QuestCustomDataEntity> QuestCustomData { get; set; }
        public DbSet<SkillEntity> Skills { get; set; }
        public DbSet<SkillCooldownEntity> SkillCooldowns { get; set; }
        public DbSet<MemoEntity> MemoEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename=./{ServerConfig.Instance.DatabaseName}");
        }
    }
}