using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model
{
    public class ClassroomInfoView
    {
        //public int ClassroomID { get; set; }
        //public string ClassroomName { get; set; }
        //public string PSClassroomName { get; set; }
        //public int ClassroomGroupID { get; set; }
        //public string PPCPublicIP { get; set; }
        //public string IPCPublicIP { get; set; }
        //public int SvrPortalPageID { get; set; }
        //public string PPCPrivateIP { get; set; }
        //public string IPCPrivateIP { get; set; }
        //public int PPCPort { get; set; }
        //public int IPCPort { get; set; }
        //public int WBNumber { get; set; }
        //public int Status { get; set; }
        //[Browsable(false)]
        public int ClassroomID { get; set; }
        public string ClassroomName { get; set; }
        public string EngineStatus { get; set; }
        [DisplayName("PPCIP")]
        public string PPCPublicIP { get; set; }
        [DisplayName("IPCIP")]
        public string IPCPublicIP { get; set; }

        public string AgentStatus { get; set; }
        public string PPCConnectionStatus { get; set; }
        //course
        public string AVStatus { get; set; }
        public int WBNumber { get; set; }
        public string Status { get; set; }
        //SD
        public int FreeDisk { get; set; }
    }
}
