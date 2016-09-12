using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.Map.Monster
{
    public class MobDrop
    {
        public int ItemID { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public int QuestID { get; set; }
        public int DropChance { get; set; }
    }
}
