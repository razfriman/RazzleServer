using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class Mob : MapObject, IMoveable, ISpawnable, IControllable
    {
        public int MapleID { get; private set; }
        public Character Controller { get; set; }
        public Dictionary<Character, uint> Attackers { get; private set; }
        public SpawnPoint SpawnPoint { get; private set; }
        public byte Stance { get; set; }
        public bool IsProvoked { get; set; }
        public bool CanDrop { get; set; }
        public List<Loot> Loots { get; private set; }
        public short Foothold { get; set; }
        public MobSkills Skills { get; private set; }
        public Dictionary<MobSkill, DateTime> Cooldowns { get; private set; }
        public List<MobStatus> Buffs { get; private set; }
        public List<int> DeathSummons { get; private set; }

        public short Level { get; private set; }
        public uint Health { get; set; }
        public uint Mana { get; set; }
        public uint MaxHealth { get; private set; }
        public uint MaxMana { get; private set; }
        public uint HealthRecovery { get; private set; }
        public uint ManaRecovery { get; private set; }
        public int ExplodeHealth { get; private set; }
        public uint Experience { get; private set; }
        public int Link { get; private set; }
        public short SummonType { get; private set; }
        public int KnockBack { get; private set; }
        public int FixedDamage { get; private set; }
        public int DeathBuff { get; private set; }
        public int DeathAfter { get; private set; }
        public double Traction { get; private set; }
        public int DamagedBySkillOnly { get; private set; }
        public int DamagedByMobOnly { get; private set; }
        public int DropItemPeriod { get; private set; }
        public byte HpBarForeColor { get; private set; }
        public byte HpBarBackColor { get; private set; }
        public byte CarnivalPoints { get; private set; }
        public int WeaponAttack { get; private set; }
        public int WeaponDefense { get; private set; }
        public int MagicAttack { get; private set; }
        public int MagicDefense { get; private set; }
        public short Accuracy { get; private set; }
        public short Avoidability { get; private set; }
        public short Speed { get; private set; }
        public short ChaseSpeed { get; private set; }

        public bool IsFacingLeft
        {
            get
            {
                return Stance % 2 == 0;
            }
        }

        public bool CanRespawn
        {
            get
            {
                return true; // TODO.
            }
        }

        public int SpawnEffect { get; set; }

        public Mob CachedReference
        {
            get
            {
                return DataProvider.Mobs[MapleID];
            }
        }

        public Mob(Datum datum)
            : base()
        {
            MapleID = (int)datum["mobid"];

            Level = (short)datum["mob_level"];
            Health = MaxHealth = (uint)datum["hp"];
            Mana = MaxMana = (uint)datum["mp"];
            HealthRecovery = (uint)datum["hp_recovery"];
            ManaRecovery = (uint)datum["mp_recovery"];
            ExplodeHealth = (int)datum["explode_hp"];
            Experience = (uint)datum["experience"];
            Link = (int)datum["link"];
            SummonType = (short)datum["summon_type"];
            KnockBack = (int)datum["knockback"];
            FixedDamage = (int)datum["fixed_damage"];
            DeathBuff = (int)datum["death_buff"];
            DeathAfter = (int)datum["death_after"];
            Traction = (double)datum["traction"];
            DamagedBySkillOnly = (int)datum["damaged_by_skill_only"];
            DamagedByMobOnly = (int)datum["damaged_by_mob_only"];
            DropItemPeriod = (int)datum["drop_item_period"];
            HpBarForeColor = (byte)(sbyte)datum["hp_bar_color"];
            HpBarBackColor = (byte)(sbyte)datum["hp_bar_bg_color"];
            CarnivalPoints = (byte)(sbyte)datum["carnival_points"];
            WeaponAttack = (int)datum["physical_attack"];
            WeaponDefense = (int)datum["physical_defense"];
            MagicAttack = (int)datum["magical_attack"];
            MagicDefense = (int)datum["magical_defense"];
            Accuracy = (short)datum["accuracy"];
            Avoidability = (short)datum["avoidability"];
            Speed = (short)datum["speed"];
            ChaseSpeed = (short)datum["chase_speed"];

            Loots = new List<Loot>();
            Skills = new MobSkills(this);
            DeathSummons = new List<int>();
        }

        public Mob(int mapleID)
        {
            MapleID = mapleID;

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
            KnockBack = CachedReference.KnockBack;
            FixedDamage = CachedReference.FixedDamage;
            DeathBuff = CachedReference.DeathBuff;
            DeathAfter = CachedReference.DeathAfter;
            Traction = CachedReference.Traction;
            DamagedBySkillOnly = CachedReference.DamagedBySkillOnly;
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
            Skills = CachedReference.Skills;
            DeathSummons = CachedReference.DeathSummons;

            Attackers = new Dictionary<Character, uint>();
            Cooldowns = new Dictionary<MobSkill, DateTime>();
            Buffs = new List<MobStatus>();
            Stance = 5;
            CanDrop = true;
        }

        public Mob(SpawnPoint spawnPoint)
            : this(spawnPoint.MapleID)
        {
            SpawnPoint = spawnPoint;
            Foothold = SpawnPoint.Foothold;
            Position = SpawnPoint.Position;
            Position.Y -= 1; // TODO: Is this needed?
        }

        public Mob(int mapleID, Point position)
            : this(mapleID)
        {
            Foothold = 0; // TODO.
            Position = position;
            Position.Y -= 5; // TODO: Is this needed?
        }

        public void AssignController()
        {
            if (Controller == null)
            {
                int leastControlled = int.MaxValue;
                Character newController = null;

                lock (Map.Characters)
                {
                    foreach (Character character in Map.Characters)
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
            short moveAction = iPacket.ReadShort();
            bool cheatResult = (iPacket.ReadByte() & 0xF) != 0;
            byte centerSplit = iPacket.ReadByte();
            int illegalVelocity = iPacket.ReadInt();
            iPacket.Skip(8);
            iPacket.ReadByte();
            iPacket.ReadInt();

            Movements movements = new Movements(iPacket);

            Position = movements.Position;
            Foothold = movements.Foothold;
            Stance = movements.Stance;

            byte skillID = 0;
            byte skillLevel = 0;
            MobSkill skill = null;

            if (skill != null)
            {
                if (Health * 100 / MaxHealth > skill.PercentageLimitHP ||
                    (Cooldowns.ContainsKey(skill) && Cooldowns[skill].AddSeconds(skill.Cooldown) >= DateTime.Now) ||
                    ((MobSkillName)skill.MapleID) == MobSkillName.Summon && Map.Mobs.Count >= 100)
                {
                    skill = null;
                }
            }

            if (skill != null)
            {
                skill.Cast(this);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.MobCtrlAck))
            {

                oPacket.WriteInt(ObjectID);
                oPacket.WriteShort(moveAction);
                oPacket.WriteBool(cheatResult);
                oPacket.WriteShort((short)Mana);
                oPacket.WriteByte(skillID);
                oPacket.WriteByte(skillLevel);

                Controller.Client.Send(oPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.MobMove))
            {

                oPacket.WriteInt(ObjectID);
                oPacket.WriteBool(false);
                oPacket.WriteBool(cheatResult);
                oPacket.WriteByte(centerSplit);
                oPacket.WriteInt(illegalVelocity);
                oPacket.WriteBytes(movements.ToByteArray());

                Map.Broadcast(oPacket, Controller);
            }
        }

        public void Buff(MobStatus buff, short value, MobSkill skill)
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.MobStatSet))
            {
                oPacket.WriteInt(ObjectID);
                oPacket.WriteLong(0);
                oPacket.WriteInt(0);
                oPacket.WriteInt((int)buff);
                oPacket.WriteShort(value);
                oPacket.WriteShort(skill.MapleID);
                oPacket.WriteShort(skill.Level);
                oPacket.WriteShort(-1);
                oPacket.WriteShort(0);// Delay
                oPacket.WriteInt(0);

                Map.Broadcast(oPacket);
            }

            Delay.Execute(() =>
            {
                using (var packet = new PacketWriter(ServerOperationCode.MobStatReset))
                {
                    packet.WriteInt(ObjectID);
                    packet.WriteLong(0);
                    packet.WriteInt(0);
                    packet.WriteInt((int)buff);
                    packet.WriteInt(0);

                    Map.Broadcast(packet);
                }

                Buffs.Remove(buff);
            }, skill.Duration * 1000);
        }

        public void Heal(uint hp, int range)
        {
            Health = Math.Min(MaxHealth, (uint)(Health + hp + Functions.Random(-range / 2, range / 2)));

            using (var packet = new PacketWriter(ServerOperationCode.MobDamaged))
            {
                packet.WriteInt(ObjectID);
                packet.WriteByte(0);
                packet.WriteInt((int)-hp);
                packet.WriteByte(0);
                packet.WriteByte(0);
                packet.WriteByte(0);
                Map.Broadcast(packet);
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
                uint originalAmount = amount;

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

                using (var oPacket = new PacketWriter(ServerOperationCode.MobHPIndicator))
                {
                    oPacket.WriteInt(ObjectID);
                    oPacket.WriteByte((byte)((Health * 100) / MaxHealth));

                    attacker.Client.Send(oPacket);
                }

                if (Health <= 0)
                {
                    return true;
                }

                return false;
            }
        }

        public PacketWriter GetCreatePacket()
        {
            return GetInternalPacket(false, true);
        }

        public PacketWriter GetSpawnPacket()
        {
            return GetInternalPacket(false, false);
        }

        public PacketWriter GetControlRequestPacket()
        {
            return GetInternalPacket(true, false);
        }

        private PacketWriter GetInternalPacket(bool requestControl, bool newSpawn)
        {
            var oPacket = new PacketWriter(requestControl ? ServerOperationCode.MobChangeController : ServerOperationCode.MobEnterField);

            if (requestControl)
            {
                oPacket.WriteByte((byte)(IsProvoked ? 2 : 1));
            }

            oPacket.WriteInt(ObjectID);
            oPacket.WriteByte((byte)(Controller == null ? 5 : 1));
            oPacket.WriteInt(MapleID);
            oPacket.WriteZeroBytes(15); // NOTE: Unknown.
            oPacket.WriteByte(0x88); // NOTE: Unknown.
            oPacket.WriteZeroBytes(6); // NOTE: Unknown.
            oPacket.WriteShort(Position.X);
            oPacket.WriteShort(Position.Y);
            oPacket.WriteByte((byte)(0x02 | (IsFacingLeft ? 0x01 : 0x00)));
            oPacket.WriteShort(Foothold);
            oPacket.WriteShort(Foothold);

            if (SpawnEffect > 0)
            {
                oPacket.WriteByte((byte)SpawnEffect);
                oPacket.WriteByte(0);
                oPacket.WriteShort(0);

                if (SpawnEffect == 15)
                {
                    oPacket.WriteByte(0);
                }
            }

            oPacket.WriteByte((byte)(newSpawn ? -2 : -1));
            oPacket.WriteByte(0);
            oPacket.WriteByte(byte.MaxValue); // NOTE: Carnival team.
            oPacket.WriteInt(0); // NOTE: Unknown.

            return oPacket;
        }

        public PacketWriter GetControlCancelPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.MobChangeController);

            oPacket.WriteBool(false);
            oPacket.WriteInt(ObjectID);

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.MobLeaveField);

            oPacket.WriteInt(ObjectID);
            oPacket.WriteByte(1);
            oPacket.WriteByte(1); // TODO: Death effects.

            return oPacket;
        }
    }
}
