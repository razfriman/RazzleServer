using RazzleServer.Data.WZ;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RazzleServer.Player
{
    public class BuffedCharacterStats
    {
        public short MaxHp, MaxMp;
        public short Str, Dex, Int, Luk;
        public int MinDamage, MaxDamage;
        public int AsrR;
        public int RecoveryHp, RecoveryMp;
        public int RecoveryHpR, RecoveryMpR;
        public int RecoveryTime;
        public int LifeStealR, LifeStealProp;
        public int MpEaterR, MpEaterProp;
        public int DoTTimeInc;
        public int CostMpR;
        public int ExpR, MesoR, DropR;
        public int PickPocketR;
        public int StunR;
        public int AmmoInc;
        public int MasteryR;
        public int BuffTimeR;
        public int PotionEffectR;
        public int ElixirEffectR;
        public int CritRate;
        public int ExpLossReductionR;

        public void Recalculate(MapleCharacter chr)
        {
            
        }
        //    int mhpX = 0, mmpX = 0;
        //    int mhpR = 0, mmpR = 0;
        //    int lv2mhp = 0, lv2mmp = 0;
        //    int strR = 0, dexR = 0, intR = 0, lukR = 0;
        //    int watk = 0, matk = 9;
        //    int damR = 100, pdR = 0;

        //    Str = chr.Str + 5;
        //    Dex = chr.Dex + 5;
        //    Int = chr.Int + 5;
        //    Luk = chr.Luk + 5;
        //    MinDamage = 0; MaxDamage = 0;
        //    AsrR = 0;
        //    RecoveryHp = 0; RecoveryMp = 0;
        //    RecoveryHpR = 0; RecoveryMpR = 0;
        //    RecoveryTime = 0;
        //    watk = 3; matk = 3;
        //    LifeStealR = 0; LifeStealProp = 0;
        //    MpEaterR = 0; MpEaterProp = 0;
        //    DoTTimeInc = 0;
        //    CostMpR = 0;
        //    ExpR = 100; MesoR = 100; DropR = 100;
        //    PickPocketR = 0;
        //    StunR = 0;
        //    AmmoInc = 0;
        //    MasteryR = 0;
        //    BuffTimeR = 100;
        //    PotionEffectR = 100;
        //    ElixirEffectR = 100;
        //    CritRate = 5;
        //    ExpLossReductionR = 0;

        //    #region PassiveSkills 
        //    IEnumerable<Skill> passiveSkills = chr.GetSkillList().Where(x => x.SkillId % 10000 < 1000);
        //    foreach (Skill skill in passiveSkills)
        //    {
        //        if (skill.Level < 1)
        //            continue;
        //        WzCharacterSkill skillInfo = DataBuffer.GetCharacterSkillById(skill.SkillID);
        //        if (skillInfo == null)
        //            continue;
        //        SkillEffect effect = skillInfo.GetEffect(skill.Level);
        //        if (effect == null)
        //            continue;
        //        foreach (var kvp in effect.Info)
        //        {
        //            switch (kvp.Key)
        //            {
        //                case CharacterSkillStat.mhpX:
        //                    mhpX += kvp.Value; break;
        //                case CharacterSkillStat.mmpX:
        //                    mmpX += kvp.Value; break;
        //                case CharacterSkillStat.mhpR:
        //                    mhpR += kvp.Value; break;
        //                case CharacterSkillStat.mmpR:
        //                    mmpR += kvp.Value; break;
        //                case CharacterSkillStat.lv2mhp:
        //                    lv2mhp += kvp.Value; break;
        //                case CharacterSkillStat.lv2mmp:
        //                    lv2mmp += kvp.Value; break;
        //                case CharacterSkillStat.strX:
        //                    Str += kvp.Value; break;
        //                case CharacterSkillStat.dexX:
        //                    Dex += kvp.Value; break;
        //                case CharacterSkillStat.intX:
        //                    Int += kvp.Value; break;
        //                case CharacterSkillStat.lukX:
        //                    Luk += kvp.Value; break;
        //                case CharacterSkillStat.asrR:
        //                    AsrR += kvp.Value; break;
        //                case CharacterSkillStat.mastery:
        //                    MasteryR = Math.Max(MasteryR, kvp.Value);
        //                    break;
        //                case CharacterSkillStat.costmpR:
        //                    CostMpR += kvp.Value; break;
        //                case CharacterSkillStat.bufftimeR:
        //                    BuffTimeR += kvp.Value; break;
        //                case CharacterSkillStat.padX:
        //                    watk += kvp.Value; break;
        //                case CharacterSkillStat.madX:
        //                    matk += kvp.Value; break;
        //                case CharacterSkillStat.pdR:
        //                    pdR += kvp.Value; break;
        //                case CharacterSkillStat.damR:
        //                    damR += kvp.Value; break;
        //                case CharacterSkillStat.cr:
        //                    CritRate += kvp.Value; break;
        //            }
        //        }
        //    }
        //    #region Specific passive skill handling
        //    byte skillLevel;
        //    if (chr.IsWarrior)
        //    {
        //        if ((skillLevel = chr.GetSkillLevel(Swordman.IRON_BODY)) > 0)
        //            mhpR += DataBuffer.CharacterSkillBuffer[Swordman.IRON_BODY].GetEffect(skillLevel).Info[CharacterSkillStat.mhpR];

        //        if (chr.IsFighter)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(Crusader.SELF_RECOVERY)) > 0)
        //            {
        //                var info = DataBuffer.CharacterSkillBuffer[Crusader.SELF_RECOVERY].GetEffect(skillLevel).Info;
        //                RecoveryHp += info[CharacterSkillStat.hp];
        //                RecoveryMp += info[CharacterSkillStat.mp];
        //            }
        //        }
        //        else if (chr.IsSpearman)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(Berserker.LORD_OF_DARKNESS)) > 0)
        //            {
        //                var info = DataBuffer.CharacterSkillBuffer[Berserker.LORD_OF_DARKNESS].GetEffect(skillLevel).Info;
        //                LifeStealProp += info[CharacterSkillStat.prop];
        //                LifeStealR += info[CharacterSkillStat.x];
        //            }
        //        }
        //    }
        //    else if (chr.IsMagician)
        //    {
        //        if (chr.IsFirePoisonMage)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(FirePoison2.SPELL_MASTERY)) > 0)
        //                matk += DataBuffer.CharacterSkillBuffer[FirePoison2.SPELL_MASTERY].GetEffect(skillLevel).Info[CharacterSkillStat.x];
        //            if ((skillLevel = chr.GetSkillLevel(FirePoison2.MP_EATER)) > 0)
        //            {
        //                var info = DataBuffer.CharacterSkillBuffer[FirePoison2.MP_EATER].GetEffect(skillLevel).Info;
        //                MpEaterProp += info[CharacterSkillStat.prop];
        //                MpEaterR += info[CharacterSkillStat.x];
        //            }
        //        }
        //        if (chr.IsIceLightningMage)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(IceLightning2.MP_EATER)) > 0)
        //            {
        //                var info = DataBuffer.CharacterSkillBuffer[IceLightning2.MP_EATER].GetEffect(skillLevel).Info;
        //                MpEaterProp += info[CharacterSkillStat.prop];
        //                MpEaterR += info[CharacterSkillStat.x];
        //            }
        //            if ((skillLevel = chr.GetSkillLevel(IceLightning2.SPELL_MASTERY)) > 0)
        //                matk += DataBuffer.CharacterSkillBuffer[IceLightning2.SPELL_MASTERY].GetEffect(skillLevel).Info[CharacterSkillStat.x];
        //        }
        //        if (chr.IsCleric)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(Cleric.MP_EATER)) > 0)
        //            {
        //                var info = DataBuffer.CharacterSkillBuffer[Cleric.MP_EATER].GetEffect(skillLevel).Info;
        //                MpEaterProp += info[CharacterSkillStat.prop];
        //                MpEaterR += info[CharacterSkillStat.x];
        //            }
        //            if ((skillLevel = chr.GetSkillLevel(Cleric.SPELL_MASTERY)) > 0)
        //                matk += DataBuffer.CharacterSkillBuffer[Cleric.SPELL_MASTERY].GetEffect(skillLevel).Info[CharacterSkillStat.x];
        //            if ((skillLevel = chr.GetSkillLevel(Priest.DIVINE_PROTECTION)) > 0)
        //                AsrR += DataBuffer.CharacterSkillBuffer[Priest.DIVINE_PROTECTION].GetEffect(skillLevel).Info[CharacterSkillStat.asrR];
        //        }
        //    }
        //    else if (chr.IsArcher)
        //    {
        //        if (chr.IsHunter)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(Bowmaster.BOW_EXPERT)) > 0)
        //                watk += DataBuffer.CharacterSkillBuffer[Bowmaster.BOW_EXPERT].GetEffect(skillLevel).Info[CharacterSkillStat.x];
        //        }
        //        else if (chr.IsCrossbowman)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(Marksman.CROSSBOW_EXPERT)) > 0)
        //                watk += DataBuffer.CharacterSkillBuffer[Marksman.CROSSBOW_EXPERT].GetEffect(skillLevel).Info[CharacterSkillStat.x];
        //        }
        //    }
        //    else if (chr.IsThief)
        //    {
        //        if (chr.IsAssassin)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(Assassin.CLAW_MASTERY)) > 0)
        //                AmmoInc += DataBuffer.CharacterSkillBuffer[Assassin.CLAW_MASTERY].GetEffect(skillLevel).Info[CharacterSkillStat.y];
        //            if ((skillLevel = chr.GetSkillLevel(Hermit.ALCHEMIC_ADRENALINE)) > 0)
        //                PotionEffectR += DataBuffer.CharacterSkillBuffer[Hermit.ALCHEMIC_ADRENALINE].GetEffect(skillLevel).Info[CharacterSkillStat.x] - 100;
        //            if ((skillLevel = chr.GetSkillLevel(NightLord.CLAW_EXPERT)) > 0)
        //                watk += DataBuffer.CharacterSkillBuffer[NightLord.CLAW_EXPERT].GetEffect(skillLevel).Info[CharacterSkillStat.x];
        //        }
        //        else if (chr.IsBandit)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(Bandit.SHIELD_MASTERY)) > 0)
        //                watk += DataBuffer.CharacterSkillBuffer[Bandit.SHIELD_MASTERY].GetEffect(skillLevel).Info[CharacterSkillStat.y];
        //            if ((skillLevel = chr.GetSkillLevel(ChiefBandit.MESO_MASTERY)) > 0)
        //            {
        //                var info = DataBuffer.CharacterSkillBuffer[ChiefBandit.MESO_MASTERY].GetEffect(skillLevel).Info;
        //                MesoR += info[CharacterSkillStat.mesoR];
        //                PickPocketR += info[CharacterSkillStat.u];
        //            }
        //            if ((skillLevel = chr.GetSkillLevel(Shadower.DAGGER_EXPERT)) > 0)
        //                watk += DataBuffer.CharacterSkillBuffer[Shadower.DAGGER_EXPERT].GetEffect(skillLevel).Info[CharacterSkillStat.x];
        //        }
        //    }
        //    else if (chr.IsDualBlade)
        //    {
        //        if ((skillLevel = chr.GetSkillLevel(DualBlade3p.LIFE_DRAIN)) > 0)
        //        {
        //            var info = DataBuffer.CharacterSkillBuffer[DualBlade3p.LIFE_DRAIN].GetEffect(skillLevel).Info;
        //            LifeStealR += info[CharacterSkillStat.x];
        //            LifeStealProp += info[CharacterSkillStat.prop];
        //        }
        //        if ((skillLevel = chr.GetSkillLevel(DualBlade4.KATARA_EXPERT)) > 0)
        //            watk += DataBuffer.CharacterSkillBuffer[DualBlade4.KATARA_EXPERT].GetEffect(skillLevel).Info[CharacterSkillStat.x];
        //    }
        //    else if (chr.IsPirate)
        //    {
        //        if (chr.IsBrawler)
        //        {
        //            if ((skillLevel = chr.GetSkillLevel(Brawler.PERSEVERANCE)) > 0)
        //            {
        //                var info = DataBuffer.CharacterSkillBuffer[Brawler.PERSEVERANCE].GetEffect(skillLevel).Info;
        //                int x = info[CharacterSkillStat.x];
        //                RecoveryHpR += x;
        //                RecoveryMpR += x;
        //                RecoveryTime = info[CharacterSkillStat.y];
        //            }
        //            if ((skillLevel = chr.GetSkillLevel(Marauder.STUN_MASTERY)) > 0)
        //                StunR += DataBuffer.CharacterSkillBuffer[Marauder.STUN_MASTERY].GetEffect(skillLevel).Info[CharacterSkillStat.subProp];
        //        }
        //        else if (chr.IsGunslinger)
        //        {
        //            if ((skillLevel = skillLevel = chr.GetSkillLevel(Gunslinger.GUN_MASTERY)) > 0)
        //                AmmoInc += DataBuffer.CharacterSkillBuffer[Gunslinger.GUN_MASTERY].GetEffect(skillLevel).Info[CharacterSkillStat.y];
        //        }
        //    }
        //    else if (chr.IsCannonneer)
        //    {
        //        if ((skillLevel = chr.GetSkillLevel(Cannoneer3.BARREL_ROULETTE)) > 0)
        //            damR += DataBuffer.CharacterSkillBuffer[Cannoneer3.BARREL_ROULETTE].GetEffect(skillLevel).Info[CharacterSkillStat.damR];
        //    }
        //    #endregion
        //    #endregion

        //    #region Buffs
        //    foreach (Buff buff in chr.GetBuffs())
        //    {
        //        var buffInfo = buff.Effect.BuffInfo;
        //        foreach (var pair in buffInfo)
        //        {
        //            //if (pair.Key == MapleBuffStat.ENHANCED_MAXHP || pair.Key == MapleBuffStat.STACKING_MAXHP)
        //            //mhpX += pair.Value;
        //            if (pair.Key == MapleBuffStat.MAXHP_R || pair.Key == MapleBuffStat.STACKING_MAXHP_R)
        //                mhpR += pair.Value;
        //            //else if (pair.Key == MapleBuffStat.ENHANCED_MAXMP || pair.Key == MapleBuffStat.STACKING_MAXMP)
        //            //mmpX += pair.Value;
        //            else if (pair.Key == MapleBuffStat.MAXMP_R || pair.Key == MapleBuffStat.STACKING_MAXMP_R)
        //                mmpR += pair.Value;
        //            //else if (pair.Key == MapleBuffStat.WATK || pair.Key == MapleBuffStat.ENHANCED_WATK || pair.Key == MapleBuffStat.STACKING_WATK)
        //            //watk += pair.Value;
        //            //else if (pair.Key == MapleBuffStat.MATK || pair.Key == MapleBuffStat.ENHANCED_MATK || pair.Key == MapleBuffStat.STACKING_MATK)
        //            //matk += pair.Value;
        //            //else if (pair.Key == MapleBuffStat.CRIT || pair.Key == MapleBuffStat.STACKING_CRIT)
        //            //CritRate += pair.Value;
        //            else if (pair.Key == MapleBuffStat.STACKING_STATS)
        //            {
        //                Str += pair.Value;
        //                Dex += pair.Value;
        //                Int += pair.Value;
        //                Luk += pair.Value;
        //            }
        //            else if (pair.Key == MapleBuffStat.STACKING_STATS_R)
        //            {
        //                Str += (int)(chr.Str * (pair.Value / 100.0)); //todo: check if this isnt math.ceil
        //                Dex += (int)(chr.Dex * (pair.Value / 100.0));
        //                Int += (int)(chr.Int * (pair.Value / 100.0));
        //                Luk += (int)(chr.Luk * (pair.Value / 100.0));
        //            }
        //            //else if (pair.Key == MapleBuffStat.HOLY_SYMBOL)
        //            {
        //                // ExpR += pair.Value;
        //            }
        //        }
        //    }
        //    #endregion

        //    #region Equips
        //    foreach (MapleItem item in chr.Inventory.GetItemsFromInventory(MapleInventoryType.Equipped))
        //    {
        //        MapleEquip equip = item as MapleEquip;
        //        if (equip == null)
        //            continue;
        //        mhpX += equip.IncMhp;
        //        mmpX += equip.IncMmp;
        //        Str += equip.Str;
        //        Dex += equip.Dex;
        //        Int += equip.Int;
        //        Luk += equip.Luk;
        //        watk += equip.Pad;
        //        matk += equip.Mad;
        //        //todo: potential stuff from here
        //    }
        //    #endregion

        //    MaxHp = chr.MaxHP;
        //    MaxHp += lv2mhp * chr.Level;
        //    MaxHp += (int)((MaxHp) * (mhpR / 100.0));
        //    MaxHp += mhpX;
        //    if (chr.HP > MaxHp)
        //        chr.AddHP(-(chr.HP - MaxHp));

        //    MaxMp = chr.MaxMP;
        //    MaxMp += (int)(chr.MaxMP * (double)(mmpR / 100.0));
        //    MaxMp += lv2mmp * chr.Level;
        //    MaxMp += mhpX;
        //    if (chr.MP > MaxMp)
        //        chr.AddMP(-(chr.MP - MaxMp));

        //    Str += (short)(chr.Str * (double)(strR / 100.0));
        //    Dex += (short)(chr.Dex * (double)(dexR / 100.0));
        //    Int += (short)(chr.Int * (double)(intR / 100.0));
        //    Luk += (short)(chr.Luk * (double)(lukR / 100.0));

        //    bool mage = false;
        //    int primaryStat = 0;
        //    int secondaryStat = 0;
        //    MapleItem weapon = chr.Inventory.GetEquippedItem((short)MapleEquipPosition.Weapon);
        //    if (weapon == null)
        //    {
        //        MinDamage = 1;
        //        MaxDamage = 1;
        //        return;
        //    }
        //    MapleItemType weaponItemType = ItemConstants.GetMapleItemType(weapon.ItemId);
        //    switch ((chr.Job % 1000) / 100)
        //    {
        //        case 1: //Warrior-type
        //            primaryStat = Str;
        //            secondaryStat = Dex;
        //            break;
        //        case 2: //Magician-type
        //        case 3: //Archer-type
        //            primaryStat = Dex;
        //            secondaryStat = Str;
        //            break;
        //        case 4: //Thief-type
        //            primaryStat = Luk;
        //            secondaryStat = Dex;
        //            break;
        //        case 5: //Pirate-type                    
        //            if (weaponItemType == MapleItemType.Gun || weaponItemType == MapleItemType.SoulShooter)
        //            {
        //                primaryStat = Dex;
        //                secondaryStat = Str;
        //            }
        //            else //counts for cannons too
        //            {
        //                primaryStat = Str;
        //                secondaryStat = Dex;
        //            }
        //            break;
        //        case 6: //Xenon
        //            primaryStat = (Str + Dex + Luk);
        //            break;
        //    }
        //    if (!mage)
        //        damR += pdR; //TODO: check, Not sure about this
        //    CalculateDamageRange(weaponItemType, primaryStat, secondaryStat, mage ? matk : watk, damR, chr.IsFighter);
        //}

        //private void CalculateDamageRange(MapleItemType itemType, int primaryStat, int secondaryStat, int atk, int damR, bool isFighter)
        //{
        //    WeaponInfo weaponInfo = ItemConstants.GetWeaponModifierInfo(itemType);
        //    double damageModifier = weaponInfo.DamageModifier;
        //    if (isFighter)
        //        damageModifier += 0.10;
        //    int baseMaxDamage = (int)Math.Ceiling(damageModifier * (4 * primaryStat + secondaryStat) * (atk / 100.0f));

        //    MaxDamage = (int)((damR / 100.0) * baseMaxDamage);
        //    MinDamage = (int)(((weaponInfo.BaseMastery + MasteryR) / 100.0) * MaxDamage);
        //}

        public bool IsCrit(int damage, double skillDamage)
        {
            double maxDamage = damage * skillDamage;
            if (maxDamage > this.MaxDamage * 1.2)
                return true;
            return false;
        }

        public int GetDamage(bool crit = false)
        {
            return Functions.Random(MinDamage, MaxDamage);
        }
    }
}
