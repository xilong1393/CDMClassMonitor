using ClassMonitor3.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ClassMonitor3.Util
{
    public static class Helper
    {
        public static ConcurrentDictionary<string, bool> dict = new ConcurrentDictionary<string, bool>();

        public static HttpClient CreateClient()
        {
            var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30); //set your own timeout.
            string address = ConfigurationManager.AppSettings.Get("WebapiAddress");
            client.BaseAddress = new Uri(address);
            return client;
        }
        public static ComboItem[] GetComboItems()
        {
            return new ComboItem[] {//new ComboItems("0"," "),
                    new ComboItem("19","CameraVideo"),
                    new ComboItem("55", "whiteboard1"),
                    new ComboItem("56","whiteboard2"),
                    new ComboItem("52","Kaptivo1"),
                    new ComboItem("53","Kaptivo2"),
                    new ComboItem("51","TeacherScreen")
            };
        }
        public static ComboItem[] GetComboItems(ClassroomView classroomView)
        {
            List<ComboItem> list = new List<ComboItem>();
            if (classroomView.NoIPC)
                list.Add(new ComboItem("50", "CameraVideo"));
            else
                list.Add(new ComboItem("19", "CameraVideo"));
            for (int i = 0; i < classroomView.ClassRoomWBNumber; i++)
            {
                int wb = 55;
                ComboItem item = new ComboItem((wb+i) + "", "whiteboard" + (1 + i));
                list.Add(item);
            }
            for (int i = 0; i < classroomView.KaptivoNumber; i++)
            {
                int kp = 52;
                ComboItem item = new ComboItem((kp + i) + "", "Kaptivo" + (1 + i));
                list.Add(item);
            }
            list.Add(new ComboItem("51", "TeacherScreen"));
            return list.ToArray();
        }
        public static void SafeInvoke(this Control uiElement, Action updater, bool forceSynchronous)
        {
            if (uiElement == null)
            {
                throw new ArgumentNullException("uiElement");
            }

            if (uiElement.InvokeRequired)
            {
                if (forceSynchronous)
                {
                    uiElement.Invoke((Action)delegate { SafeInvoke(uiElement, updater, forceSynchronous); });
                }
                else
                {
                    uiElement.BeginInvoke((Action)delegate { SafeInvoke(uiElement, updater, forceSynchronous); });
                }
            }
            else
            {
                if (uiElement.IsDisposed)
                {
                    throw new ObjectDisposedException("Control is already disposed.");
                }

                updater();
            }
        }

        public static string GenerateSessionID() {
            String UUID = Guid.NewGuid().ToString();
            return UUID;
        }
        public static IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        public static Control GetFirst(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type).First();
        }
        public static Control GetLast(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type).Last();
        }

        public static T DeepCopy<T>(this T obj)
        {
            object result = null;
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;
                result = (T)formatter.Deserialize(ms);
                ms.Close();
            }
            return (T)result;
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static string formatClassName(string originalString)
        {
            string formatString = originalString;
            try
            {
                Regex rex = new Regex(@"[^\w|\.|\[|\]|\(|\)|\-]+");
                formatString = rex.Replace(originalString, " ");
            }
            catch (Exception)
            {
                formatString = originalString;
            }
            return formatString;
        }

        public static int[] SixtyArray()
        {
            int[] result = new int[60];
            for (int i = 0; i < 60; i++)
            {
                result[i] = i;
            }
            return result;
        }

        public static int[] TwentyFourArray()
        {
            int[] result = new int[24];
            for (int i = 0; i < 24; i++)
            {
                result[i] = i;
            }
            return result;
        }

        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text };
            TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        internal static string GetFileName(string selectedValue)
        {
            switch (int.Parse(selectedValue))
            {
                case 19:
                    return "VideoFrameOrgSize.jpg";
                case 50:
                    return "VideoFrameOrgSize.jpg";
                case 51://IPC noIPC => PPC 
                    return "ContentCaptureFrameOrgSize.jpg";
                case 52:
                    return "kaptivo1captureFrameOrgSize.jpg";
                case 53:
                    return "kaptivo2captureFrameOrgSize.jpg";
                case 55:
                    return "Mimio1captureFrameOrgSize.jpg";
                case 56:
                    return "Mimio2captureFrameOrgSize.jpg";
                default:
                    return "VideoFrame.jpg";
            }
        }

        internal static string GetFileLocation(ClassroomView classroomView, string selectedValue)
        {
            switch (int.Parse(selectedValue))
            {
                case 19:
                    return @"\\" + classroomView.PPCPublicIP + @"\Monitor\VideoFrameOrgSize.jpg";
                case 50:
                    return @"\\" + classroomView.PPCPublicIP + @"\Monitor\VideoFrameOrgSize.jpg";
                case 51://IPC noIPC => PPC
                        if (classroomView.NoIPC)
                            return @"\\" + classroomView.PPCPublicIP + @"\Monitor\ContentCaptureFrameOrgSize.jpg";
                        else
                            return @"\\" + classroomView.IPCPublicIP + @"\Monitor\ContentCaptureFrameOrgSize.jpg";
                case 52:
                    return @"\\" + classroomView.PPCPublicIP + @"\Monitor\kaptivo1captureFrameOrgSize.jpg";
                case 53:
                    return @"\\" + classroomView.PPCPublicIP + @"\Monitor\kaptivo2captureFrameOrgSize.jpg";
                case 55:
                    return @"\\" + classroomView.PPCPublicIP + @"\Monitor\Mimio1captureFrameOrgSize.jpg";
                case 56:
                    return @"\\" + classroomView.PPCPublicIP + @"\Monitor\Mimio2captureFrameOrgSize.jpg";
                default:
                    return @"\\" + classroomView.PPCPublicIP + @"\Monitor\VideoFrame.jpg";
            }
        }
        internal static string GetExMessageText(string selectedValue)
        {
            switch (int.Parse(selectedValue))
            {
                case 19:
                    return "Video";
                case 50:
                    return "Video";
                case 51://IPC noIPC => PPC 
                    return "Screen";
                case 52:
                    return "KP1";
                case 53:
                    return "KP2";
                case 55:
                    return "WB1";
                case 56:
                    return "WB2";
                default:
                    return "Video";
            }
        }
    }
}
