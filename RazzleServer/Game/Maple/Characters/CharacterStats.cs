using RazzleServer.Common.Constants;

namespace RazzleServer.Game.Maple.Characters
{
    public class CharacterStats
    {
        public Character Parent { get; set; }

        public byte Level { get; set; }
        public Job Job { get; set; }
        public short BaseStrength { get; set; }
        public short BaseDexterity { get; set; }
        public short BaseIntelligence { get; set; }
        public short BaseLuck { get; set; }
        public short BaseHealth { get; set; }
        public short BaseMaxHealth { get; set; }
        public short BaseMana { get; set; }
        public short BaseMaxMana { get; set; }

        public short AbilityPoints { get; set; }
        public short SkillPoints { get; set; }
        public int Experience { get; set; }
        public short Fame { get; set; }
        public int BuddyListSlots { get; set; } = 20;

        public byte BaseSpeed { get; set; }
        public float SpeedMode { get; set; }
        public byte BaseJump { get; set; }
        public float JumpMode { get; set; }

        public short Strength => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short Dexterity => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short Intelligence => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short Luck => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short Health => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short MaxHealth => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short Mana => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);
        public short MaxMana => (short)(BaseStrength + BuffBonuses.Strength + ItemBonuses.Strength);

        public StatBonus BuffBonuses { get; set; } = new StatBonus();
        public StatBonus ItemBonuses { get; set; } = new StatBonus();


        public void SetJob(Job value)
        {
            Job = value;
            Parent.Update(StatisticType.Job);
        }
        
        public void SetLevel(byte value)
        {
            Level = value;
            Parent.Update(StatisticType.Level);
        }

        public void SetExperience(int value)
        {
            Experience = value;
            Parent.Update(StatisticType.Experience);
        }

