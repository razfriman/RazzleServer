using System;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Items;

namespace RazzleServer.Game.Maple.Characters
{
    public class Pet
    {
        public Item Item { get; set; }
        public string Name { get; set; }
        public byte Level { get; set; }
        public short Closeness { get; set; }
        public byte Fullness { get; set; }
        public DateTime Expiration { get; set; } = DateConstants.Permanent;

        public Point Position { get; set; }
        public byte Stance { get; set; }
        public short Foothold { get; set; }

        public Pet(Item item)
        {
            Item = item;
        }
    }
}
