using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class MapObjects<T> where T : MapObject
    {
        [JsonIgnore]
        public Map Map { get; private set; }

        [JsonProperty]
        private Dictionary<int, T> Objects { get; set; } = new Dictionary<int, T>();

        public MapObjects()
        {

        }

        public MapObjects(Map map)
        {
            Map = map;
        }


        public T this[int key] => Objects.ContainsKey(key) ? Objects[key] : null;

        [JsonIgnore]

        public IEnumerable<T> Values => Objects.Values;

        [JsonIgnore]
        public int Count => Values.Count();

        public IEnumerable<T> GetInRange(MapObject reference, int range)
        {
            foreach (var loopObject in Objects.Values)
            {
                if (reference.Position.DistanceFrom(loopObject.Position) <= range)
                {
                    yield return loopObject;
                }
            }
        }

        public virtual int GetId(T item) => item.ObjectId;

        public bool Add(T item)
        {
            var key = GetId(item);

            if (!Objects.ContainsKey(key))
            {
                Objects[key] = item;
                OnItemAdded(item);
                return true;
            }

            return false;
        }

        public bool Remove(T item)
        {
            return Remove(GetId(item));
        }

        public bool Remove(int key)
        {
            if (Objects.ContainsKey(key))
            {
                var item = Objects[key];
                Objects.Remove(key);
                OnItemRemoved(item);
                return true;
            }

            return false;
        }

        public virtual void OnItemAdded(T item) { }

        public virtual void OnItemRemoved(T item) { }

        public bool Contains(int id) => Objects.ContainsKey(id);
    }
}
