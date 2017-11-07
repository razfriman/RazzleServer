namespace RazzleServer.DB.Models
{
    public class CharacterStorageEntity
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public int Meso { get; set; }
        public byte Slots { get; set; }
    }
}