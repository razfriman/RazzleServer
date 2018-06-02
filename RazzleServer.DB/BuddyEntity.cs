using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class BuddyEntity
    {
        [Key]
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public int BuddyCharacterId { get; set; }
        public int AccountId { get; set; }
        public int BuddyAccountId { get; set; }
        public bool IsRequest { get; set; }
    }
}
