using RazzleServer.Common.Util;

namespace RazzleServer.Game.Server
{
    public class Worlds : MapleKeyedCollection<byte, World>
    {
        public override byte GetKey(World item) => item.Id;
    }
}
