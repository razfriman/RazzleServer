using System.Collections.Generic;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterGuild
    {
        public int Id { get; set; }
        public int Leader { get; set; }
        public int Logo { get; set; }
        public int LogoBg { get; set; }
        public int Capacity { get; set; }
        public int Gp { get; set; }
        public int Signature { get; set; }
        public short LogoColor { get; set; }
        public short LogoBgColor { get; set; }
        public string Name { get; set; }
        public string Notice { get; set; }
        public string Rank1Title { get; set; }
        public string Rank2Title { get; set; }
        public string Rank3Title { get; set; }
        public string Rank4Title { get; set; }
        public string Rank5Title { get; set; }
        public List<Character> Characters { get; private set; }
    }
}
