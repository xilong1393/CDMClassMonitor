using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model
{
    public class ClassroomView
    {
        [DisplayName("Room Type")]
        public string ClassroomTypeName { get; set; }
        [Browsable(false)]
        public int ClassroomID { get; set; }
        [DisplayName("Classroom")]
        public string ClassroomName { get; set; }
        [Browsable(false)]
        public string PPCPublicIP { get; set; }
        [Browsable(false)]
        public string IPCPublicIP { get; set; }
        [Browsable(false)]
        public DateTime? PPCReportTime { get; set; }
        [Browsable(false)]
        public DateTime? IPCReportTime { get; set; }
        [Browsable(false)]
        public long? AVCaputureFrames { get; set; }
        [Browsable(false)]
        public int AgentRecordData { get; set; }
        [Browsable(false)]
        public int PPCPort { get; set; }
        [DisplayName("PPC (Engine)")]
        public string EngineStatus { get; set; }
        [DisplayName("IPC (Agent)")]
        public string AgentStatus { get; set; }
        [Browsable(false)]//true->--
        public bool NoIPC { get; set; }
        [DisplayName("Conn")]//true->--
        public string PPCConnectionStatus { get; set; }
        [DisplayName("Course")]
        public string CourseName { get; set; }
        public string ScreenCaptureStatus { get; set; }
        [DisplayName("AV")]
        public string AVStatus { get; set; }
        public string WB
        {
            get
            {
                if (WBStatus == "OFF")
                {
                    return "OFF";
                }
                else if (WBStatus == "ON")
                {
                    if (WBNumber == ClassRoomWBNumber)
                        return WBStatus + " (" + WBNumber + ")";
                    else
                        return
                            WBStatus + " (" + ClassRoomWBNumber + "/" + WBNumber + ")";
                }
                else
                {
                    if (WBNumber == ClassRoomWBNumber)
                        return "CAP (" + WBStatus + ")";
                    else
                        return
                            "CAP (" + ClassRoomWBNumber + "/" + WBNumber + ")";
                }
            }
        }
        [Browsable(false)]
        public int WBNumber { get; set; }
        [Browsable(false)]
        public string WBStatus { get; set; }
        [Browsable(false)]
        public int ClassRoomWBNumber { get; set; }
        [Browsable(false)]
        public int KaptivoNumber { get; set; }
        [Browsable(false)]
        public int ActiveKaptivoNumber { get; set; }
        //add off no kaptivo --;one kaptivo on(off);two kaptivo on/off(depending on time)
        //idle->reporttime->OFF/ datasize -1->down !-1->idle
        public string Kaptivo
        {
            get
            {
                if (KaptivoNumber == 2)
                    return Kaptivo1Status + "/" + Kaptivo2Status;
                else if (KaptivoNumber == 1)
                    return Kaptivo1Status;
                else
                    return "--";
            }
        }
        [Browsable(false)]
        public string Status { get; set; }
        [DisplayName("Disk(M)")]
        public int FreeDisk { get; set; }
        [DisplayName("On Schedule Now")]
        public string OnScheduleNow { get; set; }
        
        [Browsable(false)]
        public string Kaptivo1Status { get; set; }
        [Browsable(false)]
        public string Kaptivo2Status { get; set; }
        [Browsable(false)]
        public int Kaptivo1DataSize { get; set; }
        [Browsable(false)]
        public int Kaptivo2DataSize { get; set; }
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                ClassroomView c = (ClassroomView)obj;
                return (ClassroomTypeName == c.ClassroomTypeName)
                    && (ClassroomID == c.ClassroomID)
                    && (OnScheduleNow == c.OnScheduleNow)
                    && (ClassroomName == c.ClassroomName)
                    && (EngineStatus == c.EngineStatus)
                    && (AgentStatus == c.AgentStatus)
                    && (CourseName == c.CourseName)
                    && (WB == c.WB)
                    && (WBStatus==c.WBStatus)
                    && (PPCConnectionStatus == c.PPCConnectionStatus)
                    && (AVStatus == c.AVStatus)
                    && (Kaptivo == c.Kaptivo)
                    && (NoIPC == c.NoIPC)
                    && (ScreenCaptureStatus == c.ScreenCaptureStatus)
                    && (Kaptivo1DataSize == c.Kaptivo1DataSize)
                    && (Kaptivo2DataSize == c.Kaptivo2DataSize)
                    && (ClassroomID == c.ClassroomID);
            }
        }
        
        public override int GetHashCode()
        {
            return ClassroomID;
        }
    }
}
