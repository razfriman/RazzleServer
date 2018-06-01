using System;

namespace RazzleServer.Data
{
    public class SkillEntity
    {
        public int Id { get; set; }
        public DateTime Expiration { get; set; }
        public int CharacterId { get; set; }
        public int SkillId { get; set; }
        public byte Level { get; set; }
        public byte MasterLevel { get; set; }
    }
}
