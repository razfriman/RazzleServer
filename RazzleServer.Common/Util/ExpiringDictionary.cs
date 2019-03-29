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
            public T Value { get; }
            public DateTime Expiry { get; }
            public ExpiringValueHolder(T value, TimeSpan expiresAfter)
            {
                Value = value;
                Expiry = DateTime.UtcNow.Add(expiresAfter);
            }

            public override string ToString() => Value.ToString();

            public override int GetHashCode() => Value.GetHashCode();
        }

        private readonly Dictionary<TKey, ExpiringValueHolder<TValue>> _innerDictionary;
        private readonly TimeSpan _expiryTimeSpan;

        private void DestoryExpiredItems(TKey key)
        {
            if (!_innerDictionary.ContainsKey(key))
            {
                return;
            }

            var value = _innerDictionary[key];

            if (value.Expiry < DateTime.UtcNow)
            {
                //Expired, nuke it in the background and continue
                _innerDictionary.Remove(key);
            }
        }

        public ExpiringDictionary(TimeSpan expiresAfter)
        {
            _expiryTimeSpan = expiresAfter;
            _innerDictionary = new Dictionary<TKey, ExpiringValueHolder<TValue>>();
        }

        public void Add(TKey key, TValue value)
        {
            DestoryExpiredItems(key);

            _innerDictionary.Add(key, new ExpiringValueHolder<TValue>(value, _expiryTimeSpan));
        }

        public bool ContainsKey(TKey key)
        {
            DestoryExpiredItems(key);

            return _innerDictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            DestoryExpiredItems(key);

            return _innerDictionary.Remove(key);
        }

        public ICollection<TKey> Keys => _innerDictionary.Keys;

        public bool TryGetValue(TKey key, out TValue value)
        {
            var returnval = false;
            DestoryExpiredItems(key);

            if (_innerDictionary.ContainsKey(key))
            {
                value = _innerDictionary[key].Value;
                returnval = true;
            }
            else { value = default; }

            return returnval;
        }

        public ICollection<TValue> Values => _innerDictionary.Values.Select(vals => vals.Value).ToList();

        public TValue this[TKey key]
        {
            get
            {
                DestoryExpiredItems(key);
                return _innerDictionary[key].Value;
            }
            set
            {
                DestoryExpiredItems(key);
                _innerDictionary[key] = new ExpiringValueHolder<TValue>(value, _expiryTimeSpan);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            DestoryExpiredItems(item.Key);

            _innerDictionary.Add(item.Key, new ExpiringValueHolder<TValue>(item.Value, _expiryTimeSpan));
        }

        public void Clear() => _innerDictionary.Clear();

        public int Count => _innerDictionary.Count;

        public bool IsReadOnly => false;

        public bool Contains(KeyValuePair<TKey, TValue> item) => _innerDictionary.ContainsKey(item.Key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => _innerDictionary.Remove(item.Key);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _innerDictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _innerDictionary.Select(x => new KeyValuePair<TKey, TValue>(x.Key, x.Value.Value)).GetEnumerator();
    }
}
