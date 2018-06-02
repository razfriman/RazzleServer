using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class QuestStatusEntity
    {
        [Key]
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public int QuestId { get; set; }
        public uint CompleteTime { get; set; }
        public byte Status { get; set; }
        [MaxLength(0xFF)]
        public string CustomData { get; set; }
    }
}
