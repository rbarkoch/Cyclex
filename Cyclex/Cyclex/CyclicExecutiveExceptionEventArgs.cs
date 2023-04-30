using System;

namespace Cyclex
{
    public class CyclicExecutiveExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; private set; }
        public CyclicExecutiveExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
