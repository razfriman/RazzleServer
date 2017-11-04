using System.Collections.ObjectModel;
using RazzleServer.Common.Constants;

namespace RazzleServer.Center.Maple
{
    public sealed class Worlds : KeyedCollection<byte, World>
    {
        internal Worlds() { }

        internal World Next(ServerType type)
        {
            lock (this)
            {
                foreach (World loopWorld in this)
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

        protected override byte GetKeyForItem(World item) => item.ID;
    }
}
