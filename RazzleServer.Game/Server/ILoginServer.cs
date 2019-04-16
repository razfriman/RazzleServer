using System.Collections.Generic;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Server
{
    public interface ILoginServer : IMapleServer
    {
        void Start();
        bool CharacterExists(string name, byte world);

        List<Character> GetCharacters(byte worldId, int accountId);
    }
}
