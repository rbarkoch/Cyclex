using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Cyclex
{
    public class CyclicExecutive : ICyclicExecutive
    {
        public delegate void CompletedEventHandler(object sender, CyclicExecutiveCycleCompletedEventArgs args);
        public delegate void OverflowEventHandler(object sender, CyclicExecutiveOverflowEventArgs args);
        public delegate void ExceptionEventHandler(object sender, CyclicExecutiveExceptionEventArgs args);

        public event EventHandler? Stopped;
        public event EventHandler? Started;
        public event CompletedEventHandler? CycleCompleted;
        public event OverflowEventHandler? CycleOverflow;
        public event ExceptionEventHandler? CycleException;

        public TimeSpan CycleTime { get; set; } = TimeSpan.FromSeconds(1);
        public bool IsRunning { get; private set; } = false;
        public TimeSpan StopTimeout { get; set; } = Timeout.InfiniteTimeSpan;
        public bool StopOnOverflow { get; set; } = false;
        public bool StopOnException { get; set; } = true;
        public bool PropogateException { get; set; } = true;

        private Action _methodToRun;
        private Stopwatch _stopwatch = new Stopwatch();

        private AutoResetEvent _stopLoopEvent = new AutoResetEvent(false);

        public CyclicExecutive(Action methodToRun)
        {
            if(methodToRun == null)
            {
                throw new ArgumentNullException(nameof(methodToRun));
            }

            _methodToRun = methodToRun;
        }

        public void Start()
        {
            IsRunning = true;
            Loop();
        }

        public void Stop()
        {
            _stopLoopEvent.Set();
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

                    if(PropogateException)
                    {
                        IsRunning = false;
                        throw ex;
                    }

                    if(StopOnException)
                    {
                        shouldStop |= true;
                    }
                }
                _stopwatch.Stop();

                CycleCompleted?.Invoke(this, new CyclicExecutiveCycleCompletedEventArgs(_stopwatch.Elapsed, CycleTime));

                TimeSpan timeDifference = CycleTime - _stopwatch.Elapsed;
                remainingTime = timeDifference > TimeSpan.Zero ? timeDifference : TimeSpan.Zero;

                if(timeDifference < TimeSpan.Zero)
                {
                    CycleOverflow?.Invoke(this, new CyclicExecutiveOverflowEventArgs(_stopwatch.Elapsed, CycleTime));

                    if(StopOnOverflow)
                    {
                        shouldStop |= true;
                    }
                }
            } while (!shouldStop && !_stopLoopEvent.WaitOne(remainingTime));

            IsRunning = false;
            Stopped?.Invoke(this, EventArgs.Empty);
        }
    }

    public class CyclicExecutiveExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public CyclicExecutiveExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    public class CyclicExecutiveOverflowEventArgs : EventArgs
    {
        public TimeSpan ExecutionTime { get; private set; }
        public TimeSpan CycleTime { get; private set; }
        public TimeSpan Overflow => ExecutionTime - CycleTime;

        public CyclicExecutiveOverflowEventArgs(TimeSpan executionTime, TimeSpan cycleTime)
        {
            ExecutionTime = executionTime;
            CycleTime = cycleTime;
        }
    }

    public class CyclicExecutiveCycleCompletedEventArgs : EventArgs
    {
        public TimeSpan ExecutionTime { get; private set; }
        public TimeSpan CycleTime { get; private set; }

        public CyclicExecutiveCycleCompletedEventArgs(TimeSpan executionTime, TimeSpan cycleTime)
        {
            ExecutionTime = executionTime;
            CycleTime = cycleTime;
        }
    }
}
