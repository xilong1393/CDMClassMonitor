using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Util
{
    class ExecutionResult
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public bool Succ { get; set; }
        public Object Obj { get; set; }
        public string Error { get; set; }
    }
}
