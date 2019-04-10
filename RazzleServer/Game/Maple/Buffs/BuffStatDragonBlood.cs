using System;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Buffs
{
    public class BuffStatDragonBlood : BuffStat
    {
        private readonly Character _owner;
        private DateTime _tLastDamaged;

        public BuffStatDragonBlood(BuffValueTypes flag, Character own) : base(flag)
        {
            _owner = own;
        }

        public override BuffValueTypes Set(int referenceId, short nValue, long expireTime)
        {
            _tLastDamaged = DateTime.UtcNow;
            return base.Set(referenceId, nValue, expireTime);
        }

        public override bool TryReset(long currentTime, ref BuffValueTypes flag)
        {
            if (DateTime.UtcNow - _tLastDamaged >= TimeSpan.FromMilliseconds(4000))
            {
                _owner.PrimaryStats.Health -= Value;
                _tLastDamaged = DateTime.UtcNow;
            }

            return base.TryReset(currentTime, ref flag);
        }
    }
}
