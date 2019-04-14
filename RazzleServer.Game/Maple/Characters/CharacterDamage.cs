using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Characters
{
    public class CharacterDamage
    {
        public MapleRandom Random { get; set; } = new MapleRandom();
        public Character Parent { get; }

        public CharacterDamage(Character parent)
        {
            Parent = parent;
        }
    }
}
