using System;
using System.Linq;
using Newtonsoft.Json;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapPortals : MapObjects<Portal>
    {
        public MapPortals(Map map) : base(map) { }

        public MapPortals() : base() { }

        [JsonIgnore]
        public Portal this[string label]
        {
            get
            {
                foreach (var portal in Values)
                {
                    if (portal.Label.ToLower() == label.ToLower())
                    {
                        return portal;
                    }
                }

                return null;
            }
        }

        public bool ContainsPortal(string label) => Values.Any(x => x.Label.Equals(label, StringComparison.InvariantCultureIgnoreCase));

        public override int GetId(Portal item) => item.Id;
    }
}
