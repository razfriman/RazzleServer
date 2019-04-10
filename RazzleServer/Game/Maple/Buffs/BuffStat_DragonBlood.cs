using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Buffs
{
    public class BuffStat_DragonBlood : BuffStat
    {
        private readonly Character Owner;
        private long tLastDamaged;

        public BuffStat_DragonBlood(BuffValueTypes flag, Character own) : base(flag)
        {
            Owner = own;
        }

        public override BuffValueTypes Set(int referenceId, short nValue, long expireTime)
        {
            tLastDamaged = CurrentTime;
            return base.Set(referenceId, nValue, expireTime);
        }

        public override bool TryReset(long currentTime, ref BuffValueTypes flag)
        {
            if (CurrentTime - tLastDamaged >= 4000)
            {
                Owner.DamageHP(N);
                tLastDamaged = CurrentTime;
            }

            return base.TryReset(currentTime, ref flag);
        }

        public override bool EncodeForCC(Packet pr, ref BuffValueTypes flag, long currentTime)
        {
            if (base.EncodeForCC(pr, ref flag, currentTime))
            {
                pr.WriteLong(tLastDamaged);
                return true;
            }

            return false;
        }

        public override bool DecodeForCC(Packet pr, BuffValueTypes flag)
        {
            if (base.DecodeForCC(pr, flag))
            {
                tLastDamaged = pr.ReadLong();
                return true;
            }

            return false;
        }
    }
}