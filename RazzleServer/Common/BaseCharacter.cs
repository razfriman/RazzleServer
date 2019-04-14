using RazzleServer.Common.Constants;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Net;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Common
{
    public abstract class BaseCharacter<TClient> where TClient : AClient
    {
        private readonly ILogger _log = Log.ForContext<BaseCharacter<TClient>>();

        public int Id { get; set; }
        public TClient Client { get; set; }
        public int AccountId { get; set; }
        public byte WorldId { get; set; }
        public string Name { get; set; }
        public bool IsInitialized { get; protected set; }
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
        public CharacterItems Items { get; set; }
        public CharacterStats PrimaryStats { get; set; }
        public CharacterTeleportRocks TeleportRocks { get; set; }

        public bool IsAlive => PrimaryStats.Health > 0;

        public bool FacesLeft => Stance % 2 == 0;

        public bool IsRanked => PrimaryStats.Level >= 30;


        protected BaseCharacter(int id, TClient client)
        {
            Id = id;
            Client = client;
            
            var gameCharacter = this is GameCharacter castedCharacter ? castedCharacter : null;
            PrimaryStats = new CharacterStats(gameCharacter);
            Items = new CharacterItems(gameCharacter, 100, 100, 100, 100, 100);
            TeleportRocks = new CharacterTeleportRocks(gameCharacter);
        }

        public virtual void Initialize()
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

        public void Send(PacketWriter packet) => Client.Send(packet);

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
            pw.WriteInt(MapId);
            pw.WriteByte(SpawnPoint);
            pw.WriteLong(0);
            pw.WriteInt(0);
            pw.WriteInt(0);

            return pw.ToArray();
        }

        public byte[] AppearanceToByteArray()
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
    }
}
