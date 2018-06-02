using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class InventoryEquipEntity
    {
        [Key]
        public long Id { get; set; }
        public long InventoryItemId { get; set; }
    }
}
