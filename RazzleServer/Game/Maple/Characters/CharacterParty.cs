using System.Collections.ObjectModel;
using RazzleServer.Game.Maple.Interaction;

namespace RazzleServer.Game.Maple.Characters
{
    public class CharacterParty : KeyedCollection<int, PartyMember>
    {
        protected override int GetKeyForItem(PartyMember item) => item.ID;
    }
}
