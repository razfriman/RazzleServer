namespace RazzleServer.Player
{
    public class MigrationData
    {
        public bool ToCashShop { get; set; }
        public byte ToChannel { get; set; }
        public byte ReturnChannel { get; set; }
        public string AccountName { get; set; }
        public int CharacterID { get; set; }
        public MapleCharacter Character { get; set; }        
    }
}