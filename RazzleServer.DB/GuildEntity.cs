﻿using System.ComponentModel.DataAnnotations;

namespace RazzleServer.Data
{
    public class GuildEntity
    {
        [Key]
        public int Id { get; set; }
        public int LeaderId { get; set; }
        public int Logo { get; set; }
        public int LogoBg { get; set; }
        public int Capacity { get; set; }
        public int Gp { get; set; }
        public int Signature { get; set; }
        public short LogoColor { get; set; }
        public short LogoBgColor { get; set; }
        [MaxLength(45)]
        public string Name { get; set; }
        [MaxLength(101)]
        public string Notice { get; set; }
        [MaxLength(45)]
        public string Rank1Title { get; set; }
        [MaxLength(45)]
        public string Rank2Title { get; set; }
        [MaxLength(45)]
        public string Rank3Title { get; set; }
        [MaxLength(45)]
        public string Rank4Title { get; set; }
        [MaxLength(45)]
        public string Rank5Title { get; set; }
        
        public CharacterEntity Leader { get; set; }
    }
}
