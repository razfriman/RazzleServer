using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazzleServer.Data.WZ
{
    public class WzItemEnhancer : WzItem
    {
        public int Chance { get; set; }

        public readonly Dictionary<string, int> StatEnhancements;

        public readonly List<int> UseableOnIds;

        public WzItemEnhancer(int chance, Dictionary<string, int> statEnhancements, List<int> applicableItemIds)
        {
            Chance = chance;
            StatEnhancements = statEnhancements;
            UseableOnIds = applicableItemIds;
        }
    }
}
