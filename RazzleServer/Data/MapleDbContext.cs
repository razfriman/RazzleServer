using Microsoft.EntityFrameworkCore;
using RazzleServer.DB;
using RazzleServer.Server;

namespace RazzleServer.Data
{
    
    public class MapleDbContext : DbContext
{
         public DbSet<Account> Accounts { get; set; }
        // public DbSet<Buddy> Buddies { get; set; }
         public DbSet<Character> Characters { get; set; }
        // public DbSet<Guild> Guilds { get; set; }
        // public DbSet<InventoryItem> InventoryItems { get; set; }
        // public DbSet<InventoryEquip> InventoryEquips { get; set; }
        // public DbSet<InventorySlot> InventorySlots { get; set; }
        // public DbSet<KeyMap> KeyMaps { get; set; }
        // public DbSet<QuickSlotKeyMap> QuickSlotKeyMaps { get; set; }
        // public DbSet<DbSkillMacro> SkillMacros { get; set; }
        // public DbSet<QuestStatus> QuestStatus { get; set; }
        // public DbSet<QuestMobStatus> QuestStatusMobs { get; set; }
        // public DbSet<QuestCustomData> QuestCustomData { get; set; }
        public DbSet<Skill> Skills { get; set; }
        // public DbSet<SkillCooldown> SkillCooldowns { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Filename=./{ServerConfig.Instance.DatabaseName}");
    }
}

  
}