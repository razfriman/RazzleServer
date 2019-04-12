using System;
using System.Threading;
using System.Threading.Tasks;

namespace RazzleServer.Common.Util
{
    public class TaskRunner
    {
        public static CancellationTokenSource Run(Func<Task> callback, TimeSpan delay = default)
        {
            var cts = new CancellationTokenSource();
            Run(callback, delay, cts.Token);
            return cts;
        }

        public static CancellationTokenSource Run(Action callback, TimeSpan delay = default)
        {
            var cts = new CancellationTokenSource();
            Run(callback, delay, cts.Token);
            return cts;
        }

        public static CancellationTokenSource RunRepeated(Func<Task> callback, TimeSpan interval = default,
            TimeSpan initialDelay = default)
        {
            var cts = new CancellationTokenSource();
            RunRepeated(callback, interval, initialDelay, cts.Token);
            return cts;
        }

        public static CancellationTokenSource RunRepeated(Action callback, TimeSpan interval = default,
            TimeSpan initialDelay = default)
        {
            var cts = new CancellationTokenSource();
            RunRepeated(callback, interval, initialDelay, cts.Token);
            return cts;
        }

        public static void Run(Func<Task> callback, TimeSpan delay, CancellationToken ct)
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(delay, ct);
                if (!ct.IsCancellationRequested)
                {
                    await callback();
                }
            }, ct);
        }

        public static void Run(Action callback, TimeSpan delay, CancellationToken ct)
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(delay, ct);
                if (!ct.IsCancellationRequested)
                {
                    callback();
                }
            }, ct);
        }

        public static void RunRepeated(Func<Task> callback, TimeSpan interval, TimeSpan initialDelay,
            CancellationToken ct)
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(initialDelay, ct);
                while (!ct.IsCancellationRequested)
                {
                    await callback();
                    await Task.Delay(interval, ct);
                }
            }, ct);
        }

        public static void RunRepeated(Action callback, TimeSpan interval, TimeSpan initialDelay,
            CancellationToken ct)
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(initialDelay, ct);
                while (!ct.IsCancellationRequested)
                {
                    callback();
                    await Task.Delay(interval, ct);
                }
            }, ct);
        }
    }
}
