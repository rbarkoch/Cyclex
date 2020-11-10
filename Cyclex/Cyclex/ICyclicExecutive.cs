using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Cyclex
{
    public interface ICyclicExecutive
    {
        public TimeSpan CycleTime { get; set; }
        public bool IsRunning { get; }
        public void Start();
        public void Stop();
    }
}
