using ClassMonitor3.Util;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace ClassMonitor3.Interfaces
{

    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
            comboBox1.Items.Add(new ComboItem("1","test1"));
            comboBox1.Items.Add(new ComboItem("2", "test2"));
            comboBox1.Items.Add(new ComboItem("3", "test3"));
            comboBox1.Items.Add(new ComboItem("4", "test4"));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string curFileLocation = @"\\140.192.42.17\Monitor\test.jpg";
            DateTime lastModified = File.GetLastWriteTime(curFileLocation);
            MessageBox.Show(lastModified.ToString());
        }
    }
}

