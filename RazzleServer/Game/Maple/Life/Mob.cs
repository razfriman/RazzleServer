using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.DataProvider;
using RazzleServer.DataProvider.References;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class Mob : IMapObject, IMoveable, ISpawnable, IControllable
    {
        public int MapleId { get; }
        public GameCharacter Controller { get; set; }
        public Dictionary<GameCharacter, uint> Attackers { get; }
        public SpawnPoint SpawnPoint { get; }
        public byte Stance { get; set; }
        public bool IsProvoked { get; set; }
        public bool CanDrop { get; set; }
        public short Foothold { get; set; }
        public MobSkills Skills { get; }
        public Dictionary<MobSkill, DateTime> Cooldowns { get; }
        public List<MobStatus> Buffs { get; }
        public List<int> DeathSummons { get; }
        public DeathEffect DeathEffect { get; set; } = DeathEffect.FadeOut;

        public short Level { get; }
        public uint Health { get; set; }
        public uint Mana { get; set; }
        public uint MaxHealth { get; }
        public uint MaxMana { get; }
        public int HealthRecovery { get; }
        public int ManaRecovery { get; }
        public uint Experience { get; }
        public int Link { get; }
        public short SummonType { get; }
        public double Traction { get; }
        public byte HpBarForeColor { get; }
        public byte HpBarBackColor { get; }
        public int WeaponAttack { get; }
        public int WeaponDefense { get; }
        public int MagicAttack { get; }
        public int MagicDefense { get; }
        public short Accuracy { get; }
        public short Avoidability { get; }
        public short Speed { get; }
        public bool IsFacingLeft => Stance % 2 == 0;

        public bool CanRespawn => true;

        public int SpawnEffect { get; set; }

        public MobReference CachedReference => CachedData.Mobs.Data[MapleId];


        public Mob(int mapleId)
        {
            MapleId = mapleId;

            Level = CachedReference.Level;
            Health = CachedReference.Health;
            Mana = CachedReference.Mana;
            MaxHealth = CachedReference.MaxHealth;
            MaxMana = CachedReference.MaxMana;
            HealthRecovery = CachedReference.HealthRecovery;
            ManaRecovery = CachedReference.ManaRecovery;
            Experience = CachedReference.Experience;
            Link = CachedReference.Link;
            SummonType = CachedReference.SummonType;
            Traction = CachedReference.Traction;
            HpBarForeColor = CachedReference.HpBarForeColor;
            HpBarBackColor = CachedReference.HpBarBackColor;
            WeaponAttack = CachedReference.WeaponAttack;
            WeaponDefense = CachedReference.WeaponDefense;
            MagicAttack = CachedReference.MagicAttack;
            MagicDefense = CachedReference.MagicDefense;
            Accuracy = CachedReference.Accuracy;
            Avoidability = CachedReference.Avoidability;
            Speed = CachedReference.Speed;
            Skills = new MobSkills(this);
            CachedReference.Skills.ForEach(x =>
            {
                Skills.Add(new MobSkill(x));
            });
            CachedReference.Skills.ForEach(x =>
            {
                Skills.Add(new MobSkill(x));
            });

            DeathSummons = CachedReference.DeathSummons;

            Attackers = new Dictionary<GameCharacter, uint>();
            Cooldowns = new Dictionary<MobSkill, DateTime>();
            Buffs = new List<MobStatus>();
            Stance = 5;
            CanDrop = true;
        }

        public Mob(SpawnPoint spawnPoint)
            : this(spawnPoint.MapleId)
        {
            SpawnPoint = spawnPoint;
            Foothold = SpawnPoint.Foothold;
            Position = new Point(SpawnPoint.Position.X, SpawnPoint.Position.Y - 1);
        }

        public Mob(int mapleId, Point position)
            : this(mapleId)
        {
            Foothold = 0; // TODO.
            Position = position;
        }

        public void AssignController()
        {
            if (Controller == null)
            {
                var leastControlled = int.MaxValue;
                GameCharacter newController = null;

                lock (Map.Characters)
                {
                    foreach (var character in Map.Characters.Values)
                    {
                        if (character.ControlledMobs.Count < leastControlled)
                        {
                            leastControlled = character.ControlledMobs.Count;
                            newController = character;
                        }
                    }
                }

                if (newController != null)
                {
                    IsProvoked = false;

                    newController.ControlledMobs.Add(this);
                }
            }
        }

        public void SwitchController(GameCharacter newController)
        {
            lock (this)
            {
                if (Controller != newController)
                {
                    Controller.ControlledMobs.Remove(this);
                    newController.ControlledMobs.Add(this);
                }
            }
        }

        public void Move(PacketReader packet)
        {
            var moveAction = packet.ReadShort();
            var skillByte = packet.ReadByte();
            var skillId = packet.ReadByte();
            var projectileTarget = packet.ReadPoint();

            var movements = new Movements(packet);

            Position = movements.Position;
            Foothold = movements.Foothold;
            Stance = movements.Stance;

            // TODO - Load the mob skill
            MobSkill skill = null;
            if (skill != null)
            {
                if (Health * 100 / MaxHealth > skill.CachedReference.PercentageLimitHp ||
                    Cooldowns.ContainsKey(skill) &&
                    Cooldowns[skill].AddSeconds(skill.CachedReference.Cooldown) >= DateTime.UtcNow ||
                    (MobSkillName)skill.MapleId == MobSkillName.Summon && Map.Mobs.Count >= 100)
                {
                    skill = null;
                }
            }

            skill?.Cast(this);

            using var pwControl = new PacketWriter(ServerOperationCode.MobControlResponse);
            pwControl.WriteInt(ObjectId);
            pwControl.WriteShort(moveAction);
            pwControl.WriteBool(skill != null); // use skills
            pwControl.WriteShort((short)Mana);
            pwControl.WriteShort(0); // skill id, skill level
            Controller?.Client?.Send(pwControl);

            using var pwMove = new PacketWriter(ServerOperationCode.MobMove);
            pwMove.WriteInt(ObjectId);
            pwMove.WriteBool(skill != null); // use skills
            pwMove.WriteInt(skillId);
            pwMove.WriteByte(0);
            pwMove.WriteBytes(movements.ToByteArray());
            Map.Send(pwMove, Controller);
        }

        public void Buff(MobStatus buff, short value, MobSkill skill)
        {
//            using (var pw = new PacketWriter(ServerOperationCode.MobStatSet))
//            {
//                pw.WriteInt(ObjectId);
//                pw.WriteLong(0);
//                pw.WriteInt(0);
//                pw.WriteInt((int)buff);
//                pw.WriteShort(value);
//                pw.WriteShort(skill.MapleId);
//                pw.WriteShort(skill.Level);
//                pw.WriteShort(-1);
//                pw.WriteShort(0); // Delay
//                pw.WriteInt(0);
//
//                Map.Send(pw);
//            }

            ScheduleBuffExpiration(buff, skill);
        }

        private void ScheduleBuffExpiration(MobStatus buff, MobSkill skill)
        {
            TaskRunner.Run(() =>
            {
//                    using (var packet = new PacketWriter(ServerOperationCode.MobStatReset))
//                    {
//                        packet.WriteInt(ObjectId);
//                        packet.WriteLong(0);
//                        packet.WriteInt(0);
//                        packet.WriteInt((int)buff);
//                        packet.WriteInt(0);
//
//                        Map.Send(packet);
//                    }

                Buffs.Remove(buff);
            }, TimeSpan.FromSeconds(skill.CachedReference.Duration));
        }

        public void Heal(uint hp, int range)
        {
            Health = Math.Min(MaxHealth, (uint)(Health + hp + Functions.Random(-range / 2, range / 2)));

            using var pw = new PacketWriter(ServerOperationCode.MobChangeHealth);
            pw.WriteInt(ObjectId);
            pw.WriteByte(0);
            pw.WriteInt((int)-hp);
            pw.WriteLong(0);
            pw.WriteLong(0);
            Map.Send(pw);
        }

        public void Die() => Map.Mobs.Remove(this);

        public bool Damage(GameCharacter attacker, uint amount)
        {
            lock (this)
            {
                var originalAmount = amount;

                amount = Math.Min(amount, Health);

                if (Attackers.ContainsKey(attacker))
                {
                    Attackers[attacker] += amount;
                }
                else
                {
                    Attackers.Add(attacker, amount);
                }

                Health -= amount;

                return Health <= 0;
            }
        }

        public PacketWriter GetCreatePacket() => GetInternalPacket(false, true);

        public PacketWriter GetSpawnPacket() => GetInternalPacket(false, false);

        public PacketWriter GetControlRequestPacket() => GetInternalPacket(true, false);

        private PacketWriter GetInternalPacket(bool requestControl, bool newSpawn)
        {
            var pw = new PacketWriter(requestControl
                ? ServerOperationCode.MobControlRequest
                : ServerOperationCode.MobEnterField);

            if (requestControl)
            {
                pw.WriteByte(IsProvoked ? 2 : 1);
            }

            pw.WriteInt(ObjectId);
            pw.WriteInt(MapleId);
            pw.WritePoint(Position);
            pw.WriteByte((byte)(0x02 | (IsFacingLeft ? 0x01 : 0x00)));
            pw.WriteShort(0); // Original foothold
            pw.WriteShort(Foothold);

            if (SpawnEffect > 0)
            {
                pw.WriteByte(SpawnEffect);
                pw.WriteByte(0);
                pw.WriteShort(0);
            }

            pw.WriteByte(newSpawn ? -2 : -1);
            pw.WriteByte(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            return pw;
        }

        public PacketWriter GetControlCancelPacket()
        {
            var pw = new PacketWriter(ServerOperationCode.MobControlRequest);
            pw.WriteBool(false);
            return pw;
        }

        public PacketWriter GetDestroyPacket()
        {
            var pw = new PacketWriter(ServerOperationCode.MobLeaveField);
            pw.WriteInt(ObjectId);
            pw.WriteByte(DeathEffect);
            pw.WriteLong(0);
            pw.WriteLong(0);
            return pw;
        }

        public Map Map { get; set; }
        public int ObjectId { get; set; }
        public Point Position { get; set; }
    }
}
