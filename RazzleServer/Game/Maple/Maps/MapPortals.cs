using System;
using System.Linq;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapPortals : MapObjects<Portal>
    {
        public MapPortals(Map map) : base(map) { }

        public MapPortals() { }

        public Portal this[string label]
        {
            get
            {
                return Values.FirstOrDefault(x => x.Label.Equals(label, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public bool ContainsPortal(string label) => Values.Any(x => x.Label.Equals(label, StringComparison.InvariantCultureIgnoreCase));

        public override int GetId(Portal item) => item.Id;
    }
}
