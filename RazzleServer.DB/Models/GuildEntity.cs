using System.ComponentModel.DataAnnotations;

namespace RazzleServer.DB.Models
{
    public class GuildEntity
    {
        public int ID { get; set; }
        public int Leader { get; set; }
        public int Logo { get; set; }
        public int LogoBG { get; set; }
        public int Capacity { get; set; }
        public int GP { get; set; }
        public int Signature { get; set; }
        public short LogoColor { get; set; }
        public short LogoBGColor { get; set; }
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
    }
}
