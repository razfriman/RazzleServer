using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.DB
{
    public class QuickSlotKeyMap
    {
        public int ID { get; set; }
        public int CharacterID { get; set; }
        public byte Index { get; set; }
        public int Key { get; set; }
    }
}
