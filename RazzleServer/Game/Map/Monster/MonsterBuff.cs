using RazzleServer.Common.Packet;
using RazzleServer.Player;
using RazzleServer.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MapleLib.PacketLib;

namespace RazzleServer.Map.Monster
{
    public class MonsterBuff
    {
        public int OwnerId { get; private set; }
        public int SkillId { get; private set; }
        public int Duration { get; private set; }
        public Task RemoveSchedule { get; private set; }
        public BuffStat BuffStat { get; private set; }
        public int BuffValue { get; private set; }
        public byte Stacks { get; private set; }
        public MapleMonster Victim { get; private set; }
        private CancellationTokenSource _cancellation;

        public MonsterBuff(int ownerId, int skillId, int durationMS, BuffStat buffStat, int buffValue, MapleMonster victim, byte stacks = 0)
        {
            OwnerId = ownerId;
            SkillId = skillId;
            Duration = durationMS;
            BuffStat = buffStat;
            BuffValue = buffValue;
            Victim = victim;
            Stacks = stacks;
            _cancellation = new CancellationTokenSource();
            if (buffStat != MonsterBuffStat.POISON) //Poison will remove itself after all ticks
                RemoveSchedule = Scheduler.ScheduleRemoveMonsterStatusEffect(this, durationMS, _cancellation.Token);
        }

        public virtual PacketWriter GetApplicationPacket()
        {
            
            var pw = new PacketWriter(ServerOperationCode.GIVE_FOREIGN_BUFF);
            pw.WriteInt(Victim.ObjectID);
            WriteSingleBuffMask(pw, BuffStat);
            pw.WriteInt(BuffValue);
            pw.WriteInt(SkillId);
            pw.WriteShort(0);
            if (BuffStat.IsStackingBuff)
            {
                pw.WriteByte(Stacks);
            }
            pw.WriteShort(0);
            pw.WriteByte(0);
            pw.WriteByte(1);
            pw.WriteByte(1);
            return pw;
        }

        public virtual PacketWriter GetRemovePacket()
        {
            
            var pw = new PacketWriter(ServerOperationCode.CANCEL_FOREIGN_BUFF);
            pw.WriteInt(Victim.ObjectID);
            WriteSingleBuffMask(pw, BuffStat);
            pw.WriteInt(0);
            return pw;
        }

        public virtual void Dispose(bool silent)
        {
            if (Victim != null && Victim.Map != null)
            {
                if (!silent)
                    Victim.Map.BroadcastPacket(GetRemovePacket());
                Victim.RemoveStatusEffect(this);
            }
            Victim = null;
            _cancellation.Cancel();
        }

        public static void WriteSingleBuffMask(PacketWriter pw, BuffStat buffStat)
        {
            WriteBuffMask(pw, new List<BuffStat>() { buffStat });
        }

        public static void WriteBuffMask(PacketWriter pw, List<BuffStat> buffStats)
        {
            int[] mask = new int[3];
            foreach (BuffStat buffStat in buffStats)
            {
                int pos = buffStat.BitIndex;
                int maskIndex = pos / 32;
                int relativeBitPos = pos % 32;
                int bit = 1 << 31 - relativeBitPos;
                mask[maskIndex] |= bit;
            }
            for (int i = 0; i < mask.Length; i++)
            {
                pw.WriteInt(mask[i]);
            }
        }
    }
}
