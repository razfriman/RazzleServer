using System.Collections.Generic;
using RazzleServer.Common.Data;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Interaction
{
    public sealed class CharacterGuild : IMapleSavable
    {
        public int Id { get; set; }
        public int Leader { get; set; }
        public int Logo { get; set; }
        public int LogoBG { get; set; }
        public int Capacity { get; set; }
        public int GP { get; set; }
        public int Signature { get; set; }
        public short LogoColor { get; set; }
        public short LogoBGColor { get; set; }
        public string Name { get; set; }
        public string Notice { get; set; }
        public string Rank1Title { get; set; }
        public string Rank2Title { get; set; }
        public string Rank3Title { get; set; }
        public string Rank4Title { get; set; }
        public string Rank5Title { get; set; }
        public List<Character> Characters { get; private set; }

        public void Create()
        {
        }

        public void Save()
        {
        }

        public void Load()
        {
        }
    }
}
