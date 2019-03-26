using System;
using System.Collections.Generic;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class Buff
    {
        public CharacterBuffs Parent { get; set; }

        public int MapleId { get; set; }
        public byte SkillLevel { get; set; }
        public byte Type { get; set; }
        public Dictionary<PrimaryBuffStat, short> PrimaryStatups { get; set; }
        public Dictionary<SecondaryBuffStat, short> SecondaryStatups { get; set; }
        public DateTime End { get; set; }
        public int Value { get; set; }

        public Character Character => Parent.Parent;

        public long PrimaryBuffMask
        {
            get
            {
                long mask = 0;

                foreach (var primaryStatup in PrimaryStatups)
                {
                    mask |= (long)primaryStatup.Key;
                }

                return mask;
            }
        }

        public long SecondaryBuffMask
        {
            get
            {
                long mask = 0;

                foreach (var secondaryStatus in SecondaryStatups)
                {
                    mask |= (long)secondaryStatus.Key;
                }

                return mask;
            }
        }

        public Buff(CharacterBuffs parent, Skill skill, int value)
        {
            Parent = parent;
            MapleId = skill.MapleId;
            SkillLevel = skill.CurrentLevel;
            Type = 1;
            Value = value;
            End = DateTime.Now.AddSeconds(skill.BuffTime);
            PrimaryStatups = new Dictionary<PrimaryBuffStat, short>();
            SecondaryStatups = new Dictionary<SecondaryBuffStat, short>();

            CalculateStatups(skill.CachedReference);
            ScheduleExpiration();
        }

        private void ScheduleExpiration()
        {
            Delay.Execute(() =>
            {
                if (Parent.Contains(this))
                {
                    Parent.Remove(this);
                }
            }, (int)(End - DateTime.Now).TotalMilliseconds);
        }

        public Buff(CharacterBuffs parent, BuffEntity entity)
        {
            Parent = parent;
            MapleId = entity.SkillId;
            SkillLevel = entity.SkillLevel;
            Type = entity.Type;
            Value = entity.Value;
            End = entity.End;
            PrimaryStatups = new Dictionary<PrimaryBuffStat, short>();
            SecondaryStatups = new Dictionary<SecondaryBuffStat, short>();

            if (Type == 1)
            {
                CalculateStatups(DataProvider.Skills.Data[MapleId][SkillLevel]);
            }

            ScheduleExpiration();
        }

        public void Save()
        {
            using (var dbContext = new MapleDbContext())
            {
                dbContext.Buffs.Add(new BuffEntity
                {
                    CharacterId = Character.Id,
                    SkillId = MapleId,
                    SkillLevel = SkillLevel,
                    Type = Type,
                    Value = Value,
                    End = End
                });
                dbContext.SaveChanges();
            }
        }

        public void Apply()
        {
            switch (MapleId)
            {
                default:
                {
                    using (var oPacket = new PacketWriter(ServerOperationCode.SkillsGiveBuff))
                    {
                        oPacket.WriteLong(PrimaryBuffMask);
                        oPacket.WriteLong(SecondaryBuffMask);

                        foreach (var primaryStatup in PrimaryStatups)
                        {
                            oPacket.WriteShort(primaryStatup.Value);
                            oPacket.WriteInt(MapleId);
                            oPacket.WriteInt((int)(End - DateTime.Now).TotalMilliseconds);
                        }

                        foreach (var secondaryStatup in SecondaryStatups)
                        {
                            oPacket.WriteShort(secondaryStatup.Value);
                            oPacket.WriteInt(MapleId);
                            oPacket.WriteInt((int)(End - DateTime.Now).TotalMilliseconds);
                        }

                        oPacket.WriteShort(0);
                        oPacket.WriteShort(0);
                        oPacket.WriteByte(0);
                        oPacket.WriteInt(0);

                        Character.Client.Send(oPacket);
                    }

                    using (var oPacket = new PacketWriter(ServerOperationCode.RemotePlayerSkillBuff))
                    {
                        oPacket.WriteInt(Character.Id);
                        oPacket.WriteLong(PrimaryBuffMask);
                        oPacket.WriteLong(SecondaryBuffMask);

                        foreach (var primaryStatup in PrimaryStatups)
                        {
                            oPacket.WriteShort(primaryStatup.Value);
                        }

                        foreach (var secondaryStatup in SecondaryStatups)
                        {
                            oPacket.WriteShort(secondaryStatup.Value);
                        }

                        oPacket.WriteInt(0);
                        oPacket.WriteShort(0);

                        Character.Map.Send(oPacket);
                    }
                }
                    break;
            }
        }

        public void Cancel()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.SkillsGiveBuff))
            {
                oPacket.WriteLong(PrimaryBuffMask);
                oPacket.WriteLong(SecondaryBuffMask);
                oPacket.WriteByte(1);

                Character.Client.Send(oPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.RemotePlayerSkillBuff))
            {
                oPacket.WriteInt(Character.Id);
                oPacket.WriteLong(PrimaryBuffMask);
                oPacket.WriteLong(SecondaryBuffMask);

                Character.Map.Send(oPacket);
            }
        }

        public void CalculateStatups(SkillReference skill)
        {
            if (skill.WeaponAttack > 0)
            {
                SecondaryStatups.Add(SecondaryBuffStat.WeaponAttack, skill.WeaponAttack);
            }

            if (skill.WeaponDefense > 0)
            {
                SecondaryStatups.Add(SecondaryBuffStat.WeaponDefense, skill.WeaponDefense);
            }

            if (skill.MagicAttack > 0)
            {
                SecondaryStatups.Add(SecondaryBuffStat.MagicAttack, skill.MagicAttack);
            }

            if (skill.MagicDefense > 0)
            {
                SecondaryStatups.Add(SecondaryBuffStat.MagicDefense, skill.MagicAttack);
            }

            if (skill.Accuracy > 0)
            {
                SecondaryStatups.Add(SecondaryBuffStat.Accuracy, skill.Accuracy);
            }

            if (skill.Avoidability > 0)
            {
                SecondaryStatups.Add(SecondaryBuffStat.Avoid, skill.Avoidability);
            }

            if (skill.Speed > 0)
            {
                SecondaryStatups.Add(SecondaryBuffStat.Speed, skill.Speed);
            }

            if (skill.Jump > 0)
            {
                SecondaryStatups.Add(SecondaryBuffStat.Jump, skill.Jump);
            }

            if (skill.Morph > 0)
            {
                SecondaryStatups.Add(SecondaryBuffStat.Morph, (short)(skill.Morph + 100 * (int)Character.Gender));
            }
        }
    }
}
