using System;
using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class MemoEntity
    {
        [Key] public int Id { get; set; }
        public int CharacterId { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public DateTime Received { get; set; }

        public CharacterEntity Character { get; set; }
    }
}
