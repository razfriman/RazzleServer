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
                    Group = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    IsRequest = table.Column<bool>(type: "INTEGER", nullable: false),
                    Memo = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 13, nullable: true)
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
                    AP = table.Column<short>(type: "INTEGER", nullable: false),
                    AccountID = table.Column<int>(type: "INTEGER", nullable: false),
                    CashSlots = table.Column<int>(type: "INTEGER", nullable: false),
                    Dex = table.Column<short>(type: "INTEGER", nullable: false),
                    EquipmentSlots = table.Column<int>(type: "INTEGER", nullable: false),
                    EtceteraSlots = table.Column<int>(type: "INTEGER", nullable: false),
                    Exp = table.Column<int>(type: "INTEGER", nullable: false),
                    Face = table.Column<int>(type: "INTEGER", nullable: false),
                    Fame = table.Column<short>(type: "INTEGER", nullable: false),
                    Gender = table.Column<byte>(type: "INTEGER", nullable: false),
                    GuildContribution = table.Column<int>(type: "INTEGER", nullable: false),
                    GuildID = table.Column<int>(type: "INTEGER", nullable: false),
                    GuildRank = table.Column<int>(type: "INTEGER", nullable: true),
                    HP = table.Column<short>(type: "INTEGER", nullable: false),
                    Hair = table.Column<int>(type: "INTEGER", nullable: false),
                    Int = table.Column<short>(type: "INTEGER", nullable: false),
                    Job = table.Column<short>(type: "INTEGER", nullable: false),
                    Level = table.Column<byte>(type: "INTEGER", nullable: false),
                    Luk = table.Column<short>(type: "INTEGER", nullable: false),
                    MP = table.Column<short>(type: "INTEGER", nullable: false),
                    MapID = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxHP = table.Column<short>(type: "INTEGER", nullable: false),
                    MaxMP = table.Column<short>(type: "INTEGER", nullable: false),
                    Mesos = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    SP = table.Column<short>(type: "INTEGER", nullable: false),
                    SetupSlots = table.Column<int>(type: "INTEGER", nullable: false),
                    Skin = table.Column<byte>(type: "INTEGER", nullable: false),
                    SpawnPoint = table.Column<byte>(type: "INTEGER", nullable: false),
                    Str = table.Column<short>(type: "INTEGER", nullable: false),
                    UsableSlots = table.Column<int>(type: "INTEGER", nullable: false),
                    WorldID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.ID);
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
                name: "InventoryEquips",
                columns: table => new
                {
                    ID = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Acc = table.Column<short>(type: "INTEGER", nullable: false),
                    BonusPotential1 = table.Column<short>(type: "INTEGER", nullable: false),
                    BonusPotential2 = table.Column<short>(type: "INTEGER", nullable: false),
                    CustomExp = table.Column<short>(type: "INTEGER", nullable: false),
                    CustomLevel = table.Column<byte>(type: "INTEGER", nullable: false),
                    Dex = table.Column<short>(type: "INTEGER", nullable: false),
                    Diligence = table.Column<short>(type: "INTEGER", nullable: false),
                    Durability = table.Column<int>(type: "INTEGER", nullable: false),
                    Enhancements = table.Column<byte>(type: "INTEGER", nullable: false),
                    Eva = table.Column<short>(type: "INTEGER", nullable: false),
                    IncMaxHP = table.Column<short>(type: "INTEGER", nullable: false),
                    IncMaxMP = table.Column<short>(type: "INTEGER", nullable: false),
                    Int = table.Column<short>(type: "INTEGER", nullable: false),
                    InventoryItemID = table.Column<long>(type: "INTEGER", nullable: false),
                    Jump = table.Column<short>(type: "INTEGER", nullable: false),
                    Luk = table.Column<short>(type: "INTEGER", nullable: false),
                    Mad = table.Column<short>(type: "INTEGER", nullable: false),
                    Mdd = table.Column<short>(type: "INTEGER", nullable: false),
                    Pad = table.Column<short>(type: "INTEGER", nullable: false),
                    Pdd = table.Column<short>(type: "INTEGER", nullable: false),
                    Potential1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Potential2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Potential3 = table.Column<short>(type: "INTEGER", nullable: false),
                    PotentialState = table.Column<byte>(type: "INTEGER", nullable: false),
                    RemainingUpgradeCount = table.Column<byte>(type: "INTEGER", nullable: false),
                    Socket1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Socket2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Socket3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Speed = table.Column<short>(type: "INTEGER", nullable: false),
                    Str = table.Column<short>(type: "INTEGER", nullable: false),
                    UpgradeCount = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryEquips", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    ID = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    Creator = table.Column<string>(type: "TEXT", maxLength: 13, nullable: true),
                    Flags = table.Column<short>(type: "INTEGER", nullable: false),
                    ItemID = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<short>(type: "INTEGER", nullable: false),
                    Quantity = table.Column<short>(type: "INTEGER", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "InventorySlots",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CashSlots = table.Column<byte>(type: "INTEGER", nullable: false),
                    CharacterID = table.Column<int>(type: "INTEGER", nullable: false),
                    EquipSlots = table.Column<byte>(type: "INTEGER", nullable: false),
                    EtcSlots = table.Column<byte>(type: "INTEGER", nullable: false),
                    SetupSlots = table.Column<byte>(type: "INTEGER", nullable: false),
                    UseSlots = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySlots", x => x.ID);
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
                    Quest = table.Column<int>(type: "INTEGER", nullable: false),
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
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "InventoryEquips");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "InventorySlots");

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