        public void SetHP(short value, bool sendPacket = true)
        {
            if (value < 0) value = 0;
            SetMaxHP(value);
            HP = value;
            if (sendPacket)
            {
                CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Hp, value);
            }
        }

        public void ModifyHP(short value, bool sendPacket = true)
        {
            if ((HP + value) < 0)
            {
                HP = 0;
            }
            else if ((HP + value) > GetMaxHP(false))
            {
                HP = GetMaxHP(false);
            }
            else
            {
                HP = (short)(HP + value);
            }

            if (sendPacket)
            {
                CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Hp, HP);

                if (this.PartyID != -1)
                {
                    MapPacket.UpdatePartyMemberHP(this);
                    MapPacket.ReceivePartyMemberHP(this);
                }
            }

            ModifiedHP();
        }

        public void DamageHP(short amount)
        {
            HP = (short)(amount > HP ? 0 : HP - amount);
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Hp, HP);
            ModifiedHP();
        }

        public void ModifiedHP()
        {
            if (HP == 0)
            {
                // lose exp
                loseEXP();

                Summons.RemoveSummon(false, 0x03);
                Summons.RemoveSummon(true, 0x03);
            }
        }

        public void SetMP(short value, bool isBySelf = false)
        {
            if (value < 0) value = 0;
            SetMaxMP(value);
            MP = value;
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Mp, value, isBySelf);
        }

        public void ModifyMP(short value, bool isSelf = false)
        {
            if ((MP + value) < 0)
            {
                MP = 0;
            }
            else if ((MP + value) > GetMaxMP(false))
            {
                MP = GetMaxMP(false);
            }
            else
            {
                MP = (short)(MP + value);
            }

            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Mp, MP, isSelf);
        }

        public void DamageMP(short amount)
        {
            MP = (short)(amount > MP ? 0 : MP - amount);
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Mp, MP, false);
        }

        public void ModifyMaxMP(short value)
        {
            MaxMP = (short)(((MaxMP + value) > Constants.MaxMaxMp) ? Constants.MaxMaxMp : (MaxMP + value));
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.MaxMp, MaxMP);
        }

        public void ModifyMaxHP(short value)
        {
            MaxHP = (short)(((MaxHP + value) > Constants.MaxMaxHp) ? Constants.MaxMaxHp : (MaxHP + value));
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.MaxHp, MaxHP);
            if (this.PartyID != -1)
            {
                MapPacket.UpdatePartyMemberHP(this);
                MapPacket.ReceivePartyMemberHP(this);
            }
        }

        public void SetMaxHP(short value)
        {
            if (value > Constants.MaxMaxHp) value = Constants.MaxMaxHp;
            else if (value < Constants.MinMaxHp) value = Constants.MinMaxHp;
            MaxHP = value;
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.MaxHp, value);
        }

        public void SetMaxMP(short value)
        {
            if (value > Constants.MaxMaxMp) value = Constants.MaxMaxMp;
            else if (value < Constants.MinMaxMp) value = Constants.MinMaxMp;
            MaxMP = value;
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.MaxMp, value);
        }

        public void SetLevel(byte value)
        {
            Level = value;
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Level, value);
            MapPacket.SendPlayerLevelupAnim(this);
            Save();
        }

        public void AddFame(short value)
        {
            if (Fame + value > short.MaxValue)
            {
                SetFame(short.MaxValue);
            }
            else
            {
                SetFame((short)(Fame + value));
            }
        }

        public void SetFame(short value)
        {
            Fame = value;
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Fame, value);
        }

        public void AddEXP(double value) { AddEXP((uint)value); }

        public void AddEXP(uint value, bool isParty = false)
        {
            int amount = (int)(value > int.MaxValue ? int.MaxValue : value);
            uint amnt = (uint)(EXP + amount);
            if (Level >= 200) return;
            if (value != 0 && !isParty)
            {
                CharacterStatsPacket.SendGainEXP(this, amount, true, false);
            }
            else if (value != 0 && isParty)
            {
                CharacterStatsPacket.SendGainEXP(this, amount, false, false);
            }

            byte level = Level;
            if (amnt > Constants.GetLevelEXP(Level))
            {
                byte levelsGained = 0;
                short apgain = 0;
                short spgain = 0;
                short mpgain = 0;
                short hpgain = 0;
                short job = (short)((Job / 100) - (Job % 100));
                short x = 1;
                short intt = (short)(GetIntAddition(true) / 10);

                while (amnt > Constants.GetLevelEXP(Level) && levelsGained < 1)
                {
                    amnt -= (uint)Constants.GetLevelEXP(Level);
                    level++;
                    levelsGained++;

                    apgain += Constants.ApPerLevel;

                    switch (job)
                    {
                        case 0:
                        {
                            hpgain += GetHPFromLevelup(Constants.BaseHp.Beginner, 0);
                            mpgain += GetMPFromLevelup(Constants.BaseMp.Beginner, intt);
                            break;
                        }

                        case 1:
                        {
                            hpgain += GetHPFromLevelup(Constants.BaseHp.Warrior, 0);
                            mpgain += GetMPFromLevelup(Constants.BaseMp.Warrior, intt);
                            break;
                        }

                        case 2:
                        {
                            hpgain += GetHPFromLevelup(Constants.BaseHp.Magician, 0);
                            mpgain += GetMPFromLevelup(Constants.BaseMp.Magician, (short)(2 * x + intt));
                            break;
                        }

                        case 3:
                        {
                            hpgain += GetHPFromLevelup(Constants.BaseHp.Bowman, 0);
                            mpgain += GetMPFromLevelup(Constants.BaseMp.Bowman, intt);
                            break;
                        }

                        case 4:
                        {
                            hpgain += GetHPFromLevelup(Constants.BaseHp.Thief, 0);
                            mpgain += GetMPFromLevelup(Constants.BaseMp.Thief, intt);
                            break;
                        }

                        default:
                        {
                            hpgain += Constants.BaseHp.Gm;
                            mpgain += Constants.BaseMp.Gm;
                            break;
                        }
                    }

                    if (Job != 0)
                    {
                        spgain = Constants.SpPerLevel;
                    }

                    if (level >= 200)
                    {
                        amnt = 0;
                        break;
                    }
                }

                if (amnt >= Constants.GetLevelEXP(Level))
                {
                    amnt = (uint)(Constants.GetLevelEXP(Level) - 1);
                }

                if (levelsGained > 0)
                {
                    ModifyMaxHP(hpgain);
                    ModifyMaxMP(mpgain);
                    SetLevel(level);
                    AddAP(apgain);
                    AddSP(spgain);
                    SetHP(GetMaxHP(false));
                    SetMP(GetMaxMP(false));
                }
            }

            EXP = (int)amnt;
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Exp, EXP);
        }

        public void AddMesos(int value, bool isSelf = false)
        {
            int newMesos = 0;
            if (value < 0)
            {
                if ((Inventory.mMesos - value) < 0) newMesos = 0;
                else newMesos = Inventory.mMesos + value; // neg - neg = pos
            }
            else
            {
                if ((Inventory.mMesos + value) > int.MaxValue) newMesos = int.MaxValue;
                else newMesos = Inventory.mMesos + value;
            }

            Inventory.mMesos = newMesos;
            CharacterStatsPacket.SendStatChange(this, (uint)CharacterStatsPacket.Constants.Mesos, Inventory.mMesos,
                isSelf);
        }
    }
}
