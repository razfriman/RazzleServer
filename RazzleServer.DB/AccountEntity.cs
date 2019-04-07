using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class AccountEntity
    {
        [Key] public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public byte Gender { get; set; }
        public byte BanReason { get; set; }
        public bool IsMaster { get; set; }
        public bool IsOnline { get; set; }
        public DateTime Birthday { get; set; }
        public DateTime Creation { get; set; }

        public List<CharacterEntity> Characters { get; set; } 
    }
}
