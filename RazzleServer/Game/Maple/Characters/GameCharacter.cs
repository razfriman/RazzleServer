﻿using System;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Interaction;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Game.Maple.Characters
{
    public partial class GameCharacter : BaseCharacter<GameClient>, IMoveable, ISpawnable
    {
        public ControlledMobs ControlledMobs { get; }
        public ControlledNpcs ControlledNpcs { get; }
        public Trade Trade { get; set; }
        public PlayerShop PlayerShop { get; set; }
        
        public CharacterParty Party { get; set; }
        public ANpcScript NpcScript { get; set; }
        public Npc CurrentNpcShop { get; set; }
        public CharacterDamage Damage { get; set; }
        private bool Assigned { get; set; }
        public DateTime LastHealthHealOverTime { get; set; }
        public DateTime LastManaHealOverTime { get; set; }
        
        private int _itemEffect;

        private readonly ILogger _log = Log.ForContext<GameCharacter>();

        public override bool IsMaster => Client.Account?.IsMaster ?? false;


        public int ItemEffect
        {
            get => _itemEffect;
            set
            {
                _itemEffect = value;
                using var pw = new PacketWriter(ServerOperationCode.ItemEffect);
                pw.WriteInt(Id);
                pw.WriteInt(_itemEffect);
                Map.Send(pw, this);
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
        
        public GameCharacter(int id = 0, GameClient client = null) : base(id, client)
        {
            Position = new Point(0, 0);
            ControlledMobs = new ControlledMobs(this);
            ControlledNpcs = new ControlledNpcs(this);
            Damage = new CharacterDamage(this);
        }

        public override void Initialize()
        {
            using var pw = new PacketWriter(ServerOperationCode.SetField);
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

            IsInitialized = true;
            Map.Characters.Add(this);
            Client.Server.World.Send(GamePackets.Notify(Client.Server.World.TickerMessage, NoticeType.ScrollingText));
            // Update buddy list
            // update quest mob kills
            PrimaryStats.UpdateStatsForParty();
            Client.StartPingCheck();
        }


        public void Release() => PrimaryStats.Update();

        public override void Notify(string message, NoticeType type = NoticeType.PinkText) =>
            Client.Send(GamePackets.Notify(message, type));

        public override void Revive()
        {
            PrimaryStats.Health = 50;
            ChangeMap(Map.CachedReference.ReturnMapId);
        }

        public override void ChangeMap(int mapId, byte? portalId = null)
        {
            base.ChangeMap(mapId, portalId);
            
            Map.Characters.Remove(this);

            using var pw = new PacketWriter(ServerOperationCode.SetField);
            pw.WriteInt(Client.Server.ChannelId);
            pw.WriteByte(++Portals);
            pw.WriteBool(false);
            pw.WriteInt(mapId);
            pw.WriteByte(portalId ?? SpawnPoint);
            pw.WriteShort(PrimaryStats.Health);
            Client.Send(pw);
            Client.Server[mapId].Characters.Add(this);
        }


        public override void Attack(PacketReader packet, AttackType type)
        {
            var attack = new Attack(packet, type);

            var skill = attack.SkillId > 0 ? Skills[attack.SkillId] : null;
            skill?.Cast();

            // TODO: Modify packet based on attack type.
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerMeleeAttack);
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

        public override void Talk(string text, bool show = true)
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerChat);
            pw.WriteInt(Id);
            pw.WriteBool(IsMaster);
            pw.WriteString(text);
            pw.WriteBool(show);
            Map.Send(pw);
        }

        public override void PerformFacialExpression(int expressionId)
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerEmote);
            pw.WriteInt(Id);
            pw.WriteInt(expressionId);
            Map.Send(pw, this);
        }

        public override void ShowLocalUserEffect(UserEffect effect)
        {
            using var pw = new PacketWriter(ServerOperationCode.Effect);
            pw.WriteByte(effect);
            Client.Send(pw);
        }

        public override void ShowRemoteUserEffect(UserEffect effect, bool skipSelf = false)
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerEffect);
            pw.WriteInt(Id);
            pw.WriteByte((int)effect);
            Map.Send(pw, skipSelf ? this : null);
        }

        public override void Converse(Npc npc) => npc.Converse(this);

        public override void Save()
        {
            if (IsInitialized)
            {
                SpawnPoint = ClosestSpawnPoint?.Id ?? 0;
            }

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


        public override void Load()
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
            MapId = character.MapId;
            Map = Client?.Server[character.MapId];
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


        public override void Hide(bool isHidden)
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
