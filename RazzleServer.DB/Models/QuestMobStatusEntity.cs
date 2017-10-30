namespace RazzleServer.DB.Models
{
    public class QuestMobStatusEntity
    {
        public int ID { get; set; }
        public int QuestStatusID { get; set; }
        public int Mob { get; set; }
        public int Count { get; set; }
    }
}
