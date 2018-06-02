﻿using System.Threading;

namespace RazzleServer.Common.Util
{
    public sealed class WaitableResult<T> where T : struct
    {
        private readonly ManualResetEvent _mEvent;

        public WaitableResult()
        {
            _mEvent = new ManualResetEvent(false);
        }

        public void Wait()
        {
            _mEvent.WaitOne();
        }

        public void Set(T value)
        {
            Value = value;

            _mEvent.Set();
        }

        public T Value { get; private set; }
    }
}