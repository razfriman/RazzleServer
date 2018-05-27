using System;
using System.Collections.Generic;
using System.Threading;

namespace RazzleServer.Common.Util
{
    public class PendingKeyedQueue<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
    {
        private ManualResetEvent _queueDone = new ManualResetEvent(false);

        public void Enqueue(TKey key, TValue value)
        {
            Add(key, value);

            _queueDone.Set();
        }

        public TValue Dequeue(TKey key)
        {
            while (!ContainsKey(key))
            {
                _queueDone.WaitOne();
            }

            var value = this[key];

            Remove(key);

            _queueDone.Reset();

            return value;
        }

        public void Dispose() => _queueDone.Dispose();
    }
}