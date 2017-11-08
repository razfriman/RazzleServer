using System.ComponentModel.DataAnnotations;

namespace RazzleServer.DB.Models
{
    public class BuddyEntity
    {
        public int ID { get; set; }
        public int CharacterID { get; set; }
        public int BuddyCharacterID { get; set; }
        public int AccountID { get; set; }
        public int BuddyAccountID { get; set; }
        public bool IsRequest { get; set; }
    }
}
