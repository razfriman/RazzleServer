using System;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple
{
    public sealed class Buff
    {
        public CharacterBuffs Parent { get; set; }

        public int MapleID { get; set; }
        public byte SkillLevel { get; set; }
        public byte Type { get; set; }
        public Dictionary<PrimaryBuffStat, short> PrimaryStatups { get; set; }
        public Dictionary<SecondaryBuffStat, short> SecondaryStatups { get; set; }
        public DateTime End { get; set; }
        public int Value { get; set; }

        public Character Character
        {
            get
            {
                return Parent.Parent;
            }
        }

        public long PrimaryBuffMask
        {
            get
            {
                long mask = 0;

                foreach (KeyValuePair<PrimaryBuffStat, short> primaryStatup in PrimaryStatups)
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

                foreach (KeyValuePair<SecondaryBuffStat, short> secondaryStatus in SecondaryStatups)
                {
                    mask |= (long)secondaryStatus.Key;
                }

                return mask;
            }
        }

        public Buff(CharacterBuffs parent, Skill skill, int value)
        {
            Parent = parent;
            MapleID = skill.MapleID;
            SkillLevel = skill.CurrentLevel;
            Type = 1;
            Value = value;
            End = DateTime.Now.AddSeconds(skill.BuffTime);
            PrimaryStatups = new Dictionary<PrimaryBuffStat, short>();
            SecondaryStatups = new Dictionary<SecondaryBuffStat, short>();

            CalculateStatups(skill);

            Delay.Execute(() =>
            {
                if (Parent.Contains(this))
                {
                    Parent.Remove(this);
                }
            }, (int)(End - DateTime.Now).TotalMilliseconds);
        }

        public Buff(CharacterBuffs parent, Datum datum)
        {
            Parent = parent;
            MapleID = (int)datum["MapleID"];
            SkillLevel = (byte)datum["SkillLevel"];
            Type = (byte)datum["Type"];
            Value = (int)datum["Value"];
            End = (DateTime)datum["End"];
            PrimaryStatups = new Dictionary<PrimaryBuffStat, short>();
            SecondaryStatups = new Dictionary<SecondaryBuffStat, short>();

            if (Type == 1)
            {
                CalculateStatups(DataProvider.Skills[MapleID][SkillLevel]);
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
            Datum datum = new Datum("buffs");

            datum["CharacterID"] = Character.ID;
            datum["MapleID"] = MapleID;
            datum["SkillLevel"] = SkillLevel;
            datum["Type"] = Type;
            datum["Value"] = Value;
            datum["End"] = End;

            datum.Insert();
        }

        public void Apply()
        {
            switch (MapleID)
            {
                default:
                    {
                        using (var oPacket = new PacketWriter(ServerOperationCode.TemporaryStatSet))
                        {
                            oPacket.WriteLong(PrimaryBuffMask);
                            oPacket.WriteLong(SecondaryBuffMask);

                            foreach (KeyValuePair<PrimaryBuffStat, short> primaryStatup in PrimaryStatups)
                            {
                                oPacket.WriteShort(primaryStatup.Value);
                                oPacket.WriteInt(MapleID);
                                oPacket.WriteInt((int)(End - DateTime.Now).TotalMilliseconds);
                            }

                            foreach (KeyValuePair<SecondaryBuffStat, short> secondaryStatup in SecondaryStatups)
                            {
                                oPacket.WriteShort(secondaryStatup.Value);
                                oPacket.WriteInt(MapleID);
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
                            oPacket.WriteInt(Character.ID);
                            oPacket.WriteLong(PrimaryBuffMask);
                            oPacket.WriteLong(SecondaryBuffMask);

                            foreach (KeyValuePair<PrimaryBuffStat, short> primaryStatup in PrimaryStatups)
                            {
                                oPacket.WriteShort(primaryStatup.Value);
                            }

                            foreach (KeyValuePair<SecondaryBuffStat, short> secondaryStatup in SecondaryStatups)
                            {
                                oPacket.WriteShort(secondaryStatup.Value);
                            }

                            oPacket.WriteInt(0);
                            oPacket.WriteShort(0);

                            Character.Map.Broadcast(oPacket);
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
                oPacket.WriteInt(Character.ID);
                oPacket.WriteLong(PrimaryBuffMask);
                oPacket.WriteLong(SecondaryBuffMask);

                Character.Map.Broadcast(oPacket);
            }
        }

        public void CalculateStatups(Skill skill)
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

            switch (MapleID)
            {
                case (int)SkillNames.SuperGM.HyperBody:
                    SecondaryStatups.Add(SecondaryBuffStat.HyperBodyHP, skill.ParameterA);
                    SecondaryStatups.Add(SecondaryBuffStat.HyperBodyMP, skill.ParameterB);
                    break;

                case (int)SkillNames.SuperGM.HolySymbol:
                    SecondaryStatups.Add(SecondaryBuffStat.HolySymbol, skill.ParameterA);
                    break;

                case (int)SkillNames.SuperGM.Hide:
                    SecondaryStatups.Add(SecondaryBuffStat.DarkSight, skill.ParameterA);
                    break;
            }
        }
    }
}
