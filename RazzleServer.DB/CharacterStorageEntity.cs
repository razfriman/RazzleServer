namespace RazzleServer.Data
{
    public class CharacterStorageEntity
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int Meso { get; set; }
        public byte Slots { get; set; }
    }
}