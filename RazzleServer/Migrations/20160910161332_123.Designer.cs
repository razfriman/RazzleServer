using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using RazzleServer.Data;

namespace RazzleServer.Migrations
{
    [DbContext(typeof(MapleDbContext))]
    [Migration("20160910161332_123")]
    partial class _123
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("RazzleServer.DB.Account", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("AccountType");

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

            modelBuilder.Entity("RazzleServer.DB.Character", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<short>("AP");

                    b.Property<int>("AccountID");

                    b.Property<byte>("AllianceRank");

                    b.Property<short>("BuddyCapacity");

                    b.Property<short>("Dex");

                    b.Property<long>("Exp");

                    b.Property<int>("Face");

                    b.Property<int>("FaceMark");

                    b.Property<int>("Fame");

                    b.Property<byte>("Fatigue");

                    b.Property<byte>("Gender");

                    b.Property<int>("GuildContribution");

                    b.Property<int>("GuildID");

                    b.Property<byte>("GuildRank");

                    b.Property<int>("HP");

                    b.Property<int>("Hair");

                    b.Property<short>("Int");

                    b.Property<short>("Job");

                    b.Property<byte>("Level");

                    b.Property<short>("Luk");

                    b.Property<int>("MP");

                    b.Property<int>("MapID");

                    b.Property<int>("MaxHP");

                    b.Property<int>("MaxMP");

                    b.Property<long>("Mesos");

                    b.Property<string>("Name");

                    b.Property<short>("SP");

                    b.Property<byte>("Skin");

                    b.Property<byte>("SpawnPoint");

                    b.Property<short>("Str");

                    b.HasKey("ID");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("RazzleServer.DB.Skill", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.HasKey("ID");

                    b.ToTable("Skills");
                });
        }
    }
}
