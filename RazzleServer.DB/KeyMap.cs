using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.DB
{
    public class KeyMap
    {
        public int ID { get; set; }

        public int CharacterID { get; set; }
        public int Action { get; set; }

        public byte Key { get; set; }
        public byte Type { get; set; }
    }
}
