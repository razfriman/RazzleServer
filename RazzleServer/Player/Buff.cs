using RazzleServer.Constants;
using RazzleServer.Data.WZ;
using RazzleServer.Packet;
using RazzleServer.Server;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RazzleServer.Player
{
    public class Buff
    {
        public int SkillId { get; private set; }
        public int Duration { get; private set; }
        public DateTime StartTime { get; private set; }
        public SkillEffect Effect { get; private set; }
        public CancellationTokenSource CancellationToken { get; private set; }
        public int Stacks { get; set; }


        /// <summary>
        ///
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="effect"></param>
        /// <param name="duration">Milliseconds</param>
        /// <param name="startTime"></param>
        /// <param name="affectedCharacter"></param>
        public Buff(int skillId, SkillEffect effect, int duration, MapleCharacter affectedCharacter, DateTime? nStartTime = null)
        {
            DateTime startTime = nStartTime ?? DateTime.UtcNow;
            SkillId = skillId;
            Effect = effect;
            Duration = duration;
            StartTime = startTime;
            if (duration < SkillEffect.MAX_BUFF_TIME_MS)
            {
                CancellationToken = new CancellationTokenSource();
                Scheduler.ScheduleRemoveBuff(affectedCharacter, skillId, duration, CancellationToken.Token);
            }
            Stacks = 1;
        }

        public void CancelRemoveBuffSchedule()
        {
            CancellationToken?.Cancel();
        }

        public static PacketWriter CancelBuff(Buff buff)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.REMOVE_BUFF);

            WriteBuffMask(pw, buff.Effect.BuffInfo.Keys);

            foreach (var kvp in buff.Effect.BuffInfo)
            {
                if (kvp.Key.IsStackingBuff)
                    pw.WriteInt(0);
            }

            pw.WriteInt(0);
            return pw;
        }

        public static void WriteBuffMask(PacketWriter pw, ICollection<BuffStat> buffStats)
        {
            int[] mask = new int[GameConstants.BUFFSTAT_MASKS];
            foreach (BuffStat buffStat in buffStats)
            {
                int maskIndex = buffStat.BitIndex / 32;
                int relativeBitPos = buffStat.BitIndex % 32;
                int bit = 1 << 31 - relativeBitPos;
                mask[maskIndex] |= bit;
            }
            for (int i = 0; i < mask.Length; i++)
            {
                pw.WriteInt(mask[i]);
            }
        }

        public static void WriteSingleBuffMask(PacketWriter pw, BuffStat buffStat)
        {
            WriteBuffMask(pw, new List<BuffStat>() { buffStat });
        }

        public static PacketWriter GiveBuff(Buff buff)
        {
            PacketWriter pw = new PacketWriter(SMSGHeader.GIVE_BUFF);

            WriteBuffMask(pw, buff.Effect.BuffInfo.Keys);
            bool stacked = false;
            foreach (var b in buff.Effect.BuffInfo)
            {
                if (b.Key.IsStackingBuff)
                {
                    if (!stacked)
                    {
                        stacked = true;
                        pw.WriteInt(0);
                        pw.WriteByte(0); //? 
                        pw.WriteInt(0);
                        if (buff.SkillId == DarkKnight.SACRIFICE || buff.SkillId == Bishop.ADVANCED_BLESSING)
                            pw.WriteInt(0);
                    }
                    pw.WriteInt(1); //amount of the same buffstat
                    pw.WriteInt(buff.SkillId);
                    if (b.Key.UsesStacksAsValue)
                        pw.WriteInt(buff.Stacks);
                    else
                        pw.WriteInt(b.Value);
                    pw.WriteInt(0); //tickcount?
                    pw.WriteInt(0);
                }
                else
                {
                    if (b.Key.UsesStacksAsValue)
                        pw.WriteShort((short)buff.Stacks);
                    /*else if (b.Key == MapleBuffStat.SHADOW_PARTNER)
                        pw.WriteInt(b.Value);*/
                    else
                        pw.WriteShort((short)b.Value);
                    pw.WriteInt(buff.SkillId);
                }
                pw.WriteInt(buff.Duration);
            }

            if (!stacked)
            {
                pw.WriteZeroBytes(9);
            }

            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteInt(0);

            return pw;
        }

        #region Special Buffs
        public static PacketWriter GiveEvilEyeBuff(Buff buff)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.GIVE_BUFF);

            pw.WriteShort(1);
            pw.WriteInt(Spearman.EVIL_EYE);
            pw.WriteInt(buff.Duration);
            pw.WriteInt(0);
            if (buff.Stacks == Berserker.EVIL_EYE_OF_DOMINATION)
            {
                pw.WriteByte(0x13);
                pw.WriteInt(Berserker.EVIL_EYE_OF_DOMINATION);
                pw.WriteInt(0);
                pw.WriteLong(0);
                pw.WriteInt(1);
                pw.WriteByte(0);
            }
            else
            {
                pw.WriteByte(0x13);
                pw.WriteInt(Spearman.EVIL_EYE);
                pw.WriteInt(0);
                pw.WriteLong(0);
                pw.WriteInt(0);
                pw.WriteByte(0);

            }
            return pw;
        }

        public static PacketWriter GiveCrossSurgeBuff(Buff buff, MapleCharacter chr, SkillEffect effect)
        {
            BuffedCharacterStats stats = chr.Stats;
            int damageIncPercent = effect.Info[CharacterSkillStat.x];
            int absorbPercent = effect.Info[CharacterSkillStat.y];
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.GIVE_BUFF);

            WriteBuffMask(pw, buff.Effect.BuffInfo.Keys);
            double hpPercent = (chr.HP / (double)stats.MaxHp) * 100;
            short dmgInc = (short)(hpPercent * (damageIncPercent / 100.0));
            pw.WriteShort(dmgInc);
            pw.WriteInt(buff.SkillId);
            pw.WriteInt(buff.Duration);

            pw.WriteInt(0);
            pw.WriteByte(0);
            int absorb = (int)((stats.MaxHp - chr.HP) * (absorbPercent / 100.0));
            absorb = Math.Min(absorb, 4000);
            pw.WriteInt(absorb);
            pw.WriteInt(0);
            pw.WriteInt(540); //?
            pw.WriteInt(1);
            pw.WriteByte(0);

            return pw;
        }

        public static PacketWriter GiveFinalPactBuff(Buff buff)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.GIVE_BUFF);

            WriteBuffMask(pw, buff.Effect.BuffInfo.Keys);

            pw.WriteShort(1);
            pw.WriteInt(DarkKnight.FINAL_PACT2);
            pw.WriteInt(buff.Duration);

            pw.WriteShort(1);
            pw.WriteInt(DarkKnight.FINAL_PACT2);
            pw.WriteInt(buff.Duration);

            pw.WriteInt(0);
            pw.WriteByte(0);
            pw.WriteInt(buff.Stacks);

            pw.WriteInt(0);
            pw.WriteInt(1);
            pw.WriteInt(DarkKnight.FINAL_PACT2);
            pw.WriteInt(buff.Effect.Info[CharacterSkillStat.indieDamR]);
            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteInt(buff.Duration);

            pw.WriteInt(0);
            pw.WriteInt(1);
            pw.WriteByte(0);

            return pw;
        }

        public static PacketWriter UpdateFinalPactKillCount(int remainingKillCount, uint remainingDurationMS)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.GIVE_BUFF);
            //WriteSingleBuffMask(pw, MapleBuffStat.FINAL_PACT3);

            pw.WriteShort(1);
            pw.WriteInt(DarkKnight.FINAL_PACT2);
            pw.WriteUInt(remainingDurationMS);

            pw.WriteInt(0);
            pw.WriteByte(0);
            pw.WriteInt(remainingKillCount);

            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteInt(0);
            pw.WriteByte(0);

            return pw;
        }
        #endregion
    }
}
