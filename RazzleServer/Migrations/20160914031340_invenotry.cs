using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RazzleServer.Migrations
{
    public partial class invenotry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buddies",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    AccountID = table.Column<int>(nullable: false),
                    BuddyAccountID = table.Column<int>(nullable: false),
                    BuddyCharacterID = table.Column<int>(nullable: false),
                    CharacterID = table.Column<int>(nullable: false),
                    Group = table.Column<string>(maxLength: 16, nullable: true),
                    IsRequest = table.Column<bool>(nullable: false),
                    Memo = table.Column<string>(maxLength: 256, nullable: true),
                    Name = table.Column<string>(maxLength: 13, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buddies", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SkillMacros",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    CharacterID = table.Column<int>(nullable: false),
                    Index = table.Column<byte>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    ShoutName = table.Column<bool>(nullable: false),
                    Skill1 = table.Column<int>(nullable: false),
                    Skill2 = table.Column<int>(nullable: false),
                    Skill3 = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillMacros", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    AllianceID = table.Column<int>(nullable: false),
                    Capacity = table.Column<int>(nullable: false),
                    GP = table.Column<int>(nullable: false),
                    Leader = table.Column<int>(nullable: false),
                    Logo = table.Column<int>(nullable: false),
                    LogoBG = table.Column<int>(nullable: false),
                    LogoBGColor = table.Column<short>(nullable: false),
                    LogoColor = table.Column<short>(nullable: false),
                    Name = table.Column<string>(maxLength: 45, nullable: true),
                    Notice = table.Column<string>(maxLength: 101, nullable: true),
                    Rank1Title = table.Column<string>(maxLength: 45, nullable: true),
                    Rank2Title = table.Column<string>(maxLength: 45, nullable: true),
                    Rank3Title = table.Column<string>(maxLength: 45, nullable: true),
                    Rank4Title = table.Column<string>(maxLength: 45, nullable: true),
                    Rank5Title = table.Column<string>(maxLength: 45, nullable: true),
                    Signature = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "InventoryEquips",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Acc = table.Column<short>(nullable: false),
                    BonusPotential1 = table.Column<short>(nullable: false),
                    BonusPotential2 = table.Column<short>(nullable: false),
                    CustomExp = table.Column<short>(nullable: false),
                    CustomLevel = table.Column<byte>(nullable: false),
                    Dex = table.Column<short>(nullable: false),
                    Diligence = table.Column<short>(nullable: false),
                    Durability = table.Column<int>(nullable: false),
                    Enhancements = table.Column<byte>(nullable: false),
                    Eva = table.Column<short>(nullable: false),
                    HammerApplied = table.Column<byte>(nullable: false),
                    IncMaxHP = table.Column<short>(nullable: false),
                    IncMaxMP = table.Column<short>(nullable: false),
                    Int = table.Column<short>(nullable: false),
                    InventoryItemID = table.Column<long>(nullable: false),
                    Jump = table.Column<short>(nullable: false),
                    Luk = table.Column<short>(nullable: false),
                    Mad = table.Column<short>(nullable: false),
                    Mdd = table.Column<short>(nullable: false),
                    Pad = table.Column<short>(nullable: false),
                    Pdd = table.Column<short>(nullable: false),
                    Potential1 = table.Column<short>(nullable: false),
                    Potential2 = table.Column<short>(nullable: false),
                    Potential3 = table.Column<short>(nullable: false),
                    PotentialState = table.Column<byte>(nullable: false),
                    RemainingUpgradeCount = table.Column<byte>(nullable: false),
                    Socket1 = table.Column<short>(nullable: false),
                    Socket2 = table.Column<short>(nullable: false),
                    Socket3 = table.Column<short>(nullable: false),
                    Speed = table.Column<short>(nullable: false),
                    Str = table.Column<short>(nullable: false),
                    UpgradeCount = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryEquips", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    ID = table.Column<long>(nullable: false)
                        .Annotation("Autoincrement", true),
                    CharacterID = table.Column<int>(nullable: false),
                    Creator = table.Column<string>(maxLength: 13, nullable: true),
                    Flags = table.Column<short>(nullable: false),
                    ItemID = table.Column<int>(nullable: false),
                    Position = table.Column<short>(nullable: false),
                    Quantity = table.Column<short>(nullable: false),
                    Source = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "InventorySlots",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    CashSlots = table.Column<byte>(nullable: false),
                    CharacterID = table.Column<int>(nullable: false),
                    EquipSlots = table.Column<byte>(nullable: false),
                    EtcSlots = table.Column<byte>(nullable: false),
                    SetupSlots = table.Column<byte>(nullable: false),
                    UseSlots = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventorySlots", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "KeyMaps",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Action = table.Column<int>(nullable: false),
                    CharacterID = table.Column<int>(nullable: false),
                    Key = table.Column<byte>(nullable: false),
                    Type = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyMaps", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QuestCustomData",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    CharacterID = table.Column<int>(nullable: false),
                    Key = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestCustomData", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QuestStatusMobs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Count = table.Column<int>(nullable: false),
                    Mob = table.Column<int>(nullable: false),
                    QuestStatusID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestStatusMobs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QuestStatus",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    CharacterID = table.Column<int>(nullable: false),
                    CompleteTime = table.Column<uint>(nullable: false),
                    CustomData = table.Column<string>(maxLength: 255, nullable: true),
                    Quest = table.Column<int>(nullable: false),
                    Status = table.Column<byte>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestStatus", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "QuickSlotKeyMaps",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    CharacterID = table.Column<int>(nullable: false),
                    Index = table.Column<byte>(nullable: false),
                    Key = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickSlotKeyMaps", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SkillCooldowns",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    CharacterID = table.Column<int>(nullable: false),
                    Length = table.Column<int>(nullable: false),
                    SkillID = table.Column<int>(nullable: false),
                    StartTime = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkillCooldowns", x => x.ID);
                });

            migrationBuilder.AddColumn<int>(
                name: "CharacterID",
                table: "Skills",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "Expiration",
                table: "Skills",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<byte>(
                name: "Level",
                table: "Skills",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "MasterLevel",
                table: "Skills",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<short>(
                name: "SkillExp",
                table: "Skills",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<int>(
                name: "SkillID",
                table: "Skills",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CharacterID",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Expiration",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "MasterLevel",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "SkillExp",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "SkillID",
                table: "Skills");

            migrationBuilder.DropTable(
                name: "Buddies");

            migrationBuilder.DropTable(
                name: "SkillMacros");

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
                name: "QuestCustomData");

            migrationBuilder.DropTable(
                name: "QuestStatusMobs");

            migrationBuilder.DropTable(
                name: "QuestStatus");

            migrationBuilder.DropTable(
                name: "QuickSlotKeyMaps");

            migrationBuilder.DropTable(
                name: "SkillCooldowns");
        }
    }
}
