using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace RazzleServer.Common.Util
{
    public abstract class MapleKeyedCollection<TKey, TValue>
    {
        [JsonProperty] protected ConcurrentDictionary<TKey, TValue> Objects { get; set; } = new ConcurrentDictionary<TKey, TValue>();

        public TValue this[TKey key] => Objects.ContainsKey(key) ? Objects[key] : default;

        [JsonIgnore] public IEnumerable<TValue> Values => Objects.Values;

        [JsonIgnore] public int Count => Values.Count();

        public abstract TKey GetKey(TValue item);

        public virtual void Add(TValue item)
        {
            var key = GetKey(item);
            Objects[key] = item;
        }

        public virtual void Remove(TValue item)
        {
            var key = GetKey(item);
            Objects.TryRemove(key, out _);
        }

        public virtual void Remove(TKey key)
        {
            if (Contains(key))
            {
                Remove(this[key]);
            }
        }

        public virtual void Clear() => Values.Select(x => x).ToList().ForEach(Remove);

        public bool Contains(TKey id) => Objects.ContainsKey(id);
    }
}
