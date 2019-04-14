using System;
using RazzleServer.Common;
using RazzleServer.Common.Constants;

namespace RazzleServer.Game.Maple.Buffs
{
    public class BuffStatDragonBlood : BuffStat
    {
        private readonly ICharacter _owner;
        private DateTime _tLastDamaged;

        public BuffStatDragonBlood(BuffValueTypes flag, ICharacter own) : base(flag)
        {
            _owner = own;
        }

        public override BuffValueTypes Set(int referenceId, short nValue, DateTime expireTime)
        {
            _tLastDamaged = DateTime.UtcNow;
            return base.Set(referenceId, nValue, expireTime);
        }

        public override bool TryReset(DateTime currentTime, ref BuffValueTypes flag)
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
