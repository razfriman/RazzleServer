using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class KeyMapEntity
    {
        [Key]
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public int Action { get; set; }
        public byte Key { get; set; }
        public byte Type { get; set; }
        
        public CharacterEntity Character { get; set; }
    }
}
