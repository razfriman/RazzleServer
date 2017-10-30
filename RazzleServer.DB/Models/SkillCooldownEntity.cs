namespace RazzleServer.DB.Models
{
    public class SkillCooldownEntity
    {
        public int ID { get; set; }
        public long StartTime { get; set; }
        public int Length { get; set; }
        public int CharacterID { get; set; }
        public int SkillID { get; set; }
    }
}
