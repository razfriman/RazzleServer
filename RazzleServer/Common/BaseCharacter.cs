using System;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Data;
using RazzleServer.Game;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Common
{
    public class BaseCharacter
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public byte WorldId { get; set; }
        public string Name { get; set; }
        public bool IsInitialized { get; private set; }
        public byte SpawnPoint { get; set; }
        public byte Stance { get; set; }
        public short Foothold { get; set; }
        public byte Portals { get; set; }
        public int Chair { get; set; }
        public int Rank { get; set; }
        public int RankMove { get; set; }
        public int JobRank { get; set; }
        public int JobRankMove { get; set; }
        private bool Assigned { get; set; }

        public CharacterStats PrimaryStats { get; set; }

        private int _itemEffect;

        private readonly ILogger _log = Log.ForContext<Character>();

        public bool IsAlive => PrimaryStats.Health > 0;

        public bool FacesLeft => Stance % 2 == 0;

        public bool IsRanked => PrimaryStats.Level >= 30;

        public QuestReference LastQuest { get; set; }

        public Character(int id = 0, GameClient client = null)
        {
            Id = id;
            Client = client;
            Items = new CharacterItems(this, 100, 100, 100, 100, 100);
            Pets = new CharacterPets(this);
            Skills = new CharacterSkills(this);
            Quests = new CharacterQuests(this);
            Rings = new CharacterRings(this);
            Summons = new CharacterSummons(this);
            Buffs = new CharacterBuffs(this);
            PrimaryStats = new CharacterStats(this);
            Pets = new CharacterPets(this);
            TeleportRocks = new CharacterTeleportRocks(this);
            Storage = new CharacterStorage(this);
            Position = new Point(0, 0);
            ControlledMobs = new ControlledMobs(this);
            ControlledNpcs = new ControlledNpcs(this);
            Damage = new CharacterDamage(this);
        }

        public virtual void Initialize()
        {
            
        }

        public void Load()
        {
            using var dbContext = new MapleDbContext();
            var character = dbContext.Characters.Find(Id);

            if (character == null)
            {
                _log.Error($"Cannot find character [{Id}]");
                return;
            }

            Assigned = true;
            Name = character.Name;
            AccountId = character.AccountId;
            PrimaryStats.Load(character);
            SpawnPoint = character.SpawnPoint;
            WorldId = character.WorldId;
            Items.MaxSlots[ItemType.Equipment] = character.EquipmentSlots;
            Items.MaxSlots[ItemType.Usable] = character.UsableSlots;
            Items.MaxSlots[ItemType.Setup] = character.SetupSlots;
            Items.MaxSlots[ItemType.Etcetera] = character.EtceteraSlots;
            Items.MaxSlots[ItemType.Pet] = character.CashSlots;
            Items.Load();
            Skills.Load();
            Quests.Load();
            TeleportRocks.Load();
        }

        public void Send(PacketWriter packet) => Client.Send(packet);

        public void Hide(bool isHidden)
        {
            if (isHidden)
            {
                Map.Characters.Hide(this);
            }
            else
            {
                Map.Characters.Show(this);
            }

            using var pw = new PacketWriter(ServerOperationCode.AdminResult);
            pw.WriteByte(AdminResultType.Hide);
            pw.WriteBool(isHidden);
            Send(pw);
        }
    }
}
