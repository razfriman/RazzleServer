using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;

namespace RazzleServer.Common
{
    public sealed class Worlds : MapleKeyedCollection<byte, World>
    {
        internal Worlds() { }

        internal World Next(ServerType type)
        {
            lock (this)
            {
                foreach (var loopWorld in Values)
                {
                    if (type == ServerType.Channel && loopWorld.IsFull)
                    {
                        continue;
                    }

                    return loopWorld;
                }

                return null;
            }
        }

        public override byte GetKey(World item) => item.Id;
    }
}
