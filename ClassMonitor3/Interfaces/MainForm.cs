using ClassMonitor3.Model;
using ClassMonitor3.Properties;
using ClassMonitor3.Util;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace ClassMonitor3.Interfaces
{
    public partial class MainForm : Form
    {
        static ReaderWriterLock rwl = new ReaderWriterLock();
        List<Task> tasks = new List<Task>();
        List<ClassroomView> list = new List<ClassroomView>();
        BindingList<ClassroomView> updateList;
        BindingList<ClassroomView> UpdateList
        {
            get
            {
                rwl.AcquireReaderLock(1000);
                try
                {
                    return updateList;
                }
                finally
                {
                    // Ensure that the lock is released.
                    rwl.ReleaseReaderLock();
                }
            }
            set
            {
                try
                {
                    rwl.AcquireWriterLock(1000);
                    updateList = value;
                }
                finally
                {
                    rwl.ReleaseWriterLock();
                }
            }
        }
        List<CancellationTokenSource> tokenSourceList = new List<CancellationTokenSource>();
        DateTime lastSoundTime = DateTime.Now;
        IWavePlayer waveOutDevice = null;
        AudioFileReader audioFileReader = null;

        public CancellationTokenSource TokenSource
        {
            get
            {
                CancellationTokenSource tokenSource = new CancellationTokenSource();
                tokenSourceList.Add(tokenSource);
                return tokenSource;
            }
            set { TokenSource = value; }
        }
        //this two methods are used to adjust the window size
        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height;
        public MainForm()
        {
            InitializeComponent();
            comboBox.MouseWheel += comboBox_MouseWheel;
            menuCB.MouseWheel += comboBox_MouseWheel;
            SetStyle(ControlStyles.ResizeRedraw, true);
            LoadData();
        }

        private void comboBox_MouseWheel(object sender, MouseEventArgs e)
        {
            ((HandledMouseEventArgs)e).Handled = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rc = new Rectangle(this.ClientSize.Width - cGrip, this.ClientSize.Height - cGrip, cGrip, cGrip);
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, rc);
            rc = new Rectangle(0, 0, this.ClientSize.Width, cCaption);
            e.Graphics.FillRectangle(Brushes.DarkBlue, rc);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {  // Trap WM_NCHITTEST
                Point pos = new Point(m.LParam.ToInt32());
                pos = this.PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;  // HTCAPTION
                    return;
                }
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }

        public async void LoadData()
        {
            try
            {
                List<ClassroomGroup> list = await GetProductAsync();
                if (list != null && list.Count != 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ID", typeof(int));
                    dt.Columns.Add("Text", typeof(string));
                    foreach (var data in list)
                    {
                        dt.Rows.Add(data.ClassroomGroupID, data.ClassroomgroupName);
                    }
                    comboBox.ValueMember = "ID";
                    comboBox.DisplayMember = "Text";
                    comboBox.DataSource = dt;
                    comboBox.Parent.Focus();
                    menuCB.DisplayMember = "Text";
                    menuCB.ValueMember = "Value";
                    menuCB.DataSource = Helper.GetComboItems();
                }
                else
                {
                    MessageBox.Show("no data");
                }

                if (LoginInfo.user != null)
                    label1.Text = "Remote Monitor - " + LoginInfo.user.FullName + ": " + LoginInfo.sessionID;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        static async Task<List<ClassroomGroup>> GetProductAsync()
        {
            var client = Helper.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //var tuple = new { sessionID = LoginInfo.sessionID, roleID = LoginInfo.user.RoleID };
            //HttpResponseMessage response = await client.PostAsJsonAsync("api/classroom/GetClassroomGroupListByRoleID", tuple);
            HttpResponseMessage response = await client.GetAsync("api/classroom/GetClassroomGroupListByRoleID?roleID=" + 20/*LoginInfo.user.RoleID*/);
            List<ClassroomGroup> list = null;
            if (response.IsSuccessStatusCode)
            {
                list = await response.Content.ReadAsAsync<List<ClassroomGroup>>();
            }
            return list;
        }

        private void CreatePanel(string value)
        {
            disposeWave();
            flRightPanel.Controls.Clear();
            foreach (CancellationTokenSource cts in tokenSourceList)
                cts.Cancel();
            tokenSourceList.RemoveRange(0, tokenSourceList.Count);
            tasks.RemoveRange(0, tasks.Count);

            int length = list.Count;
            if (length == 0) return;
            int flpWidth = flRightPanel.Width;
            int flpHeight = flRightPanel.Height;
            int panelLength = (int)Math.Sqrt((flpWidth - 50) * (flpHeight - 50) / length) - 30;
            panelLength = panelLength > 350 ? 350 : panelLength;

            for (int i = 0; i < length; i++)
            {
                string str = list[i].ClassroomName;

                Panel panel = new Panel();
                panel.Margin = new Padding(1, 0, 1, 2);
                panel.BackColor = Color.FromArgb(28, 31, 31);
                panel.Width = panelLength;
                panel.Height = panelLength;
                panel.BorderStyle = BorderStyle.FixedSingle;

                Label l = new Label();
                l.Text = str;
                l.BackColor = Color.Transparent;
                l.ForeColor = Color.White;
                l.AutoSize = true;
                l.MouseLeave += Hide_OperationPanel;
                l.MouseEnter += Show_OperationPanel;

                PictureBox p = new PictureBox();
                p.Name = "p" + i;
                p.SizeMode = PictureBoxSizeMode.Zoom;
                p.Dock = DockStyle.Fill;
                p.DoubleClick += (sender, e) => Panel_DoubleClick(sender, e, panelLength);
                p.Controls.Add(l);

                Label errorLabel = new Label();
                errorLabel.FlatStyle = FlatStyle.Flat;
                errorLabel.Font = new Font("Times New Roman", 8);
                errorLabel.Anchor = AnchorStyles.None;
                errorLabel.Text = "error";
                errorLabel.AutoSize = true;
                errorLabel.ForeColor = Color.Red;
                //Point locationToDraw = new Point();
                //locationToDraw.X = (panel.Width / 2) - (errorLabel.Width / 2);
                //locationToDraw.Y = (panel.Height / 2) - (errorLabel.Height / 2);
                //errorLabel.Location = locationToDraw;
                errorLabel.BackColor = Color.Transparent;
                //errorLabel.Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom);
                //errorLabel.MouseLeave += Hide_OperationPanel;
                //errorLabel.MouseEnter += Show_OperationPanel;
                panel.Controls.Add(errorLabel);
                //errorLabel.BringToFront();

                Panel operationPanel = new Panel();
                operationPanel.BackColor = Color.FromArgb(28, 31, 31);
                operationPanel.Dock = DockStyle.Bottom;
                operationPanel.Height = 24;
                operationPanel.Tag = i;
                operationPanel.MouseLeave += Hide_Self;
                p.MouseEnter += SetVisible;
                p.MouseLeave += SetInvisible;

                Button detail = new Button();
                Button sound = new Button();

                detail.Text = "  ";
                detail.Height = 18;
                detail.Width = 18;
                detail.FlatStyle = FlatStyle.Flat;
                detail.FlatAppearance.BorderSize = 0;
                detail.BackgroundImageLayout = ImageLayout.Zoom;
                detail.BackgroundImage = Resources.detail;
                detail.Name = p.Name;
                detail.Tag = list[i];
                detail.Click += (s, e) => Panel_Pop(s, e, sound);
                detail.MouseLeave += Hide_Parent;
                detail.Location = new Point(1, 1);
                operationPanel.Controls.Add(detail);

                sound.Text = " ";
                sound.Tag = i;
                sound.Height = 18;
                sound.Width = 18;
                sound.FlatStyle = FlatStyle.Flat;
                sound.FlatAppearance.BorderSize = 0;
                sound.BackgroundImageLayout = ImageLayout.Zoom;
                sound.BackgroundImage = Resources.mute;
                sound.Click += async (s, e) => await Play_SoundAsync(s, e);
                sound.MouseLeave += Hide_Parent;
                sound.Location = new Point(22, 1);
                operationPanel.Controls.Add(sound);

                ComboBox cb = new ComboBox();
                cb.BackColor = Color.FromArgb(28, 31, 31);
                cb.ForeColor = Color.White;
                cb.FlatStyle = FlatStyle.Flat;
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                cb.DropDownWidth = 85;
                cb.Margin = new Padding(0, 0, 0, 0);
                cb.Width = cb.Width < panelLength - 50 ? cb.Width : panelLength - 50;
                cb.ValueMember = "Value";
                cb.DisplayMember = "Text";
                cb.Items.AddRange(Helper.GetComboItems(list[i]));
                cb.Text = menuCB.Text;
                cb.SelectedValueChanged += (s, e) => Slide_Change(s, e, sound);
                cb.MouseLeave += Hide_Parent;
                cb.Dock = DockStyle.Right;
                operationPanel.Controls.Add(cb);
                operationPanel.Hide();

                panel.Controls.Add(p);
                panel.Controls.Add(operationPanel);
                panel.Controls.SetChildIndex(p, 0);
                panel.Controls.SetChildIndex(operationPanel, 1);
                panel.Tag = TokenSource;

                flRightPanel.Controls.Add(panel);
                tasks.Add(UpdatePictureBox(p, panel, cb, i));
            }
        }

        private void Hide_Self(object sender, EventArgs e)
        {
            Panel op = (Panel)sender;
            bool isMouseHoverself = IsMouseHover(op.Parent);
            if (!isMouseHoverself)
            {
                foreach (Button l in Helper.GetAll(op, typeof(Button)).Where(a => a.Text == ""))
                {
                    l.PerformClick();
                }
            }
            op.Hide();
        }

        private void Hide_Parent(object sender, EventArgs e)
        {
            Panel op = (Panel)((Control)sender).Parent;
            bool isMouseHoverp = IsMouseHover(op.Parent);
            if (!isMouseHoverp)
            {
                foreach (Button l in Helper.GetAll(op, typeof(Button)).Where(a => a.Text == ""))
                {
                    l.PerformClick();
                }
            }
            op.Hide();
        }

        private void Hide_OperationPanel(object sender, EventArgs e)
        {
            Panel p = (Panel)((Control)sender).Parent.Parent;
            bool isMouseHoverp = IsMouseHover(p);
            if (!isMouseHoverp)
            {
                foreach (Panel op in Helper.GetAll(p, typeof(Panel)))
                {
                    op.Hide();
                }
            }
        }

        private void Show_OperationPanel(object sender, EventArgs e)
        {
            Panel p = (Panel)((Control)sender).Parent.Parent;
            foreach (Panel op in Helper.GetAll(p, typeof(Panel)))
            {
                op.Show();
            }
        }

        private void SetInvisible(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            List<Control> list = Helper.GetAll(p.Parent, typeof(Panel)).ToList();
            bool isMouseHoverp = IsMouseHover(p.Parent);
            if (!isMouseHoverp)
            {
                foreach (Button l in Helper.GetAll(p.Parent, typeof(Button)).Where(a => a.Text == ""))
                {
                    l.PerformClick();
                }
                list[0].Hide();
            }
        }

        bool IsMouseHover(Control container)
        {
            Point p = container.PointToClient(MousePosition);
            if (container.DisplayRectangle.Contains(p))
                return true;
            else
                return false;
        }

        private void SetVisible(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)sender;
            List<Control> list = Helper.GetAll(p.Parent, typeof(Panel)).ToList();
            Panel fl = (Panel)list[0];
            fl.Show();
        }

        private Task GetWave(int i)
        {
            return Task.Run(() =>
            {
                for (int j = 0; j < 5; j++)
                {
                    try
                    {
                        //ClassroomData classroomData = new ClassroomData(list[i].PPCPublicIP, list[i].PPCPort);
                        string filename = @"\\" + list[i].PPCPublicIP + @"\Monitor\AudioMp35s.mp4";
                        if (!Directory.Exists(list[i].PPCPublicIP))
                            Directory.CreateDirectory(list[i].PPCPublicIP);
                        File.Copy(filename, Path.Combine(list[i].PPCPublicIP, "AudioMp35s.mp4"), true);
                        return;
                    }
                    catch (Exception ex)
                    {
                        //LogHelper.GetLogger().Error(ex.ToString());
                        Thread.Sleep(20);
                    }
                }
            });
        }

        private async Task Play_SoundAsync(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            int i = (int)b.Tag;
            if (!Helper.dict.ContainsKey(list[i].PPCPublicIP))
            {
                try
                {
                    if (b.Text == " ")
                    {
                        b.Text = "";
                        b.BackgroundImage = Resources.sound;
                        disposeWave();
                        await GetWave(i);
                        string fileName = Path.Combine(list[i].PPCPublicIP, "AudioMp35s.mp4");
                        if (File.Exists(fileName))
                        {
                            DateTime lastModified = File.GetLastWriteTime(fileName);
                            if (lastModified.AddSeconds(60) < DateTime.Now)
                            {
                                MessageBox.Show("Audio unavaiable!");
                                return;
                            }
                        }
                        if (waveOutDevice == null) waveOutDevice = new WaveOut();
                        if (audioFileReader == null) audioFileReader = new AudioFileReader(fileName);
                        waveOutDevice.Init(audioFileReader);
                        waveOutDevice.PlaybackStopped += async (send, args) => await AutoPlaySoundAsync(send, args, i);
                        waveOutDevice.Play();
                    }
                    else
                    {
                        b.Text = " ";
                        b.BackgroundImage = Resources.mute;
                        disposeWave();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.GetLogger().Error(ex.ToString());
                    //MessageBox.Show(ex.Message);
                    MessageBox.Show("Audio unavaiable!");
                }
            }
        }

        private async Task AutoPlaySoundAsync(object sender, EventArgs e, int i)
        {
            try
            {
                disposeWave();
                await GetWave(i);
                string fileName = Path.Combine(list[i].PPCPublicIP, "AudioMp35s.mp4");
                if (File.Exists(fileName))
                {
                    DateTime lastModified = File.GetLastWriteTime(fileName);
                    if (lastModified.AddSeconds(60) < DateTime.Now)
                    {
                        MessageBox.Show("Audio Outdated!");
                        //return;
                    }
                }
                if (waveOutDevice == null) waveOutDevice = new WaveOut();
                waveOutDevice.PlaybackStopped += async (send, args) => await AutoPlaySoundAsync(send, args, i);
                if (audioFileReader == null) audioFileReader = new AudioFileReader(fileName);
                waveOutDevice.Init(audioFileReader);
                waveOutDevice.Play();
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger().Error(ex.ToString());
                MessageBox.Show(ex.Message);
            }
        }
        private void disposeWave()
        {
            waveOutDevice?.Stop();
            audioFileReader?.Dispose();
            waveOutDevice?.Dispose();
            audioFileReader = null;
            waveOutDevice = null;
        }

        private void Slide_Change(object sender, EventArgs e, Button sound)
        {
            if (sound.Text == "")
                sound.PerformClick();
            ComboBox cb = (ComboBox)(sender);
            //this is remove the comboBox shadow
            cb.Parent.Focus();
            CancellationTokenSource tokenSource = (CancellationTokenSource)cb.Parent.Parent.Tag;
            int taskIndex = (int)cb.Parent.Tag;
            tokenSource.Cancel();
            tokenSourceList.Remove(tokenSource);
            tasks.RemoveAt(taskIndex);
            ((Panel)cb.Parent.Parent).Tag = TokenSource;
            tasks.Insert(taskIndex, UpdatePictureBox((PictureBox)(Helper.GetAll(cb.Parent.Parent, typeof(PictureBox)).First()), (Panel)cb.Parent.Parent, cb, taskIndex));
        }
        private void Panel_Pop(object sender, EventArgs e, Button sound)
        {
            int i = (int)sound.Tag;
            if (!Helper.dict.ContainsKey(list[i].PPCPublicIP))
            {
                Button b = (Button)sender;
                if (sound.Text == "")
                    sound.PerformClick();
                //CancellationTokenSource tokenSource = (CancellationTokenSource)b.Parent.Parent.Tag;
                //tokenSource.Cancel();
                ClassroomDetailForm classroomDetail = new ClassroomDetailForm(b);
                classroomDetail.Show();
                Helper.dict.TryAdd(list[i].PPCPublicIP, true);
            }
        }

        private void Panel_DoubleClick(object sender, EventArgs e, int panelLength)
        {
            PictureBox p = (PictureBox)(sender);
            Panel panel = (Panel)p.Parent;
            p_DoubleClick(panel, e, panelLength);
        }

        private void p_DoubleClick(object sender, EventArgs e, int panelLength)
        {
            var p = (Panel)sender;
            if (p.Width > panelLength)
            {
                p.Width /= 2;
                p.Height /= 2;
            }
            else
            {
                p.Width *= 2;
                p.Height *= 2;
            }
        }

        private bool isMac(string classroomtype)
        {
            return classroomtype != "" && classroomtype.StartsWith("Mac");
        }

        private Task UpdatePictureBox(PictureBox p, Panel panel, ComboBox cb, int i)
        {
            CancellationTokenSource tokenSource = (CancellationTokenSource)panel.Tag;
            Label errorLabel = (Label)Helper.GetLast(panel, typeof(Label));
            Label classroomLabel = (Label)Helper.GetFirst(p, typeof(Label));

            return Task.Run(() =>
            {
                while (true)
                {
                    string exMessage = "";
                    if (tokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    NetworkShareAccesser accessor = null;
                    try
                    {
                        List<string> itemList = new List<string>();
                        foreach (var item in cb.Items)
                            itemList.Add(item.ToString());
                        //ClassroomData data = new ClassroomData(list[i].PPCPublicIP, list[i].PPCPort);
                        string fileName = Helper.GetFileLocation(list[i], cb.SelectedItem.ToString());
                        string audioFileName = @"\\" + list[i].PPCPublicIP + @"\Monitor\AudioMp35s.mp4";
                        bool isSuccess = false;

                        if (isMac(list[i].ClassroomTypeName)&&false)
                        {
                            var userName = ConfigurationManager.AppSettings.Get("UserName");
                            var password = ConfigurationManager.AppSettings.Get("Password");
                            accessor = NetworkShareAccesser.Access(@"\\" + list[i].PPCPublicIP,
                                AesHelper.DecryptString(userName), 
                                AesHelper.DecryptString(password));
                        }

                        for (int j = 0; j < 5; j++)
                        {
                            exMessage = "";
                            try
                            {
                                foreach (var s in itemList)
                                {
                                    string curFileLocation = Helper.GetFileLocation(list[i], s);
                                    if (File.Exists(curFileLocation))
                                    {
                                        DateTime lastModified = File.GetLastWriteTime(curFileLocation);
                                        if (lastModified.AddSeconds(60) < DateTime.Now)
                                        {
                                            exMessage += Helper.GetExMessageText(s) + " outdated\n";
                                        }
                                    }
                                    else
                                    {
                                        exMessage += "No " + Helper.GetExMessageText(s) + "\n";
                                    }
                                }
                                if (File.Exists(audioFileName))
                                {
                                    DateTime audioLastModified = File.GetLastWriteTime(audioFileName);
                                    if (audioLastModified.AddSeconds(60) < DateTime.Now)
                                    {
                                        exMessage += "Audio outdated\n";
                                    }
                                }
                                else
                                {
                                    exMessage += "No Audio\n";
                                }
                                //if (exMessage != "")
                                //    throw new Exception(exMessage);
                                byte[] bytes = File.ReadAllBytes(fileName);
                                isSuccess = true;
                                using (MemoryStream loadStream = new MemoryStream(bytes, 0, bytes.Length))
                                {
                                    Action action = () =>
                                    {
                                        classroomLabel.ForeColor = Color.White;
                                        errorLabel.Text = exMessage;
                                        Point locationToDraw = new Point();
                                        //locationToDraw.X = 0;
                                        locationToDraw.Y = panel.Height - errorLabel.Height - 24;
                                        errorLabel.Location = locationToDraw;
                                        errorLabel.BringToFront();
                                        p.Image = Image.FromStream(loadStream);
                                    };
                                    p.SafeInvoke(action, true);
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                Thread.Sleep(20);
                                //exMessage += "current failure";

                                throw new Exception(exMessage);
                            }
                        }
                        if (!isSuccess)
                        {
                            //exMessage += "current failure!";
                            throw new Exception(exMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        Action action = () =>
                        {
                            classroomLabel.ForeColor = Color.Red;
                            p.Image = null;
                            errorLabel.ForeColor = Color.Red;
                            errorLabel.Text = ex.Message;
                            Point locationToDraw = new Point();
                            //locationToDraw.X = (panel.Width / 2) - (errorLabel.Width / 2);
                            //locationToDraw.Y = (panel.Height / 2) - (errorLabel.Height / 2);
                            ////locationToDraw.X = 0;
                            locationToDraw.Y = panel.Height - errorLabel.Height - 24;
                            errorLabel.Location = locationToDraw;
                            errorLabel.BringToFront();
                        };
                        p.SafeInvoke(action, true);
                        if (list != null && list.Count > i && list[i].OnScheduleNow == "True")
                        {
                            Color temp = dataGridView.Rows[i].DefaultCellStyle.BackColor;
                            dataGridView.Rows[i].DefaultCellStyle.BackColor = Color.Yellow;
                            Thread.Sleep(500);
                            dataGridView.Rows[i].DefaultCellStyle.BackColor = temp;
                            lock (this)
                            {
                                if (DateTime.Now.Subtract(lastSoundTime) > TimeSpan.FromSeconds(5))
                                {
                                    SystemSounds.Hand.Play();
                                    lastSoundTime = DateTime.Now;
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (accessor != null) accessor.Dispose();
                        Thread.Sleep(2000);
                    }
                }
            }, tokenSource.Token);

        }

        private async void button11_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                button11.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    int classroomID = list[row].ClassroomID;
                    var client = Helper.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID };
                    HttpResponseMessage response = await client.GetAsync("api/Operation/ClassroomInfo?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);
                    if (response.IsSuccessStatusCode)
                    {
                        ClassroomInfoView classroomInfoView = await response.Content.ReadAsAsync<ClassroomInfoView>();
                        ClassroomInfoForm classroomInfoForm = new ClassroomInfoForm(classroomInfoView);
                        classroomInfoForm.Show();
                    }
                    else
                    {
                        //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                        MessageBox.Show("failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("service error!");
            }
            finally
            {
                button11.Enabled = true;
            }
        }

        private void dataGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                int index = e.RowIndex;
                DataGridViewRow row = dataGridView.Rows[index];
                string es = UpdateList[index].EngineStatus?.ToUpper();
                es = es == null ? "" : es;
                string clrname = UpdateList[index].ClassroomName?.ToUpper();
                string ags = UpdateList[index].AgentStatus?.ToUpper();
                string ppcs = UpdateList[index].PPCConnectionStatus?.ToUpper();
                string course = UpdateList[index].CourseName?.ToUpper();
                string sc = UpdateList[index].ScreenCaptureStatus?.ToUpper();
                string avs = UpdateList[index].AVStatus?.ToUpper();
                //string cs = row.Cells["CameraStatus"].Value?.ToString().ToUpper();
                string ActiveWBNumber = UpdateList[index].ClassRoomWBNumber.ToString();
                string WBNumber = UpdateList[index].WBNumber.ToString();
                string wb = UpdateList[index].WB?.ToString();
                string wbs = UpdateList[index].WBStatus?.ToString();
                string kap = UpdateList[index].Kaptivo?.ToString();
                string freedisk = UpdateList[index].FreeDisk.ToString();

                Color color = ClassroomStatusUtil.TransformInfoByEngineStatus(es, clrname);
                row.Cells["ClassroomName"].Style.ForeColor = color;
                row.Cells["EngineStatus"].Style.ForeColor = ClassroomStatusUtil.TransferEngineStatus(es);
                row.Cells["AgentStatus"].Style.ForeColor = ClassroomStatusUtil.TransferAgentStatus(es, ags);
                row.Cells["PPCConnectionStatus"].Style.ForeColor = ClassroomStatusUtil.TransferConnStatus(es, ppcs);
                row.Cells["CourseName"].Style.ForeColor = ClassroomStatusUtil.TransferStatusEx(es, course);
                row.Cells["ScreenCaptureStatus"].Style.ForeColor = ClassroomStatusUtil.TransferStatusEx(es, sc);
                row.Cells["AVStatus"].Style.ForeColor = ClassroomStatusUtil.TransferStatusExAV(es, avs);
                row.Cells["FreeDisk"].Style.ForeColor = ClassroomStatusUtil.TransferDiskStatus(es, freedisk);
                //to be changed
                row.Cells["WB"].Style.ForeColor = (wbs == "OFF" || ActiveWBNumber != WBNumber) ? Color.Red : color;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                LogHelper.GetLogger().Error(ex.ToString());
            }
        }

        private async void menuCB_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            try
            {
                //remove the shadow
                menuCB.Parent.Focus();
                if (list == null || list.Count == 0)
                {
                    int groupID = (int)comboBox.SelectedValue;
                    await LoadClassroomList(LoginInfo.sessionID, groupID);
                    dataGridView.DataSource = UpdateList;
                    dataGridView.ClearSelection();
                    CreatePanel(menuCB.SelectedItem.ToString());
                    tasks.Add(UpdatingClassroomList(LoginInfo.sessionID, groupID, TokenSource));
                }
                else
                {
                    IEnumerable<Control> comboBoxes = Helper.GetAll(flRightPanel, typeof(ComboBox));
                    string selectedValue = menuCB.SelectedItem.ToString();
                    string selectedText = menuCB.Text;
                    foreach (Control c in comboBoxes)
                    {
                        ComboBox b = (ComboBox)c;
                        foreach (ComboItem i in b.Items)
                        {
                            if (i.Text.Equals(selectedText))
                                b.SelectedItem = i;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger().Error(ex.ToString());
            }
        }
        private async Task LoadClassroomList(string sessionID, int groupID)
        {
            var client = Helper.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var tuple = new { sessionID = sessionID, groupID = groupID };
            HttpResponseMessage response = await client.PostAsJsonAsync("api/classroom/GetClassroomListByGroupID", tuple);
            if (response.IsSuccessStatusCode)
            {
                list = await response.Content.ReadAsAsync<List<ClassroomView>>();
                list = ClassroomStatusUtil.TransformClassroomView(list);
                UpdateList = new BindingList<ClassroomView>(list);

            }
        }

        private async Task UpdatingClassroomList(string sessionID, int groupID, CancellationTokenSource tokenSource)
        {
            await Task.Run(async () =>
            {

                while (true)
                {
                    if (tokenSource.IsCancellationRequested)
                        break;
                    try
                    {
                        var client = Helper.CreateClient();
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var tuple = new { sessionID = sessionID, groupID = groupID };
                        HttpResponseMessage response = await client.PostAsJsonAsync("api/classroom/GetClassroomListByGroupID", tuple);
                        if (response.IsSuccessStatusCode && !tokenSource.IsCancellationRequested)
                        {
                            List<ClassroomView> curList = await response.Content.ReadAsAsync<List<ClassroomView>>();
                            curList = ClassroomStatusUtil.TransformClassroomView(curList);
                            if (curList != null && UpdateList != null && !tokenSource.IsCancellationRequested)
                            {
                                if (curList.Count != UpdateList.Count)
                                {
                                    UpdateList = new BindingList<ClassroomView>(curList);
                                    Action action = () => { dataGridView.DataSource = UpdateList; dataGridView.ClearSelection(); };
                                    dataGridView.SafeInvoke(action, true);
                                    continue;
                                }
                                else
                                {
                                    for (int i = 0; i < curList.Count; i++)
                                    {
                                        if (!curList[i].Equals(UpdateList[i]))
                                        {
                                            UpdateList[i] = curList[i];
                                        }
                                    }
                                }
                            }
                        }
                        Thread.Sleep(2000);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.GetLogger().Error(ex.ToString());
                    }
                }
            });
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            //remove the shadow
            comboBox.Parent.Focus();
            menuCB.DisplayMember = "Text";
            menuCB.ValueMember = "Value";
            list = null;
            menuCB.DataSource = Helper.GetComboItems();
        }

        private void dataGridView_MouseClick(object sender, MouseEventArgs e)
        {
            var ht = dataGridView.HitTest(e.X, e.Y);

            if (ht.Type == DataGridViewHitTestType.None)
            {
                dataGridView.ClearSelection();
                List<Control> list = Helper.GetAll(flRightPanel, typeof(PictureBox)).ToList();
                foreach (Control c in list)
                {
                    c.Paint -= p_Paint;
                    c.Invalidate();
                }
            }
            else
            {
                if (dataGridView.SelectedRows.Count > 0)
                {
                    int selectedRowIndex = dataGridView.SelectedRows[0].Index;
                    List<Control> list = Helper.GetAll(flRightPanel, typeof(PictureBox)).ToList();
                    foreach (Control c in list)
                    {
                        c.Paint -= p_Paint;
                        c.Invalidate();
                    }
                    PictureBox p = (PictureBox)list[selectedRowIndex];
                    p.Paint += p_Paint;
                    p.Invalidate();
                }
            }
        }
        private void p_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.YellowGreen, 5))
            {
                // Create a local version of the graphics object for the sender.
                PictureBox p = (PictureBox)sender;
                Graphics g = e.Graphics;
                Rectangle RcDraw = new Rectangle(-1, -1, p.Width, p.Height);
                e.Graphics.DrawRectangle(pen, RcDraw);
            }
        }
        private async void btnStopCourse_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                btnStopCourse.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom first");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    int classroomID = list[row].ClassroomID;
                    var client = Helper.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync("api/Operation/GetClassroomStatusbyClassroomID?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                    if (response.IsSuccessStatusCode)
                    {
                        ClassroomEngineAgentStatus status = await response.Content.ReadAsAsync<ClassroomEngineAgentStatus>();
                        string prompt = "";
                        if (status.EngineStatus.ToUpper() != "RECORDING A LECTURE" || status.ClassTypeID == null)
                        {
                            MessageBox.Show("There is not recording in this room!");
                            return;
                        }
                        string confirmStr = "";
                        prompt = GetStopOrAbortPromptString(status, isStop: true, ref confirmStr);

                        if (status.ClassTypeID == 3 &&
                            MessageBox.Show(prompt, "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        {
                            return;
                        }
                        if (status.ClassTypeID != 3 && confirmStr != Microsoft.VisualBasic.Interaction.InputBox(prompt, "confirm"))
                        {
                            return;
                        }

                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        HttpResponseMessage resp = await client.GetAsync("api/Operation/StopRecord?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                        if (resp.IsSuccessStatusCode)
                        {
                            //ExecutionResult result= await response.Content.ReadAsAsync<ExecutionResult>();
                            MessageBox.Show("succeeded!");
                        }
                        else
                        {
                            //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                            MessageBox.Show("failure!");
                        }

                    }
                    else
                    {
                        MessageBox.Show("failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("service error!");
            }
            finally
            {
                btnStopCourse.Enabled = true;
            }
        }

        private async void btnPushSchedule_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                btnPushSchedule.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    DialogResult res = MessageBox.Show("Are you sure you want to push schedule for " + list[row].ClassroomName, "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (res == DialogResult.Cancel)
                        return;
                    int classroomID = list[row].ClassroomID;
                    var client = Helper.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID };
                    HttpResponseMessage response = await client.GetAsync("api/Operation/PushSchedule?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                    if (response.IsSuccessStatusCode)
                    {
                        //ExecutionResult result= await response.Content.ReadAsAsync<ExecutionResult>();
                        MessageBox.Show("succeeded!");
                    }
                    else
                    {
                        //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                        MessageBox.Show("failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("service error!");
            }
            finally
            {
                btnPushSchedule.Enabled = true;
            }
        }
        // btn abort
        private async void button2_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                button2.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    int classroomID = list[row].ClassroomID;

                    var client = Helper.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    HttpResponseMessage response = await client.GetAsync("api/Operation/GetClassroomStatusbyClassroomID?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                    if (response.IsSuccessStatusCode)
                    {
                        ClassroomEngineAgentStatus status = await response.Content.ReadAsAsync<ClassroomEngineAgentStatus>();
                        string prompt = "";
                        string input = "";
                        if (status.EngineStatus.ToUpper() != "RECORDING A LECTURE" || status.ClassTypeID == null)
                        {
                            MessageBox.Show("There is not recording in this room!");
                            return;
                        }
                        else if (status.ClassTypeID == 3)
                        {
                            prompt = GetStopOrAbortPromptString(status, isStop: false, ref input);
                            if (MessageBox.Show(prompt, "", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == System.Windows.Forms.DialogResult.No)
                            {
                                return;
                            }
                        }
                        else
                        {
                            prompt = GetStopOrAbortPromptString(status, isStop: false, ref input);

                            string myinput = Microsoft.VisualBasic.Interaction.InputBox(prompt,
                               "confirm", "", -1, -1);
                            MessageBox.Show(myinput + ": " + input);

                            if (myinput != input || myinput == "")
                                return;
                        }

                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        //var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID };
                        HttpResponseMessage resp = await client.GetAsync("api/Operation/AbortRecord?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                        if (resp.IsSuccessStatusCode)
                        {
                            //ExecutionResult result= await response.Content.ReadAsAsync<ExecutionResult>();
                            MessageBox.Show("succeeded!");
                        }
                        else
                        {
                            //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                            MessageBox.Show("failure!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("service error!");
            }
            finally
            {
                button2.Enabled = true;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                button3.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    StartTestCourseForm form = new StartTestCourseForm(list[row]);
                    form.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                button3.Enabled = true;
            }
        }

        private async void button6_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                button6.Text = "Checking";
                button6.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    int classroomID = list[row].ClassroomID;
                    var client = Helper.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID };
                    HttpResponseMessage response = await client.GetAsync("api/Operation/CheckSchedule?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                    if (response.IsSuccessStatusCode)
                    {
                        ExecutionResult result = await response.Content.ReadAsAsync<ExecutionResult>();
                        DataTable list = JsonConvert.DeserializeObject<DataTable>((string)result.Obj);
                        CheckScheduleForm cs = new CheckScheduleForm(list);
                        cs.Show();
                    }
                    else
                    {
                        //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                        MessageBox.Show("failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("service failure!");
            }
            finally
            {
                button6.Text = "Check Schedule";
                button6.Enabled = true;
            }
        }

        private async void button10_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                button10.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    int classroomID = list[row].ClassroomID;
                    var client = Helper.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID };
                    HttpResponseMessage response = await client.GetAsync("api/Operation/ListLocalData?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                    if (response.IsSuccessStatusCode)
                    {
                        ExecutionResult result = await response.Content.ReadAsAsync<ExecutionResult>();
                        string resultObj = result.Obj.ToString();
                        List<string> list = JsonConvert.DeserializeObject<List<string>>(resultObj);
                        ListLocalDataForm cs = new ListLocalDataForm(list, classroomID);
                        cs.Show();
                    }
                    else
                    {
                        //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                        MessageBox.Show("failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                //make the exception message not freeze the application
                var thread = new Thread(() => { MessageBox.Show("service error!"); });
                thread.Start();
            }
            finally
            {
                button10.Enabled = true;
            }
        }

        private async void button5_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                button5.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    DialogResult res = MessageBox.Show("Are you sure you want push config for " + list[row].ClassroomName, "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (res == DialogResult.Cancel)
                        return;
                    int classroomID = list[row].ClassroomID;
                    var client = Helper.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID };
                    HttpResponseMessage response = await client.GetAsync("api/Operation/PushConfig?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                    if (response.IsSuccessStatusCode)
                    {
                        //ExecutionResult result= await response.Content.ReadAsAsync<ExecutionResult>();
                        PPCIPCReturnParameter result = await response.Content.ReadAsAsync<PPCIPCReturnParameter>();
                        MessageBox.Show("result for " + list[row].ClassroomName + ":\npush ppc config: " + result.PPCReturnParameter.succ.ToString() + "\n" +
                            "push ipc config: " + result.IPCReturnParameter.succ.ToString());
                    }
                    else
                    {
                        //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                        MessageBox.Show("failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show("service error!");
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                button5.Enabled = true;
            }
        }

        private async void button8_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                button8.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    DialogResult res = MessageBox.Show("Are you sure you want reboot PPC for " + list[row].ClassroomName, "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (res == DialogResult.Cancel)
                        return;
                    int classroomID = list[row].ClassroomID;
                    var client = Helper.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID };
                    HttpResponseMessage response = await client.GetAsync("api/Operation/RebootPPC?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                    if (response.IsSuccessStatusCode)
                    {
                        //ExecutionResult result= await response.Content.ReadAsAsync<ExecutionResult>();
                        MessageBox.Show("succeeded!");
                    }
                    else
                    {
                        //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                        MessageBox.Show("failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("service error!");
            }
            finally
            {
                button8.Enabled = true;
            }
        }

        private async void button9_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                button9.Enabled = false;
                if (dataGridView.SelectedRows.Count == 0)
                    MessageBox.Show("Please select a classroom");
                else
                {
                    int row = dataGridView.SelectedRows[0].Index;
                    DialogResult res = MessageBox.Show("Are you sure you want reboot IPC for " + list[row].ClassroomName, "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (res == DialogResult.Cancel)
                        return;
                    int classroomID = list[row].ClassroomID;
                    var client = Helper.CreateClient();
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    //var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID };
                    HttpResponseMessage response = await client.GetAsync("api/Operation/RebootIPC?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);

                    if (response.IsSuccessStatusCode)
                    {
                        //ExecutionResult result= await response.Content.ReadAsAsync<ExecutionResult>();
                        MessageBox.Show("succeeded!");
                    }
                    else
                    {
                        //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                        MessageBox.Show("failure!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("service error!");
            }
            finally
            {
                button9.Enabled = true;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                button7.Enabled = false;
                if (comboBox.Items.Count == 0)
                    MessageBox.Show("there is no group");
                else
                {
                    int groupID = (int)comboBox.SelectedValue;
                    GroupScheduleForm gs = new GroupScheduleForm(groupID);
                    gs.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("service error!");
            }
            finally
            {
                button7.Enabled = true;
            }
        }


        protected string GetStopOrAbortPromptString(ClassroomEngineAgentStatus status, bool isStop, ref string promtstr)
        {
            bool isTestClass = status.ClassTypeID == 3;
            string coursename = status.CourseName.Replace("\"", "").Replace("'", "");
            if (coursename.Length > 20)
            {
                coursename = coursename.Substring(0, 20);
            }
            promtstr = coursename.Replace(" ", "");
            if (promtstr.Length > 8)
            {
                promtstr = promtstr.Substring(0, 8);
            }
            if (!isTestClass)
            {
                promtstr = (isStop ? "STOP" : "ABORT") + promtstr;
            }

            string commandstring = isStop ? "Stop" : "Abort";

            if (isTestClass)
            {
                return string.Format("Do you want to {0} course: {1} in classroom {2}?", commandstring, coursename, status.ClassroomName);
            }
            else
            {
                return string.Format("If you want to {0} Course: {1} in classroom {2} ? Please input:  {3}", commandstring, coursename, status.ClassroomName, promtstr);
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
        }
        //this two methods is to used for the mouse to move the header of the panel
        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - lastPoint.X;
                this.Top += e.Y - lastPoint.Y;
            }
        }
        Point lastPoint;
        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }

        private void comboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //remove the shadow
            comboBox.Parent.Focus();
            menuCB.DisplayMember = "Text";
            menuCB.ValueMember = "Value";
            list = null;
            menuCB.DataSource = Helper.GetComboItems();
        }
    }
}