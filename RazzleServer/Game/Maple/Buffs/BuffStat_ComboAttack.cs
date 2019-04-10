using RazzleServer.Common.Constants;

namespace RazzleServer.Game.Maple.Buffs
{
    public class BuffStat_ComboAttack : BuffStat
    {
        public int MaxOrbs { get; set; }

        public BuffStat_ComboAttack(BuffValueTypes flag) : base(flag)
        {
        }

        public override BuffValueTypes Set(int referenceId, short nValue, long expireTime)
        {
            MaxOrbs = nValue;
            return base.Set(referenceId, 1, expireTime);
        }
    }
}