using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class QuestCustomDataEntity
    {
        [Key] public int Id { get; set; }
        [Required] public int CharacterId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public CharacterEntity Character { get; set; }
    }
}
