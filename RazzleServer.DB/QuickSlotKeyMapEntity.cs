using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class QuickSlotKeyMapEntity
    {
        [Key]
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public byte Index { get; set; }
        public int Key { get; set; }
    }
}
