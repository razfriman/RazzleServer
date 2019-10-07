using ProtoBuf;
using RazzleServer.Common.Constants;
using RazzleServer.Wz;

namespace RazzleServer.DataProvider.References
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
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
