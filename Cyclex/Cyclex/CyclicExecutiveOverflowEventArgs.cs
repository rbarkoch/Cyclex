using System;

namespace Cyclex
{
    public class CyclicExecutiveOverflowEventArgs : EventArgs
    {
        /// <summary>
        /// Time spent within the loop executing the user action.
        /// </summary>
        public TimeSpan ExecutionTime { get; private set; }


        /// <summary>
        /// The target cycle time of the cyclic executive.
        /// </summary>
        public TimeSpan TargetCycleTime { get; private set; }

        /// <summary>
        /// Time spent executing over the target cycle time.
        /// </summary>
        public TimeSpan Overflow => ExecutionTime - TargetCycleTime;

        public CyclicExecutiveOverflowEventArgs(TimeSpan executionTime, TimeSpan targetCycleTime)
        {
            ExecutionTime = executionTime;
            TargetCycleTime = targetCycleTime;
        }
    }
}
