using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Buffs;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Skills;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterBuffs : IEnumerable<Buff>
    {
        public Character Parent { get; }

        private List<Buff> Buffs { get; }

        public byte ComboCount { get; set; }


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
            using (var pw = new PacketWriter())
            {
                return pw.ToArray();
            }
        }

        public bool HasGmHide() => Parent.PrimaryStats.HasBuff((int)SkillNames.Gm.Hide);

        public void AddItemBuff(int itemid)
        {
            var data = DataProvider.Items.Data[itemid];
            long buffTime = data.CBuffTime;

            var expireTime = BuffStat.GetTimeForBuff(buffTime);
            var ps = Parent.PrimaryStats;
            var value = -itemid;
            BuffValueTypes added = 0;

            if (data.Accuracy > 0)
                added |= ps.BuffAccurancy.Set(value, data.Accuracy, expireTime);

            if (data.Avoidability > 0)
                added |= ps.BuffAvoidability.Set(value, data.Avoidability, expireTime);

            if (data.Speed > 0)
                added |= ps.BuffSpeed.Set(value, data.Speed, expireTime);

            if (data.MagicAttack > 0)
                added |= ps.BuffMagicAttack.Set(value, data.MagicAttack, expireTime);

            if (data.WeaponAttack > 0)
                added |= ps.BuffWeaponAttack.Set(value, data.WeaponAttack, expireTime);

            if (data.WeaponDefense > 0)
                added |= ps.BuffWeaponDefense.Set(value, data.WeaponDefense, expireTime);

            if (data.Thaw > 0)
                added |= ps.BuffThaw.Set(value, data.Thaw, expireTime);

            if (added != 0)
            {
                FinalizeBuff(added, 0);
            }

            BuffValueTypes removed = 0;

            if (data.Cures.HasFlag(CureFlags.Weakness))
                removed |= ps.BuffWeakness.Reset();

            if (data.Cures.HasFlag(CureFlags.Poison))
                removed |= ps.BuffPoison.Reset();

            if (data.Cures.HasFlag(CureFlags.Curse))
                removed |= ps.BuffCurse.Reset();

            if (data.Cures.HasFlag(CureFlags.Darkness))
                removed |= ps.BuffDarkness.Reset();

            if (data.Cures.HasFlag(CureFlags.Seal))
                removed |= ps.BuffSeal.Reset();

            FinalizeDebuff(removed);
        }

        public void Dispell()
        {
            var ps = Parent.PrimaryStats;
            BuffValueTypes removed = 0;

            removed |= ps.BuffWeakness.Reset();
            removed |= ps.BuffPoison.Reset();
            removed |= ps.BuffCurse.Reset();
            removed |= ps.BuffDarkness.Reset();
            removed |= ps.BuffSeal.Reset();
            removed |= ps.BuffStun.Reset();

            FinalizeDebuff(removed);
        }

        public void CancelHyperBody()
        {
            var primaryStats = Parent.PrimaryStats;
            primaryStats.BuffBonuses.MaxHealth = 0;
            primaryStats.BuffBonuses.MaxMana = 0;


            if (primaryStats.Health > primaryStats.BaseMaxHealth)
            {
                Parent.PrimaryStats.Health = Parent.PrimaryStats.BaseMaxHealth;
            }

            if (primaryStats.Mana > primaryStats.BaseMaxMana)
            {
                Parent.PrimaryStats.Mana = Parent.PrimaryStats.BaseMaxMana;
            }

            Parent.PrimaryStats.MaxHealth = Parent.PrimaryStats.BaseMaxHealth;
            Parent.PrimaryStats.MaxMana = Parent.PrimaryStats.BaseMaxMana;
        }


        public void AddBuff(int skillId, short delay = 0)
        {
            AddBuff(skillId, Parent.Skills[skillId].CurrentLevel, delay);
        }

        public void AddBuff(int skillId, byte level, short delay = 0)
        {
            if (!DataProvider.Buffs.Data.TryGetValue(skillId, out var flags))
            {
                return;
            }

            if (level == 0xFF)
            {
                level = Parent.Skills.GetCurrentLevel(skillId);
            }

            var data = DataProvider.Skills.Data[skillId][level];
            long time = data.BuffTime * 1000;
            time += delay;

            // Fix for MesoGuard expiring... hurr
            if (skillId == (int)SkillNames.ChiefBandit.MesoGuard)
            {
                time += 1000 * 1000;
            }

            var expireTime = BuffStat.GetTimeForBuff(time);
            var ps = Parent.PrimaryStats;
            BuffValueTypes added = 0;

            if (flags.HasFlag(BuffValueTypes.WeaponAttack))
                added |= ps.BuffWeaponAttack.Set(skillId, data.WeaponAttack, expireTime);
            if (flags.HasFlag(BuffValueTypes.WeaponDefense))
                added |= ps.BuffWeaponDefense.Set(skillId, data.WeaponDefense, expireTime);
            if (flags.HasFlag(BuffValueTypes.MagicAttack))
                added |= ps.BuffMagicAttack.Set(skillId, data.MagicAttack, expireTime);
            if (flags.HasFlag(BuffValueTypes.MagicDefense))
                added |= ps.BuffMagicDefense.Set(skillId, data.MagicDefense, expireTime);
            if (flags.HasFlag(BuffValueTypes.Accuracy))
                added |= ps.BuffAccurancy.Set(skillId, data.Accuracy, expireTime);
            if (flags.HasFlag(BuffValueTypes.Avoidability))
                added |= ps.BuffAvoidability.Set(skillId, data.Avoidability, expireTime);
            if (flags.HasFlag(BuffValueTypes.Speed)) added |= ps.BuffSpeed.Set(skillId, data.Speed, expireTime);
            if (flags.HasFlag(BuffValueTypes.Jump)) added |= ps.BuffJump.Set(skillId, data.Jump, expireTime);
            if (flags.HasFlag(BuffValueTypes.MagicGuard))
                added |= ps.BuffMagicGuard.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.DarkSight))
                added |= ps.BuffDarkSight.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.Booster))
                added |= ps.BuffBooster.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.PowerGuard))
                added |= ps.BuffPowerGuard.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.MaxHealth))
                added |= ps.BuffMaxHealth.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.MaxMana))
                added |= ps.BuffMaxMana.Set(skillId, data.ParameterB, expireTime);
            if (flags.HasFlag(BuffValueTypes.Invincible))
                added |= ps.BuffInvincible.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.SoulArrow))
                added |= ps.BuffSoulArrow.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.ComboAttack))
                added |= ps.BuffComboAttack.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.Charges))
                added |= ps.BuffCharges.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.DragonBlood))
                added |= ps.BuffDragonBlood.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.HolySymbol))
                added |= ps.BuffHolySymbol.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.MesoUp)) added |= ps.BuffMesoUp.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.ShadowPartner))
                added |= ps.BuffShadowPartner.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.PickPocketMesoUp))
                added |= ps.BuffPickPocketMesoUp.Set(skillId, data.ParameterA, expireTime);
            if (flags.HasFlag(BuffValueTypes.MesoGuard))
            {
                added |= ps.BuffMesoGuard.Set(skillId, data.ParameterA, expireTime);
                ps.BuffMesoGuard.MesosLeft = data.CostMeso;
            }

            FinalizeBuff(added, delay);
        }

        public void FinalizeBuff(BuffValueTypes added, short delay, bool sendPacket = true)
        {
            if (added == 0)
            {
                return;
            }

            if (!sendPacket)
            {
                return;
            }

            //BuffPacket.SetTempStats(Character, added, delay);
            //MapPacket.SendPlayerBuffed(Character, added, delay);
        }

        public void FinalizeDebuff(BuffValueTypes removed, bool sendPacket = true)
        {
            if (removed == 0)
            {
                return;
            }

            if (!sendPacket)
            {
                return;
            }

            //BuffPacket.ResetTempStats(Character, removed);
            //MapPacket.SendPlayerDebuffed(Character, removed);
        }
    }
}
