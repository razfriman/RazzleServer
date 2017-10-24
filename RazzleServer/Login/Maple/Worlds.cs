using System.Collections.ObjectModel;

namespace RazzleServer.Login.Maple
{
    public sealed class Worlds : KeyedCollection<byte, World>
    {
        protected override byte GetKeyForItem(World item) => item.ID;
    }
}
