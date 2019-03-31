using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterBuffs : IEnumerable<Buff>
    {
        public Character Parent { get; }

        private List<Buff> Buffs { get; }

        public Buff this[int mapleId] => Buffs.FirstOrDefault(x => x.MapleId == mapleId);

        public CharacterBuffs(Character parent)
        {
            Parent = parent;
            Buffs = new List<Buff>();
        }

        public void Load()
        {
            using (var dbContext = new MapleDbContext())
            {
                var buffs = dbContext.Buffs.Where(x => x.CharacterId == Parent.Id).ToList();
                buffs.ForEach(x => Add(new Buff(this, x)));
            }
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
            using (var dbContext = new MapleDbContext())
            {
                dbContext.Buffs.RemoveRange(dbContext.Buffs.Where(x => x.CharacterId == Parent.Id).ToList());
                dbContext.SaveChanges();
            }
        }

        public bool Contains(Buff buff) => Buffs.Contains(buff);

        public bool Contains(int mapleId) => Buffs.Any(x => x.MapleId == mapleId);

        public void Add(Skill skill, uint value) => Add(new Buff(this, skill, value));

        public void Add(Item item)
        {
            Add(new Buff(this, item));
        }

        public void Add(Buff buff)
        {
            foreach (var loopBuff in Buffs.ToList())
            {
                if (loopBuff.MapleId == buff.MapleId)
                {
                    Remove(loopBuff);
                    break;
                }
            }

            buff.Parent = this;

            Buffs.Add(buff);

            if (Parent.IsInitialized && buff.Type == BuffType.Skill)
            {
                buff.Apply();
            }
        }


        public void Remove(int mapleId) => Remove(this[mapleId]);

        public void Remove(Buff buff)
        {
            Buffs.Remove(buff);

            if (Parent.IsInitialized)
            {
                buff.Cancel();
            }
        }

        public IEnumerator<Buff> GetEnumerator() => Buffs.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Buffs).GetEnumerator();

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                return oPacket.ToArray();
            }
        }
    }
}
