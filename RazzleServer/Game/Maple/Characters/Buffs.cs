//        public void EncodeForRemote(ref BuffValueTypes flag, long currentTime, Action<BuffStat> func, BuffValueTypes specificFlag = BuffValueTypes.All)
//        {
//            if (!IsSet(currentTime) || !specificFlag.HasFlag(Flag))
//            {
//                return;
//            }
//
//            flag |= Flag;
//            func?.Invoke(this);
//        }
//
//        public void EncodeForLocal(PacketWriter pw, ref BuffValueTypes flag, long currentTime, BuffValueTypes specificFlag = BuffValueTypes.All)
//        {
//            if (!IsSet(currentTime) || !specificFlag.HasFlag(Flag))
//            {
//                return;
//            }
//
//            flag |= Flag;
//            pw.WriteShort(N);
//            pw.WriteInt(R);
//            pw.WriteShort((short)((TM - currentTime) / 100)); // If its not divided, it will not flash.
//        }
//
//        public virtual bool EncodeForCC(PacketWriter pr, ref BuffValueTypes flag, long currentTime)
//        {
//            if (!IsSet(currentTime))
//            {
//                return false;
//            }
//
//            flag |= Flag;
//            pr.WriteShort(N);
//            pr.WriteInt(R);
//            pr.WriteLong(TM);
//            return true;
//        }
//
//        public virtual bool DecodeForCC(PacketReader pr, BuffValueTypes flag)
//        {
//            if (!flag.HasFlag(Flag))
//            {
//                Reset();
//                return false;
//            }
//            else
//            {
//                N = pr.ReadShort();
//                R = pr.ReadInt();
//                TM = pr.ReadLong();
//                return true;
//            }
//        }
//    }
//
//    public class BuffStat_DragonBlood : BuffStat
//    {
//        private readonly Character Owner;
//        private long tLastDamaged;
//
//        public BuffStat_DragonBlood(BuffValueTypes flag, Character own) : base(flag)
//        {
//            Owner = own;
//        }
//
//        public override BuffValueTypes Set(int referenceId, short nValue, long expireTime)
//        {
//            tLastDamaged = CurrentTime;
//            return base.Set(referenceId, nValue, expireTime);
//        }
//
//        public override bool TryReset(long currentTime, ref BuffValueTypes flag)
//        {
//            if (CurrentTime - tLastDamaged >= 4000)
//            {
//                Owner.Health -= N;
//                tLastDamaged = CurrentTime;
//            }
//            return base.TryReset(currentTime, ref flag);
//        }
//
//        public override bool EncodeForCC(PacketWriter pr, ref BuffValueTypes flag, long currentTime)
//        {
//            if (base.EncodeForCC(pr, ref flag, currentTime))
//            {
//                pr.WriteLong(tLastDamaged);
//                return true;
//            }
//            return false;
//        }
//
//        public override bool DecodeForCC(PacketReader pr, BuffValueTypes flag)
//        {
//            if(base.DecodeForCC(pr, flag))
//            {
//                tLastDamaged = pr.ReadLong();
//                return true;
//            }
//            return false;
//        }
//    }
//
//    public class BuffStat_ComboAttack : BuffStat
//    {
//        public int MaxOrbs { get; set; }
//
//        public BuffStat_ComboAttack(BuffValueTypes flag) : base(flag)
//        {
//        }
//
//        public override BuffValueTypes Set(int referenceId, short nValue, long expireTime)
//        {
//            MaxOrbs = nValue;
//            return base.Set(referenceId, 1, expireTime);
//        }
//    }
//
//    public class BuffStat_MesoGuard : BuffStat
//    {
//        public int MesosLeft { get; set; }
//
//        public BuffStat_MesoGuard(BuffValueTypes flag) : base(flag)
//        {
//        }
//    }
//
//    public class CharacterPrimaryStats
//    {
//        private Character Char { get; }
//
//        public byte Level
//        {
//            get => Char.Level;
//            set => Char.Level = value;
//        }
//        public short Job
//        {
//            get => (short)Char.Job;
//            set => Char.Job = (Job)value;
//        }
//        public short Str { get; set; }
//        public short Dex { get; set; }
//        public short Int { get; set; }
//        public short Luk { get; set; }
//        public short MaxHP { get; set; }
//        public short MP { get; set; }
//        public short MaxMP { get; set; }
//        public short AP { get; set; }
//        public short SP { get; set; }
//        public int EXP { get; set; }
//        public short Fame { get; set; }
//
//        public float speedMod => TotalSpeed + 100.0f;
//
//        public short MAD => Int;
//        public short MDD => Int;
//        public int EVA
//        {
//            get
//            {
//                int eva = Luk / 2 + Dex / 4;
//
//                var buff = Char.Skills.GetSkillLevelData(4000000, out byte lvl2);
//                if (buff != null)
//                {
//                    eva += buff.YValue;
//                }
//
//                return eva;
//            }
//        }
//
//        public int ACC
//        {
//            get
//            {
//                int acc = 0;
//
//                if (Job / 100 == 3 || Job / 100 == 4)
//                {
//                    acc = (int)((Luk * 0.3) + (Dex * 0.6));
//                }
//                else
//                {
//                    acc = (int)((Luk * 0.5) + (Dex * 0.8));
//                }
//
//                var buff = Char.Skills.GetSkillLevelData(Constants.Archer.Skills.BlessingOfAmazon, out byte lvl1);
//                if (buff != null)
//                {
//                    acc += buff.XValue;
//                }
//
//                buff = Char.Skills.GetSkillLevelData(Constants.Rogue.Skills.NimbleBody, out byte lvl2);
//                if (buff != null)
//                {
//                    acc += buff.XValue;
//                }
//
//                // TODO: Weapon mastery buff
//                /*
//                buff = Char.Skills.GetSkillLevelData(Char.Skills.GetMastery(), out byte lvl3);
//                if (buff != null)
//                {
//                    acc += buff.Accurancy;
//                }
//                */
//
//                return Math.Max(0, Math.Min(acc, 999));
//            }
//        }
//
//        public int Craft => Dex + Luk + Int;
//

