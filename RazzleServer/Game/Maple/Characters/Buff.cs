using System;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Data;
using RazzleServer.Game.Maple.Items;
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
            End = DateTime.UtcNow.AddSeconds(skill.BuffTime);
        }

        public Buff(CharacterBuffs parent, Item item)
        {
            Parent = parent;
            MapleId = item.MapleId;
            SkillLevel = 0;
            Type = BuffType.Item;
            Flag = 0;
            End = DateTime.UtcNow.AddSeconds(item.CBuffTime);

            if (item.Accuracy > 0)
            {
                Flag |= BuffValueTypes.Accuracy;
            }

            if (item.Avoidability > 0)
            {
                Flag |= BuffValueTypes.Avoidability;
            }

            if (item.Speed > 0)
            {
                Flag |= BuffValueTypes.Speed;
            }

            if (item.MagicAttack > 0)
            {
                Flag |= BuffValueTypes.MagicAttack;
            }

            if (item.WeaponAttack > 0)
            {
                Flag |= BuffValueTypes.WeaponAttack;
            }

            if (item.WeaponDefense > 0)
            {
                Flag |= BuffValueTypes.WeaponDefense;
            }

            if (item.Thaw > 0)
            {
                Flag |= BuffValueTypes.Thaw;
            }

//            if (data.Accuracy > 0)
//                added |= ps.BuffAccurancy.Set(value, data.Accuracy, expireTime);
//
//            if (data.Avoidance > 0)
//                added |= ps.BuffAvoidability.Set(value, data.Avoidance, expireTime);
//
//            if (data.Speed > 0)
//                added |= ps.BuffSpeed.Set(value, data.Speed, expireTime);
//
//            if (data.MagicAttack > 0)
//                added |= ps.BuffMagicAttack.Set(value, data.MagicAttack, expireTime);
//
//            if (data.WeaponAttack > 0)
//                added |= ps.BuffWeaponAttack.Set(value, data.WeaponAttack, expireTime);
//
//            if (data.WeaponDefense > 0)
//                added |= ps.BuffWeaponDefense.Set(value, data.WeaponDefense, expireTime);
//
//            if (data.Thaw > 0)
//                added |= ps.BuffThaw.Set(value, data.Thaw, expireTime);
        }

        public Buff(CharacterBuffs parent, BuffEntity entity)
        {
            Parent = parent;
            MapleId = entity.SkillId;
            SkillLevel = entity.SkillLevel;
            Type = (BuffType)entity.Type;
            Flag = (BuffValueTypes)entity.Value;
            End = entity.End;
        }

        public void ScheduleExpiration()
        {
            Delay.Execute(() =>
            {
                if (Parent.Contains(this))
                {
                    Parent.Remove(this);
                }
            }, (int)(End - DateTime.UtcNow).TotalMilliseconds);
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
            using (var pw = new PacketWriter(ServerOperationCode.SkillsGiveBuff))
            {
                //Character.Client.Send(pw);
            }

            using (var pw = new PacketWriter(ServerOperationCode.RemotePlayerSkillBuff))
            {
                //Character.Map.Send(pw);
            }

            ScheduleExpiration();
        }

        public void Cancel()
        {
            if (MapleId == (int)SkillNames.Gm.Hide)
            {
                Character.Hide(false);
            }

            using (var pw = new PacketWriter(ServerOperationCode.SkillsGiveBuff))
            {
                //Character.Client.Send(pw);
            }

            using (var pw = new PacketWriter(ServerOperationCode.RemotePlayerSkillBuff))
            {
                //Character.Map.Send(pw);
            }
        }
    }
}
