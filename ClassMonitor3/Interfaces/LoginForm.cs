using ClassMonitor3.Model;
using ClassMonitor3.Models;
using ClassMonitor3.Util;
using System;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClassMonitor3.Interfaces
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            int workerThreadsMin = 200, completionPortThreadsMin = 200;
            ThreadPool.SetMinThreads(workerThreadsMin, completionPortThreadsMin);
        }

        //this two methods are used to adjust the window size
        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height;
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
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private async void btnLogin_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                ((Button)sender).Enabled = false;
                UserLoginForm userLoginForm = new UserLoginForm();
                userLoginForm.UserName = txtName.Text;
                userLoginForm.Password = txtPwd.Text;
                string sessionID = Helper.GenerateSessionID();
                userLoginForm.SessionID = sessionID;
                userLoginForm.LoginIP = Helper.GetLocalIPAddress();

                User user = await GetUserAsync(userLoginForm);
                if (user != null)
                {
                    LoginInfo.sessionID = sessionID;
                    LoginInfo.user = user;
                    Hide();
                    var mainForm = new MainForm();
                    mainForm.ShowDialog();
                    Close();
                }
                else
                {
                    MessageBox.Show("Please provide correct username and password and try again!");
                    ((Button)sender).Enabled = true;
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                MessageBox.Show("service failure!");
            }
            finally
            {
                ((Button)sender).Enabled = true;
            }

        }
        static async Task<User> GetUserAsync(UserLoginForm userLoginForm)
        {
            //string path = "http://localhost:8080/api/user/GetUserLogin" + "?username=" + txtName.Text + "&password=" + txtPwd.Text;
            var client=Helper.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            User user = null;
            HttpResponseMessage response = await client.PostAsJsonAsync("api/user/GetUserLogin", userLoginForm);
            if (response.IsSuccessStatusCode)
            {
                user = await response.Content.ReadAsAsync<User>();
            }
            return user;
        }

        private void LoginForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            MessageBox.Show(e.KeyChar.ToString());
            if (e.KeyChar == 13)
            {
                MessageBox.Show("Enter key pressed");
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }
        //this two methods are used for the mouse to move the header of the panel
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Left += e.X - lastPoint.X;
                Top += e.Y - lastPoint.Y;
            }
        }
        Point lastPoint;
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            lastPoint = new Point(e.X, e.Y);
        }
    }
}
