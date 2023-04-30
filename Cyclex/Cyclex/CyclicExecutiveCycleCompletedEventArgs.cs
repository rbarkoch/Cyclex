using System;

namespace Cyclex
{
    public class CyclicExecutiveCycleCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Time spent within the loop executing the user action.
        /// </summary>
        public TimeSpan ExecutionTime { get; private set; }

        /// <summary>
        /// Time spent within the cycle.
        /// </summary>
        public TimeSpan ActualCycleTime { get; private set; }

        /// <summary>
        /// Percent of the loop which was utilized by the execution.
        /// </summary>
        public double Utilitization => ExecutionTime.TotalMilliseconds / ActualCycleTime.TotalMilliseconds;

        public CyclicExecutiveCycleCompletedEventArgs(TimeSpan executionTime, TimeSpan actualCycleTime)
        {
            ExecutionTime = executionTime;
            ActualCycleTime = actualCycleTime;
        }
    }
}
