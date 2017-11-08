using System;
using System.Collections;
using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterBuffs : IEnumerable<Buff>
    {
        public Character Parent { get; private set; }

        private List<Buff> Buffs { get; set; }

        public Buff this[int mapleId]
        {
            get
            {
                foreach (Buff loopBuff in Buffs)
                {
                    if (loopBuff.MapleID == mapleId)
                    {
                        return loopBuff;
                    }
                }

                return null;
            }
        }

        public CharacterBuffs(Character parent)
            : base()
        {
            Parent = parent;

            Buffs = new List<Buff>();
        }

        public void Load()
        {
            foreach (Datum datum in new Datums("buffs").Populate("CharacterID = {0}", Parent.ID))
            {
                if ((DateTime)datum["End"] > DateTime.Now)
                {
                    Add(new Buff(this, datum));
                }
            }
        }

        public void Save()
        {
            Delete();

            foreach (Buff loopBuff in Buffs)
            {
                loopBuff.Save();
            }
        }

        public void Delete()
        {
            //Database.Delete("buffs", "CharacterID = {0}", this.Parent.ID);
        }

        public bool Contains(Buff buff)
        {
            return Buffs.Contains(buff);
        }

        public bool Contains(int mapleId)
        {
            foreach (Buff loopBuff in Buffs)
            {
                if (loopBuff.MapleID == mapleId)
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
            foreach (Buff loopBuff in Buffs)
            {
                if (loopBuff.MapleID == buff.MapleID)
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

        public void Cancel(PacketReader iPacket)
        {
            int mapleID = iPacket.ReadInt();

            switch (mapleID)
            {
                // TODO: Handle special skills.

                default:
                    Remove(mapleID);
                    break;
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
                int value = 0;

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


                int magic = Functions.Random();


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
