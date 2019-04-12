using RazzleServer.Common.Constants;

namespace RazzleServer.Game.Maple.Buffs
{
    public class BuffStatMesoGuard : BuffStat
    {
        public int MesosLeft { get; set; }

        public BuffStatMesoGuard(BuffValueTypes flag) : base(flag)
        {
        }
    }
}
