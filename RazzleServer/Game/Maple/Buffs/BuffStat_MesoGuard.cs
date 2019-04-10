using RazzleServer.Common.Constants;

namespace RazzleServer.Game.Maple.Buffs
{
    public class BuffStat_MesoGuard : BuffStat
    {
        public int MesosLeft { get; set; }

        public BuffStat_MesoGuard(BuffValueTypes flag) : base(flag)
        {
        }
    }
}