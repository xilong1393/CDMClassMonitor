using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ClassMonitor3.Util
{
    public class UDPClientFull : UdpClient
    {
        public UDPClientFull(string ip, int port)
            : base(ip, port)
        {
        }
        public bool Poll(int microSeconds, SelectMode mode)
        {
            return Client.Poll(microSeconds, mode);
        }
    }
    class ClassroomData
    {
        const string ZIPERROR = "Zip Data failed";
        const string SENDCOMMANDERROR = "unable send command";
        const string RECEIVERESPONSEERROR = "no response";
        const string PARSERESPONSEERROR = "reponse invalid";

        protected string _ip;
        protected int _port;

        public ClassroomData(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }
        private const string PROTOCOL_VERSION_STR = "version";
        private const string PROTOCOL_VERSION_VALUE = "04.00.00";

        private const string XML_VERSION_STR = "1.0";

        private const string UTF_VSESION_STR = "UTF-8";

        private const int PROTOCOL_RESPONSE_ID = 5;

        private const string EVENT_REPORT_ID = "11";

        private const string RESPONSE_SUCC_STR = "Succeed";
        private const string RESPONSE_FAILED_STR = "Failed";
        private const string RESPONSE_GOT_STR = "Got";


        private const string RESPONSE_COMMAND_STR = "col-response";


        private const string SCHEDULE_HEAD_TAG_STR = "col-schedule";
        private const int NEW_SCHEDULE_COMMAND_ID = 3;
        private const string NEW_SCHEDULE_COMMAND_STR = "colengine-new-schedule";

        private const int STOP_COMMAND_ID = 6;
        private const string STOP_COMMAND_STR = "colengine-stop-recording";

        private const int ABORT_COMMAND_ID = 7;
        private const string ABORT_COMMAND_STR = "colengine-abort-recording";

        private const int QUERY_ENGINE_STATUS_ID = 8;
        private const string QUERY_ENGINE_STATUS_STR = "colengine-status-report";

        private const int ENGINE_UPLOADED_STATUS_ID = 17;
        private const string ENGINE_UPLOADED_STATUS_STR = "uploaded";
        private const string ENGINE_UPLOADED_FLAG_STR = "Yes";


        private const int UPDATEENGINECONFIGURATION_COMMAND_ID = 9;
        private const string UPDATEENGINECONFIGURATION_COMMAND_STR = "colengine-update-config";

        private const string UPDATEAGENTCONFIGURATION_COMMAND_STR = "colagent-update-config";
        private const int UPDATEAGENTCONFIGURATION_COMMAND_ID = 24;

        private const int LIST_LOCALDATA_COMMAND_ID = 12;
        private const int LIST_LOCALDATA_REPONSE_ID = 13;
        private const string LIST_LOCALDATA_COMMAND_STR = "colengine-list-localdata";

        private const int DEL_LOCALDATA_COMMAND_ID = 14;
        private const string DEL_LOCALDATA_COMMAND_STR = "colengine-deletedata";

        private const int COLEngineQueryVideoImageType = 50;       // new protocol for original size image
        private const int COLEngineQueryScreenImageType = 51;
        private const int COLEngineQueryKaptivo1ImageType = 52;
        private const int COLEngineQueryKaptivo2ImageType = 53;
        private const int COLEngineQueryMimio1ImageType = 55;
        private const int COLEngineQueryMimio2ImageType = 56;
        private const int QUERY_IMAGE_TYPE_ID = 19;
        private const string QUERY_IMAGE_TYPE_STR = "colengine-query-image";

        private const int UPLOAD_LOCALDATA_COMMAND_ID = 20;
        private const string UPLOAD_LOCALDATA_COMMAND_STR = "colengine-upload-data";

        private const int REBOOTPC_COMMAND_ID = 22;
        private const string REBOOTPC_COMMAND_STR = "colengine-reboot-pc";

        private const int QUERY_AGENT_STATUS_ID = 25;
        private const string QUERY_AGENT_STATUS_STR = "colagent-status-report";

        private const int QUERY_AUDIO_COMMAND_ID = 26;
        private const string QUERY_AUDIO_COMMAND_STR = "colengine-query-audio";

        private const int QUERY_SCHEDULE_COMMAND_ID = 27;
        private const string QUERY_SCHEDULE_COMMAND_STR = "colegine_courseschedule_report";

        private const string SCHEDULE_TIMESTAMP_STR = "timestamp";
        private const string SERVER_STR = "server";

        private const string PROCESSING_STR = "postprocessing";
        private const string DELAY_STR = "delay";

        private const string WHITEBOARD_STR = "whiteboard";
        private const string WB_NUM_STR = "wb-num";
        private const string INTERVAL_STR = "interval";

        private const string FTP_STR = "ftp";
        private const string UPLOADING_STR = "uploading";
        private const string USER_STR = "user";
        private const string PASSWORD_STR = "password";
        private const string INSTRUCTOR_PC_STR = "instructor-pc";

        private const string IP_STR = "ip";
        private const string PORTAL_PAGE_STR = "portal-page";
        private const string CLASSROOMID_STR = "classroomid";
        private const string PPC_STR = "ppc";
        private const string DIRECTORY_STR = "directory";

        private const string SCREENSHOT_STR = "screenshot";
        private const string COURSE_INFO_STR = "course-info";
        private const string ID_STR = "id";
        private const string PPCIP_STR = "PPCIP";
        private const string IPC_STR = "ipc";
        private const string DATAVERSION_STR = "data-version";

        private const string RATE_STR = "rate";
        private const string WIDTH_STR = "width";
        private const string HEIGHT_STR = "height";
        private const string VIDEO_STR = "video";
        private const string HI_VIDEO_STR = "hi-video";
        private const string LO_VIDEO_STR = "lo-video";

        private const string H_DIVIDE_STR = "h-divide";
        private const string V_DIVIDE_STR = "v-divide";
        private const string HI_COLORBITS_STR = "hi-colorbits";
        private const string LO_COLORBITS_STR = "lo-colorbits";
        private const string CHOP_LEN_STR = "chop-len";
        private const string DENOISE_STR = "denoise";
        private const string IMAGE_INTERVCAL_STR = "image-interval";

        private const string START_TIME_STR = "start-time";
        private const string DURATION_STR = "duration";
        private const string START_TIMER_STR = "start-timer";

        private const string NAME_STR = "name";
        private const string TYPE_STR = "type";

        private const int INSTRUCTOR_SENDMESSAGE_COMMAND_ID = 101;
        private const string INSTRUCTOR_SENDMESSAGE_COMMAND_STR = "instructor-instant-message-response";
        private const string INSTRUCTOR_SENDMESSAGE_ATTR_MESSAGE = "message";
        private const string INSTRUCTOR_SENDMESSAGE_ATTR_SENDER = "sender";

        private const string INSTRUCTOR_SET_CLASS_ATTR_SETVALUE = "setvalue";

        private const string ScreenShrinkDeptList_STR = "screenshrinkdeptlist";
        private const string ScreenShrinkToSizeInM_STR = "screenshrinktosizeinm";

        public Task<byte[]> GetBinary(string commandxml)
        {
            return Task.Run(() =>
            {
                byte[] returndata = SendBinaryReturnCommand(commandxml);
                return returndata;
            });
        }

        private byte[] SendBinaryReturnCommand(string commandxml)
        {
            UDPClientFull udpClient = new UDPClientFull(_ip, _port);

            MemoryStream ms = new MemoryStream();
            ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream zips = new ICSharpCode.SharpZipLib.Zip.Compression.Streams.DeflaterOutputStream(ms);
            byte[] bytData = System.Text.Encoding.UTF8.GetBytes(commandxml);
            zips.Write(bytData, 0, bytData.Length);
            zips.Close();
            byte[] compressedData = (byte[])ms.ToArray();

            if (udpClient.Send(compressedData, compressedData.Length) == 0)
            {
                throw new Exception("Send Data failed");
            }

            try
            {
                if (!udpClient.Poll(5000000 * 10, SelectMode.SelectRead))
                {
                    throw new Exception("in 5 second no response");
                }

                byte[] recvbuf = new byte[2 * 64 * 1024];
                IPEndPoint otherpoint = new IPEndPoint(IPAddress.Any, 0);
                recvbuf = udpClient.Receive(ref otherpoint);
                return recvbuf;
            }
            catch (Exception)
            {
                //throw new Exception("failed in waiting for response");
                throw;
            }
        }
        public Task<string> GetImageString(string FTPUser, string FTPPassword, int classroomid, String screenshrinkdeptList, string screenshrinktoSizeInM, int queryImageTypeID = 19)
        {
            return Task.Run(() =>
            { return updateengineconfiguration_command_xml(QUERY_IMAGE_TYPE_STR, queryImageTypeID, _ip, _port + "", FTPUser, FTPPassword, classroomid, screenshrinkdeptList, screenshrinktoSizeInM); }
            );
        }
        public Task<string> GetAudioData(string FTPUser, string FTPPassword, int classroomid, String screenshrinkdeptList, string screenshrinktoSizeInM)
        {
            return Task.Run(() =>
                 { return updateengineconfiguration_command_xml(QUERY_AUDIO_COMMAND_STR, QUERY_AUDIO_COMMAND_ID, _ip, _port + "", FTPUser, FTPPassword, classroomid, screenshrinkdeptList, screenshrinktoSizeInM); }
                );
        }
        static private string updateengineconfiguration_command_xml(string commandname, int commandid, string IPCIP, string SvrPortalpage, string FTPUser, string FTPPassword, int classroomid, string screenshrinkdeptList, string screenshrinktoSizeInM)
        {

            XmlDocument doc = create_node_document();

            XmlNode commandNode = doc.CreateElement(commandname);

            append_XmlNode_VersionAndCommandID(doc, commandNode, commandid);

            doc.AppendChild(commandNode);
            //--ipc ip node
            XmlNode ipcNode = doc.CreateElement(INSTRUCTOR_PC_STR);
            append_XmlNode_attribute(doc, ipcNode, IP_STR, IPCIP);
            commandNode.AppendChild(ipcNode);

            //--svr portal page node
            XmlNode svrPageNode = doc.CreateElement(SERVER_STR);
            append_XmlNode_attribute(doc, svrPageNode, PORTAL_PAGE_STR, SvrPortalpage);
            commandNode.AppendChild(svrPageNode);

            //----ftp node
            XmlNode ftpNode = doc.CreateElement(FTP_STR);
            append_XmlNode_attribute(doc, ftpNode, USER_STR, FTPUser);
            append_XmlNode_attribute(doc, ftpNode, PASSWORD_STR, FTPPassword);
            commandNode.AppendChild(ftpNode);

            //-- uploading 
            XmlNode uploadNode = doc.CreateElement(UPLOADING_STR);
            append_XmlNode_attribute(doc, uploadNode, USER_STR, FTPUser);
            append_XmlNode_attribute(doc, uploadNode, PASSWORD_STR, FTPPassword);
            commandNode.AppendChild(uploadNode);

            //--classroomid in colengine
            XmlNode ppcNode = doc.CreateElement(PPC_STR);
            append_XmlNode_attribute(doc, ppcNode, CLASSROOMID_STR, classroomid.ToString());
            append_XmlNode_attribute(doc, ppcNode, ScreenShrinkDeptList_STR, screenshrinkdeptList);
            append_XmlNode_attribute(doc, ppcNode, ScreenShrinkToSizeInM_STR, screenshrinktoSizeInM);
            commandNode.AppendChild(ppcNode);

            return doc.OuterXml.ToString();
        }

        static private XmlDocument create_node_document()
        {
            XmlDocument doc = new XmlDocument();

            XmlNode docNode = doc.CreateXmlDeclaration(XML_VERSION_STR, UTF_VSESION_STR, null);
            doc.AppendChild(docNode);
            return doc;
        }

        static private void append_XmlNode_VersionAndCommandID(XmlDocument doc, XmlNode node, int commandid)
        {
            append_XmlNode_attribute(doc, node, PROTOCOL_VERSION_STR, PROTOCOL_VERSION_VALUE);
            append_XmlNode_attribute(doc, node, ID_STR, commandid.ToString());
        }

        static private void append_XmlNode_attribute(XmlDocument doc, XmlNode node, string attrname, string attrvalue)
        {
            XmlAttribute attr = doc.CreateAttribute(attrname);
            attr.Value = attrvalue;
            node.Attributes.Append(attr);
        }
    }
}
