using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cyclex
{
    public class CyclicExecutive : ICyclicExecutive
    {
        // ===== EVENTS ===== //

        /// <inheritdoc />
        public event EventHandler? Started;

        /// <inheritdoc />
        public event EventHandler? Stopped;

        /// <inheritdoc />
        public event EventHandler<CyclicExecutiveCycleCompletedEventArgs>? CycleCompleted;

        /// <inheritdoc />
        public event EventHandler<CyclicExecutiveOverflowEventArgs>? CycleOverflow;

        /// <inheritdoc />
        public event EventHandler<CyclicExecutiveExceptionEventArgs>? CycleException;



        // ===== PROPERTIES ====== //

        /// <inheritdoc />
        public TimeSpan CycleTime { get; set; } = TimeSpan.FromSeconds(1);

        /// <inheritdoc />
        public bool IsRunning { get; private set; } = false;

        /// <inheritdoc />
        public bool StopOnOverflow { get; set; } = false;

        /// <inheritdoc />
        public bool StopOnException { get; set; } = true;

        /// <inheritdoc />
        public bool PropogateException { get; set; } = true;



        // ===== FIELDS ===== //

        private Action _methodToRun;
        private Stopwatch _stopwatch = new Stopwatch();

        private AutoResetEvent _stopLoopEvent = new AutoResetEvent(false);
        private AutoResetEvent _stoppedEvent = new AutoResetEvent(false);


        
        // ===== CONSTRUCTOR ====== //

        public CyclicExecutive(Action methodToRun)
        {
            if (methodToRun == null)
            {
                throw new ArgumentNullException(nameof(methodToRun));
            }

            _methodToRun = methodToRun;
        }



        // ===== METHODS ===== //

        /// <inheritdoc />
        public void Start()
        {
            if (IsRunning)
            {
                throw new Exception("Cyclic executive cannot be started when it is already running.");
            }
            IsRunning = true;
            Loop();
        }

        /// <inheritdoc />
        public void Stop()
        {
            _stopLoopEvent.Set();
        }

        /// <inheritdoc />
        public bool TryWait(TimeSpan timeout)
        {
            return _stoppedEvent.WaitOne(timeout);
        }

        private void Loop()
        {
            Started?.Invoke(this, EventArgs.Empty);
            TimeSpan remainingTime;
            bool shouldStop;
            do
            {
                shouldStop = false;
                _stopwatch.Restart();
                try
                {
                    _methodToRun.Invoke();
                }
                catch (Exception ex)
                {
                    CycleException?.Invoke(this, new CyclicExecutiveExceptionEventArgs(ex));

                    if (PropogateException)
                    {
                        IsRunning = false;
                        throw ex;
                    }

                    if (StopOnException)
                    {
                        shouldStop |= true;
                    }
                }

                TimeSpan executionTime = _stopwatch.Elapsed;
                TimeSpan timeDifference = CycleTime - _stopwatch.Elapsed;
                remainingTime = timeDifference > TimeSpan.Zero ? timeDifference : TimeSpan.Zero;

                if (timeDifference < TimeSpan.Zero)
                {
                    CycleOverflow?.Invoke(this, new CyclicExecutiveOverflowEventArgs(_stopwatch.Elapsed, CycleTime));

                    if (StopOnOverflow)
                    {
                        shouldStop |= true;
                    }
                }

                shouldStop |= _stopLoopEvent.WaitOne(remainingTime);

                _stopwatch.Stop();
                TimeSpan actualCycleTime = _stopwatch.Elapsed;

                CycleCompleted?.Invoke(this, new CyclicExecutiveCycleCompletedEventArgs(executionTime, actualCycleTime));
            } while (!shouldStop);

            IsRunning = false;
            _stoppedEvent.Set();
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }
}
