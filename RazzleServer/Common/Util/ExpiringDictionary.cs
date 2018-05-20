using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RazzleServer.Common.Util
{
    public class ExpiringDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private class ExpiringValueHolder<T>
        {
            public T Value { get; set; }
            public DateTime Expiry { get; private set; }
            public ExpiringValueHolder(T value, TimeSpan expiresAfter)
            {
                Value = value;
                Expiry = DateTime.UtcNow.Add(expiresAfter);
            }

            public override string ToString() { return Value.ToString(); }

            public override int GetHashCode() { return Value.GetHashCode(); }
        };

        private Dictionary<TKey, ExpiringValueHolder<TValue>> innerDictionary;
        private TimeSpan expiryTimeSpan;

        private void DestoryExpiredItems(TKey key)
        {
            if (innerDictionary.ContainsKey(key))
            {
                var value = innerDictionary[key];

                if (value.Expiry < DateTime.UtcNow)
                {
                    //Expired, nuke it in the background and continue
                    innerDictionary.Remove(key);
                }
            }
        }

        public ExpiringDictionary(TimeSpan expiresAfter)
        {
            expiryTimeSpan = expiresAfter;
            innerDictionary = new Dictionary<TKey, ExpiringValueHolder<TValue>>();
        }

        public void Add(TKey key, TValue value)
        {
            DestoryExpiredItems(key);

            innerDictionary.Add(key, new ExpiringValueHolder<TValue>(value, expiryTimeSpan));
        }

        public bool ContainsKey(TKey key)
        {
            DestoryExpiredItems(key);

            return innerDictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            DestoryExpiredItems(key);

            return innerDictionary.Remove(key);
        }

        public ICollection<TKey> Keys => innerDictionary.Keys;

        public bool TryGetValue(TKey key, out TValue value)
        {
            var returnval = false;
            DestoryExpiredItems(key);

            if (innerDictionary.ContainsKey(key))
            {
                value = innerDictionary[key].Value;
                returnval = true;
            }
            else { value = default(TValue); }

            return returnval;
        }

        public ICollection<TValue> Values => innerDictionary.Values.Select(vals => vals.Value).ToList();

        public TValue this[TKey key]
        {
            get
            {
                DestoryExpiredItems(key);
                return innerDictionary[key].Value;
            }
            set
            {
                DestoryExpiredItems(key);
                innerDictionary[key] = new ExpiringValueHolder<TValue>(value, expiryTimeSpan);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            DestoryExpiredItems(item.Key);

            innerDictionary.Add(item.Key, new ExpiringValueHolder<TValue>(item.Value, expiryTimeSpan));
        }

        public void Clear() => innerDictionary.Clear();

        public int Count => innerDictionary.Count;

        public bool IsReadOnly => false;

        public bool Contains(KeyValuePair<TKey, TValue> item) => innerDictionary.ContainsKey(item.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => innerDictionary.Remove(item.Key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => innerDictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => innerDictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();
    }
}