using System;
using Microsoft.EntityFrameworkCore;
using RazzleServer.Common;

namespace RazzleServer.Data
{
    public class MapleDbContext : DbContext
    {
        public DbSet<AccountEntity> Accounts { get; set; }
        public DbSet<BuddyEntity> Buddies { get; set; }
        public DbSet<CharacterEntity> Characters { get; set; }
        public DbSet<AccountStorageEntity> CharacterStorages { get; set; }
        public DbSet<ItemEntity> Items { get; set; }
        public DbSet<QuestStatusEntity> QuestStatus { get; set; }
        public DbSet<QuestMobStatusEntity> QuestStatusMobs { get; set; }
        public DbSet<QuestCustomDataEntity> QuestCustomData { get; set; }
        public DbSet<SkillEntity> Skills { get; set; }
        public DbSet<BuffEntity> Buffs { get; set; }
        public DbSet<MemoEntity> Memos { get; set; }
        public DbSet<LootEntity> Loots { get; set; }
        public DbSet<CheatEntity> Cheats { get; set; }
        public DbSet<TeleportRockEntity> TeleportRocks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            switch (ServerConfig.Instance.DatabaseConnectionType)
            {
                case Common.Constants.DatabaseConnectionType.Sqlite:
                    optionsBuilder.UseSqlite($"Filename=./{ServerConfig.Instance.DatabaseConnection}");
                    break;
                case Common.Constants.DatabaseConnectionType.InMemory:
                    optionsBuilder.UseInMemoryDatabase(ServerConfig.Instance.DatabaseConnection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
