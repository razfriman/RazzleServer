using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RazzleServer.Common.Util
{
    public abstract class MapleKeyedCollection<TKey, TValue> where TValue : class
    {
        [JsonIgnore]
        protected readonly ILogger Logger;

        [JsonProperty]
        protected Dictionary<TKey, TValue> Objects { get; set; } = new Dictionary<TKey, TValue>();

        protected MapleKeyedCollection()
        {
            Logger = LogManager.CreateLogger<MapleKeyedCollection<TKey, TValue>>();
        }

        public TValue this[TKey key] => Objects.ContainsKey(key) ? Objects[key] : null;

        [JsonIgnore]

        public IEnumerable<TValue> Values => Objects.Values;

        [JsonIgnore]
        public int Count => Values.Count();

        public abstract TKey GetKey(TValue item);

        public virtual void Add(TValue item)
        {
            var key = GetKey(item);
            Objects[key] = item;
        }

        public virtual void Remove(TValue item)
        {
            var key = GetKey(item);
            if (Contains(key))
            {
                Objects.Remove(key);
            }
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
