using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple
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

            Delay.Execute(() =>
            {
                if (Parent.Contains(this))
                {
                    Parent.Remove(this);
                }
            }, (int)(End - DateTime.Now).TotalMilliseconds);
        }

        public Buff(CharacterBuffs parent, BuffEntity datum)
        {
            Parent = parent;
            //MapleId = (int)datum["MapleId"];
            //SkillLevel = (byte)datum["SkillLevel"];
            //Type = (byte)datum["Type"];
            //Value = (int)datum["Value"];
            //End = (DateTime)datum["End"];
            PrimaryStatups = new Dictionary<PrimaryBuffStat, short>();
            SecondaryStatups = new Dictionary<SecondaryBuffStat, short>();

            if (Type == 1)
            {
                CalculateStatups(DataProvider.Skills.Data[MapleId][SkillLevel]);
            }

            Delay.Execute(() =>
            {
                if (Parent.Contains(this))
                {
                    Parent.Remove(this);
                }
            }, (int)(End - DateTime.Now).TotalMilliseconds);
        }

        public void Save()
        {
            //Datum datum = new Datum("buffs");

            //datum["CharacterId"] = Character.Id;
            //datum["MapleId"] = MapleId;
            //datum["SkillLevel"] = SkillLevel;
            //datum["Type"] = Type;
            //datum["Value"] = Value;
            //datum["End"] = End;

            //datum.Insert();
        }

        public void Apply()
        {
            switch (MapleId)
            {
                default:
                    {
                        using (var oPacket = new PacketWriter(ServerOperationCode.TemporaryStatSet))
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

                        using (var oPacket = new PacketWriter(ServerOperationCode.SetTemporaryStat))
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
            using (var oPacket = new PacketWriter(ServerOperationCode.TemporaryStatReset))
            {
                oPacket.WriteLong(PrimaryBuffMask);
                oPacket.WriteLong(SecondaryBuffMask);
                oPacket.WriteByte(1);

                Character.Client.Send(oPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.ResetTemporaryStat))
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

            switch (MapleId)
            {
                case (int)SkillNames.SuperGm.HolySymbol:
                    SecondaryStatups.Add(SecondaryBuffStat.HolySymbol, skill.ParameterA);
                    break;

                case (int)SkillNames.SuperGm.Hide:
                    SecondaryStatups.Add(SecondaryBuffStat.DarkSight, skill.ParameterA);
                    break;
            }
        }
    }
}
