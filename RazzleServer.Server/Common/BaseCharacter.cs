using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.DataProvider;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Net;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Common
{
    public abstract class BaseCharacter<TClient> : ICharacter where TClient : AClient
    {
        private readonly ILogger _log = Log.ForContext<BaseCharacter<TClient>>();

        public int Id { get; set; }
        public TClient Client { get; set; }
        public int AccountId { get; set; }
        
        public byte WorldId { get; set; }
        public string Name { get; set; }
        public bool IsInitialized { get; set; }
        public byte SpawnPoint { get; set; }
        public byte Stance { get; set; }
        public int MapId { get; set; }
        public short Foothold { get; set; }
        public byte Portals { get; set; }
        public int Chair { get; set; }
        public int Rank { get; set; }
        public int RankMove { get; set; }
        public int JobRank { get; set; }
        public int JobRankMove { get; set; }
        public CharacterItems Items { get; }
        public CharacterStats PrimaryStats { get; }
        public CharacterTeleportRocks TeleportRocks { get; }
        public CharacterSkills Skills { get; }
        public CharacterQuests Quests { get; }
        public CharacterRings Rings { get; }
        public CharacterBuffs Buffs { get; }
        public CharacterSummons Summons { get; set; }
        public CharacterStorage Storage { get; }
        public CharacterPets Pets { get; set; }
        public virtual bool IsMaster { get; } = false;


        public bool IsAlive => PrimaryStats.Health > 0;

        public bool FacesLeft => Stance % 2 == 0;

        public bool IsRanked => PrimaryStats.Level >= 30;


        protected BaseCharacter(int id, TClient client)
        {
            Id = id;
            Client = client;
            PrimaryStats = new CharacterStats(this);
            Items = new CharacterItems(this, 100, 100, 100, 100, 100);
            TeleportRocks = new CharacterTeleportRocks(this);
            Skills = new CharacterSkills(this);
            Quests = new CharacterQuests(this);
            Rings = new CharacterRings(this);
            Summons = new CharacterSummons(this);
            Buffs = new CharacterBuffs(this);
            Storage = new CharacterStorage(this);
            Pets = new CharacterPets(this);
        }

        public virtual void Initialize()
        {
        }

        public virtual void Save()
        {
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

            Name = character.Name;
            AccountId = character.AccountId;
            PrimaryStats.Load(character);
            MapId = character.MapId;
            SpawnPoint = character.SpawnPoint;
            WorldId = character.WorldId;
            Items.MaxSlots[ItemType.Equipment] = character.EquipmentSlots;
            Items.MaxSlots[ItemType.Usable] = character.UsableSlots;
            Items.MaxSlots[ItemType.Setup] = character.SetupSlots;
            Items.MaxSlots[ItemType.Etcetera] = character.EtceteraSlots;
            Items.MaxSlots[ItemType.Pet] = character.CashSlots;
            Items.Load();
            TeleportRocks.Load();
        }

        public virtual void Hide(bool isHidden)
        {
        }

        public virtual void Send(PacketWriter packet) => Client.Send(packet);

        public virtual byte[] ToByteArray()
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

        public virtual byte[] StatisticsToByteArray()
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
            pw.WriteInt(MapId);
            pw.WriteByte(SpawnPoint);
            pw.WriteLong(0);
            pw.WriteInt(0);
            pw.WriteInt(0);

            return pw.ToArray();
        }

        public virtual byte[] AppearanceToByteArray()
        {
            using var pw = new PacketWriter();

            var equippedSlots = Items.CalculateEquippedSlots();

            foreach (var entry in equippedSlots.visibleLayer)
            {
                pw.WriteByte(entry.Key);
                pw.WriteInt(entry.Value);
            }

            pw.WriteByte(0);

            foreach (var entry in equippedSlots.hiddenLayer)
            {
                pw.WriteByte(entry.Key);
                pw.WriteInt(entry.Value);
            }

            pw.WriteByte(0);

            return pw.ToArray();
        }

        public virtual void ChangeMap(int mapId, string portalLabel)
        {
            var portal = CachedData.Maps.Data[mapId].Portals.FirstOrDefault(x => x.Label == portalLabel);

            if (portal == null)
            {
                LogCheatWarning(CheatType.InvalidMapChange);
                return;
            }

            ChangeMap(mapId, portal.Id);
        }

        public virtual void ChangeMap(int mapId, byte? portalId = null)
        {
            _log.Information($"ChangeMap: Character={Id} Map={mapId} Portal={portalId}");
            MapId = mapId;
        }

        public virtual void Notify(string message, NoticeType type = NoticeType.PinkText)
        {
        }

        public virtual void Revive()
        {
        }

        public virtual void Attack(PacketReader packet, AttackType type)
        {
        }

        public virtual void Talk(string text, bool show = true)
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

        public virtual void Converse(Npc npc)
        {
        }

        public virtual void LogCheatWarning(CheatType type)
        {
            using var dbContext = new MapleDbContext();
            _log.Information($"Cheat Warning: Character={Id} CheatType={type}");
            dbContext.Cheats.Add(new CheatEntity {CharacterId = Id, CheatType = (int)type});
            dbContext.SaveChanges();
        }

        public virtual byte[] DataToByteArray()
        {
            return new byte[] { };
        }

        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }
    }
}
