using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class MobSkill
    {
        public static Dictionary<short, List<int>> Summons { get; set; }

        public byte MapleId { get; private set; }
        public byte Level { get; private set; }
        public short EffectDelay { get; private set; }

        public int Duration { get; private set; }
        public short MpCost { get; private set; }
        public int ParameterA { get; private set; }
        public int ParameterB { get; private set; }
        public short Chance { get; private set; }
        public short TargetCount { get; private set; }
        public int Cooldown { get; private set; }
        public Point LT { get; private set; }
        public Point RB { get; private set; }
        public short PercentageLimitHP { get; private set; }
        public short SummonLimit { get; private set; }
        public short SummonEffect { get; private set; }

        public MobSkill(WzImageProperty img)
        {
            MapleId = (byte)(img["skill"]?.GetInt() ?? 0);
            Level = (byte)(img["level"]?.GetInt() ?? 0);
            EffectDelay = img["effectAfter"]?.GetShort() ?? 0;
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
                    affectedCharacter.ChangeMap(affectedCharacter.Map.ReturnMapId);
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
            var Rectangle = new Rectangle(LT + caster.Position, RB + caster.Position);

            foreach (var character in caster.Map.Characters)
            {
                if (character.Position.IsInRectangle(Rectangle))
                {
                    yield return character;
                }
            }
        }

        private IEnumerable<Mob> GetAffectedMobs(Mob caster)
        {
            var Rectangle = new Rectangle(LT + caster.Position, RB + caster.Position);

            foreach (var mob in caster.Map.Mobs)
            {
                if (mob.Position.IsInRectangle(Rectangle))
                {
                    yield return mob;
                }
            }
        }
    }
}
