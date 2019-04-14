using RazzleServer.Server;
using RazzleServer.Server.Server;

namespace RazzleServer.Common
{
    public class Worlds : AWorlds
    {
        public override byte GetKey(AWorld item) => item.Id;
    }
}
