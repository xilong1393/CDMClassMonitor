using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model
{
    public class PPCIPCReturnParameter
    {
        public CommandParameter PPCReturnParameter { get; set; }
        public CommandParameter IPCReturnParameter { get; set; }
        public PPCIPCReturnParameter(CommandParameter ppc, CommandParameter ipc)
        {
            PPCReturnParameter = ppc;
            IPCReturnParameter = ipc;
        }
    }
}
