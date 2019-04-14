using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Characters
{
    public class CharacterDamage
    {
        public MapleRandom Random { get; set; } = new MapleRandom();
        public GameCharacter Parent { get; }

        public CharacterDamage(GameCharacter parent)
        {
            Parent = parent;
        }
    }
}
