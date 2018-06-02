using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class LootEntity
    {
        [Key]
        public int Id { get; set; }
        public int MobId { get; set; }
        public bool IsMeso { get; set; }
        public int ItemId { get; set; }
        public int MinimumQuantity { get; set; }
        public int MaximumQuantity { get; set; }
        public int QuestId { get; set; }
        public int Chance { get; set; }
    }
}
