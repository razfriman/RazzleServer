using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace RazzleServer.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Birthday = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Creation = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Gender = table.Column<byte>(type: "INTEGER", nullable: false),
                    IsBanned = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsMaster = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxCharacters = table.Column<int>(type: "INTEGER", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    Pin = table.Column<string>(type: "TEXT", nullable: true),
                    Salt = table.Column<string>(type: "TEXT", nullable: true),
                    Username = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Buddies",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountID = table.Column<int>(type: "INTEGER", nullable: false),
                    BuddyAccountID = table.Column<int>(type: "INTEGER", nullable: false),
                    BuddyCharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRequest = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buddies", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AbilityPoints = table.Column<short>(type: "INTEGER", nullable: false),
                    AccountID = table.Column<int>(type: "INTEGER", nullable: false),
                    BuddyListSlots = table.Column<int>(type: "INTEGER", nullable: false),
                    CashSlots = table.Column<byte>(type: "INTEGER", nullable: false),
                    Dexterity = table.Column<short>(type: "INTEGER", nullable: false),
                    EquipmentSlots = table.Column<byte>(type: "INTEGER", nullable: false),
                    EtceteraSlots = table.Column<byte>(type: "INTEGER", nullable: false),
                    Experience = table.Column<int>(type: "INTEGER", nullable: false),
                    Face = table.Column<int>(type: "INTEGER", nullable: false),
                    Fame = table.Column<short>(type: "INTEGER", nullable: false),
                    Gender = table.Column<byte>(type: "INTEGER", nullable: false),
                    GuildContribution = table.Column<int>(type: "INTEGER", nullable: false),
                    GuildID = table.Column<int>(type: "INTEGER", nullable: false),
                    GuildRank = table.Column<int>(type: "INTEGER", nullable: true),
                    Hair = table.Column<int>(type: "INTEGER", nullable: false),
                    Health = table.Column<short>(type: "INTEGER", nullable: false),
                    Intelligence = table.Column<short>(type: "INTEGER", nullable: false),
                    Job = table.Column<short>(type: "INTEGER", nullable: false),
                    JobRank = table.Column<int>(type: "INTEGER", nullable: false),
                    JobRankMove = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    Luck = table.Column<short>(type: "INTEGER", nullable: false),
                    Mana = table.Column<short>(type: "INTEGER", nullable: false),
                    MapID = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxHealth = table.Column<short>(type: "INTEGER", nullable: false),
                    MaxMana = table.Column<short>(type: "INTEGER", nullable: false),
                    Meso = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    RankMove = table.Column<int>(type: "INTEGER", nullable: false),
                    SetupSlots = table.Column<byte>(type: "INTEGER", nullable: false),
                    SkillPoints = table.Column<short>(type: "INTEGER", nullable: false),
                    Skin = table.Column<byte>(type: "INTEGER", nullable: false),
                    SpawnPoint = table.Column<byte>(type: "INTEGER", nullable: false),
                    Strength = table.Column<short>(type: "INTEGER", nullable: false),
                    UsableSlots = table.Column<byte>(type: "INTEGER", nullable: false),
                    WorldID = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "CharacterStorages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountID = table.Column<int>(type: "INTEGER", nullable: false),
                    Meso = table.Column<int>(type: "INTEGER", nullable: false),
                    Slots = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterStorages", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    GP = table.Column<int>(type: "INTEGER", nullable: false),
                    Leader = table.Column<int>(type: "INTEGER", nullable: false),
                    Logo = table.Column<int>(type: "INTEGER", nullable: false),
                    LogoBG = table.Column<int>(type: "INTEGER", nullable: false),
                    LogoBGColor = table.Column<short>(type: "INTEGER", nullable: false),
                    LogoColor = table.Column<short>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    Notice = table.Column<string>(type: "TEXT", maxLength: 101, nullable: true),
                    Rank1Title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    Rank2Title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    Rank3Title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    Rank4Title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    Rank5Title = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    Signature = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountID = table.Column<int>(type: "INTEGER", nullable: false),
                    Accuracy = table.Column<short>(type: "INTEGER", nullable: false),
                    Agility = table.Column<short>(type: "INTEGER", nullable: false),
                    Avoidability = table.Column<short>(type: "INTEGER", nullable: false),
                    BonusPotential1 = table.Column<short>(type: "INTEGER", nullable: false),
                    BonusPotential2 = table.Column<short>(type: "INTEGER", nullable: false),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", maxLength: 13, nullable: true),
                    CustomExp = table.Column<short>(type: "INTEGER", nullable: false),
                    CustomLevel = table.Column<byte>(type: "INTEGER", nullable: false),
                    Dexterity = table.Column<short>(type: "INTEGER", nullable: false),
                    Durability = table.Column<int>(type: "INTEGER", nullable: false),
                    Enhancements = table.Column<byte>(type: "INTEGER", nullable: false),
                    Expiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Flags = table.Column<short>(type: "INTEGER", nullable: false),
                    Health = table.Column<short>(type: "INTEGER", nullable: false),
                    Intelligence = table.Column<short>(type: "INTEGER", nullable: false),
                    IsStored = table.Column<bool>(type: "INTEGER", nullable: false),
                    Jump = table.Column<short>(type: "INTEGER", nullable: false),
                    Luck = table.Column<short>(type: "INTEGER", nullable: false),
                    MagicAttack = table.Column<short>(type: "INTEGER", nullable: false),
                    MagicDefense = table.Column<short>(type: "INTEGER", nullable: false),
                    Mana = table.Column<short>(type: "INTEGER", nullable: false),
                    MapleID = table.Column<int>(type: "INTEGER", nullable: false),
                    PetID = table.Column<int>(type: "INTEGER", nullable: true),
                    Position = table.Column<short>(type: "INTEGER", nullable: false),
                    Potential1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Potential2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Potential3 = table.Column<short>(type: "INTEGER", nullable: false),
                    PotentialState = table.Column<byte>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<short>(type: "INTEGER", nullable: false),
                    Slot = table.Column<byte>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    Speed = table.Column<short>(type: "INTEGER", nullable: false),
                    Strength = table.Column<short>(type: "INTEGER", nullable: false),
                    UpgradesApplied = table.Column<byte>(type: "INTEGER", nullable: false),
                    UpgradesAvailable = table.Column<byte>(type: "INTEGER", nullable: false),
                    WeaponAttack = table.Column<short>(type: "INTEGER", nullable: false),
                    WeaponDefense = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "KeyMaps",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<byte>(type: "INTEGER", nullable: false),
                    Type = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyMaps", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MemoEntities",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    Received = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Sender = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemoEntities", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QuestCustomData",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestCustomData", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QuestStatus",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    CompleteTime = table.Column<uint>(type: "INTEGER", nullable: false),
                    CustomData = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    QuestID = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestStatus", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QuestStatusMobs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Count = table.Column<int>(type: "INTEGER", nullable: false),
                    Mob = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestStatusID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestStatusMobs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QuickSlotKeyMaps",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    Index = table.Column<byte>(type: "INTEGER", nullable: false),
                    Key = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickSlotKeyMaps", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SkillCooldowns",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    Length = table.Column<int>(type: "INTEGER", nullable: false),
                    SkillID = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillCooldowns", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    Expiration = table.Column<long>(type: "INTEGER", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    MasterLevel = table.Column<byte>(type: "INTEGER", nullable: false),
                    SkillExp = table.Column<short>(type: "INTEGER", nullable: false),
                    SkillID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.ID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Buddies");

            migrationBuilder.DropTable(
                name: "Characters");

            migrationBuilder.DropTable(
                name: "CharacterStorages");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "KeyMaps");

            migrationBuilder.DropTable(
                name: "MemoEntities");

            migrationBuilder.DropTable(
                name: "QuestCustomData");

            migrationBuilder.DropTable(
                name: "QuestStatus");

            migrationBuilder.DropTable(
                name: "QuestStatusMobs");

            migrationBuilder.DropTable(
                name: "QuickSlotKeyMaps");

            migrationBuilder.DropTable(
                name: "SkillCooldowns");

            migrationBuilder.DropTable(
                name: "Skills");
        }
    }
}
