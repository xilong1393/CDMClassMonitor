using ClassMonitor3.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Util
{
    class ClassroomStatusUtil
    {
        /*
         * color and meaning: green/blue:info, orange:warning, red:error
         */
        private const string STATUS_ON = "ON";
        private const string STATUS_OFF = "OFF";
        private const string STATUS_REC = "REC";

        private const string STATUS_FALSE = "FALSE";

        private const string ENGINE_IDLE = "IDLE";
        private const string ENGINE_RECORDING = "RECORDING A LECTURE";
        private const string ENGINE_PROCESSING = "PROCESSING DATA";
        private const string ENGINE_WAITING = "WAITING FOR UPLOAD";
        private const string ENGINE_UPLOADING = "UPLOADING DATA";

        private const string AGENT_IDLE = "IDLE";
        private const string AGENT_RECORDING = "RECORDING PC SCREEN";
        private const string AGENT_NOTLOGIN = "NOT LOGIN";
        private const string AGENT_LOADAGENT = "LOAD AGENT";

        private const string AGENT_DISABLE = "DISABLED";
        private const string AGENT_LOADAGENT_DISP_MESSAGE = "AGENT LOADING";
        private const string AGENT_DISABLE_DISP_MESSAGE = "AGENT PAUSE";

        private const string AGENT_NOT_RUNNING = "AGENT NOT RUNNING";

        private const string AV_CAPTURING = "CAPTURING VIDEO";
        private const string AV_INITIALIZED = "INITIALIZED";

        private const string WB_INITIALIZED = "INITIALIZED";
        private const string WB_CAPTURING = "CAPTURING WHITEBOARD";

        private const string CAMERA_ON = "ON";
        private const string CAMERA_OFF = "OFF";

        private const string SG_GRABBING = "GRABBING SOUND";
        private const string SD_DETECTING = "DETECTING SOUND";
        private const string SS_CONNECTED = "CONNECTED";

        private const string NOUPDATE = "NO CHANGE";
        private const string FIELD_AVCaptureFramesUpdateTime = "AVCaptureFramesUpdateTime";
        private const string FIELD_AgentRecordDataUpdateTime = "AgentRecordDataUpdateTime";

        private const string CN_NULL = "";

        public static Color TransferConnStatus(string es, string msg)
        {
            if (msg == STATUS_OFF || (msg != null && msg.ToUpper() == STATUS_FALSE))
                return Color.FromArgb(0, 176, 240);
            else
                return TransformInfoByEngineStatus(es, msg);
        }
        public static Color TransferStatusExAV(string es, string msg)
        {
            if (msg == STATUS_OFF || (msg != null && msg.ToUpper() == STATUS_FALSE))
                return Color.Red;
            else
            {
                if (msg != null && msg.IndexOf(NOUPDATE) >= 0)
                    return Color.Red;
                else
                    return TransformInfoByEngineStatus(es, msg);
            }
        }
        public static Color TransferStatusEx(string es, string msg)
        {
            if (msg == STATUS_OFF || (msg != null && msg.ToUpper() == STATUS_FALSE))
                return Color.Red;
            else
                return TransformInfoByEngineStatus(es, msg);
        }
        public static Color TransferEngineStatus(string msg)
        {
            switch (msg)
            {
                case ENGINE_IDLE:
                    return Color.Green;
                case ENGINE_RECORDING:
                    return Color.Blue;
                case ENGINE_PROCESSING:
                case ENGINE_WAITING:
                case ENGINE_UPLOADING:
                    return Color.Orange;
                default:
                    return Color.Red;
            }
        }
        public static string TransferEngineStatusText(string msg)
        {
            switch (msg.ToUpper())
            {
                case STATUS_OFF:
                    return "ENGINEOFFLINE!";
                default:
                    return msg;
            }
        }
        public static Color TransformInfoByEngineStatus(string enginestatus, string agentstatus)
        {
            return enginestatus == ENGINE_RECORDING ? Color.Blue : Color.Green;
        }
        public static Color TransferAgentStatus(string enginestatus, string agentstatus)
        {
            if (enginestatus.ToUpper() == ENGINE_RECORDING && agentstatus.ToUpper() == AGENT_IDLE)
            {
                return Color.Red;
            }
            else
            {
                if (agentstatus.ToUpper().IndexOf(AGENT_NOT_RUNNING) >= 0)
                {
                    return Color.Red;
                }
                if (agentstatus.ToUpper().IndexOf(AGENT_LOADAGENT_DISP_MESSAGE) >= 0)
                {
                    return Color.FromArgb(146, 208, 80);
                }
                if (agentstatus.ToUpper().IndexOf(AGENT_DISABLE_DISP_MESSAGE) >= 0)
                {
                    return Color.FromArgb(255, 192, 0);
                }
                if (agentstatus.ToUpper().IndexOf(NOUPDATE) >= 0)
                {
                    return Color.Orange;
                }

                switch (agentstatus.ToUpper())
                {
                    case "AGENTOFFLINE!":
                        return Color.Red;
                    case "IPC LOGGED OFF":
                        return Color.FromArgb(0, 176, 240);
                    default:
                        return TransformInfoByEngineStatus(enginestatus, agentstatus);
                }
            }
        }
        public static string TransferAgentStatusText(string msg)
        {
            switch (msg.ToUpper())
            {
                case STATUS_OFF:
                    return "AGENTOFFLINE!";
                case AGENT_NOTLOGIN:
                    return "IPC LOGGED OFF";
                default:
                    return msg;
            }
        }
        public static Color TransferDiskStatus(string es, string msg)
        {
            int nFreedisk = 0;
            if (int.TryParse(msg, out nFreedisk))
            {
                if (nFreedisk > 50000)
                {
                    return TransformInfoByEngineStatus(es, msg);
                }
                else
                {
                    return Color.Red;
                }
            }
            return Color.Red;
        }

        public static List<ClassroomView> TransformClassroomView(List<ClassroomView> list)
        {
            foreach (ClassroomView i in list)
            {
                i.PPCConnectionStatus = (i.NoIPC) ? "--" : i.PPCConnectionStatus?.ToUpper();
                if (!IsDBNULL(i.PPCReportTime))
                {
                    TimeSpan timesapnEngine = DateTime.Now.Subtract(Convert.ToDateTime(i.PPCReportTime));
                    if (timesapnEngine.TotalSeconds > 50)
                    {
                        i.EngineStatus = STATUS_OFF;
                        i.AVStatus = STATUS_OFF;
                        i.ScreenCaptureStatus = STATUS_OFF;
                        //i.CameraStatus = STATUS_OFF;
                        i.WBStatus = STATUS_OFF;
                        //i.SSStatus = STATUS_OFF;
                        //i.SGStatus = STATUS_OFF;
                        //i.SDSStatus = STATUS_OFF;
                        i.CourseName = CN_NULL;
                        i.PPCConnectionStatus = i.PPCConnectionStatus == "--" ? i.PPCConnectionStatus?.ToUpper() : STATUS_FALSE;
                        if (i.KaptivoNumber == 2)
                        {
                            i.Kaptivo1Status = STATUS_OFF;
                            i.Kaptivo2Status = STATUS_OFF;
                        }
                        else if (i.KaptivoNumber == 1)
                            i.Kaptivo1Status = STATUS_OFF;
                    }
                    else
                    {
                        //Engine Status
                        if (i.EngineStatus.ToUpper() != ENGINE_IDLE
                            && i.EngineStatus.ToUpper() != ENGINE_RECORDING
                            && i.EngineStatus.ToUpper() != ENGINE_PROCESSING
                            && i.EngineStatus.ToUpper() != ENGINE_WAITING
                            && i.EngineStatus.ToUpper() != ENGINE_UPLOADING)
                        {
                            i.EngineStatus = STATUS_OFF;
                        }
                        // AV status
                        if (i.AVStatus.ToUpper() == AV_INITIALIZED)
                        {
                            i.AVStatus = STATUS_ON;
                        }
                        else if (i.AVStatus.ToUpper() == AV_CAPTURING)
                        {
                            i.AVStatus = i.AVCaputureFrames.ToString();
                        }
                        else
                        {
                            i.AVStatus = STATUS_OFF;
                        }
                        // ScreenCaptureStatus status
                        //if (i.ScreenCaptureStatus.ToUpper() == AV_INITIALIZED)
                        //{
                        //    i.ScreenCaptureStatus = STATUS_ON;
                        //}
                        //else if (i.ScreenCaptureStatus.ToUpper() == AV_CAPTURING)
                        //{
                        //    i.ScreenCaptureStatus = i.AVCaputureFrames.ToString();
                        //}
                        //else
                        //{
                        //    i.ScreenCaptureStatus = STATUS_OFF;
                        //}
                        //Whiteboard Status                        
                        if (i.WBStatus.ToUpper() == WB_INITIALIZED)
                        {
                            i.WBStatus = STATUS_ON;
                        }
                        else if (i.WBStatus.ToUpper().Contains(WB_CAPTURING))
                        {
                            string WBStatus = i.WBStatus;
                            string[] stringSeparators = new string[] { "WB1 ", "WB2 " };
                            string[] msg = WBStatus.Split(stringSeparators, StringSplitOptions.None);
                            switch (i.ClassRoomWBNumber.ToString())
                            {
                                case "0": i.WBStatus = ""; break;
                                case "1": i.WBStatus = msg[1].Substring(0, msg[1].Length - 9); break;
                                default:
                                    i.WBStatus = msg[1].Substring(0, msg[1].Length - 9) + "," + msg[2].Substring(0, msg[2].Length - 7);
                                    break;
                            }
                        }
                        else
                        {
                            i.WBStatus = STATUS_OFF;
                        }

                        //Camera Status
                        //if (i.CameraStatus.ToUpper() == CAMERA_ON)
                        //{
                        //    i.CameraStatus = STATUS_ON;
                        //}
                        //else
                        //{
                        //    i.CameraStatus = STATUS_OFF;
                        //}
                        ////SG Status
                        //if (i.SGStatus.ToUpper() == SG_GRABBING)
                        //{
                        //    i.SGStatus = STATUS_ON;
                        //}
                        //else
                        //{
                        //    i.SGStatus = STATUS_OFF;
                        //}
                        ////SD Status
                        //if (i.SDSStatus.ToUpper() == SD_DETECTING)
                        //{
                        //    i.SDSStatus = STATUS_ON;
                        //}
                        //else
                        //{
                        //    i.SDSStatus = STATUS_OFF;
                        //}
                        ////SS Status
                        //if (i.SSStatus.ToString().ToUpper() == SS_CONNECTED)
                        //{
                        //    i.SSStatus = STATUS_ON;
                        //}
                        //else
                        //{
                        //    i.SSStatus = STATUS_OFF;
                        //}
                        //Course Name
                        if (i.CourseName != CN_NULL && i.CourseName != String.Empty && i.CourseName != "")
                        {
                            String[] sCourse = i.CourseName.Split('\\');
                            if (sCourse[sCourse.Length - 1] != String.Empty && sCourse[sCourse.Length - 1] != "")
                            {
                                i.CourseName = sCourse[sCourse.Length - 1];
                            }
                            else if (sCourse.Length >= 2)
                            {
                                i.CourseName = sCourse[sCourse.Length - 2];
                            }
                            else
                            {
                                i.CourseName = CN_NULL;
                            }
                        }
                        if (i.KaptivoNumber == 2)
                        {
                            i.Kaptivo1Status = i.Kaptivo1DataSize == -1 ? "DOWN" : i.Kaptivo1Status;
                            i.Kaptivo1Status = i.Kaptivo2DataSize == -1 ? "DOWN" : i.Kaptivo2Status;
                        }
                        else if (i.KaptivoNumber == 1)
                            i.Kaptivo1Status = i.Kaptivo1DataSize == -1 ? "DOWN" : i.Kaptivo1Status;
                    }
                    i.EngineStatus = TransferEngineStatusText(i.EngineStatus);
                }
                if (!IsDBNULL(i.IPCReportTime))
                {
                    TimeSpan timesapnAgent = DateTime.Now.Subtract(Convert.ToDateTime(i.IPCReportTime));
                    if (timesapnAgent.TotalSeconds > 50)
                    {
                        //i.AgentStatus = STATUS_OFF;
                        //i.AgentRecordData = -1;
                        if (i.AgentStatus != AGENT_LOADAGENT)
                        {
                            i.AgentStatus = STATUS_OFF;
                            i.PPCConnectionStatus = STATUS_FALSE;
                        }
                    }
                    //tranfer Agent Status Text
                    i.AgentStatus = TransferAgentStatusText(i.AgentStatus);
                    //if (i.AgentStatus.ToUpper() != AGENT_IDLE && i.AgentStatus.ToUpper() != AGENT_RECORDING)
                    //{
                    //    i.AgentStatus = i.AgentStatus;
                    //}
                    //else
                    //{
                    //    if (i.AgentStatus.ToUpper() == AGENT_IDLE)
                    //    {
                    //    }
                    //    else if (i.AgentStatus.ToUpper() == AGENT_RECORDING)
                    //    {
                    //        i.AgentStatus = i.AgentRecordData.ToString();
                    //    }
                    //    else
                    //    {
                    //        i.AgentStatus = STATUS_OFF;
                    //    }
                    //}
                }
            }
            return list;
        }

        private static List<ClassroomEngineAgentStatus> TransformClassroomStatus(List<ClassroomEngineAgentStatus> list)
        {
            foreach (ClassroomEngineAgentStatus i in list)
            {
                if (!IsDBNULL(i.PPCReportTime))
                {
                    TimeSpan timesapnEngine = DateTime.Now.Subtract(Convert.ToDateTime(i.PPCReportTime));
                    if (timesapnEngine.TotalSeconds > 50)
                    {
                        i.EngineStatus = STATUS_OFF;
                        i.AVStatus = STATUS_OFF;
                        i.CameraStatus = STATUS_OFF;
                        i.WBStatus = STATUS_OFF;
                        i.SSStatus = STATUS_OFF;
                        i.SGStatus = STATUS_OFF;
                        i.SDSStatus = STATUS_OFF;
                        i.CourseName = CN_NULL;
                        i.PPCConnectionStatus = STATUS_FALSE;
                    }
                    else
                    {
                        //Engine Status
                        if (i.EngineStatus.ToUpper() != ENGINE_IDLE
                            && i.EngineStatus.ToUpper() != ENGINE_RECORDING
                            && i.EngineStatus.ToUpper() != ENGINE_PROCESSING
                            && i.EngineStatus.ToUpper() != ENGINE_WAITING
                            && i.EngineStatus.ToUpper() != ENGINE_UPLOADING)
                        {
                            i.EngineStatus = STATUS_OFF;
                        }
                        // AV status
                        if (i.AVStatus.ToUpper() == AV_INITIALIZED)
                        {
                            i.AVStatus = STATUS_ON;
                        }
                        else if (i.AVStatus.ToUpper() == AV_CAPTURING)
                        {
                            i.AVStatus = i.AVCaputureFrames.ToString();
                        }
                        else
                        {
                            i.AVStatus = STATUS_OFF;
                        }
                        //Whiteboard Status                        
                        if (i.WBStatus.ToUpper() == WB_INITIALIZED)
                        {
                            i.WBStatus = STATUS_ON;
                        }
                        else if (i.WBStatus.ToUpper().Contains(WB_CAPTURING))
                        {
                            string WBStatus = i.WBStatus;
                            string[] stringSeparators = new string[] { "WB1 ", "WB2 " };
                            string[] msg = WBStatus.Split(stringSeparators, StringSplitOptions.None);
                            switch (i.ClassRoomWBNumber.ToString())
                            {
                                case "0": i.WBStatus = ""; break;
                                case "1": i.WBStatus = msg[1].Substring(0, msg[1].Length - 9); break;
                                default:
                                    i.WBStatus = msg[1].Substring(0, msg[1].Length - 9) + "," + msg[2].Substring(0, msg[2].Length - 7);
                                    break;
                            }
                        }
                        else
                        {
                            i.WBStatus = STATUS_OFF;
                        }

                        //Camera Status
                        if (i.CameraStatus.ToUpper() == CAMERA_ON)
                        {
                            i.CameraStatus = STATUS_ON;
                        }
                        else
                        {
                            i.CameraStatus = STATUS_OFF;
                        }
                        //SG Status
                        if (i.SGStatus.ToUpper() == SG_GRABBING)
                        {
                            i.SGStatus = STATUS_ON;
                        }
                        else
                        {
                            i.SGStatus = STATUS_OFF;
                        }
                        //SD Status
                        if (i.SDSStatus.ToUpper() == SD_DETECTING)
                        {
                            i.SDSStatus = STATUS_ON;
                        }
                        else
                        {
                            i.SDSStatus = STATUS_OFF;
                        }
                        //SS Status
                        if (i.SSStatus.ToString().ToUpper() == SS_CONNECTED)
                        {
                            i.SSStatus = STATUS_ON;
                        }
                        else
                        {
                            i.SSStatus = STATUS_OFF;
                        }
                        //Course Name
                        if (i.CourseName != CN_NULL && i.CourseName != String.Empty && i.CourseName != "")
                        {
                            String[] sCourse = i.CourseName.Split('\\');
                            if (sCourse[sCourse.Length - 1] != String.Empty && sCourse[sCourse.Length - 1] != "")
                            {
                                i.CourseName = sCourse[sCourse.Length - 1];
                            }
                            else if (sCourse.Length >= 2)
                            {
                                i.CourseName = sCourse[sCourse.Length - 2];
                            }
                            else
                            {
                                i.CourseName = CN_NULL;
                            }
                        }
                    }
                }
                if (!IsDBNULL(i.IPCReportTime))
                {
                    TimeSpan timesapnAgent = DateTime.Now.Subtract(Convert.ToDateTime(i.IPCReportTime));
                    if (timesapnAgent.TotalSeconds > 50)
                    {
                        i.AgentStatus = STATUS_OFF;
                        i.AgentRecordData = -1;
                    }
                    else
                    {
                        if (i.AgentStatus.ToUpper() == AGENT_IDLE)
                        {
                        }
                        else if (i.AgentStatus.ToUpper() == AGENT_RECORDING)
                        {
                            i.AgentStatus = i.AgentRecordData.ToString();
                        }
                        else
                        {
                            i.AgentStatus = STATUS_OFF;
                        }
                    }
                }
            }
            return list;
        }

        private static DataTable TransformClassroomStatus(DataTable dtgroup)
        {
            foreach (DataRow drgroup in dtgroup.Rows)
            {
                if (!IsDBNULL(drgroup["PPCReportTime"]))
                {
                    TimeSpan timesapnEngine = DateTime.Now.Subtract(Convert.ToDateTime(drgroup["PPCReportTime"]));
                    if (timesapnEngine.TotalSeconds > 50)
                    {
                        drgroup["EngineStatus"] = STATUS_OFF;
                        drgroup["AVStatus"] = STATUS_OFF;
                        drgroup["CameraStatus"] = STATUS_OFF;
                        drgroup["WBStatus"] = STATUS_OFF;
                        drgroup["SSStatus"] = STATUS_OFF;
                        drgroup["SGStatus"] = STATUS_OFF;
                        drgroup["SDStatus"] = STATUS_OFF;
                        drgroup["CourseName"] = CN_NULL;
                        drgroup["PPCConnectionStatus"] = STATUS_FALSE;
                    }
                    else
                    {
                        //Engine Status
                        if (drgroup["EngineStatus"].ToString().ToUpper() != ENGINE_IDLE
                            && drgroup["EngineStatus"].ToString().ToUpper() != ENGINE_RECORDING
                            && drgroup["EngineStatus"].ToString().ToUpper() != ENGINE_PROCESSING
                            && drgroup["EngineStatus"].ToString().ToUpper() != ENGINE_WAITING
                            && drgroup["EngineStatus"].ToString().ToUpper() != ENGINE_UPLOADING)
                        {
                            drgroup["EngineStatus"] = STATUS_OFF;
                        }
                        // AV status
                        if (drgroup["AVStatus"].ToString().ToUpper() == AV_INITIALIZED)
                        {
                            drgroup["AVStatus"] = STATUS_ON;
                        }
                        else if (drgroup["AVStatus"].ToString().ToUpper() == AV_CAPTURING)
                        {
                            drgroup["AVStatus"] = drgroup["AVCaputureFrames"].ToString();
                        }
                        else
                        {
                            drgroup["AVStatus"] = STATUS_OFF;
                        }
                        //Whiteboard Status                        
                        if (drgroup["WBStatus"].ToString().ToUpper() == WB_INITIALIZED)
                        {
                            drgroup["WBStatus"] = STATUS_ON;
                        }
                        else if (drgroup["WBStatus"].ToString().ToUpper().Contains(WB_CAPTURING))
                        {
                            string WBStatus = drgroup["WBStatus"].ToString();
                            string[] stringSeparators = new string[] { "WB1 ", "WB2 " };
                            string[] msg = WBStatus.Split(stringSeparators, StringSplitOptions.None);
                            switch (drgroup["ClassRoomWBNumber"].ToString())
                            {
                                case "0": drgroup["WBStatus"] = ""; break;
                                case "1": drgroup["WBStatus"] = msg[1].Substring(0, msg[1].Length - 9); break;
                                default:
                                    drgroup["WBStatus"] = msg[1].Substring(0, msg[1].Length - 9) + "," + msg[2].Substring(0, msg[2].Length - 7);
                                    break;
                            }
                        }
                        else
                        {
                            drgroup["WBStatus"] = STATUS_OFF;
                        }

                        //Camera Status
                        if (drgroup["CameraStatus"].ToString().ToUpper() == CAMERA_ON)
                        {
                            drgroup["CameraStatus"] = STATUS_ON;
                        }
                        else
                        {
                            drgroup["CameraStatus"] = STATUS_OFF;
                        }
                        //SG Status
                        if (drgroup["SGStatus"].ToString().ToUpper() == SG_GRABBING)
                        {
                            drgroup["SGStatus"] = STATUS_ON;
                        }
                        else
                        {
                            drgroup["SGStatus"] = STATUS_OFF;
                        }
                        //SD Status
                        if (drgroup["SDStatus"].ToString().ToUpper() == SD_DETECTING)
                        {
                            drgroup["SDStatus"] = STATUS_ON;
                        }
                        else
                        {
                            drgroup["SDStatus"] = STATUS_OFF;
                        }
                        //SS Status
                        if (drgroup["SSStatus"].ToString().ToUpper() == SS_CONNECTED)
                        {
                            drgroup["SSStatus"] = STATUS_ON;
                        }
                        else
                        {
                            drgroup["SSStatus"] = STATUS_OFF;
                        }
                        //Course Name
                        if (drgroup["CourseName"].ToString() != CN_NULL && drgroup["CourseName"].ToString() != String.Empty && drgroup["CourseName"].ToString() != "")
                        {
                            String[] sCourse = drgroup["CourseName"].ToString().Split('\\');
                            if (sCourse[sCourse.Length - 1] != String.Empty && sCourse[sCourse.Length - 1] != "")
                            {
                                drgroup["CourseName"] = sCourse[sCourse.Length - 1];
                            }
                            else if (sCourse.Length >= 2)
                            {
                                drgroup["CourseName"] = sCourse[sCourse.Length - 2];
                            }
                            else
                            {
                                drgroup["CourseName"] = CN_NULL;
                            }
                        }
                    }
                }
                if (!IsDBNULL(drgroup["IPCReportTime"]))
                {
                    TimeSpan timesapnAgent = DateTime.Now.Subtract(Convert.ToDateTime(drgroup["IPCReportTime"]));
                    if (timesapnAgent.TotalSeconds > 50)
                    {
                        drgroup["AgentStatus"] = STATUS_OFF;
                        drgroup["AgentRecordData"] = -1;
                    }
                    else
                    {
                        if (drgroup["AgentStatus"].ToString().ToUpper() == AGENT_IDLE)
                        {
                        }
                        else if (drgroup["AgentStatus"].ToString().ToUpper() == AGENT_RECORDING)
                        {
                            drgroup["AgentStatus"] = drgroup["AgentRecordData"].ToString();
                        }
                        else
                        {
                            drgroup["AgentStatus"] = STATUS_OFF;
                        }
                    }
                }
            }
            return dtgroup;
        }

        static public bool IsDBNULL(object obj)
        {
            return obj == null || obj == DBNull.Value;
        }
        protected static DataTable CreateDataTable<T>(IEnumerable<T> list)
        {
            System.Reflection.PropertyInfo[] pis = typeof(T).GetProperties();

            //create data table
            DataTable table = new DataTable();
            foreach (System.Reflection.PropertyInfo pi in pis)
            {
                if (pi.PropertyType == typeof(System.Nullable<Boolean>))
                {
                    table.Columns.Add(pi.Name, typeof(Boolean));
                }
                else if (pi.PropertyType == typeof(System.Nullable<Int16>))
                {
                    table.Columns.Add(pi.Name, typeof(Int16));
                }
                else if (pi.PropertyType == typeof(System.Nullable<Int32>))
                {
                    table.Columns.Add(pi.Name, typeof(Int32));
                }
                else if (pi.PropertyType == typeof(System.Nullable<Int64>))
                {
                    table.Columns.Add(pi.Name, typeof(Int64));
                }
                else if (pi.PropertyType == typeof(System.Nullable<Char>))
                {
                    table.Columns.Add(pi.Name, typeof(Char));
                }
                else if (pi.PropertyType == typeof(System.Nullable<DateTime>))
                {
                    table.Columns.Add(pi.Name, typeof(DateTime));
                }
                else
                {
                    table.Columns.Add(pi.Name, pi.PropertyType);
                }
            }

            //add rows to DataTable
            foreach (T elem in list)
            {
                DataRow row = table.NewRow();

                foreach (System.Reflection.PropertyInfo pi in pis)
                {
                    if (pi.GetValue(elem, null) == null)
                    {
                        row[pi.Name] = DBNull.Value;
                    }
                    else
                    {
                        row[pi.Name] = pi.GetValue(elem, null);
                    }
                }
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
