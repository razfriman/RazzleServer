using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterSkills : MapleKeyedCollection<int, Skill>
    {
        public Character Parent { get; }

        public CharacterSkills(Character parent)
        {
            Parent = parent;
        }

        public void Load()
        {
            using var dbContext = new MapleDbContext();
            var skills = dbContext.Skills.Where(x => x.CharacterId == Parent.Id).ToList();
            skills.ForEach(x => Add(new Skill(x)));
        }

        public void Save()
        {
            foreach (var skill in Values)
            {
                skill.Save();
            }
        }

        public void Delete()
        {
            foreach (var skill in Values)
            {
                skill.Delete();
            }
        }

        public void Cast(PacketReader iPacket)
        {
            iPacket.ReadInt(); // NOTE: Ticks.
            var mapleId = iPacket.ReadInt();
            var level = iPacket.ReadByte();

            var skill = this[mapleId];

            if (level != skill.CurrentLevel)
            {
                return;
            }

            skill.Recalculate();
            skill.Cast();
        }

        public byte[] ToByteArray()
        {
            using var pw = new PacketWriter();
            pw.WriteShort((short)Count);

            foreach (var loopSkill in Values)
            {
                pw.WriteBytes(loopSkill.ToByteArray());
            }

            return pw.ToArray();
        }

        public override void Add(Skill item)
        {
            item.Parent = this;
            base.Add(item);
        }

        public override void Remove(Skill item)
        {
            item.Parent = null;
            base.Remove(item);
        }

        public override int GetKey(Skill item) => item.MapleId;

        public byte GetCurrentLevel(int id) => Contains(id)
            ? this[id].CurrentLevel
            : (byte)0;
    }
}
