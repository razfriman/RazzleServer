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
        public DbSet<CharacterStorageEntity> CharacterStorages { get; set; }
        public DbSet<GuildEntity> Guilds { get; set; }
        public DbSet<ItemEntity> Items { get; set; }
        public DbSet<KeyMapEntity> KeyMaps { get; set; }
        public DbSet<QuickSlotKeyMapEntity> QuickSlotKeyMaps { get; set; }
        public DbSet<QuestStatusEntity> QuestStatus { get; set; }
        public DbSet<QuestMobStatusEntity> QuestStatusMobs { get; set; }
        public DbSet<QuestCustomDataEntity> QuestCustomData { get; set; }
        public DbSet<SkillEntity> Skills { get; set; }
        public DbSet<BuffEntity> Buffs { get; set; }
        public DbSet<MemoEntity> MemoEntities { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename=./{ServerConfig.Instance.DatabaseName}");
        }
    }
}