using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.DB
{
    public class QuestCustomData
    {
        public int ID { get; set; }
        public int CharacterID { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
