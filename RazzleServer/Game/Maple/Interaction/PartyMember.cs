using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Interaction
{
    public class PartyMember
    {
        public int Id { get; private set; }

        public GameCharacter GameCharacter { get; private set; }
    }
}
