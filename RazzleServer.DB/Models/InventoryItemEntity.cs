using System.ComponentModel.DataAnnotations;

namespace RazzleServer.DB.Models
{
    public class InventoryItemEntity
    {
        public long ID { get; set; }
        public int ItemID { get; set; }
        public int CharacterID { get; set; }
        public short Position { get; set; }
        public short Quantity { get; set; }
        public short Flags { get; set; }
        [MaxLength(13)]
        public string Creator { get; set; }
        public string Source { get; set; }
    }
}
