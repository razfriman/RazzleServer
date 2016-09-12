using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazzleServer.Data.WZ
{
    public class WzRecipe
    {
        public int ReqSkill { get; set; }

        public byte ReqSkillLevel { get; set; }
        public byte IncProficiency { get; set; }
        public byte IncFatigue { get; set; }

        public List<Item> ReqItems = new List<Item>();
        public List<Item> CreateItems = new List<Item>();

        public class Item
        {
            public int ItemId { get; set; }
            public short Count { get; set; }
            public byte Chance { get; set; }
        }
    }
}