//
//        private Dictionary<byte, EquipBonus> EquipStats { get; } = new Dictionary<byte, EquipBonus>();
//        public BonusSet EquipBonuses = new BonusSet();
//        public BonusSet BuffBonuses = new BonusSet();
//
//      
//
//
//        // Real Stats
//
//        public BuffStat BuffWeaponAttack { get; } = new BuffStat(BuffValueTypes.WeaponAttack);
//        public BuffStat BuffWeaponDefense { get; } = new BuffStat(BuffValueTypes.WeaponDefense);
//        public BuffStat BuffMagicAttack { get; } = new BuffStat(BuffValueTypes.MagicAttack);
//        public BuffStat BuffMagicDefense { get; } = new BuffStat(BuffValueTypes.MagicDefense);
//        public BuffStat BuffAccurancy { get; } = new BuffStat(BuffValueTypes.Accurancy);
//        public BuffStat BuffAvoidability { get; } = new BuffStat(BuffValueTypes.Avoidability);
//        public BuffStat BuffHands { get; } = new BuffStat(BuffValueTypes.Hands);
//        public BuffStat BuffSpeed { get; } = new BuffStat(BuffValueTypes.Speed);
//        public BuffStat BuffJump { get; } = new BuffStat(BuffValueTypes.Jump);
//        public BuffStat BuffMagicGuard { get; } = new BuffStat(BuffValueTypes.MagicGuard);
//        public BuffStat BuffDarkSight { get; } = new BuffStat(BuffValueTypes.DarkSight);
//        public BuffStat BuffBooster { get; } = new BuffStat(BuffValueTypes.Booster);
//        public BuffStat BuffPowerGuard { get; } = new BuffStat(BuffValueTypes.PowerGuard);
//        public BuffStat BuffMaxHP { get; } = new BuffStat(BuffValueTypes.MaxHp);
//        public BuffStat BuffMaxMP { get; } = new BuffStat(BuffValueTypes.MaxMp);
//        public BuffStat BuffInvincible { get; } = new BuffStat(BuffValueTypes.Invincible);
//        public BuffStat BuffSoulArrow { get; } = new BuffStat(BuffValueTypes.SoulArrow);
//        public BuffStat BuffStun { get; } = new BuffStat(BuffValueTypes.Stun);
//        public BuffStat BuffPoison { get; } = new BuffStat(BuffValueTypes.Poison);
//        public BuffStat BuffSeal { get; } = new BuffStat(BuffValueTypes.Seal);
//        public BuffStat BuffDarkness { get; } = new BuffStat(BuffValueTypes.Darkness);
//        public BuffStat_ComboAttack BuffComboAttack { get; } = new BuffStat_ComboAttack(BuffValueTypes.ComboAttack);
//        public BuffStat BuffCharges { get; } = new BuffStat(BuffValueTypes.Charges);
//        public BuffStat_DragonBlood BuffDragonBlood { get; }
//        public BuffStat BuffHolySymbol { get; } = new BuffStat(BuffValueTypes.HolySymbol);
//        public BuffStat BuffMesoUP { get; } = new BuffStat(BuffValueTypes.MesoUp);
//        public BuffStat BuffShadowPartner { get; } = new BuffStat(BuffValueTypes.ShadowPartner);
//        public BuffStat BuffPickPocketMesoUP { get; } = new BuffStat(BuffValueTypes.PickPocketMesoUp);
//        public BuffStat_MesoGuard BuffMesoGuard { get; } = new BuffStat_MesoGuard(BuffValueTypes.MesoGuard);
//        public BuffStat BuffThaw { get; } = new BuffStat(BuffValueTypes.Thaw);
//        public BuffStat BuffWeakness { get; } = new BuffStat(BuffValueTypes.Weakness);
//        public BuffStat BuffCurse { get; } = new BuffStat(BuffValueTypes.Curse);
//
//
//        public CharacterPrimaryStats(Character chr)
//        {
//            Char = chr;
//            BuffDragonBlood = new BuffStat_DragonBlood(BuffValueTypes.DragonBlood, Char);
//        }
//
//        public void AddEquipStats(sbyte slot, EquipItem equip, bool isLoading)
//        {
//            try
//            {
//                byte realSlot = (byte)Math.Abs(slot);
//                if (equip != null)
//                {
//                    EquipBonus equipBonus;
//                    if (!EquipStats.TryGetValue(realSlot, out equipBonus))
//                    {
//                        equipBonus = new EquipBonus();
//                    }
//
//                    equipBonus.ID = equip.ItemID;
//                    equipBonus.MaxHP = equip.HP;
//                    equipBonus.MaxMP = equip.MP;
//                    equipBonus.Str = equip.Str;
//                    equipBonus.Int = equip.Int;
//                    equipBonus.Dex = equip.Dex;
//                    equipBonus.Luk = equip.Luk;
//                    equipBonus.Speed = equip.Speed;
//                    equipBonus.PAD = equip.Watk;
//                    equipBonus.PDD = equip.Wdef;
//                    equipBonus.MAD = equip.Matk;
//                    equipBonus.MDD = equip.Mdef;
//                    equipBonus.EVA = equip.Avo;
//                    equipBonus.ACC = equip.Acc;
//                    equipBonus.Craft = equip.Hands;
//                    equipBonus.Jump = equip.Jump;
//                    EquipStats[realSlot] = equipBonus;
//                }
//                else
//                {
//                    EquipStats.Remove(realSlot);
//                }
//                CalculateAdditions(true, isLoading);
//            }
//            catch (Exception ex)
//            {
//            }
//        }
//
//        public void CalculateAdditions(bool updateEquips, bool isLoading)
//        {
//            if (updateEquips)
//            {
//                EquipBonuses = new BonusSet();
//                foreach (var data in EquipStats)
//                {
//                    EquipBonus item = data.Value;
//                    if (EquipBonuses.Dex + item.Dex > short.MaxValue)
//                    {
//                        EquipBonuses.Dex = short.MaxValue;
//                    }
//                    else
//                    {
//                        EquipBonuses.Dex += item.Dex;
//                    }
//
//                    if (EquipBonuses.Int + item.Int > short.MaxValue)
//                    {
//                        EquipBonuses.Int = short.MaxValue;
//                    }
//                    else
//                    {
//                        EquipBonuses.Int += item.Int;
//                    }
//
//                    if (EquipBonuses.Luk + item.Luk > short.MaxValue)
//                    {
//                        EquipBonuses.Luk = short.MaxValue;
//                    }
//                    else
//                    {
//                        EquipBonuses.Luk += item.Luk;
//                    }
//
//                    if (EquipBonuses.Str + item.Str > short.MaxValue)
//                    {
//                        EquipBonuses.Str = short.MaxValue;
//                    }
//                    else
//                    {
//                        EquipBonuses.Str += item.Str;
//                    }
//
//                    if (EquipBonuses.MaxMP + item.MaxMP > short.MaxValue)
//                    {
//                        EquipBonuses.MaxMP = short.MaxValue;
//                    }
//                    else
//                    {
//                        EquipBonuses.MaxMP += item.MaxMP;
//                    }
//
//                    if (EquipBonuses.MaxHP + item.MaxHP > short.MaxValue)
//                    {
//                        EquipBonuses.MaxHP = short.MaxValue;
//                    }
//                    else
//                    {
//                        EquipBonuses.MaxHP += item.MaxHP;
//                    }
//
//                    EquipBonuses.PAD += item.PAD;
//
//                    // TODO: Shield mastery buff
//                    if (data.Key == (byte)Constants.EquipSlots.Slots.Shield)
//                    {
//
//                    }
//
//                    EquipBonuses.PDD += item.PDD;
//                    EquipBonuses.MAD += item.MAD;
//                    EquipBonuses.MDD += item.MDD;
//                    EquipBonuses.ACC += item.ACC;
//                    EquipBonuses.EVA += item.EVA;
//                    EquipBonuses.Speed += item.Speed;
//                    EquipBonuses.Jump += item.Jump;
//                    EquipBonuses.Craft += item.Craft;
//
//                    EquipBonuses.PAD = (short)Math.Max(0, Math.Min((int)EquipBonuses.PAD, 1999));
//                    EquipBonuses.PDD = (short)Math.Max(0, Math.Min((int)EquipBonuses.PDD, 1999));
//                    EquipBonuses.MAD = (short)Math.Max(0, Math.Min((int)EquipBonuses.MAD, 1999));
//                    EquipBonuses.MDD = (short)Math.Max(0, Math.Min((int)EquipBonuses.MDD, 1999));
//                    EquipBonuses.ACC = (short)Math.Max(0, Math.Min((int)EquipBonuses.ACC, 999));
//                    EquipBonuses.EVA = (short)Math.Max(0, Math.Min((int)EquipBonuses.EVA, 999));
//                    EquipBonuses.Craft = (short)Math.Max(0, Math.Min((int)EquipBonuses.Craft, 999));
//                    EquipBonuses.Speed = (short)Math.Max(100, Math.Min((int)EquipBonuses.Speed, 200));
//                    EquipBonuses.Jump = (short)Math.Max(100, Math.Min((int)EquipBonuses.Jump, 123));
//                }
//
//            }
//            if (!isLoading)
//            {
//                CheckHPMP();
//                Char.FlushDamageLog();
//            }
//        }
//
//        public void CheckHPMP()
//        {
//            short mhp = GetMaxHP(false);
//            short mmp = GetMaxMP(false);
//            if (HP > mhp)
//            {
//                Char.ModifyHP(mhp);
//            }
//            if (MP > mmp)
//            {
//                Char.ModifyMP(mmp);
//            }
//        }
//        
//        public void CheckBoosters()
//        {
//            var equippedId = Char.Inventory.GetEquippedItemId(Constants.EquipSlots.Slots.Weapon, false);
//
//            if (equippedId != 0)
//            {
//                return;
//            }
//
//            BuffValueTypes removed = 0;
//            var currentTime = BuffStat.GetTimeForBuff();
//            if (BuffBooster.IsSet(currentTime))
//            {
//                removed |= RemoveByReference(BuffBooster.R, true);
//            }
//
//            if (BuffCharges.IsSet(currentTime))
//            {
//                removed |= RemoveByReference(BuffCharges.R, true);
//            }
//
//            if (BuffComboAttack.IsSet(currentTime))
//            {
//                removed |= RemoveByReference(BuffComboAttack.R, true);
//            }
//
//            if (BuffSoulArrow.IsSet(currentTime))
//            {
//                removed |= RemoveByReference(BuffSoulArrow.R, true);
//            }
//
//            Char.Buffs.FinalizeDebuff(removed);
//        }
//
//        public short getTotalStr() { return (short)(Str + EquipBonuses.Str); }
//        public short getTotalDex() { return (short)(Dex + EquipBonuses.Dex); }
//        public short getTotalInt() { return (short)(Int + EquipBonuses.Int); }
//        public short getTotalLuk() { return (short)(Luk + EquipBonuses.Luk); }
//        public short getTotalMagicAttack() { return (short)(Int + EquipBonuses.MAD); }
//        public short getTotalMagicDef() { return (short)(Int + EquipBonuses.MDD); }
//
//        public short GetStrAddition(bool nobonus = false)
//        {
//            if (!nobonus)
//            {
//                return (short)((Str + EquipBonuses.Str + BuffBonuses.Str) > short.MaxValue ? short.MaxValue : (Str + EquipBonuses.Str + BuffBonuses.Str));
//            }
//            return Str;
//        }
//        public short GetDexAddition(bool nobonus = false)
//        {
//            if (!nobonus)
//            {
//                return (short)((Dex + EquipBonuses.Dex + BuffBonuses.Dex) > short.MaxValue ? short.MaxValue : (Dex + EquipBonuses.Dex + BuffBonuses.Dex));
//            }
//            return Dex;
//        }
//        public short GetIntAddition(bool nobonus = false)
//        {
//            if (!nobonus)
//            {
//                return (short)((Int + EquipBonuses.Int + BuffBonuses.Int) > short.MaxValue ? short.MaxValue : (Int + EquipBonuses.Int + BuffBonuses.Int));
//            }
//            return Int;
//        }
//        public short GetLukAddition(bool nobonus = false)
//        {
//            if (!nobonus)
//            {
//                return (short)((Luk + EquipBonuses.Luk + BuffBonuses.Luk) > short.MaxValue ? short.MaxValue : (Luk + EquipBonuses.Luk + BuffBonuses.Luk));
//            }
//            return Luk;
//        }
//        public short GetMaxHP(bool nobonus = false)
//        {
//            if (!nobonus)
//            {
//                return (short)((MaxHP + EquipBonuses.MaxHP + BuffBonuses.MaxHP) > short.MaxValue ? short.MaxValue : (MaxHP + EquipBonuses.MaxHP + BuffBonuses.MaxHP));
//            }
//            return MaxHP;
//        }
//        public short GetMaxMP(bool nobonus = false)
//        {
//            if (!nobonus)
//            {
//                return (short)((MaxMP + EquipBonuses.MaxMP + BuffBonuses.MaxMP) > short.MaxValue ? short.MaxValue : (MaxMP + EquipBonuses.MaxMP + BuffBonuses.MaxMP));
//            }
//            return MaxMP;
//        }
//
//        public void Reset(bool sendPacket)
//        {
//            BuffValueTypes flags = 0;
//            flags |= BuffWeaponAttack.Reset();
//            flags |= BuffWeaponDefense.Reset();
//            flags |= BuffMagicAttack.Reset();
//            flags |= BuffMagicDefense.Reset();
//            flags |= BuffAccurancy.Reset();
//            flags |= BuffAvoidability.Reset();
//            flags |= BuffHands.Reset();
//            flags |= BuffSpeed.Reset();
//            flags |= BuffJump.Reset();
//            flags |= BuffMagicGuard.Reset();
//            flags |= BuffDarkSight.Reset();
//            flags |= BuffBooster.Reset();
//            flags |= BuffPowerGuard.Reset();
//            flags |= BuffMaxHP.Reset();
//            flags |= BuffMaxMP.Reset();
//            if (flags.HasFlag(BuffValueTypes.MaxHP))
//            {
//                Char.Buffs.CancelHyperBody();
//            }
//
//            flags |= BuffInvincible.Reset();
//            flags |= BuffSoulArrow.Reset();
//            flags |= BuffStun.Reset();
//            flags |= BuffPoison.Reset();
//            flags |= BuffSeal.Reset();
//            flags |= BuffDarkness.Reset();
//            flags |= BuffComboAttack.Reset();
//            flags |= BuffCharges.Reset();
//            flags |= BuffDragonBlood.Reset();
//            flags |= BuffHolySymbol.Reset();
//            flags |= BuffMesoUP.Reset();
//            flags |= BuffShadowPartner.Reset();
//            flags |= BuffPickPocketMesoUP.Reset();
//            flags |= BuffMesoGuard.Reset();
//            flags |= BuffThaw.Reset();
//            flags |= BuffWeakness.Reset();
//            flags |= BuffCurse.Reset();
//
//            Char.Buffs.FinalizeDebuff(flags, sendPacket);
//        }
//
//        public void DecodeForCC(PacketWriter PacketWriter)
//        {
//            var flags = (BuffValueTypes)PacketWriter.ReadUInt();
//
//            BuffWeaponAttack.DecodeForCC(PacketWriter, flags);
//            BuffWeaponDefense.DecodeForCC(PacketWriter, flags);
//            BuffMagicAttack.DecodeForCC(PacketWriter, flags);
//            BuffMagicDefense.DecodeForCC(PacketWriter, flags);
//            BuffAccurancy.DecodeForCC(PacketWriter, flags);
//            BuffAvoidability.DecodeForCC(PacketWriter, flags);
//            BuffHands.DecodeForCC(PacketWriter, flags);
//            BuffSpeed.DecodeForCC(PacketWriter, flags);
//            BuffJump.DecodeForCC(PacketWriter, flags);
//            BuffMagicGuard.DecodeForCC(PacketWriter, flags);
//            BuffDarkSight.DecodeForCC(PacketWriter, flags);
//            BuffBooster.DecodeForCC(PacketWriter, flags);
//            BuffPowerGuard.DecodeForCC(PacketWriter, flags);
//            BuffMaxHP.DecodeForCC(PacketWriter, flags);
//            BuffMaxMP.DecodeForCC(PacketWriter, flags);
//            if (BuffMaxHP.IsSet())
//            {
//                short hpmpBonus = (short)((double)Char.PrimaryStats.MaxHP * ((double)BuffMaxHP.N / 100.0d));
//                Char.PrimaryStats.BuffBonuses.MaxHP = hpmpBonus;
//                hpmpBonus = (short)((double)Char.PrimaryStats.MaxMP * ((double)BuffMaxMP.N / 100.0d));
//                Char.PrimaryStats.BuffBonuses.MaxMP = hpmpBonus;
//            }
//            BuffInvincible.DecodeForCC(PacketWriter, flags);
//            BuffSoulArrow.DecodeForCC(PacketWriter, flags);
//            BuffStun.DecodeForCC(PacketWriter, flags);
//            BuffPoison.DecodeForCC(PacketWriter, flags);
//            BuffSeal.DecodeForCC(PacketWriter, flags);
//            BuffDarkness.DecodeForCC(PacketWriter, flags);
//            BuffComboAttack.DecodeForCC(PacketWriter, flags);
//            BuffCharges.DecodeForCC(PacketWriter, flags);
//            BuffDragonBlood.DecodeForCC(PacketWriter, flags);
//            BuffHolySymbol.DecodeForCC(PacketWriter, flags);
//            BuffMesoUP.DecodeForCC(PacketWriter, flags);
//            BuffShadowPartner.DecodeForCC(PacketWriter, flags);
//            BuffPickPocketMesoUP.DecodeForCC(PacketWriter, flags);
//            BuffMesoGuard.DecodeForCC(PacketWriter, flags);
//            BuffThaw.DecodeForCC(PacketWriter, flags);
//            BuffWeakness.DecodeForCC(PacketWriter, flags);
//            BuffCurse.DecodeForCC(PacketWriter, flags);
//
//            if (BuffComboAttack.IsSet())
//            {
//                var sld = Char.Skills.GetSkillLevelData(BuffComboAttack.R);
//                if (sld != null)
//                {
//                    BuffComboAttack.MaxOrbs = sld.XValue;
//                }
//            }
//
//            if (BuffMesoGuard.IsSet())
//            {
//                var sld = Char.Skills.GetSkillLevelData(BuffMesoGuard.R);
//                if (sld != null)
//                {
//                    BuffMesoGuard.MesosLeft = sld.MesosUsage;
//                }
//            }
//        }
//
//        public void EncodeForCC(PacketWriter PacketWriter)
//        {
//            long currentTime = BuffStat.GetTimeForBuff();
//            int offset = PacketWriter.Position;
//            PacketWriter.WriteUInt(0);
//            BuffValueTypes flags = 0;
//
//            BuffWeaponAttack.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffWeaponDefense.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffMagicAttack.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffMagicDefense.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffAccurancy.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffAvoidability.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffHands.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffSpeed.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffJump.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffMagicGuard.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffDarkSight.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffBooster.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffPowerGuard.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffMaxHP.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffMaxMP.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffInvincible.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffSoulArrow.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffStun.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffPoison.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffSeal.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffDarkness.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffComboAttack.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffCharges.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffDragonBlood.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffHolySymbol.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffMesoUP.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffShadowPartner.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffPickPocketMesoUP.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffMesoGuard.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffThaw.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffWeakness.EncodeForCC(PacketWriter, ref flags, currentTime);
//            BuffCurse.EncodeForCC(PacketWriter, ref flags, currentTime);
//
//            PacketWriter.SetUInt(offset, (uint)flags);
//        }
//
//        public void CheckExpired(long currentTime)
//        {
//            BuffValueTypes endFlag = 0;
//
//            BuffWeaponAttack.TryReset(currentTime, ref endFlag);
//            BuffWeaponDefense.TryReset(currentTime, ref endFlag);
//            BuffMagicAttack.TryReset(currentTime, ref endFlag);
//            BuffMagicDefense.TryReset(currentTime, ref endFlag);
//            BuffAccurancy.TryReset(currentTime, ref endFlag);
//            BuffAvoidability.TryReset(currentTime, ref endFlag);
//            BuffHands.TryReset(currentTime, ref endFlag);
//            BuffSpeed.TryReset(currentTime, ref endFlag);
//            BuffJump.TryReset(currentTime, ref endFlag);
//            BuffMagicGuard.TryReset(currentTime, ref endFlag);
//            BuffDarkSight.TryReset(currentTime, ref endFlag);
//            BuffBooster.TryReset(currentTime, ref endFlag);
//            BuffPowerGuard.TryReset(currentTime, ref endFlag);
//            if (BuffMaxHP.TryReset(currentTime, ref endFlag) &&
//                BuffMaxMP.TryReset(currentTime, ref endFlag))
//            {
//                Char.Buffs.CancelHyperBody();
//            }
//
//            BuffInvincible.TryReset(currentTime, ref endFlag);
//            BuffSoulArrow.TryReset(currentTime, ref endFlag);
//            BuffStun.TryReset(currentTime, ref endFlag);
//            BuffPoison.TryReset(currentTime, ref endFlag);
//            BuffSeal.TryReset(currentTime, ref endFlag);
//            BuffDarkness.TryReset(currentTime, ref endFlag);
//            BuffComboAttack.TryReset(currentTime, ref endFlag);
//            BuffCharges.TryReset(currentTime, ref endFlag);
//            BuffDragonBlood.TryReset(currentTime, ref endFlag);
//            BuffHolySymbol.TryReset(currentTime, ref endFlag);
//            BuffMesoUP.TryReset(currentTime, ref endFlag);
//            BuffShadowPartner.TryReset(currentTime, ref endFlag);
//            BuffPickPocketMesoUP.TryReset(currentTime, ref endFlag);
//            BuffMesoGuard.TryReset(currentTime, ref endFlag);
//            BuffThaw.TryReset(currentTime, ref endFlag);
//            BuffWeakness.TryReset(currentTime, ref endFlag);
//            BuffCurse.TryReset(currentTime, ref endFlag);
//
//            Char.Buffs.FinalizeDebuff(endFlag);
//        }
//
//        public BuffValueTypes AllActiveBuffs()
//        {
//            long currentTime = BuffStat.GetTimeForBuff();
//            BuffValueTypes flags = 0;
//            flags |= BuffWeaponAttack.GetState(currentTime);
//            flags |= BuffWeaponDefense.GetState(currentTime);
//            flags |= BuffMagicAttack.GetState(currentTime);
//            flags |= BuffMagicDefense.GetState(currentTime);
//            flags |= BuffAccurancy.GetState(currentTime);
//            flags |= BuffAvoidability.GetState(currentTime);
//            flags |= BuffHands.GetState(currentTime);
//            flags |= BuffSpeed.GetState(currentTime);
//            flags |= BuffJump.GetState(currentTime);
//            flags |= BuffMagicGuard.GetState(currentTime);
//            flags |= BuffDarkSight.GetState(currentTime);
//            flags |= BuffBooster.GetState(currentTime);
//            flags |= BuffPowerGuard.GetState(currentTime);
//            flags |= BuffMaxHP.GetState(currentTime);
//            flags |= BuffMaxMP.GetState(currentTime);
//            flags |= BuffInvincible.GetState(currentTime);
//            flags |= BuffSoulArrow.GetState(currentTime);
//            flags |= BuffStun.GetState(currentTime);
//            flags |= BuffPoison.GetState(currentTime);
//            flags |= BuffSeal.GetState(currentTime);
//            flags |= BuffDarkness.GetState(currentTime);
//            flags |= BuffComboAttack.GetState(currentTime);
//            flags |= BuffCharges.GetState(currentTime);
//            flags |= BuffDragonBlood.GetState(currentTime);
//            flags |= BuffHolySymbol.GetState(currentTime);
//            flags |= BuffMesoUP.GetState(currentTime);
//            flags |= BuffShadowPartner.GetState(currentTime);
//            flags |= BuffPickPocketMesoUP.GetState(currentTime);
//            flags |= BuffMesoGuard.GetState(currentTime);
//            flags |= BuffThaw.GetState(currentTime);
//            flags |= BuffWeakness.GetState(currentTime);
//            flags |= BuffCurse.GetState(currentTime);
//
//            return flags;
//
//        }
//
//        public BuffValueTypes RemoveByReference(int pBuffValue, bool onlyReturn = false)
//        {
//            if (pBuffValue == 0)
//            {
//                return 0;
//            }
//
//            BuffValueTypes endFlag = 0;
//
//            BuffWeaponAttack.TryResetByReference(pBuffValue, ref endFlag);
//            BuffWeaponDefense.TryResetByReference(pBuffValue, ref endFlag);
//            BuffMagicAttack.TryResetByReference(pBuffValue, ref endFlag);
//            BuffMagicDefense.TryResetByReference(pBuffValue, ref endFlag);
//            BuffAccurancy.TryResetByReference(pBuffValue, ref endFlag);
//            BuffAvoidability.TryResetByReference(pBuffValue, ref endFlag);
//            BuffHands.TryResetByReference(pBuffValue, ref endFlag);
//            BuffSpeed.TryResetByReference(pBuffValue, ref endFlag);
//            BuffJump.TryResetByReference(pBuffValue, ref endFlag);
//            BuffMagicGuard.TryResetByReference(pBuffValue, ref endFlag);
//            BuffDarkSight.TryResetByReference(pBuffValue, ref endFlag);
//            BuffBooster.TryResetByReference(pBuffValue, ref endFlag);
//            BuffPowerGuard.TryResetByReference(pBuffValue, ref endFlag);
//            BuffMaxHP.TryResetByReference(pBuffValue, ref endFlag);
//            BuffMaxMP.TryResetByReference(pBuffValue, ref endFlag);
//            BuffInvincible.TryResetByReference(pBuffValue, ref endFlag);
//            BuffSoulArrow.TryResetByReference(pBuffValue, ref endFlag);
//            BuffStun.TryResetByReference(pBuffValue, ref endFlag);
//            BuffPoison.TryResetByReference(pBuffValue, ref endFlag);
//            BuffSeal.TryResetByReference(pBuffValue, ref endFlag);
//            BuffDarkness.TryResetByReference(pBuffValue, ref endFlag);
//            BuffComboAttack.TryResetByReference(pBuffValue, ref endFlag);
//            BuffCharges.TryResetByReference(pBuffValue, ref endFlag);
//            BuffDragonBlood.TryResetByReference(pBuffValue, ref endFlag);
//            BuffHolySymbol.TryResetByReference(pBuffValue, ref endFlag);
//            BuffMesoUP.TryResetByReference(pBuffValue, ref endFlag);
//            BuffShadowPartner.TryResetByReference(pBuffValue, ref endFlag);
//            BuffPickPocketMesoUP.TryResetByReference(pBuffValue, ref endFlag);
//            BuffMesoGuard.TryResetByReference(pBuffValue, ref endFlag);
//            BuffThaw.TryResetByReference(pBuffValue, ref endFlag);
//            BuffWeakness.TryResetByReference(pBuffValue, ref endFlag);
//            BuffCurse.TryResetByReference(pBuffValue, ref endFlag);
//
//            if (!onlyReturn)
//            {
//                Char.Buffs.FinalizeDebuff(endFlag);
//            }
//            return endFlag;
//        }
//
//        public void EncodeForLocal(PacketWriter pPacket, BuffValueTypes pSpecificFlag = BuffValueTypes.All)
//        {
//            long currentTime = BuffStat.GetTimeForBuff();
//            int tmpBuffPos = pPacket.Position;
//            BuffValueTypes endFlag = 0;
//            pPacket.WriteUInt((uint)endFlag);
//
//
//            BuffWeaponAttack.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffWeaponDefense.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffMagicAttack.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffMagicDefense.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffAccurancy.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffAvoidability.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffHands.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffSpeed.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffJump.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffMagicGuard.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//
//            // Do not activate it in hide
//            if (BuffDarkSight.HasReferenceId(Constants.Gm.Skills.Hide) == false)
//            {
//                BuffDarkSight.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            }
//
//            BuffBooster.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffPowerGuard.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffMaxHP.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffMaxMP.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffInvincible.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffSoulArrow.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffStun.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffPoison.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffSeal.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffDarkness.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffComboAttack.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffCharges.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffDragonBlood.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffHolySymbol.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffMesoUP.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffShadowPartner.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffPickPocketMesoUP.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffMesoGuard.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffThaw.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffWeakness.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//            BuffCurse.EncodeForLocal(pPacket, ref endFlag, currentTime, pSpecificFlag);
//
//
//            pPacket.SetUInt(tmpBuffPos, (uint)endFlag);
//        }
//
//        public bool HasBuff(int skillOrItemID)
//        {
//            long currentTime = BuffStat.GetTimeForBuff();
//            return
//                BuffWeaponAttack.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffWeaponDefense.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffMagicAttack.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffMagicDefense.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffAccurancy.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffAvoidability.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffHands.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffSpeed.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffJump.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffMagicGuard.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffDarkSight.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffBooster.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffPowerGuard.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffMaxHP.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffMaxMP.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffInvincible.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffSoulArrow.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffStun.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffPoison.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffSeal.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffDarkness.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffComboAttack.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffCharges.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffDragonBlood.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffHolySymbol.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffMesoUP.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffShadowPartner.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffPickPocketMesoUP.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffMesoGuard.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffThaw.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffWeakness.HasReferenceId(skillOrItemID, currentTime) ||
//                BuffCurse.HasReferenceId(skillOrItemID, currentTime);
//        }
//    }
//}


