using System;
using System.Collections.Generic;
using RazzleServer.Common.Util;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data.References;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class MobSkill
    {
        public static Dictionary<short, List<int>> Summons { get; set; }

        public byte MapleId { get; }
        public byte Level { get; }
        public short EffectDelay { get; }

        public int Duration { get; private set; }
        public short MpCost { get; private set; }
        public int ParameterA { get; private set; }
        public int ParameterB { get; private set; }
        public short Chance { get; private set; }
        public short TargetCount { get; private set; }
        public int Cooldown { get; private set; }
        public Point Lt { get; private set; }
        public Point Rb { get; private set; }
        public short PercentageLimitHp { get; private set; }
        public short SummonLimit { get; private set; }
        public short SummonEffect { get; private set; }

        public MobSkill(MobSkillReference reference)
        {
            MapleId = reference.MapleId;
            Level = reference.Level;
            EffectDelay = reference.EffectDelay;
        }


        //public void Load(Datum datum)
        //{
        //    Duration = (short)datum["buff_time"];
        //    MpCost = (short)datum["mp_cost"];
        //    ParameterA = (int)datum["x_property"];
        //    ParameterB = (int)datum["y_property"];
        //    Chance = (short)datum["chance"];
        //    TargetCount = (short)datum["target_count"];
        //    Cooldown = (int)datum["cooldown"];
        //    LT = new Point((short)datum["ltx"], (short)datum["lty"]);
        //    RB = new Point((short)datum["rbx"], (short)datum["rby"]);
        //    PercentageLimitHP = (short)datum["hp_limit_percentage"];
        //    SummonLimit = (short)datum["summon_limit"];
        //    SummonEffect = (short)datum["summon_effect"];
        //}

        public void Cast(Mob caster)
        {
            var status = MobStatus.None;
            var disease = CharacterDisease.None;
            var heal = false;
            var banish = false;
            var dispel = false;

            switch ((MobSkillName)MapleId)
            {
                case MobSkillName.WeaponAttackUp:
                case MobSkillName.WeaponAttackUpAreaOfEffect:
                case MobSkillName.WeaponAttackUpMonsterCarnival:
                    status = MobStatus.WeaponAttackUp;
                    break;

                case MobSkillName.MagicAttackUp:
                case MobSkillName.MagicAttackUpAreaOfEffect:
                case MobSkillName.MagicAttackUpMonsterCarnival:
                    status = MobStatus.MagicAttackUp;
                    break;

                case MobSkillName.WeaponDefenseUp:
                case MobSkillName.WeaponDefenseUpAreaOfEffect:
                case MobSkillName.WeaponDefenseUpMonsterCarnival:
                    status = MobStatus.WeaponDefenseUp;
                    break;

                case MobSkillName.MagicDefenseUp:
                case MobSkillName.MagicDefenseUpAreaOfEffect:
                case MobSkillName.MagicDefenseUpMonsterCarnival:
                    status = MobStatus.MagicDefenseUp;
                    break;

                case MobSkillName.HealAreaOfEffect:
                    heal = true;
                    break;

                case MobSkillName.Seal:
                    disease = CharacterDisease.Sealed;
                    break;

                case MobSkillName.Darkness:
                    disease = CharacterDisease.Darkness;
                    break;

                case MobSkillName.Weakness:
                    disease = CharacterDisease.Weaken;
                    break;

                case MobSkillName.Stun:
                    disease = CharacterDisease.Stun;
                    break;

                case MobSkillName.Curse:
                    // TODO: Curse.
                    break;

                case MobSkillName.Poison:
                    disease = CharacterDisease.Poison;
                    break;

                case MobSkillName.Slow:
                    disease = CharacterDisease.Slow;
                    break;

                case MobSkillName.Dispel:
                    dispel = true;
                    break;

                case MobSkillName.Seduce:
                    disease = CharacterDisease.Seduce;
                    break;

                case MobSkillName.SendToTown:
                    // TODO: Send to town.
                    break;

                case MobSkillName.PoisonMist:
                    // TODO: Spawn poison mist.
                    break;

                case MobSkillName.Confuse:
                    disease = CharacterDisease.Confuse;
                    break;

                case MobSkillName.Zombify:
                    // TODO: Zombify.
                    break;

                case MobSkillName.WeaponImmunity:
                    status = MobStatus.WeaponImmunity;
                    break;

                case MobSkillName.MagicImmunity:
                    status = MobStatus.MagicImmunity;
                    break;

                case MobSkillName.WeaponDamageReflect:
                case MobSkillName.MagicDamageReflect:
                case MobSkillName.AnyDamageReflect:
                    // TODO: Reflect.
                    break;

                case MobSkillName.AccuracyUpMonsterCarnival:
                case MobSkillName.AvoidabilityUpMonsterCarnival:
                case MobSkillName.SpeedUpMonsterCarnival:
                    // TODO: Monster carnival buffs.
                    break;

                case MobSkillName.Summon:

                    foreach (var mobId in Summons[Level])
                    {
                        var summon = new Mob(mobId)
                        {
                            Position = caster.Position,
                            SpawnEffect = SummonEffect
                        };

                        caster.Map.Mobs.Add(summon);
                    }
                    break;
            }

            foreach (var affectedMob in GetAffectedMobs(caster))
            {
                if (heal)
                {
                    affectedMob.Heal((uint)ParameterA, ParameterB);
                }

                if (status != MobStatus.None && !affectedMob.Buffs.Contains(status))
                {
                    affectedMob.Buff(status, (short)ParameterA, this);
                }
            }

            foreach (var affectedCharacter in GetAffectedCharacters(caster))
            {
                if (dispel)
                {
                    //affectedCharacter.Dispel();
                }

                if (banish)
                {
                    affectedCharacter.ChangeMap(affectedCharacter.Map.CachedReference.ReturnMapId);
                }

                if (disease != CharacterDisease.None)
                {
                    using (var oPacket = new PacketWriter(ServerOperationCode.TemporaryStatSet))
                    {
                        oPacket.WriteLong(0);
                        oPacket.WriteLong((long)disease);

                        oPacket.WriteShort((short)ParameterA);
                        oPacket.WriteShort(MapleId);
                        oPacket.WriteShort(Level);
                        oPacket.WriteInt(Duration);

                        oPacket.WriteShort(0);
                        oPacket.WriteShort(900);
                        oPacket.WriteByte(1);

                        affectedCharacter.Client.Send(oPacket);
                    }

                    //map packet.
                }
            }

            caster.Mana -= (uint)MpCost;

            if (caster.Cooldowns.ContainsKey(this))
            {
                caster.Cooldowns[this] = DateTime.Now;
            }
            else
            {
                caster.Cooldowns.Add(this, DateTime.Now);
            }
        }

        private IEnumerable<Character> GetAffectedCharacters(Mob caster)
        {
            var rectangle = new Rectangle(Lt + caster.Position, Rb + caster.Position);

            foreach (var character in caster.Map.Characters.Values)
            {
                if (character.Position.IsInRectangle(rectangle))
                {
                    yield return character;
                }
            }
        }

        private IEnumerable<Mob> GetAffectedMobs(Mob caster)
        {
            var rectangle = new Rectangle(Lt + caster.Position, Rb + caster.Position);

            foreach (var mob in caster.Map.Mobs.Values)
            {
                if (mob.Position.IsInRectangle(rectangle))
                {
                    yield return mob;
                }
            }
        }
    }
}
