using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RazzleServer.Migrations
{
    public partial class Migraiton1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Characters",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    AP = table.Column<short>(nullable: false),
                    AccountID = table.Column<int>(nullable: false),
                    AllianceRank = table.Column<byte>(nullable: false),
                    BuddyCapacity = table.Column<short>(nullable: false),
                    Dex = table.Column<short>(nullable: false),
                    Exp = table.Column<long>(nullable: false),
                    Face = table.Column<int>(nullable: false),
                    FaceMark = table.Column<int>(nullable: false),
                    Fame = table.Column<int>(nullable: false),
                    Fatigue = table.Column<byte>(nullable: false),
                    Gender = table.Column<byte>(nullable: false),
                    GuildContribution = table.Column<int>(nullable: false),
                    GuildID = table.Column<int>(nullable: false),
                    GuildRank = table.Column<byte>(nullable: false),
                    HP = table.Column<int>(nullable: false),
                    Hair = table.Column<int>(nullable: false),
                    Int = table.Column<short>(nullable: false),
                    Job = table.Column<short>(nullable: false),
                    Level = table.Column<byte>(nullable: false),
                    Luk = table.Column<short>(nullable: false),
                    MP = table.Column<int>(nullable: false),
                    MapID = table.Column<int>(nullable: false),
                    MaxHP = table.Column<int>(nullable: false),
                    MaxMP = table.Column<int>(nullable: false),
                    Mesos = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    SP = table.Column<short>(nullable: false),
                    Skin = table.Column<byte>(nullable: false),
                    SpawnPoint = table.Column<byte>(nullable: false),
                    Str = table.Column<short>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.ID);
                });

            migrationBuilder.AddColumn<byte>(
                name: "AccountType",
                table: "Accounts",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "Gender",
                table: "Accounts",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<int>(
                name: "MaplePoints",
                table: "Accounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NXCredit",
                table: "Accounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NXPrepaid",
                table: "Accounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Accounts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Accounts",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pic",
                table: "Accounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountType",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "MaplePoints",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NXCredit",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "NXPrepaid",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "Pic",
                table: "Accounts");

            migrationBuilder.DropTable(
                name: "Characters");
        }
    }
}
