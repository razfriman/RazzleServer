using System.Collections.Generic;

namespace RazzleServer.Wz
{
    public abstract class WzExtended : WzImageProperty
    {
        public override Dictionary<string, WzImageProperty> WzProperties { get; set; } =
            new Dictionary<string, WzImageProperty>();
    }
}
