using System.ComponentModel.DataAnnotations;

namespace RazzleServer.DB.Models
{
    public class QuestStatusEntity
    {
        public int ID { get; set; }
        public int CharacterID { get; set; }
        public int Quest { get; set; }
        public uint CompleteTime { get; set; }
        public byte Status { get; set; }
        [MaxLength(0xFF)]
        public string CustomData { get; set; }
    }
}
