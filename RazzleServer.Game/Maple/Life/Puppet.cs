using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Skills;

namespace RazzleServer.Game.Maple.Life
{
    public class Puppet : Summon
    {
        public int Health { get; private set; }

        public Puppet(Character owner, Skill skill, Point position, bool moveAction) : base(owner, skill,
            position, moveAction) => Health = skill.ParameterA;

        public void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0)
            {
                Parent.Summons.Remove(MapleId);
            }
        }
    }
}
