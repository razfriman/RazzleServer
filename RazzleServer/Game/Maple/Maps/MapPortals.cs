using System.Linq;

namespace RazzleServer.Game.Maple.Maps
{
    public sealed class MapPortals : MapObjects<Portal>
    {
        public MapPortals(Map map) : base(map) { }

        public MapPortals() { }

        public Portal this[string label] => Values.FirstOrDefault(x => x.Label == label);

        public bool ContainsPortal(string label) => Values.Any(x => x.Label == label);

        public override int GetId(Portal item) => item.Id;
    }
}
