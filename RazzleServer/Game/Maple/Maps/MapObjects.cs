using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Common.Util;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Maps
{
    public abstract class MapObjects<T> where T : MapObject
    {
        [JsonIgnore]
        protected readonly ILogger Logger;

        [JsonIgnore]
        public Map Map { get; }

        [JsonProperty]
        private Dictionary<int, T> Objects { get; set; } = new Dictionary<int, T>();

        protected MapObjects()
        {
            Logger = LogManager.LogByName(GetType().FullName);
        }

        protected MapObjects(Map map)
        {
            Map = map;
            Logger = LogManager.LogByName(GetType().FullName);
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

        public virtual void Add(T item)
        {
            item.Map = Map;

            if (!(item is Character) && !(item is Portal))
            {
                item.ObjectId = Map.AssignObjectId();
            }

            var key = GetId(item);

            Objects[key] = item;
        }

        public virtual void Remove(T item)
        {
            var key = GetId(item);
            if (Contains(key))
            {
                item.Map = null;

                if (!(item is Character) && !(item is Portal))
                {
                    item.ObjectId = -1;
                }

                Objects.Remove(key);
            }
        }

        public virtual void Remove(int key)
        {
            if (Contains(key))
            {
                Remove(this[key]);
            }
        }

        public bool Contains(int id) => Objects.ContainsKey(id);
    }
}
