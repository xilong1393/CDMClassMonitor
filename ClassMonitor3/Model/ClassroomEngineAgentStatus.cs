using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model
{
    public class ClassroomEngineAgentStatus:ClassroomEngineStatus
    {
        public DateTime? IPCReportTime { get; set; }
        public string AgentVersion { get; set; }
        public string AgentStatus { get; set; }
        public long? AgentRecordData { get; set; }
        public string ClassroomName { get; set; }
        public string ClassroomGroupName { get; set; }
        public int? ClassTypeID { get; set; }

    }
}
