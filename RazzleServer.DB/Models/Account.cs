namespace RazzleServer.DB.Models
{
    public class Account
    {
        public int ID { get; set; }
        public int MaplePoints { get; set; }
        public int NXPrepaid { get; set; }
        public int NXCredit { get; set; }
        public byte AccountType { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Pic { get; set; }
        public byte Gender { get; set; }
        public int CharacterSlots { get; set; }
    }
}