using System;
using System.Collections.Generic;
using System.Threading;

namespace RazzleServer.Common.Util
{
    public class PendingKeyedQueue<TKey, TValue> : Dictionary<TKey, TValue>, IDisposable
    {
        private ManualResetEvent QueueDone = new ManualResetEvent(false);

        public void Enqueue(TKey key, TValue value)
        {
            Add(key, value);

            QueueDone.Set();
        }

        public TValue Dequeue(TKey key)
        {
            while (!ContainsKey(key))
            {
                QueueDone.WaitOne();
            }

            TValue value = this[key];

            Remove(key);

            QueueDone.Reset();

            return value;
        }

        public void Dispose() => QueueDone.Dispose();
    }
}