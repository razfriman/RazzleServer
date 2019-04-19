using RazzleServer.Common.Constants;
using RazzleServer.Wz;

namespace RazzleServer.DataProvider.References
{
    public class MapNpcReference : LifeObjectReference
    {
        public MapNpcReference()
        {
        }

        public MapNpcReference(WzImageProperty img) : base(img, LifeObjectType.Npc)
        {
        }
    }
}
