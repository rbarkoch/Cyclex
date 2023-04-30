using System;
using System.Threading;

namespace Cyclex.Example
{
    class Program
    {
        const int CycleTimeMillis = 5000;

        static Random rand = new Random();
        static CyclicExecutive cyclicExecutive;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            cyclicExecutive = new CyclicExecutive(Loop)
            {
                CycleTime = TimeSpan.FromMilliseconds(CycleTimeMillis),
                PropogateException = false,
                StopOnException = false,
                StopOnOverflow = false
            };

            cyclicExecutive.Started += CyclicExecutive_Started;
            cyclicExecutive.Stopped += CyclicExecutive_Stopped;
            cyclicExecutive.CycleCompleted += CyclicExecutive_CycleCompleted;
            cyclicExecutive.CycleOverflow += CyclicExecutive_CycleOverflow;
            cyclicExecutive.CycleException += CyclicExecutive_CycleException;

            cyclicExecutive.Start();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Log("Stopping...");
            Log();
            cyclicExecutive.Stop();
            e.Cancel = true;
        }

        static void Loop()
        {
            Log();
            int situation = rand.Next() % 3;
            switch(situation)
            {
                case 0: // Normal execution.
                    Log("Running normally.");
                    Thread.Sleep(rand.Next() % CycleTimeMillis);
                    Log();
                    break;
                case 1: // Overflow execution.
                    Log("Running too long.");
                    Thread.Sleep( (rand.Next() % 1000) + CycleTimeMillis);
                    Log();
                    break;
                case 2: // Exception.
                    Log("Throwing an exception!");
                    Log();
                    throw new Exception("This was a planned exception!");
            }
        }

        static void Log(string format = "", params object[] args)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss")}] {string.Format(format, args)}");
        }


        private static void CyclicExecutive_Started(object sender, EventArgs e)
        {
            Log("Cyclic executive was started!");
            Log();
        }

        private static void CyclicExecutive_Stopped(object sender, EventArgs e)
        {
            Log("Cyclic executive was stopped!");
        }

        private static void CyclicExecutive_CycleCompleted(object sender, CyclicExecutiveCycleCompletedEventArgs args)
        {
            Log("Cyclic executive completed a cycle.");
            Log($"    Execution Time: {args.ExecutionTime.TotalMilliseconds} ms");
            Log($"    Cycle Time:     {args.ActualCycleTime.TotalMilliseconds} ms");
            Log($"    Utilization:    {args.Utilitization * 100:N0}%");
            Log();
        }

        private static void CyclicExecutive_CycleException(object sender, CyclicExecutiveExceptionEventArgs args)
        {
            Log("Cyclic executive threw an exception!");
            Log($"    Exception: {args.Exception.Message}");
            Log();
        }

        private static void CyclicExecutive_CycleOverflow(object sender, CyclicExecutiveOverflowEventArgs args)
        {
            Log("Cyclic executive overflowed.");
            Log($"    Execution Time: {args.ExecutionTime.TotalMilliseconds} ms");
            Log($"    Cycle Time:     {args.TargetCycleTime.TotalMilliseconds} ms");
            Log($"    Cycle Time:     {args.Overflow.TotalMilliseconds} ms");
            Log();
        }

        
    }
}
