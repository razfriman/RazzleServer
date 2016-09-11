using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazzleServer.DB
{
    public class SkillCooldown
    {
        public int ID { get; set; }
        public long StartTime { get; set; }
        public int Length { get; set; }
        public int CharacterID { get; set; }
        public int SkillID { get; set; }
    }
}
