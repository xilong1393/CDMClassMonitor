using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model
{
    class ClassroomGroup
    {
        public int ClassroomGroupID { get; set; }
        public string ClassroomgroupName { get; set; }
        public int FileServerID { get; set; }

        public int Status { get; set; }
        public bool IsNoIPCGroup { get; set; }
    }
}
