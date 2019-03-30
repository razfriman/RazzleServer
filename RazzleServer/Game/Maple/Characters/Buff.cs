using System;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class Buff
    {
        public CharacterBuffs Parent { get; set; }
        public int MapleId { get; set; }
        public byte SkillLevel { get; set; }
        public BuffType Type { get; set; }
        public DateTime End { get; set; }
        public short Parameter { get; set; }
        public BuffValueTypes Flag { get; set; }

        public Character Character => Parent.Parent;

        public Buff(CharacterBuffs parent, Skill skill, uint value)
        {
            Parent = parent;
            MapleId = skill.MapleId;
            SkillLevel = skill.CurrentLevel;
            Type = BuffType.Skill;
            Flag = (BuffValueTypes)value;
            End = DateTime.Now.AddSeconds(skill.BuffTime);
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
            Type = (BuffType)entity.Type;
            Flag = (BuffValueTypes)entity.Value;
            End = entity.End;

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
                    Type = (byte)Type,
                    Value = (uint)Flag,
                    End = End
                });
                dbContext.SaveChanges();
            }
        }

        public void Apply()
        {
            using (var oPacket = new PacketWriter(ServerOperationCode.SkillsGiveBuff))
            {
                //Character.Client.Send(oPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.RemotePlayerSkillBuff))
            {
                //Character.Map.Send(oPacket);
            }
        }

        public void Cancel()
        {
            if (MapleId == (int)SkillNames.Gm.Hide)
            {
                Character.Hide(false);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.SkillsGiveBuff))
            {
                Character.Client.Send(oPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.RemotePlayerSkillBuff))
            {
                Character.Map.Send(oPacket);
            }
        }
    }
}
