using RazzleServer.Common.Util;

namespace RazzleServer.Common
{
    public sealed class Worlds : MapleKeyedCollection<byte, World>
    {
        internal Worlds() { }

        public override byte GetKey(World item) => item.Id;
    }
}
