namespace RazzleServer.DB
{
    public class Skill
    {
        public int ID { get; set; }
        public long Expiration { get; set; }
        public int CharacterID { get; set; }
        public int SkillId { get; set; }
        public byte Level { get; set; }
        public byte MasterLevel { get; set; }
        public short SkillExp { get; set; }
    }
}
