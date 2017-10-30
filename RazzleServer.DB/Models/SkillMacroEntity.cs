namespace RazzleServer.DB.Models
{
    public class SkillMacroEntity
    {
        public int ID { get; set; }
        public int CharacterID { get; set; }
        public byte Index { get; set; }
        public string Name { get; set; }
        public bool ShoutName { get; set; }
        public int Skill1 { get; set; }
        public int Skill2 { get; set; }
        public int Skill3 { get; set; }
    }
}