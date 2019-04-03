﻿using System;
using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class SkillEntity
    {
        [Key] public int Id { get; set; }
        [Required] public int CharacterId { get; set; }
        public DateTime Expiration { get; set; }
        public DateTime CooldownEnd { get; set; }
        public int SkillId { get; set; }
        public byte Level { get; set; }
        public byte MasterLevel { get; set; }

        public CharacterEntity Character { get; set; }
    }
}
