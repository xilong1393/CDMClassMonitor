using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model
{
    public class CommandParameter
    {
        public string ip;
        public int port;
        public bool succ;
        public Object obj;
        public string error;
        public CommandParameter()
        {
            succ = false;
            obj = null;
        }
    }
}
