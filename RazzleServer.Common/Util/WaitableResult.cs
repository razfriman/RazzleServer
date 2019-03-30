using System.Threading;

namespace RazzleServer.Common.Util
{
    public sealed class WaitableResult<T>
    {
        private readonly ManualResetEvent _mEvent;

        public T Value { get; private set; }

        public WaitableResult() => _mEvent = new ManualResetEvent(false);

        public void Wait() => _mEvent.WaitOne();

        public void Set(T value)
        {
            Value = value;
            _mEvent.Set();
        }
    }
}
