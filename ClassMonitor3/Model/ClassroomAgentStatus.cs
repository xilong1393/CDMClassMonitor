using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model
{
    class ClassroomAgentStatus
    {
        public int ClassroomID { get; set; }

        public DateTime IPCReportTime { get; set; }

        public DateTime AgentVersion { get; set; }

        public string AgentStatus { get; set; }

        public int AgentRecordData { get; set; }
    }
}
