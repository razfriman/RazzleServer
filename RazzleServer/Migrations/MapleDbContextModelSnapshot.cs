﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using RazzleServer.Data;
using System;

namespace RazzleServer.Migrations
{
    [DbContext(typeof(MapleDbContext))]
    partial class MapleDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("RazzleServer.DB.Models.Account", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("AccountType");

                    b.Property<int>("CharacterSlots");

                    b.Property<byte>("Gender");

                    b.Property<int>("MaplePoints");

                    b.Property<int>("NXCredit");

                    b.Property<int>("NXPrepaid");

                    b.Property<string>("Name");

                    b.Property<string>("Password");

                    b.Property<string>("Pic");

                    b.HasKey("ID");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.Buddy", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountID");

                    b.Property<int>("BuddyAccountID");

                    b.Property<int>("BuddyCharacterID");

                    b.Property<int>("CharacterID");

                    b.Property<string>("Group")
                        .HasMaxLength(16);

                    b.Property<bool>("IsRequest");

                    b.Property<string>("Memo")
                        .HasMaxLength(256);

                    b.Property<string>("Name")
                        .HasMaxLength(13);

                    b.HasKey("ID");

                    b.ToTable("Buddies");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.Character", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<short>("AP");

                    b.Property<int>("AccountID");

                    b.Property<byte>("AllianceRank");

                    b.Property<byte>("BuddyCapacity");

                    b.Property<short>("Dex");

                    b.Property<int>("Exp");

                    b.Property<int>("Face");

                    b.Property<int>("FaceMark");

                    b.Property<short>("Fame");

                    b.Property<byte>("Fatigue");

                    b.Property<byte>("Gender");

                    b.Property<int>("GuildContribution");

                    b.Property<int>("GuildID");

                    b.Property<byte>("GuildRank");

                    b.Property<short>("HP");

                    b.Property<int>("Hair");

                    b.Property<short>("Int");

                    b.Property<short>("Job");

                    b.Property<byte>("Level");

                    b.Property<short>("Luk");

                    b.Property<short>("MP");

                    b.Property<int>("MapID");

                    b.Property<short>("MaxHP");

                    b.Property<short>("MaxMP");

                    b.Property<int>("Mesos");

                    b.Property<string>("Name");

                    b.Property<short>("SP");

                    b.Property<byte>("Skin");

                    b.Property<byte>("SpawnPoint");

                    b.Property<short>("Str");

