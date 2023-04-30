using System;

namespace Cyclex
{
    public interface ICyclicExecutive
    {
        // ====== EVENTS ====== //

        /// <summary>
        /// Triggered when the cyclic executive is started.
        /// </summary>
        event EventHandler? Started;

        /// <summary>
        /// Triggered when the cyclic executive has officially stopped.
        /// </summary>
        /// <remarks>
        /// This is not when <see cref="Stop"/> is called. It is when the cycle has completed and exitted.
        /// </remarks>
        event EventHandler? Stopped;

        /// <summary>
        /// Triggered at the end of every cycle and contains metrics about the cycle time.
        /// </summary>
        event EventHandler<CyclicExecutiveCycleCompletedEventArgs>? CycleCompleted;

        /// <summary>
        /// Triggered when an exception happens on the loop method.
        /// </summary>
        event EventHandler<CyclicExecutiveExceptionEventArgs>? CycleException;

        /// <summary>
        /// Triggered when the execution time of the method is longer than the loop time.
        /// </summary>
        event EventHandler<CyclicExecutiveOverflowEventArgs>? CycleOverflow;



        // ====== PROPERTIES ====== //

        /// <summary>
        /// Interval for the cyclic executive loop.
        /// </summary>
        TimeSpan CycleTime { get; set; }

        /// <summary>
        /// Indicates if the cyclic executive is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// 'true' if you want the loop rethrow exceptions from the execution loop. (Default: true)
        /// </summary>
        /// <remarks>
        /// Allowing exceptions to propogate will inherently end the execution loop on exception regardless of the <see cref="StopOnException"/> property.
        /// </remarks>
        bool PropogateException { get; set; }

        /// <summary>
        /// 'true' if you want the loop to end on an exception. (Default: 'true')
        /// </summary>
        bool StopOnException { get; set; }

        /// <summary>
        /// 'true' if you want the cyclic exceutive to automatically exit on an execution overflow. (Default: 'false')
        /// </summary>
        bool StopOnOverflow { get; set; }



        // ====== METHODS ====== //

        /// <summary>
        /// Starts the cyclic exceutive loop. This is a blocking operation for as long as the execution loop is running.
        /// </summary>
        /// <remarks>
        /// You can start the cyclic executive asynchronously by calling this method using <see cref="Task.Run(Action)"/> or any other method of calling a method asynchronously.
        /// </remarks>
        void Start();

        /// <summary>
        /// Triggers the cyclic executive loop to stop.
        /// </summary>
        /// <remarks>
        /// This does not immediately stop the loop, it only indicates to the loop that it should stop when the next cycle is complete. To block until the loop ends, use <see cref="TryWait(TimeSpan)"/>
        /// </remarks>
        void Stop();

        /// <summary>
        /// Blocks execution until the loop is complete.
        /// </summary>
        /// <param name="timeout">Timeout to wait.</param>
        /// <returns>'true' if the loop stopped within the timeout time. 'false' otherwise.</returns>
        bool TryWait(TimeSpan timeout);
    }
}