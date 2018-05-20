using System;
using System.Threading;

namespace RazzleServer.Common.Util
{
    public sealed class Delay : IDisposable
    {
        private Action mAction;
        private TimeSpan mPeriod;
        private DateTime mNext;
        private readonly Timer mTimer;

        public TimeSpan Period => mPeriod;

        public DateTime Next => mNext;

        public TimeSpan DueTime => mNext - DateTime.Now;

        public static Delay Execute(Action action, int timeout)
        {
            return new Delay(action, timeout, Timeout.Infinite);
        }

        public Delay(Action action, int timeout, int repeat = Timeout.Infinite)
        {
            mAction = action;
            mPeriod = TimeSpan.FromMilliseconds(repeat);
            mNext = DateTime.Now.AddMilliseconds(timeout);
            mTimer = new Timer(Callback, null, timeout, repeat);
        }

        private void Callback(object state)
        {
            mNext = DateTime.Now.Add(mPeriod);

            mAction();
        }

        public void Dispose()
        {
            mTimer.Dispose();
        }
    }
}