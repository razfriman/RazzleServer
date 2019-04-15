using System;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.DataProvider.References;
using RazzleServer.Game.Maple.Interaction;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Game.Maple.Characters
{
    public abstract class Character : IMapObject
    {
        public int Id { get; set; }
        public AMapleClient BaseClient { get; set; }
        public int AccountId { get; set; }
        public byte WorldId { get; set; }
        public string Name { get; set; }
        public int MapId { get; set; }
        public bool IsInitialized { get; set; }
        public byte SpawnPoint { get; set; }
        public byte Stance { get; set; }
        public short Foothold { get; set; }
        public byte Portals { get; set; }
        public int Chair { get; set; }
        public int Rank { get; set; }
        public int RankMove { get; set; }
        public int JobRank { get; set; }
        public int JobRankMove { get; set; }
        public CharacterItems Items { get; }
        public CharacterSkills Skills { get; }
        public CharacterQuests Quests { get; }
        public CharacterRings Rings { get; }
        public CharacterBuffs Buffs { get; }
        public CharacterSummons Summons { get; set; }
        public CharacterTeleportRocks TeleportRocks { get; }
        public CharacterStorage Storage { get; }
        public Trade Trade { get; set; }
        public PlayerShop PlayerShop { get; set; }
        public CharacterPets Pets { get; set; }
        public CharacterParty Party { get; set; }
        public CharacterStats PrimaryStats { get; set; }
        public ControlledMobs ControlledMobs { get; }
        public ControlledNpcs ControlledNpcs { get; }
        public Npc CurrentNpcShop { get; set; }
        public CharacterDamage Damage { get; set; }

        public Map Map { get; set; }
        
        public int ObjectId { get; set; }
        public Point Position { get; set; }
        private bool Assigned { get; set; }
        public DateTime LastHealthHealOverTime { get; set; }
        public DateTime LastManaHealOverTime { get; set; }
        private int _itemEffect;
        private readonly ILogger _log = Log.ForContext<Character>();
        public bool IsAlive => PrimaryStats.Health > 0;

        public bool IsMaster => false;

        public bool FacesLeft => Stance % 2 == 0;

        public bool IsRanked => PrimaryStats.Level >= 30;

        public QuestReference LastQuest { get; set; }

        public int ItemEffect
        {
            get => _itemEffect;
            set
            {
                _itemEffect = value;
                SendItemEffect();
            }
        }

        protected Character(int id = 0, AMapleClient client = null)
        {
            Id = id;
            BaseClient = client;
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

        public virtual void Release()
        {
        }

        public virtual void Notify(string message, NoticeType type = NoticeType.PinkText)
        {
            
        }

        public virtual void Revive()
        {
            
        }
        public virtual void ChangeMap(int mapId, string portalLabel)
        {
           
        }

        public virtual void ChangeMap(int mapId, byte? portalId = null)
        {
            
        }


        public virtual void Attack(PacketReader packet, AttackType type)
        {
           
        }

        public virtual void Talk(string text, bool show = true)
        {
           
        }
        
        public virtual void SendItemEffect()
        {
        }

        public virtual void PerformFacialExpression(int expressionId)
        {
        }

        public virtual void ShowLocalUserEffect(UserEffect effect)
        {
           
        }

        public virtual void ShowRemoteUserEffect(UserEffect effect, bool skipSelf = false)
        {
        }

        public virtual void Initialize()
        {
            
        }

        public virtual void LogCheatWarning(CheatType type)
        {
            using var dbContext = new MapleDbContext();
            _log.Information($"Cheat Warning: Character={Id} CheatType={type}");
            dbContext.Cheats.Add(new CheatEntity {CharacterId = Id, CheatType = (int)type});
            dbContext.SaveChanges();
        }

        public virtual void Save()
        {
            using var dbContext = new MapleDbContext();
            var character = dbContext.Characters
                .Where(x => x.Name == Name)
                .FirstOrDefault(x => x.WorldId == WorldId);

            if (character == null)
            {
                _log.Error($"Cannot find account [{Name}] in World [{WorldId}]");
                return;
            }

            character.AccountId = AccountId;
            character.AbilityPoints = PrimaryStats.AbilityPoints;
            character.Dexterity = PrimaryStats.Dexterity;
            character.Experience = PrimaryStats.Experience;
            character.Face = PrimaryStats.Face;
            character.Fame = PrimaryStats.Fame;
            character.Gender = (byte)PrimaryStats.Gender;
            character.Hair = PrimaryStats.Hair;
            character.Health = PrimaryStats.Health;
            character.Intelligence = PrimaryStats.Intelligence;
            character.Job = (short)PrimaryStats.Job;
            character.Level = PrimaryStats.Level;
            character.Luck = PrimaryStats.Luck;
            character.MapId = Map?.MapleId ?? ServerConfig.Instance.DefaultMapId;
            character.MaxHealth = PrimaryStats.MaxHealth;
            character.MaxMana = PrimaryStats.MaxMana;
            character.Meso = PrimaryStats.Meso;
            character.Mana = PrimaryStats.Mana;
            character.Skin = PrimaryStats.Skin;
            character.SkillPoints = PrimaryStats.SkillPoints;
            character.SpawnPoint = SpawnPoint;
            character.WorldId = WorldId;
            character.Strength = PrimaryStats.Strength;
            character.Name = Name;
            character.BuddyListSlots = PrimaryStats.BuddyListSlots;
            character.EquipmentSlots = Items.MaxSlots[ItemType.Equipment];
            character.UsableSlots = Items.MaxSlots[ItemType.Usable];
            character.SetupSlots = Items.MaxSlots[ItemType.Setup];
            character.EtceteraSlots = Items.MaxSlots[ItemType.Etcetera];
            character.CashSlots = Items.MaxSlots[ItemType.Pet];
            dbContext.SaveChanges();

            Items.Save();
            Skills.Save();
            Quests.Save();
            Rings.Save();
            TeleportRocks.Save();

            _log.Information($"Saved character '{Name}' to database.");
        }

        public virtual void Load()
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
            //Map = Client?.Server[character.MapId] ?? new Map(character.MapId);
            MapId = character.MapId;
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

        public virtual void Send(PacketWriter packet) => BaseClient.Send(packet);

        public virtual void Hide(bool isHidden)
        {
           
        }
        
        public byte[] ToByteArray()
        {
            using var pw = new PacketWriter();
            pw.WriteBytes(StatisticsToByteArray());
            pw.WriteBytes(AppearanceToByteArray());
            pw.WriteBool(IsRanked);

            if (IsRanked)
            {
                pw.WriteInt(Rank);
                pw.WriteInt(RankMove);
                pw.WriteInt(JobRank);
                pw.WriteInt(JobRankMove);
            }

            return pw.ToArray();
        }

        public byte[] StatisticsToByteArray()
        {
            using var pw = new PacketWriter();
            pw.WriteInt(Id);
            pw.WriteString(Name, 13);
            pw.WriteByte(PrimaryStats.Gender);
            pw.WriteByte(PrimaryStats.Skin);
            pw.WriteInt(PrimaryStats.Face);
            pw.WriteInt(PrimaryStats.Hair);
            pw.WriteLong(0); // Pet SN
            pw.WriteByte(PrimaryStats.Level);
            pw.WriteShort((short)PrimaryStats.Job);
            pw.WriteShort(PrimaryStats.Strength);
            pw.WriteShort(PrimaryStats.Dexterity);
            pw.WriteShort(PrimaryStats.Intelligence);
            pw.WriteShort(PrimaryStats.Luck);
            pw.WriteShort(PrimaryStats.Health);
            pw.WriteShort(PrimaryStats.MaxHealth);
            pw.WriteShort(PrimaryStats.Mana);
            pw.WriteShort(PrimaryStats.MaxMana);
            pw.WriteShort(PrimaryStats.AbilityPoints);
            pw.WriteShort(PrimaryStats.SkillPoints);
            pw.WriteInt(PrimaryStats.Experience);
            pw.WriteShort(PrimaryStats.Fame);
            pw.WriteInt(Map.MapleId);
            pw.WriteByte(SpawnPoint);
            pw.WriteLong(0);
            pw.WriteInt(0);
            pw.WriteInt(0);

            return pw.ToArray();
        }

        public byte[] AppearanceToByteArray()
        {
            using var pw = new PacketWriter();
            var visibleLayer = new Dictionary<byte, int>();
            var hiddenLayer = new Dictionary<byte, int>();

            foreach (var item in Items.GetEquipped())
            {
                var slot = item.AbsoluteSlot;

                if (slot < 100 && !visibleLayer.ContainsKey(slot))
                {
                    visibleLayer[slot] = item.MapleId;
                }
                else if (slot > 100 && slot != 111)
                {
                    slot -= 100;

                    if (visibleLayer.ContainsKey(slot))
                    {
                        hiddenLayer[slot] = visibleLayer[slot];
                    }

                    visibleLayer[slot] = item.MapleId;
                }
                else if (visibleLayer.ContainsKey(slot))
                {
                    hiddenLayer[slot] = item.MapleId;
                }
            }

            foreach (var entry in visibleLayer)
            {
                pw.WriteByte(entry.Key);
                pw.WriteInt(entry.Value);
            }

            pw.WriteByte(0);

            foreach (var entry in hiddenLayer)
            {
                pw.WriteByte(entry.Key);
                pw.WriteInt(entry.Value);
            }

            pw.WriteByte(0);

            return pw.ToArray();
        }
    }
}
