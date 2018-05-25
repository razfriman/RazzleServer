using System;
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
                foreach (var portal in this)
                {
                    if (portal.Label.ToLower() == label.ToLower())
                    {
                        return portal;
                    }
                }

                return null;
            }
        }

        public bool ContainsPortal(string label) => this.Any(x => x.Label.Equals(label, StringComparison.InvariantCultureIgnoreCase));

        protected override int GetKeyForItem(Portal item)
        {
            return item.Id;
        }
    }
}
