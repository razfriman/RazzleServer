using System;
using System.Collections.Generic;
using RazzleServer.Common.Util;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class MobSkill
    {
        public byte MapleId { get; }
        public byte Level { get; }
        public short EffectDelay { get; }
        public MobSkillDataReference CachedReference => DataProvider.MobSkills.Data[MapleId][Level];

        public MobSkill(MobSkillReference reference)
        {
            MapleId = reference.MapleId;
            Level = reference.Level;
            EffectDelay = reference.EffectDelay;
        }

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

                    foreach (var mobId in CachedReference.Summons)
                    {
                        var summon = new Mob(mobId)
                        {
                            Position = caster.Position, SpawnEffect = CachedReference.SummonEffect
                        };

                        caster.Map.Mobs.Add(summon);
                    }

                    break;
            }

            foreach (var affectedMob in GetAffectedMobs(caster))
            {
                if (heal)
                {
                    affectedMob.Heal((uint)CachedReference.ParameterA, CachedReference.ParameterB);
                }

                if (status != MobStatus.None && !affectedMob.Buffs.Contains(status))
                {
                    affectedMob.Buff(status, (short)CachedReference.ParameterA, this);
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
                    using (var oPacket = new PacketWriter(ServerOperationCode.SkillsGiveBuff))
                    {
                        oPacket.WriteLong(0);
                        oPacket.WriteLong((long)disease);

                        oPacket.WriteShort((short)CachedReference.ParameterA);
                        oPacket.WriteShort(MapleId);
                        oPacket.WriteShort(Level);
                        oPacket.WriteInt(CachedReference.Duration);

                        oPacket.WriteShort(0);
                        oPacket.WriteShort(900);
                        oPacket.WriteByte(1);

                        affectedCharacter.Client.Send(oPacket);
                    }

                    //TODO - the remote packet
                    using (var oPacket = new PacketWriter(ServerOperationCode.RemotePlayerSkillBuff))
                    {
                    }
                }
            }

            caster.Mana -= (uint)CachedReference.MpCost;

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
            var rectangle = new Rectangle((CachedReference.Lt ?? new Point(0, 0)) + caster.Position,
                (CachedReference.Rb ?? new Point(0, 0)) + caster.Position);

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
            var rectangle = new Rectangle((CachedReference.Lt ?? new Point(0, 0)) + caster.Position,
                (CachedReference.Rb ?? new Point(0, 0)) + caster.Position);

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
