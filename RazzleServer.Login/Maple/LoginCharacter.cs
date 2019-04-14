using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Maple;
using RazzleServer.Data;
using RazzleServer.Net.Packet;
using RazzleServer.Server.Maple;
using Serilog;

namespace RazzleServer.Login.Maple
{
    public class LoginCharacter : ICharacter
    {
        private readonly ILogger _log = Log.ForContext<LoginCharacter>();

        public LoginClient Client { get; set; }
        public int Id { get; set; }
        public int AccountId { get; set; }
        public bool IsMaster => Client.Account.IsMaster;
        public byte WorldId { get; set; }
        public string Name { get; set; }
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
        public BasicCharacterStats PrimaryStats { get; set; }
        public BasicCharacterItems Items { get; set; }
        public bool IsRanked => PrimaryStats.Level >= 30;


        public LoginCharacter(int id = 0, LoginClient client = null)
        {
            Id = id;
            Client = client;
            PrimaryStats = new BasicCharacterStats(this);
        }

        public void LogCheatWarning(CheatType type)
        {
            using var dbContext = new MapleDbContext();
            _log.Information($"Cheat Warning: Character={Id} CheatType={type}");
            dbContext.Cheats.Add(new CheatEntity {CharacterId = Id, CheatType = (int)type});
            dbContext.SaveChanges();
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

            Name = character.Name;
            AccountId = character.AccountId;
            MapId = character.MapId;
            SpawnPoint = character.SpawnPoint;
            WorldId = character.WorldId;
            PrimaryStats.Load(character);
            Items = new BasicCharacterItems(this, character.EquipmentSlots, character.UsableSlots, character.SetupSlots,
                character.EtceteraSlots, character.CashSlots);
            Items.Load();
        }

        internal static void Delete(int accountId, int characterId)
        {
            using var dbContext = new MapleDbContext();
            var entity = dbContext.Characters.Find(characterId);
            if (entity == null || entity.AccountId != accountId)
            {
                return;
            }

            dbContext.Characters.Remove(entity);
            dbContext.SaveChanges();
        }

        public void Send(PacketWriter packet) => Client.Send(packet);

        public byte[] ToByteArray()
        {
            using var pw = new PacketWriter();
            pw.WriteBytes(PrimaryStats.ToByteArray());
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

        public void Create()
        {
            using var dbContext = new MapleDbContext();
            var character = dbContext.Characters
                .Where(x => x.Name == Name)
                .FirstOrDefault(x => x.WorldId == WorldId);

            if (character != null)
            {
                _log.Error($"Error creating account - [{Name}] already exists in World [{WorldId}]");
                return;
            }

            character = new CharacterEntity
            {
                AccountId = AccountId,
                AbilityPoints = PrimaryStats.AbilityPoints,
                Dexterity = PrimaryStats.Dexterity,
                Experience = PrimaryStats.Experience,
                Face = PrimaryStats.Face,
                Fame = PrimaryStats.Fame,
                Gender = (byte)PrimaryStats.Gender,
                Hair = PrimaryStats.Hair,
                Health = PrimaryStats.Health,
                Intelligence = PrimaryStats.Intelligence,
                Job = (short)PrimaryStats.Job,
                Level = PrimaryStats.Level,
                Luck = PrimaryStats.Luck,
                MapId = ServerConfig.Instance.DefaultMapId,
                MaxHealth = PrimaryStats.MaxHealth,
                MaxMana = PrimaryStats.MaxMana,
                Meso = PrimaryStats.Meso,
                Mana = PrimaryStats.Mana,
                Skin = PrimaryStats.Skin,
                SkillPoints = PrimaryStats.SkillPoints,
                SpawnPoint = SpawnPoint,
                WorldId = WorldId,
                Strength = PrimaryStats.Strength,
                Name = Name,
                BuddyListSlots = PrimaryStats.BuddyListSlots,
                EquipmentSlots = Items.MaxSlots[ItemType.Equipment],
                UsableSlots = Items.MaxSlots[ItemType.Usable],
                SetupSlots = Items.MaxSlots[ItemType.Setup],
                EtceteraSlots = Items.MaxSlots[ItemType.Etcetera],
                CashSlots = Items.MaxSlots[ItemType.Pet]
            };

            dbContext.Characters.Add(character);
            dbContext.SaveChanges();
            Id = character.Id;

            Items.Save();
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
