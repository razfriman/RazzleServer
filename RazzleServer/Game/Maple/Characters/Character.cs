using System;
using System.Linq;
using System.Threading.Tasks;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Interaction;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Game.Maple.Shops;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Game.Maple.Characters
{
    public partial class Character : MapObject, IMoveable, ISpawnable
    {
        public GameClient Client { get; }
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
        public CharacterItems Items { get; }
        public CharacterSkills Skills { get; }
        public CharacterQuests Quests { get; }
        public CharacterRings Rings { get; }
        public CharacterBuffs Buffs { get; }
        public CharacterSummons Summons { get; set; }
        public CharacterTeleportRocks TeleportRocks { get; }
        public CharacterStorage Storage { get; }
        public ControlledMobs ControlledMobs { get; }
        public ControlledNpcs ControlledNpcs { get; }
        public Trade Trade { get; set; }
        public PlayerShop PlayerShop { get; set; }
        public CharacterPets Pets { get; set; }
        public CharacterParty Party { get; set; }
        public ANpcScript NpcScript { get; set; }
        public Shop CurrentNpcShop { get; set; }
        public CharacterDamage Damage { get; set; }
        private bool Assigned { get; set; }

        public CharacterStats PrimaryStats { get; set; }

        public DateTime LastHealthHealOverTime { get; set; }
        public DateTime LastManaHealOverTime { get; set; }

        private int _itemEffect;

        private readonly ILogger _log = Log.ForContext<Character>();

        public bool IsAlive => PrimaryStats.Health > 0;

        public bool IsMaster => Client.Account?.IsMaster ?? false;

        public bool FacesLeft => Stance % 2 == 0;

        public bool IsRanked => PrimaryStats.Level >= 30;

        public QuestReference LastQuest { get; set; }

        public int ItemEffect
        {
            get => _itemEffect;
            set
            {
                _itemEffect = value;
                using (var pw = new PacketWriter(ServerOperationCode.ItemEffect))
                {
                    pw.WriteInt(Id);
                    pw.WriteInt(_itemEffect);
                    Map.Send(pw, this);
                }
            }
        }

        public Portal ClosestPortal
        {
            get
            {
                Portal closestPortal = null;
                var shortestDistance = double.PositiveInfinity;

                foreach (var loopPortal in Map.Portals.Values)
                {
                    var distance = loopPortal.Position.DistanceFrom(Position);

                    if (distance < shortestDistance)
                    {
                        closestPortal = loopPortal;
                        shortestDistance = distance;
                    }
                }

                return closestPortal;
            }
        }

        public Portal ClosestSpawnPoint
        {
            get
            {
                Portal closestPortal = null;
                var shortestDistance = double.PositiveInfinity;

                foreach (var loopPortal in Map.Portals.Values)
                {
                    if (loopPortal.IsSpawnPoint)
                    {
                        var distance = loopPortal.Position.DistanceFrom(Position);

                        if (distance < shortestDistance)
                        {
                            closestPortal = loopPortal;
                            shortestDistance = distance;
                        }
                    }
                }

                return closestPortal;
            }
        }


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

        public void Initialize()
        {
            using (var pw = new PacketWriter(ServerOperationCode.SetField))
            {
                pw.WriteInt(Client.Server.ChannelId);
                pw.WriteByte(++Portals);
                pw.WriteBool(true);
                pw.WriteInt(Damage.Random.OriginalSeed1);
                pw.WriteInt(Damage.Random.OriginalSeed2);
                pw.WriteInt(Damage.Random.OriginalSeed3);
                pw.WriteInt(Damage.Random.OriginalSeed3);
                pw.WriteShort(-1); // flags
                pw.WriteBytes(DataToByteArray());
                pw.WriteLong(0);
                pw.WriteLong(0);
                pw.WriteLong(0);
                pw.WriteLong(0);
                pw.WriteLong(0);
                pw.WriteLong(0);
                Client.Send(pw);
            }

            IsInitialized = true;

            Map.Characters.Add(this);
            // Send server message
            // Update buddy list
            // update quest mob kills
            PrimaryStats.UpdateStatsForParty();
            Task.Factory.StartNew(Client.StartPingCheck);
        }


        public void Release() => PrimaryStats.Update();

        public void Notify(string message, NoticeType type = NoticeType.PinkText)
        {
            Client.Send(GamePackets.Notify(message, type));
        }

        public void Revive()
        {
            PrimaryStats.Health = 50;
            ChangeMap(Map.CachedReference.ReturnMapId);
        }

        public void ChangeMap(int mapId, string portalLabel)
        {
            var portal = DataProvider.Maps.Data[mapId].Portals.FirstOrDefault(x => x.Label == portalLabel);

            if (portal == null)
            {
                LogCheatWarning(CheatType.InvalidMapChange);
                return;
            }

            ChangeMap(mapId, portal.Id);
        }

        public void ChangeMap(int mapId, byte? portalId = null)
        {
            Map.Characters.Remove(this);

            using (var pw = new PacketWriter(ServerOperationCode.SetField))
            {
                pw.WriteInt(Client.Server.ChannelId);
                pw.WriteByte(++Portals);
                pw.WriteBool(false);
                pw.WriteInt(mapId);
                pw.WriteByte(portalId ?? SpawnPoint);
                pw.WriteShort(PrimaryStats.Health);
                Client.Send(pw);
            }

            Client.Server[mapId].Characters.Add(this);
        }


        public void Attack(PacketReader packet, AttackType type)
        {
            var attack = new Attack(packet, type);

            var skill = attack.SkillId > 0 ? Skills[attack.SkillId] : null;
            skill?.Cast();

            // TODO: Modify packet based on attack type.
            using (var pw = new PacketWriter(ServerOperationCode.RemotePlayerMeleeAttack))
            {
                pw.WriteInt(Id);
                pw.WriteByte((byte)(attack.Targets * 0x10 + attack.Hits));
                pw.WriteByte(skill?.CurrentLevel ?? 0);

                if (attack.SkillId != 0)
                {
                    pw.WriteInt(attack.SkillId);
                }

                pw.WriteByte(attack.Display);
                pw.WriteByte(attack.Animation);
                pw.WriteByte(attack.WeaponSpeed);
                pw.WriteByte(0); // NOTE: Skill mastery.
                pw.WriteInt(0); // NOTE: StarId = Item ID at attack.StarPosition

                foreach (var target in attack.Damages)
                {
                    pw.WriteInt(target.Key);
                    pw.WriteByte(6);
                    if (attack.IsMesoExplosion)
                    {
                        pw.WriteByte(target.Value.Count);
                    }

                    foreach (var hit in target.Value)
                    {
                        pw.WriteUInt(hit);
                    }
                }

                Map.Send(pw, this);
            }

            foreach (var target in attack.Damages)
            {
                if (!Map.Mobs.Contains(target.Key))
                {
                    continue;
                }

                var mob = Map.Mobs[target.Key];
                mob.IsProvoked = true;
                mob.SwitchController(this);
                foreach (var hit in target.Value)
                {
                    if (mob.Damage(this, hit))
                    {
                        mob.Die();
                    }
                }
            }
        }

        public void Talk(string text, bool show = true)
        {
            using (var pw = new PacketWriter(ServerOperationCode.RemotePlayerChat))
            {
                pw.WriteInt(Id);
                pw.WriteBool(IsMaster);
                pw.WriteString(text);
                pw.WriteBool(show);
                Map.Send(pw);
            }
        }

        public void PerformFacialExpression(int expressionId)
        {
            using (var pw = new PacketWriter(ServerOperationCode.RemotePlayerEmote))
            {
                pw.WriteInt(Id);
                pw.WriteInt(expressionId);
                Map.Send(pw, this);
            }
        }

        public void ShowLocalUserEffect(UserEffect effect)
        {
            using (var pw = new PacketWriter(ServerOperationCode.Effect))
            {
                pw.WriteByte(effect);
                Client.Send(pw);
            }
        }

        public void ShowRemoteUserEffect(UserEffect effect, bool skipSelf = false)
        {
            using (var pw = new PacketWriter(ServerOperationCode.RemotePlayerEffect))
            {
                pw.WriteInt(Id);
                pw.WriteByte((int)effect);
                Map.Send(pw, skipSelf ? this : null);
            }
        }

        public void Converse(Npc npc, QuestReference quest = null)
        {
            LastQuest = quest;
            npc.Converse(this);
        }

        public void LogCheatWarning(CheatType type)
        {
            using (var dbContext = new MapleDbContext())
            {
                _log.Information($"Cheat Warning: Character={Id} CheatType={type}");
                dbContext.Cheats.Add(new CheatEntity {CharacterId = Id, CheatType = (int)type});
                dbContext.SaveChanges();
            }
        }

        internal static void Delete(int characterId)
        {
            using (var dbContext = new MapleDbContext())
            {
                var entity = dbContext.Characters.Find(characterId);
                if (entity != null)
                {
                    dbContext.Characters.Remove(entity);
                }

                dbContext.SaveChanges();
            }
        }

        public void Save()
        {
            if (IsInitialized)
            {
                SpawnPoint = ClosestSpawnPoint?.Id ?? 0;
            }

            using (var dbContext = new MapleDbContext())
            {
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
                character.CashSlots = Items.MaxSlots[ItemType.Cash];

                dbContext.SaveChanges();
            }

            Items.Save();
            Skills.Save();
            Quests.Save();
            Rings.Save();
            Buffs.Save();
            TeleportRocks.Save();

            _log.Information($"Saved character '{Name}' to database.");
        }

        public void Create()
        {
            using (var dbContext = new MapleDbContext())
            {
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
                    CashSlots = Items.MaxSlots[ItemType.Cash]
                };

                dbContext.Characters.Add(character);
                dbContext.SaveChanges();
                Id = character.Id;

                Items.Save();
                Skills.Save();
                Quests.Save();
                Rings.Save();
                Buffs.Save();
                TeleportRocks.Save();
            }
        }

        public void Load()
        {
            using (var dbContext = new MapleDbContext())
            {
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
                Map = Client?.Server[character.MapId] ?? new Map(character.MapId);
                SpawnPoint = character.SpawnPoint;
                WorldId = character.WorldId;
                Items.MaxSlots[ItemType.Equipment] = character.EquipmentSlots;
                Items.MaxSlots[ItemType.Usable] = character.UsableSlots;
                Items.MaxSlots[ItemType.Setup] = character.SetupSlots;
                Items.MaxSlots[ItemType.Etcetera] = character.EtceteraSlots;
                Items.MaxSlots[ItemType.Cash] = character.CashSlots;
            }

            Items.Load();
            Skills.Load();
            Quests.Load();
            Buffs.Load();
            TeleportRocks.Load();
        }

        public void Send(PacketWriter packet) => Client.Send(packet);
        public void Send(byte[] packet) => Client.Send(packet);

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

            using (var pw = new PacketWriter(ServerOperationCode.AdminResult))
            {
                pw.WriteByte(AdminResultType.Hide);
                pw.WriteBool(isHidden);
                Send(pw);
            }
        }
    }
}
