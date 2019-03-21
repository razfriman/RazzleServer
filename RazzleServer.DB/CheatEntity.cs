using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class CheatEntity
    {
        [Key]
        public int Id { get; set; }

        public int CharacterId { get; set; }

        public int CheatType { get; set; }

        public CharacterEntity Character { get; set; }
    }
}
