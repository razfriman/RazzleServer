using System;
using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class AccountEntity
    {
        [Key]
        public int Id { get; private set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public byte Gender { get; set; }
        public string Pin { get; set; }
        public bool IsBanned { get; set; }
        public bool IsMaster { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Creation { get; set; }
        public int MaxCharacters { get; set; }
    }
}