                    b.HasKey("ID");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.DbSkillMacro", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CharacterID");

                    b.Property<byte>("Index");

                    b.Property<string>("Name");

                    b.Property<bool>("ShoutName");

                    b.Property<int>("Skill1");

                    b.Property<int>("Skill2");

                    b.Property<int>("Skill3");

                    b.HasKey("ID");

                    b.ToTable("SkillMacros");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.Guild", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AllianceID");

                    b.Property<int>("Capacity");

                    b.Property<int>("GP");

                    b.Property<int>("Leader");

                    b.Property<int>("Logo");

                    b.Property<int>("LogoBG");

                    b.Property<short>("LogoBGColor");

                    b.Property<short>("LogoColor");

                    b.Property<string>("Name")
                        .HasMaxLength(45);

                    b.Property<string>("Notice")
                        .HasMaxLength(101);

                    b.Property<string>("Rank1Title")
                        .HasMaxLength(45);

                    b.Property<string>("Rank2Title")
                        .HasMaxLength(45);

                    b.Property<string>("Rank3Title")
                        .HasMaxLength(45);

                    b.Property<string>("Rank4Title")
                        .HasMaxLength(45);

                    b.Property<string>("Rank5Title")
                        .HasMaxLength(45);

                    b.Property<int>("Signature");

                    b.HasKey("ID");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.InventoryEquip", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<short>("Acc");

                    b.Property<short>("BonusPotential1");

                    b.Property<short>("BonusPotential2");

                    b.Property<short>("CustomExp");

                    b.Property<byte>("CustomLevel");

                    b.Property<short>("Dex");

                    b.Property<short>("Diligence");

                    b.Property<int>("Durability");

                    b.Property<byte>("Enhancements");

                    b.Property<short>("Eva");

                    b.Property<byte>("HammerApplied");

                    b.Property<short>("IncMaxHP");

                    b.Property<short>("IncMaxMP");

                    b.Property<short>("Int");

                    b.Property<long>("InventoryItemID");

                    b.Property<short>("Jump");

                    b.Property<short>("Luk");

                    b.Property<short>("Mad");

                    b.Property<short>("Mdd");

                    b.Property<short>("Pad");

                    b.Property<short>("Pdd");

                    b.Property<short>("Potential1");

                    b.Property<short>("Potential2");

                    b.Property<short>("Potential3");

                    b.Property<byte>("PotentialState");

                    b.Property<byte>("RemainingUpgradeCount");

                    b.Property<short>("Socket1");

                    b.Property<short>("Socket2");

                    b.Property<short>("Socket3");

                    b.Property<short>("Speed");

                    b.Property<short>("Str");

                    b.Property<byte>("UpgradeCount");

                    b.HasKey("ID");

                    b.ToTable("InventoryEquips");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.InventoryItem", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CharacterID");

                    b.Property<string>("Creator")
                        .HasMaxLength(13);

                    b.Property<short>("Flags");

                    b.Property<int>("ItemID");

                    b.Property<short>("Position");

                    b.Property<short>("Quantity");

                    b.Property<string>("Source");

                    b.HasKey("ID");

                    b.ToTable("InventoryItems");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.InventorySlot", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("CashSlots");

                    b.Property<int>("CharacterID");

                    b.Property<byte>("EquipSlots");

                    b.Property<byte>("EtcSlots");

                    b.Property<byte>("SetupSlots");

                    b.Property<byte>("UseSlots");

                    b.HasKey("ID");

                    b.ToTable("InventorySlots");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.KeyMap", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Action");

                    b.Property<int>("CharacterID");

                    b.Property<byte>("Key");

                    b.Property<byte>("Type");

                    b.HasKey("ID");

                    b.ToTable("KeyMaps");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.QuestCustomData", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CharacterID");

                    b.Property<string>("Key");

                    b.Property<string>("Value");

                    b.HasKey("ID");

                    b.ToTable("QuestCustomData");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.QuestMobStatus", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Count");

                    b.Property<int>("Mob");

                    b.Property<int>("QuestStatusID");

                    b.HasKey("ID");

                    b.ToTable("QuestStatusMobs");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.QuestStatus", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CharacterID");

                    b.Property<uint>("CompleteTime");

                    b.Property<string>("CustomData")
                        .HasMaxLength(255);

                    b.Property<int>("Quest");

                    b.Property<byte>("Status");

                    b.HasKey("ID");

                    b.ToTable("QuestStatus");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.QuickSlotKeyMap", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CharacterID");

                    b.Property<byte>("Index");

                    b.Property<int>("Key");

                    b.HasKey("ID");

                    b.ToTable("QuickSlotKeyMaps");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.Skill", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CharacterID");

                    b.Property<long>("Expiration");

                    b.Property<byte>("Level");

                    b.Property<byte>("MasterLevel");

                    b.Property<short>("SkillExp");

                    b.Property<int>("SkillID");

                    b.HasKey("ID");

                    b.ToTable("Skills");
                });

            modelBuilder.Entity("RazzleServer.DB.Models.SkillCooldown", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CharacterID");

                    b.Property<int>("Length");

                    b.Property<int>("SkillID");

                    b.Property<long>("StartTime");

                    b.HasKey("ID");

                    b.ToTable("SkillCooldowns");
                });
#pragma warning restore 612, 618
        }
    }
}
