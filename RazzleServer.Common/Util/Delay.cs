using System;
using System.Threading;
using System.Threading.Tasks;

namespace RazzleServer.Common.Util
{
    public sealed class Delay : IDisposable
    {
        private readonly Action _mAction;
        private readonly Timer _mTimer;
        private TimeSpan Period { get; }
        private DateTime Next { get; set; }

        public static async Task ExecuteViaTask(Action action, int timeout)
        {
            await Task.Delay(timeout);
            action.Invoke();
        }

        public static Delay Execute(Action action, int timeout) => new Delay(action, timeout);

        public Delay(Action action, int timeout, int repeat = Timeout.Infinite)
        {
            _mAction = action;
            Period = TimeSpan.FromMilliseconds(repeat);
            Next = DateTime.Now.AddMilliseconds(timeout);
            if (timeout >= 0)
            {
                _mTimer = new Timer(Callback, null, timeout, repeat);
            }
        }

        private void Callback(object state)
        {
            Next = DateTime.Now.Add(Period);

            _mAction();
        }

        public void Dispose() => _mTimer?.Dispose();
    }
}
