namespace RazzleServer.DB.Models
{
    public class InventorySlotEntity
    {
        public int ID { get; set; }
        public int CharacterID { get; set; }
        public byte EquipSlots { get; set; }
        public byte UseSlots { get; set; }
        public byte SetupSlots { get; set; }
        public byte EtcSlots { get; set; }
        public byte CashSlots { get; set; }
    }
}
