using System;
using RazzleServer.Common.Constants;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Maple.Buffs
{
    public class BuffStat
    {
        public short Value { get; set; }
        public int ReferenceId { get; set; }
        public DateTime ExpireTime { get; set; }
        public BuffValueTypes Flag { get; set; }
        public BuffType Type => ReferenceId == 0 ? BuffType.None : ReferenceId > 0 ? BuffType.Skill : BuffType.Item;

        public bool IsActive(DateTime? time) => Value != 0 && ExpireTime > (time ?? DateTime.UtcNow);

        public BuffValueTypes GetState(DateTime? time = null) => IsActive(time) ? Flag : 0;

        public bool HasReferenceId(int referenceId, DateTime? currenTime = null) =>
            IsActive(currenTime) && ReferenceId == referenceId;

        public BuffStat(BuffValueTypes flag)
        {
            Flag = flag;
            Value = 0;
            ReferenceId = 0;
            ExpireTime = DateTime.MinValue;
        }

        public BuffValueTypes Reset()
        {
            if (ReferenceId == 0 && Value == 0 && ExpireTime == DateTime.MinValue)
            {
                return 0;
            }

            Value = 0;
            ReferenceId = 0;
            ExpireTime = DateTime.MinValue;
            return Flag;
        }

        public virtual bool TryReset(DateTime currentTime, ref BuffValueTypes flag)
        {
            if (Value == 0 || ExpireTime >= currentTime)
            {
                return false;
            }

            flag |= Reset();
            return true;
        }

        public void TryResetByReference(int reference, ref BuffValueTypes flag)
        {
            if (Value == 0 || ReferenceId != reference)
            {
                return;
            }

            flag |= Reset();
        }

        public virtual BuffValueTypes Set(int referenceId, short value, DateTime expireTime)
        {
            if (value == 0)
            {
                return 0;
            }

            ReferenceId = referenceId;
            Value = value;
            ExpireTime = expireTime;
            return Flag;
        }

        public void EncodeForRemote(ref BuffValueTypes flag, DateTime currentTime, Action<BuffStat> func,
            BuffValueTypes specificFlag = BuffValueTypes.All)
        {
            if (!IsActive(currentTime) || !specificFlag.HasFlag(Flag))
            {
                return;
            }

            flag |= Flag;
            func?.Invoke(this);
        }

        public void EncodeForLocal(PacketWriter pw, ref BuffValueTypes flag, DateTime currentTime,
            BuffValueTypes specificFlag = BuffValueTypes.All)
        {
            if (!IsActive(currentTime) || !specificFlag.HasFlag(Flag))
            {
                return;
            }

            flag |= Flag;
            pw.WriteShort(Value);
            pw.WriteInt(ReferenceId);
            pw.WriteShort((short)((ExpireTime - currentTime).TotalMilliseconds / 100));
        }
    }
}
