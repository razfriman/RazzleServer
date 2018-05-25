using System.Collections.Generic;
using System.Collections.ObjectModel;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterSkills : KeyedCollection<int, Skill>
    {
        public Character Parent { get; private set; }

        public CharacterSkills(Character parent)
        {
            Parent = parent;
        }

        public void Load()
        {
            //foreach (Datum datum in new Datums("skills").Populate("CharacterId = {0}", Parent.Id))
            //{
            //    Add(new Skill(datum));
            //}
        }

        public void Save()
        {
            foreach (var skill in this)
            {
                skill.Save();
            }
        }

        public void Delete()
        {
            foreach (var skill in this)
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

            switch (skill.MapleId)
            {
                case (int)SkillNames.SuperGM.Resurrection:
                    {
                        var targets = iPacket.ReadByte();

                        while (targets-- > 0)
                        {
                            var targetId = iPacket.ReadInt();

                            var target = Parent.Map.Characters[targetId];

                            if (!target.IsAlive)
                            {
                                target.Health = target.MaxHealth;
                            }
                        }
                    }
                    break;
            }
        }

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WriteShort((short)Count);

                var cooldownSkills = new List<Skill>();

                foreach (var loopSkill in this)
                {
                    oPacket.WriteBytes(loopSkill.ToByteArray());

                    if (loopSkill.IsCoolingDown)
                    {
                        cooldownSkills.Add(loopSkill);
                    }
                }

                oPacket.WriteShort((short)cooldownSkills.Count);

                foreach (var loopCooldown in cooldownSkills)
                {

                    oPacket.WriteInt(loopCooldown.MapleId);
                    oPacket.WriteShort((short)loopCooldown.RemainingCooldownSeconds);
                }

                return oPacket.ToArray();
            }
        }

        protected override void InsertItem(int index, Skill item)
        {
            item.Parent = this;

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = Items[index];

            item.Parent = null;

            base.RemoveItem(index);
        }

        protected override int GetKeyForItem(Skill item)
        {
            return item.MapleId;
        }
    }
}

