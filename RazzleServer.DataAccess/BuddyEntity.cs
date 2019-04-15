using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class BuddyEntity
    {
        [Key] public int Id { get; set; }
        [Required] public int CharacterId { get; set; }
        [Required] public int BuddyCharacterId { get; set; }
        [Required] public int AccountId { get; set; }
        [Required] public int BuddyAccountId { get; set; }
        public bool IsRequest { get; set; }

        public CharacterEntity Character { get; set; }
        public CharacterEntity BuddyCharacter { get; set; }
        public AccountEntity Account { get; set; }
        public AccountEntity BuddyAccount { get; set; }
    }
}
