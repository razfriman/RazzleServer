using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazzleServer.Data.WZ
{
    public class WzPet : WzItem
    {
        public int Life { get; set; }        
        public int Hungry { get; set; }
        public bool MultiPet { get; set; }
        public bool Permanent { get; set; }
        public bool PickUpItem { get; set; }
        public bool AutoBuff { get; set; }

    }
}
