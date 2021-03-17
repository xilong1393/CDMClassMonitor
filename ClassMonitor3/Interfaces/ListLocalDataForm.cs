using ClassMonitor3.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;


namespace ClassMonitor3.Interfaces
{
    public partial class ListLocalDataForm : Form
    {
        private class LocalData
        {
            public string Value { get; set; }
            public bool Checked { get; set; }
            public string Datetime { get; set; }

            public override string ToString()
            {
                return Value;
            }
        }
        private int classroomID;
        private LocalData[] localData;
        public ListLocalDataForm(List<string> list, int crID)
        {
            InitializeComponent();
            int count = list.Count;
            localData = new LocalData[count];
            classroomID = crID;
            for (int i = 0; i < count; i++)
            {
                localData[i] = new LocalData();
                localData[i].Value = list[i];
                int start = list[i].LastIndexOf("at ");
                if (start == -1)
                    continue;
                localData[i].Datetime = list[i].Substring(start);
            }

            checkedListAddRange(localData);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (button3.Text == "Select All")
            {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, true);
                    localData[i].Checked = true;
                }
                button3.Text = "Clear All";                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
            }
            else
            {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                {
                    checkedListBox1.SetItemChecked(i, false);
                    localData[i].Checked = false;
                }
                button3.Text = "Select All";
            }
            
        }

        private void checkedListAddRange(LocalData[] ld)
        {
            foreach (LocalData l in ld)
                checkedListBox1.Items.Add(l, l.Checked);
        }

        private void refreshListAfterDelete(LocalData[] ld) {
            checkedListBox1.Items.Clear();
            foreach (LocalData l in ld)
            {
                if(!l.Checked)
                    checkedListBox1.Items.Add(l);
            }
        }

        private void SortSelect_Click(object sender, EventArgs e)
        {
            var selectedlist = localData.Where(o => o.Checked).OrderBy(o => o.Value.ToLower()).ToArray();
            var unselectedlist = localData.Where(o => !o.Checked).ToArray();

            localData = selectedlist.Concat(unselectedlist).ToArray();
            checkedListBox1.Items.Clear();
            checkedListAddRange(localData);
        }

        private void SortDate_Click(object sender, EventArgs e)
        {
            localData = localData.OrderBy(a => a.Datetime).Reverse().ToArray();
            checkedListBox1.Items.Clear();
            checkedListAddRange(localData);
        }

        private void SortName_Click(object sender, EventArgs e)
        {
            localData = localData.OrderBy(a => a.Value.ToLower()).ToArray();
            checkedListBox1.Items.Clear();
            checkedListAddRange(localData);
        }

        private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int index = e.Index;
            bool curChecked = !checkedListBox1.GetItemChecked(index);
            localData[index].Checked = curChecked;
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count == 0)
            {
                MessageBox.Show("No data is selected.");
                return;
            }
            DialogResult res = MessageBox.Show("Are you sure to upload selected data?", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (res == DialogResult.Cancel)
                return;
            ArrayList selectedItems = new ArrayList();
            int count = checkedListBox1.Items.Count;
            for (int i = 0; i < count; i++)
            {
                if (checkedListBox1.GetItemCheckState(i) == CheckState.Checked)
                {
                    string s = checkedListBox1.Items[i].ToString();
                    selectedItems.Add(s);
                }
            }

            var client = Helper.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID, dirnames = selectedItems };
            //HttpResponseMessage response = await client.GetAsync("api/Operation/UploadLocalData?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);
            HttpResponseMessage response = await client.PostAsJsonAsync("api/Operation/UploadLocalData", tuple);
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

        private async void button2_ClickAsync(object sender, EventArgs e)
        {
            ArrayList selectedItems = new ArrayList();
            int count = checkedListBox1.Items.Count;
            for (int i = 0; i < count; i++)
            {
                if (checkedListBox1.GetItemCheckState(i) == CheckState.Checked)
                {
                    string s = checkedListBox1.Items[i].ToString();
                    selectedItems.Add(s);
                }
            }

            var client = Helper.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID, dirnames = selectedItems };
            //HttpResponseMessage response = await client.GetAsync("api/Operation/UploadLocalData?classroomID=" + classroomID + "&sessionID=" + LoginInfo.sessionID);
            HttpResponseMessage response = await client.PostAsJsonAsync("api/Operation/DeleteLocalData", tuple);
            if (response.IsSuccessStatusCode)
            {
                //ExecutionResult result= await response.Content.ReadAsAsync<ExecutionResult>();
                MessageBox.Show("succeeded!");
                refreshListAfterDelete(localData);
            }
            else
            {
                //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                MessageBox.Show("failure!");
            }
        }
    }
}
