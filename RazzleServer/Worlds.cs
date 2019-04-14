using RazzleServer.Game;

namespace RazzleServer
{
    public class Worlds : AWorlds
    {
        public override byte GetKey(AWorld item) => item.Id;
    }
}
