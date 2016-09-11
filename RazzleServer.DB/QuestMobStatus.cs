using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.DB
{
    public class QuestMobStatus
    {
        public int ID { get; set; }
        public int QuestStatusID { get; set; }
        public int Mob { get; set; }
        public int Count { get; set; }
    }
}
