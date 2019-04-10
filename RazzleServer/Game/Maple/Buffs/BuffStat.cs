using System;
using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Buffs
{
    public class BuffStat
    {
        public static long GetTimeForBuff(long additionalMillis = 0) =>
            DateTime.UtcNow.AddMilliseconds(additionalMillis).ToFileTimeUtc() / 10000;

        // Number. Most of the time, this is the X or Y value of the skill/buff
        public short Value { get; set; }

        // Reference ID. For Item IDs, use a negative number
        public int ReferenceId { get; set; }

        // Expire Time. Extended version of T (full time in millis)
        public long ExpireTime { get; set; }
        public BuffValueTypes Flag { get; set; }

        public bool IsSet(long? time = null)
        {
            if (Value == 0) return false;
            if (time == null) time = GetTimeForBuff();
            return ExpireTime > time;
        }

        public BuffValueTypes GetState(long? time = null)
        {
            return IsSet(time) ? Flag : 0;
        }

        public bool HasReferenceId(int referenceId, long? currenTime = null)
        {
            return IsSet(currenTime) && ReferenceId == referenceId;
        }

        public BuffStat(BuffValueTypes flag)
        {
            Flag = flag;
            Value = 0;
            ReferenceId = 0;
            ExpireTime = 0;
        }

        public BuffValueTypes Reset()
        {
            if (ReferenceId == 0 && Value == 0 && ExpireTime == 0)
            {
                return 0;
            }

            Value = 0;
            ReferenceId = 0;
            ExpireTime = 0;
            return Flag;
        }

        public virtual bool TryReset(long currentTime, ref BuffValueTypes flag)
        {
            if (Value == 0 || ExpireTime >= currentTime) return false;

            flag |= Reset();
            return true;
        }

        public void TryResetByReference(int reference, ref BuffValueTypes flag)
        {
            if (Value == 0 || ReferenceId != reference) return;
            flag |= Reset();
        }

        public virtual BuffValueTypes Set(int referenceId, short nValue, long expireTime)
        {
            // Ignore 0 N-values
            if (nValue == 0)
            {
                return 0;
            }

            ReferenceId = referenceId;
            Value = nValue;
            ExpireTime = expireTime;
            return Flag;
        }

        public void EncodeForRemote(ref BuffValueTypes flag, long currentTime, Action<BuffStat> func,
            BuffValueTypes specificFlag = BuffValueTypes.All)
        {
            if (!IsSet(currentTime) || !specificFlag.HasFlag(Flag))
            {
                return;
            }

            flag |= Flag;
            func?.Invoke(this);
        }

        public void EncodeForLocal(PacketWriter pw, ref BuffValueTypes flag, long currentTime,
            BuffValueTypes specificFlag = BuffValueTypes.All)
        {
            if (!IsSet(currentTime) || !specificFlag.HasFlag(Flag))
            {
                return;
            }

            flag |= Flag;
            pw.WriteShort(Value);
            pw.WriteInt(ReferenceId);
            pw.WriteShort((short)((ExpireTime - currentTime) / 100)); // If its not divided, it will not flash.
        }
    }
}
