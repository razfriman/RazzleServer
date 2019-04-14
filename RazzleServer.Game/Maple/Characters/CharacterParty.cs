using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Interaction;

namespace RazzleServer.Game.Maple.Characters
{
    public class CharacterParty : MapleKeyedCollection<int, PartyMember>
    {
        public override int GetKey(PartyMember item) => item.Id;
    }
}
