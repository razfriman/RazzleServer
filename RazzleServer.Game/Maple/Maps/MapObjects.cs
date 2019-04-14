using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class MapObjects<T> : MapleKeyedCollection<int, T> where T : IMapObject
    {
        [JsonIgnore] public Map Map { get; }

        protected MapObjects() { }

        protected MapObjects(Map map) : this() => Map = map;

        public IEnumerable<T> GetInRange(IMapObject reference, int range) => Objects.Values.Where(loopObject =>
            reference.Position.DistanceFrom(loopObject.Position) <= range);

        public override int GetKey(T item) => item.ObjectId;

        public override void Add(T item)
        {
            item.Map = Map;

            if (!(item is Character) && !(item is Portal))
            {
                item.ObjectId = Map.AssignObjectId();
            }

            var key = GetKey(item);

            Objects[key] = item;
        }

        public override void Remove(T item)
        {
            var key = GetKey(item);
            if (Contains(key))
            {
                item.Map = null;

                if (!(item is Character) && !(item is Portal))
                {
                    item.ObjectId = -1;
                }

                Objects.TryRemove(key, out _);
            }
        }

        public override void Remove(int key)
        {
            if (Contains(key))
            {
                Remove(this[key]);
            }
        }
    }
}
