using System;
using System.Threading;
using System.Threading.Tasks;

namespace MowaInfo.Promises
{
    public class Promise<T> : TaskCompletionSource<T>
    {
        private readonly object _lockObject = new object();
        private DateTimeOffset? _deadline;

        public Promise()
        {
            Init();
        }

        public Promise(object state) : base(state)
        {
            Init();
        }

        public Promise(object state, TaskCreationOptions creationOptions) : base(state, creationOptions)
        {
            Init();
        }

        public Promise(TaskCreationOptions creationOptions) : base(creationOptions)
        {
            Init();
        }

        protected CancellationTokenSource CancellationTokenSource { get; private set; } = new CancellationTokenSource();

        public DateTimeOffset Starting { get; } = DateTimeOffset.Now;

        public DateTimeOffset? Deadline
        {
            get => _deadline;
            set
            {
                _deadline = value;
                lock (_lockObject)
                {
                    if (Countdown.HasValue)
                    {
                        var countdown = Countdown.Value;
                        if (countdown < TimeSpan.Zero)
                        {
                            CancellationTokenSource?.Cancel();
                        }
                        else
                        {
                            CancellationTokenSource?.CancelAfter(countdown);
                        }
                    }
                    else
                    {
                        CancellationTokenSource.CancelAfter(Timeout.Infinite);
                    }
                }
            }
        }

        public TimeSpan? Countdown => Deadline - DateTimeOffset.Now;

        private void Init()
        {
            CancellationTokenSource.Token.Register(() => { SetException(new TimeoutException()); });

            Task.ContinueWith(_ =>
            {
                lock (_lockObject)
                {
                    var source = CancellationTokenSource;
                    CancellationTokenSource = null;
                    source.Dispose();
                }
            });
        }

        public DateTimeOffset? SetTimeout(TimeSpan timeout, TimeOrigin origin)
        {
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                Deadline = null;
            }
            else if (timeout < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, null);
            }
            else
            {
                switch (origin)
                {
                    case TimeOrigin.Begin:
                        Deadline = Starting + timeout;
                        break;

                    case TimeOrigin.Current:
                        Deadline = DateTimeOffset.Now + timeout;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
                }
            }

            return Deadline;
        }

        public DateTimeOffset? SetTimeout(int millisecondsTimeout, TimeOrigin origin)
        {
            return SetTimeout(TimeSpan.FromMilliseconds(millisecondsTimeout), origin);
        }
    }
}
