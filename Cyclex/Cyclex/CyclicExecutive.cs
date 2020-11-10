using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Cyclex
{
    public class CyclicExecutive
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
        public bool StopOnOverflow { get; set; } = false;
        public bool StopOnException { get; set; } = true;
        public bool PropogateException { get; set; } = true;

        private Action _methodToRun;
        private Stopwatch _stopwatch = new Stopwatch();

        private AutoResetEvent _stopLoopEvent = new AutoResetEvent(false);
        private AutoResetEvent _stoppedEvent = new AutoResetEvent(false);

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
                TimeSpan executionTime = _stopwatch.Elapsed;

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
        public TimeSpan TargetCycleTime { get; private set; }
        public TimeSpan Overflow => ExecutionTime - TargetCycleTime;

        public CyclicExecutiveOverflowEventArgs(TimeSpan executionTime, TimeSpan targetCycleTime)
        {
            ExecutionTime = executionTime;
            TargetCycleTime = targetCycleTime;
        }
    }

    public class CyclicExecutiveCycleCompletedEventArgs : EventArgs
    {
        public TimeSpan ExecutionTime { get; private set; }
        public TimeSpan ActualCycleTime { get; private set; }
        public double Utilitization => ExecutionTime.TotalMilliseconds / ActualCycleTime.TotalMilliseconds;

        public CyclicExecutiveCycleCompletedEventArgs(TimeSpan executionTime, TimeSpan actualCycleTime)
        {
            ExecutionTime = executionTime;
            ActualCycleTime = actualCycleTime;
        }
    }
}
