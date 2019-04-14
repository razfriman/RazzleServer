using System;
using RazzleServer.Common.Constants;

namespace RazzleServer.Game.Maple.Buffs
{
    public class BuffStatComboAttack : BuffStat
    {
        public int MaxOrbs { get; set; }

        public BuffStatComboAttack(BuffValueTypes flag) : base(flag)
        {
        }

        public override BuffValueTypes Set(int referenceId, short value, DateTime expireTime)
        {
            MaxOrbs = value;
            return base.Set(referenceId, 1, expireTime);
        }
    }
}
