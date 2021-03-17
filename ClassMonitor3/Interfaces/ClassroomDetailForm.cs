using ClassMonitor3.Model;
using ClassMonitor3.Properties;
using ClassMonitor3.Util;
using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassMonitor3.Interfaces
{
    public partial class ClassroomDetailForm : Form
    {
        public Button lb;
        int h = 0;
        int w = 0;
        int ph = 0;
        int wh = 0;
        public ClassroomView classroomView;
        List<Task> tasks = new List<Task>();
        List<CancellationTokenSource> tokenSourceList = new List<CancellationTokenSource>();
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
        public ClassroomDetailForm(Button button)
        {
            lb = button;
            InitializeComponent();
            classroomView = (ClassroomView)lb.Tag;
            Text = classroomView.ClassroomName;
            h = flowLayoutPanel1.Height;
            w = flowLayoutPanel1.Width;
            ph = h / 2 - 2;
            wh = w / 2 - 2;

            Panel panel1 = new Panel();
            panel1.Margin = new Padding(1, 1, 1, 1);
            panel1.Height = ph;
            panel1.Width = wh;
            PictureBox p1 = new PictureBox();
            p1.BackColor = Color.Black;
            p1.Dock = DockStyle.Fill;
            p1.Tag = TokenSource;
            p1.SizeMode = PictureBoxSizeMode.Zoom;
            p1.Name = lb.Name;
            p1.DoubleClick += panel_doubleClick;
            panel1.Controls.Add(p1);
            flowLayoutPanel1.Controls.Add(panel1);
            GenerateErrorLabel(panel1);

            Button sound = new Button();
            sound.Height = 18;
            sound.Width = 18;
            sound.FlatStyle = FlatStyle.Flat;
            sound.BackgroundImageLayout = ImageLayout.Zoom;
            sound.BackgroundImage = Resources.mute;
            sound.BackColor = Color.Transparent;
            sound.Text = " ";
            sound.FlatAppearance.BorderSize = 0;
            sound.Click += Play_Sound;
            tasks.Add(UpdatePictureBox(p1, panel1, "CameraVideo", 19, sound));

            for (int i = 0; i < classroomView.ClassRoomWBNumber; i++)
            {
                Panel panel2 = new Panel();
                panel2.Margin = new Padding(1, 1, 1, 1);
                panel2.Height = ph;
                panel2.Width = wh;
                PictureBox p2 = new PictureBox();
                p2.BackColor = Color.Black;
                p2.Dock = DockStyle.Fill;
                p2.Tag = TokenSource;
                p2.SizeMode = PictureBoxSizeMode.Zoom;
                p2.Name = lb.Name;
                p2.DoubleClick += panel_doubleClick;
                panel2.Controls.Add(p2);
                flowLayoutPanel1.Controls.Add(panel2);
                GenerateErrorLabel(panel2);
                tasks.Add(UpdatePictureBox(p2, panel2, "whiteboard" + (i + 1), 55 + i));
            }

            for (int i = 0; i < classroomView.KaptivoNumber; i++)
            {
                Panel panel3 = new Panel();
                panel3.Margin = new Padding(1, 1, 1, 1);
                panel3.Height = ph;
                panel3.Width = wh;
                PictureBox p3 = new PictureBox();
                p3.BackColor = Color.Black;
                p3.Dock = DockStyle.Fill;
                p3.Tag = TokenSource;
                p3.SizeMode = PictureBoxSizeMode.Zoom;
                p3.Name = lb.Name;
                p3.DoubleClick += panel_doubleClick;
                panel3.Controls.Add(p3);
                flowLayoutPanel1.Controls.Add(panel3);
                GenerateErrorLabel(panel3);
                tasks.Add(UpdatePictureBox(p3, panel3, "kaptivo" + (i + 1), 52 + i));
            }

            Panel panel4 = new Panel();
            panel4.Margin = new Padding(1, 1, 1, 1);
            panel4.Height = ph;
            panel4.Width = wh;
            PictureBox p4 = new PictureBox();
            p4.BackColor = Color.Black;
            p4.Dock = DockStyle.Fill;
            p4.Tag = TokenSource;
            p4.SizeMode = PictureBoxSizeMode.Zoom;
            p4.Name = lb.Name;
            p4.DoubleClick += panel_doubleClick;
            panel4.Controls.Add(p4);
            flowLayoutPanel1.Controls.Add(panel4);
            GenerateErrorLabel(panel4);
            tasks.Add(UpdatePictureBox(p4, panel4, "TeacherScreen", 51));
        }

        private void GenerateErrorLabel(Panel panel)
        {
            Label errorLabel1 = new Label();
            errorLabel1.Anchor = AnchorStyles.None;
            errorLabel1.Text = "this is the error";
            errorLabel1.AutoSize = true;
            errorLabel1.ForeColor = Color.Red;
            Point locationToDraw = new Point();
            locationToDraw.X = (panel.Width / 2) - (errorLabel1.Width / 2);
            locationToDraw.Y = (panel.Height / 2) - (errorLabel1.Height / 2);
            errorLabel1.Location = locationToDraw;
            errorLabel1.BackColor = Color.Transparent;
            //errorLabel.Anchor = (AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom);
            //errorLabel.MouseLeave += Hide_OperationPanel;
            //errorLabel.MouseEnter += Show_OperationPanel;
            panel.Controls.Add(errorLabel1);
            //errorLabel1.BringToFront();
        }

        IWavePlayer waveOutDevice = null;
        AudioFileReader audioFileReader = null;
        private async void Play_Sound(object sender, EventArgs e)
        {
            Button b = (Button)(sender);
            try
            {
                if (b.Text == " ")
                {
                    b.Text = "";
                    b.BackgroundImage = Resources.sound;
                    disposeWave();
                    await GetWave();
                    string fileName = Path.Combine(classroomView.PPCPublicIP, "Audio5s.mp4");
                    if (File.Exists(fileName))
                    {
                        DateTime lastModified = File.GetLastWriteTime(fileName);
                        if (lastModified.AddSeconds(60) < DateTime.Now)
                        {
                            MessageBox.Show("Audio Outdated!");
                            return;
                        }
                    }
                    if (waveOutDevice == null) waveOutDevice = new WaveOut();
                    if (audioFileReader == null) audioFileReader = new AudioFileReader(fileName);
                    waveOutDevice.Init(audioFileReader);
                    waveOutDevice.PlaybackStopped += (send, args) => AutoPlaySound(send, args);
                    waveOutDevice.Play();
                }
                else
                {
                    b.Text = " ";
                    b.BackgroundImage = Resources.mute;
                    waveOutDevice?.Stop();
                    audioFileReader?.Dispose();
                    waveOutDevice?.Dispose();
                    audioFileReader = null;
                    waveOutDevice = null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.GetLogger().Error(ex.ToString());
                //MessageBox.Show(ex.Message);
                MessageBox.Show("Audio unavaiable!");
            }
        }

        private void disposeWave()
        {
            if (audioFileReader != null && waveOutDevice != null)
            {
                waveOutDevice.Stop();
                audioFileReader.Dispose();
                waveOutDevice.Dispose();
                audioFileReader = null;
                waveOutDevice = null;
            }
        }

        private Task GetWave()
        {
            return Task.Run(() =>
            {
                for (int j = 0; j < 5; j++)
                {
                    try
                    {
                        //ClassroomData classroomData = new ClassroomData(list[i].PPCPublicIP, list[i].PPCPort);
                        string fileName = @"\\" + classroomView.PPCPublicIP + @"\Monitor\Audio5s.mp4";
                        if (!Directory.Exists(classroomView.PPCPublicIP))
                            Directory.CreateDirectory(classroomView.PPCPublicIP);
                        File.Copy(fileName, Path.Combine(classroomView.PPCPublicIP, "Audio5s.mp4"), true);
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.GetLogger().Error(ex.ToString());
                        Thread.Sleep(20);
                    }
                }
            });
        }

        private async void AutoPlaySound(object sender, EventArgs e)
        {
            try
            {
                disposeWave();
                await GetWave();
                string fileName = Path.Combine(classroomView.PPCPublicIP, "Audio5s.mp4");
                if (File.Exists(fileName))
                {
                    DateTime lastModified = File.GetLastWriteTime(fileName);
                    if (lastModified.AddSeconds(60) < DateTime.Now)
                    {
                        MessageBox.Show("Audio Outdated!");
                        return;
                    }
                }

                if (waveOutDevice == null) waveOutDevice = new WaveOut();
                waveOutDevice.PlaybackStopped += (send, args) => AutoPlaySound(send, args);
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

        private void panel_doubleClick(object sender, EventArgs e)
        {
            PictureBox p = (PictureBox)(sender);
            Panel panel = (Panel)p.Parent;
            p_DoubleClick(panel, e);
        }

        private void p_DoubleClick(Panel panel, EventArgs e)
        {
            var p = panel;
            if (p.Width < 500)
            {
                p.Width = w;
                p.Height = h;
            }
            else
            {
                p.Width = wh;
                p.Height = ph;
            }
            flowLayoutPanel1.ScrollControlIntoView(p);
        }

        private void ClassroomDetail_FormClosing(object sender, FormClosingEventArgs e)
        {

            foreach (CancellationTokenSource cts in tokenSourceList)
                cts.Cancel();
            tokenSourceList.RemoveRange(0, tokenSourceList.Count);
            tasks.RemoveRange(0, tasks.Count);
            bool result = false;
            Helper.dict.TryRemove(classroomView.PPCPublicIP, out result);
            
            //ComboBox cb = (ComboBox)Helper.GetAll((lb.Parent), typeof(ComboBox)).First();
            //cb.Items.Clear();
            //cb.Items.AddRange(Helper.GetComboItems(classroomView));
            //cb.SelectedIndex = 0;
            disposeWave();
        }
        private Task UpdatePictureBox(PictureBox p, Panel panel, string text, int i, Button sound = null)
        {
            CancellationTokenSource tokenSource = (CancellationTokenSource)p.Tag;
            Label l = new Label();
            l.Text = text;
            l.ForeColor = Color.White;
            l.BackColor = Color.Transparent;
            l.AutoSize = true;
            l.Location = new Point(0, 3);
            p.Controls.Add(l);
            if (i == 19)
            {
                p.Controls.Add(sound);
                sound.Location = new Point(l.Width, 1);
            }
            Label errorLabel = (Label)Helper.GetLast(panel, typeof(Label));
            return Task.Run(() =>
            {
                while (true)
                {
                    if (tokenSource.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    try
                    {
                        //ClassroomData data = new ClassroomData(classroomView.PPCPublicIP, classroomView.PPCPort);
                        string fileName = Helper.GetFileLocation(classroomView,i.ToString());
                        bool isSuccess = false;
                        bool isOutdated = false;
                        for (int j = 0; j < 5; j++)
                        {
                            try
                            {
                                isOutdated = false;
                                if (File.Exists(fileName))
                                {
                                    DateTime lastModified = File.GetLastWriteTime(fileName);
                                    if (lastModified.AddSeconds(60) < DateTime.Now)
                                    {
                                        isOutdated = true;
                                        throw new Exception("image outdated!");
                                    }
                                }
                                byte[] bytes = File.ReadAllBytes(fileName);
                                isSuccess = true;
                                using (MemoryStream loadStream = new MemoryStream(bytes, 0, bytes.Length))
                                {
                                    Action action = () =>
                                    {
                                        l.ForeColor = Color.White;
                                        errorLabel.Text = "";
                                        p.Image = Image.FromStream(loadStream);
                                    };
                                    p.SafeInvoke(action, true);
                                }
                                break;
                            }
                            catch (Exception)
                            {
                                Thread.Sleep(20);
                                if (isOutdated)
                                    throw;
                            }
                        }
                        if (!isSuccess)
                        {
                            throw new Exception("failed to get image!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Action action = () =>
                        {
                            l.ForeColor = Color.Red;
                            p.Image = null;
                            errorLabel.ForeColor = Color.Red;
                            errorLabel.Text = ex.Message;
                            errorLabel.BringToFront();
                        };
                        p.SafeInvoke(action, true);
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
            }, tokenSource.Token);
        }
    }
}
