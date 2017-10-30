namespace RazzleServer.DB.Models
{
    public class KeyMapEntity
    {
        public int ID { get; set; }
        public int CharacterID { get; set; }
        public int Action { get; set; }
        public byte Key { get; set; }
        public byte Type { get; set; }
    }
}
