using System.Collections.Generic;
using System.Linq;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapPortals : MapObjects<Portal>
    {
        public MapPortals(Map map) : base(map) { }

        public Portal this[string label]
        {
            get
            {
                foreach (Portal portal in this)
                {
                    if (portal.Label.ToLower() == label.ToLower())
                    {
                        return portal;
                    }
                }

                throw new KeyNotFoundException();
            }
        }

        public bool ContainsPortal(string label) => this.Any(x => x.Label.Equals(label, System.StringComparison.InvariantCultureIgnoreCase));

        protected override int GetKeyForItem(Portal item)
        {
            return item.ID;
        }
    }
}
