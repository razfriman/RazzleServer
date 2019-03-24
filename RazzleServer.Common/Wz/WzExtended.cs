using System.Collections.Generic;

namespace RazzleServer.Common.Wz
{
    public abstract class WzExtended : WzImageProperty
    {
        public override List<WzImageProperty> WzProperties { get; set; } = new List<WzImageProperty>();
    }
}
