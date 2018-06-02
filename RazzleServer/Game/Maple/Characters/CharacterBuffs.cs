using System.Collections;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Skills;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterBuffs : IEnumerable<Buff>
    {
        public Character Parent { get; }

        private List<Buff> Buffs { get; }

        public Buff this[int mapleId]
        {
            get
            {
                foreach (var loopBuff in Buffs)
                {
                    if (loopBuff.MapleId == mapleId)
                    {
                        return loopBuff;
                    }
                }

                return null;
            }
        }

        public CharacterBuffs(Character parent)
        {
            Parent = parent;

            Buffs = new List<Buff>();
        }

        public void Load()
        {
            //foreach (Datum datum in new Datums("buffs").Populate("CharacterId = {0}", Parent.Id))
            //{
            //    if ((DateTime)datum["End"] > DateTime.Now)
            //    {
            //        Add(new Buff(this, datum));
            //    }
            //}
        }

        public void Save()
        {
            Delete();

            foreach (var loopBuff in Buffs)
            {
                loopBuff.Save();
            }
        }

        public void Delete()
        {
            //Database.Delete("buffs", "CharacterId = {0}", this.Parent.Id);
        }

        public bool Contains(Buff buff)
        {
            return Buffs.Contains(buff);
        }

        public bool Contains(int mapleId)
        {
            foreach (var loopBuff in Buffs)
            {
                if (loopBuff.MapleId == mapleId)
                {
                    return true;
                }
            }

            return false;
        }

        public void Add(Skill skill, int value)
        {
            Add(new Buff(this, skill, value));
        }

        public void Add(Buff buff)
        {
            foreach (var loopBuff in Buffs)
            {
                if (loopBuff.MapleId == buff.MapleId)
                {
                    Remove(loopBuff);

                    break;
                }
            }

            buff.Parent = this;

            Buffs.Add(buff);

            if (Parent.IsInitialized && buff.Type == 1)
            {
                buff.Apply();
            }
        }

        public void Remove(int mapleId)
        {
            Remove(this[mapleId]);
        }

        public void Remove(Buff buff)
        {
            Buffs.Remove(buff);

            if (Parent.IsInitialized)
            {
                buff.Cancel();
            }
        }

        public IEnumerator<Buff> GetEnumerator()
        {
            return Buffs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Buffs).GetEnumerator();
        }

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                long mask = 0;
                var value = 0;

                if (Contains((int)SkillNames.Rogue.DarkSight))
                {
                    mask |= (long)SecondaryBuffStat.DarkSight;
                }

                if (Contains((int)SkillNames.Crusader.ComboAttack))
                {
                    mask |= (long)SecondaryBuffStat.Combo;
                    value = this[(int)SkillNames.Crusader.ComboAttack].Value;
                }

                if (Contains((int)SkillNames.Hermit.ShadowPartner))
                {
                    mask |= (long)SecondaryBuffStat.ShadowPartner;
                }

                if (Contains((int)SkillNames.Hunter.SoulArrow) || Contains((int)SkillNames.Crossbowman.SoulArrow))
                {
                    mask |= (long)SecondaryBuffStat.SoulArrow;
                }

                
                oPacket.WriteLong(mask);
                if (value != 0)
                {
                    oPacket.WriteByte((byte)value);
                }


                var magic = Functions.Random();


                oPacket.WriteZeroBytes(6);
                oPacket.WriteInt(magic);
                oPacket.WriteZeroBytes(11);
                oPacket.WriteInt(magic);
                oPacket.WriteZeroBytes(11);
                oPacket.WriteInt(magic);
                oPacket.WriteShort(0);
                oPacket.WriteByte(0);
                oPacket.WriteLong(0);
                oPacket.WriteInt(magic);
                oPacket.WriteZeroBytes(9);
                oPacket.WriteInt(magic);
                oPacket.WriteShort(0);
                oPacket.WriteInt(0);
                oPacket.WriteZeroBytes(10);
                oPacket.WriteInt(magic);
                oPacket.WriteZeroBytes(13);
                oPacket.WriteInt(magic);
                oPacket.WriteShort(0);
                oPacket.WriteByte(0);

                return oPacket.ToArray();
            }
        }
    }
}
