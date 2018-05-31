using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class Mob : MapObject, IMoveable, ISpawnable, IControllable
    {
        public int MapleId { get; }
        public Character Controller { get; set; }
        public Dictionary<Character, uint> Attackers { get; }
        public SpawnPoint SpawnPoint { get; }
        public byte Stance { get; set; }
        public bool IsProvoked { get; set; }
        public bool CanDrop { get; set; }
        public List<Loot> Loots { get; }
        public short Foothold { get; set; }
        public MobSkills Skills { get; }
        public Dictionary<MobSkill, DateTime> Cooldowns { get; }
        public List<MobStatus> Buffs { get; }
        public List<int> DeathSummons { get; }

        public short Level { get; }
        public uint Health { get; set; }
        public uint Mana { get; set; }
        public uint MaxHealth { get; }
        public uint MaxMana { get; }
        public uint HealthRecovery { get; }
        public uint ManaRecovery { get; }
        public int ExplodeHealth { get; }
        public uint Experience { get; }
        public int Link { get; }
        public short SummonType { get; }
        public int FixedDamage { get; }
        public int DeathBuff { get; }
        public int DeathAfter { get; }
        public double Traction { get; }
        public bool DamagedByMobOnly { get; }
        public int DropItemPeriod { get; }
        public byte HpBarForeColor { get; }
        public byte HpBarBackColor { get; }
        public byte CarnivalPoints { get; }
        public int WeaponAttack { get; }
        public int WeaponDefense { get; }
        public int MagicAttack { get; }
        public int MagicDefense { get; }
        public short Accuracy { get; }
        public short Avoidability { get; }
        public short Speed { get; }
        public short ChaseSpeed { get; }

        public bool IsFacingLeft => Stance % 2 == 0;

        public bool CanRespawn => true;

        public int SpawnEffect { get; set; }

        public MobReference CachedReference => DataProvider.Mobs.Data[MapleId];



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
            ExplodeHealth = CachedReference.ExplodeHealth;
            Experience = CachedReference.Experience;
            Link = CachedReference.Link;
            SummonType = CachedReference.SummonType;
            FixedDamage = CachedReference.FixedDamage;
            DeathBuff = CachedReference.DeathBuff;
            DeathAfter = CachedReference.DeathAfter;
            Traction = CachedReference.Traction;
            DamagedByMobOnly = CachedReference.DamagedByMobOnly;
            DropItemPeriod = CachedReference.DropItemPeriod;
            HpBarForeColor = CachedReference.HpBarForeColor;
            HpBarBackColor = CachedReference.HpBarBackColor;
            CarnivalPoints = CachedReference.CarnivalPoints;
            WeaponAttack = CachedReference.WeaponAttack;
            WeaponDefense = CachedReference.WeaponDefense;
            MagicAttack = CachedReference.MagicAttack;
            MagicDefense = CachedReference.MagicDefense;
            Accuracy = CachedReference.Accuracy;
            Avoidability = CachedReference.Avoidability;
            Speed = CachedReference.Speed;
            ChaseSpeed = CachedReference.ChaseSpeed;

            Loots = CachedReference.Loots;
            Skills = new MobSkills(this);
            CachedReference.Skills.ForEach(x =>
            {
                Skills.Add(new MobSkill(x));
            });

            DeathSummons = CachedReference.DeathSummons;

            Attackers = new Dictionary<Character, uint>();
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
            Position = SpawnPoint.Position;
            Position.Y -= 1; // TODO: Is this needed?
        }

        public Mob(int mapleId, Point position)
            : this(mapleId)
        {
            Foothold = 0; // TODO.
            Position = position;
            Position.Y -= 5; // TODO: Is this needed?
        }

        public void AssignController()
        {
            if (Controller == null)
            {
                var leastControlled = int.MaxValue;
                Character newController = null;

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

        public void SwitchController(Character newController)
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

        public void Move(PacketReader iPacket)
        {
            var moveAction = iPacket.ReadShort();
            var skillByte = iPacket.ReadByte();
            var skillId = iPacket.ReadInt();
            iPacket.ReadShort();
            iPacket.ReadInt();

            var movements = new Movements(iPacket);

            Position = movements.Position;
            Foothold = movements.Foothold;
            Stance = movements.Stance;

            // TODO - Load the mob skill
            MobSkill skill = null;
            if (skill != null)
            {
                if (Health * 100 / MaxHealth > skill.PercentageLimitHp ||
                    Cooldowns.ContainsKey(skill) && Cooldowns[skill].AddSeconds(skill.Cooldown) >= DateTime.Now ||
                    (MobSkillName)skill.MapleId == MobSkillName.Summon && Map.Mobs.Count >= 100)
                {
                    skill = null;
                }
            }

            skill?.Cast(this);

            using (var oPacket = new PacketWriter(ServerOperationCode.MobMoveResponse))
            {
                oPacket.WriteInt(ObjectId);
                oPacket.WriteShort(moveAction);
                oPacket.WriteBool(skill != null); // use skills
                oPacket.WriteShort((short)Mana);
                oPacket.WriteShort(0); // skill id, skill level
                Controller?.Client?.Send(oPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.MobMove))
            {
                oPacket.WriteInt(ObjectId);
                oPacket.WriteBool(skill != null); // use skills
                oPacket.WriteInt(skillId);
                oPacket.WriteByte(0);
                oPacket.WriteBytes(movements.ToByteArray());
                Map.Send(oPacket, Controller);
            }
        }

        public void Buff(MobStatus buff, short value, MobSkill skill)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.MobStatSet))
            {
                oPacket.WriteInt(ObjectId);
                oPacket.WriteLong(0);
                oPacket.WriteInt(0);
                oPacket.WriteInt((int)buff);
                oPacket.WriteShort(value);
                oPacket.WriteShort(skill.MapleId);
                oPacket.WriteShort(skill.Level);
                oPacket.WriteShort(-1);
                oPacket.WriteShort(0);// Delay
                oPacket.WriteInt(0);

                Map.Send(oPacket);
            }

            Delay.Execute(() =>
            {
                using (var packet = new PacketWriter(ServerOperationCode.MobStatReset))
                {
                    packet.WriteInt(ObjectId);
                    packet.WriteLong(0);
                    packet.WriteInt(0);
                    packet.WriteInt((int)buff);
                    packet.WriteInt(0);

                    Map.Send(packet);
                }

                Buffs.Remove(buff);
            }, skill.Duration * 1000);
        }

        public void Heal(uint hp, int range)
        {
            Health = Math.Min(MaxHealth, (uint)(Health + hp + Functions.Random(-range / 2, range / 2)));

            using (var packet = new PacketWriter(ServerOperationCode.MobDamaged))
            {
                packet.WriteInt(ObjectId);
                packet.WriteByte(0);
                packet.WriteInt((int)-hp);
                packet.WriteByte(0);
                packet.WriteByte(0);
                packet.WriteByte(0);
                Map.Send(packet);
            }
        }

        public void Die()
        {
            if (!Map.Mobs.Remove(this))
            {

            }
        }

        public bool Damage(Character attacker, uint amount)
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

                using (var oPacket = new PacketWriter(ServerOperationCode.MobHpIndicator))
                {
                    oPacket.WriteInt(ObjectId);
                    oPacket.WriteByte((byte)(Health * 100 / MaxHealth));

                    attacker.Client.Send(oPacket);
                }

                return Health <= 0;
            }
        }

        public PacketWriter GetCreatePacket() => GetInternalPacket(false, true);

        public PacketWriter GetSpawnPacket() => GetInternalPacket(false, false);

        public PacketWriter GetControlRequestPacket() => GetInternalPacket(true, false);

        private PacketWriter GetInternalPacket(bool requestControl, bool newSpawn)
        {
            var pw = new PacketWriter(requestControl ? ServerOperationCode.MobChangeController : ServerOperationCode.MobEnterField);

            if (requestControl)
            {
                pw.WriteByte((byte)(IsProvoked ? 2 : 1));
            }

            pw.WriteInt(ObjectId);
            pw.WriteByte((byte)(Controller == null ? 5 : 1));
            pw.WriteInt(MapleId);
            pw.WriteShort(0);
            pw.WriteByte(0);
            pw.WriteByte(8);
            pw.WriteInt(0);
            pw.WritePoint(Position);
            pw.WriteByte((byte)(0x02 | (IsFacingLeft ? 0x01 : 0x00)));
            pw.WriteShort(0);
            pw.WriteShort(Foothold);

            if (SpawnEffect > 0)
            {
                pw.WriteByte((byte)SpawnEffect);
                pw.WriteByte(0);
                pw.WriteShort(0);
            }

            pw.WriteShort((byte)(newSpawn ? -2 : -1));
            pw.WriteInt(0);

            return pw;
        }

        public PacketWriter GetControlCancelPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.MobChangeController);

            oPacket.WriteBool(false);
            oPacket.WriteInt(ObjectId);

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.MobLeaveField);
            var animated = true;
            oPacket.WriteInt(ObjectId);
            oPacket.WriteBool(animated);

            return oPacket;
        }
    }
}
