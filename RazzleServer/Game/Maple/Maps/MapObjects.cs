using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class MapObjects<T> where T : MapObject
    {
        [JsonIgnore]
        public Map Map { get; }

        [JsonProperty]
        private Dictionary<int, T> Objects { get; set; } = new Dictionary<int, T>();

        protected MapObjects()
        {

        }

        protected MapObjects(Map map)
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

        public void Add(T item)
        {
            item.Map = Map;

            if (!(item is Character) && !(item is Portal))
            {
                item.ObjectId = Map.AssignObjectId();
            }

            var key = GetId(item);

            Objects[key] = item;
            OnItemAdded(item);
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

                OnItemRemoved(item);

                item.Map = null;

                if (!(item is Character) && !(item is Portal))
                {
                    item.ObjectId = -1;
                }

                Objects.Remove(key);
                return true;
            }

            return false;
        }

        public virtual void OnItemAdded(T item) { }

        public virtual void OnItemRemoved(T item) { }

        public bool Contains(int id) => Objects.ContainsKey(id);
    }
}
