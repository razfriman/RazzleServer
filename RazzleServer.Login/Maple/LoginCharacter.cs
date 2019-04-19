using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Server;
using Serilog;

namespace RazzleServer.Login.Maple
{
    public class LoginCharacter : Character
    {
        private readonly ILogger _log = Log.ForContext<LoginCharacter>();

        public LoginClient Client { get; }
        public override AMapleClient BaseClient => Client;

        public LoginCharacter()
        {
        }

        public LoginCharacter(int id, LoginClient client) : base(id) => Client = client;

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
            Skills.Save();
            Quests.Save();
            Rings.Save();
            TeleportRocks.Save();
        }
    }
}
