using ClassMonitor3.Util;
using System;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;

namespace ClassMonitor3.Interfaces
{
    public partial class GroupScheduleForm : Form
    {
        private int groupID = 0;
        public GroupScheduleForm(int groupID)
        {
            InitializeComponent();
            this.groupID = groupID;
            //dateTimePicker1.MinDate = DateTime.Today;
            dateTimePicker1.Value = DateTime.Today;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Text = "checking";
                button1.Enabled = false;
                var client = Helper.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(260);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync("api/Operation/GroupSchedule?groupID=" + groupID + "&sessionID= 123456" /*+ LoginInfo.sessionID*/+ "&dateTime=" + dateTimePicker1.Value);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        DataTable result = await response.Content.ReadAsAsync<DataTable>();
                        if (result != null)
                            dataGridView1.DataSource = result;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("failure!");
                    }
                }
                else
                {
                    MessageBox.Show("failure!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //LogHelper.GetLogger().Error(ex.ToString());
                MessageBox.Show("service failure!");
            }
            finally
            {
                button1.Text = "check";
                button1.Enabled = true;
            }
        }
    }
}
