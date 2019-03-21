using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class QuestMobStatusEntity
    {
        [Key]
        public int Id { get; set; }
        public int QuestStatusId { get; set; }
        public int Mob { get; set; }
        public int Count { get; set; }
        
        public QuestStatusEntity QuestStatus { get; set; }
    }
}
