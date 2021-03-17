using ClassMonitor3.Model;
using ClassMonitor3.Model.TestCourseModel;
using ClassMonitor3.Util;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Windows.Forms;

namespace ClassMonitor3.Interfaces
{
    public partial class StartTestCourseForm : Form
    {
        ClassroomView classroomView;
        public StartTestCourseForm(ClassroomView classroomView)
        {
            InitializeComponent();
            this.classroomView = classroomView;
            this.Text = classroomView.ClassroomName + "";
            fillModel(classroomView.ClassroomID);
        }
        public async void fillModel(int classroomID) {
            var client = Helper.CreateClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var tuple = new { sessionID = LoginInfo.sessionID, classroomID = classroomID };
            HttpResponseMessage response = await client.PostAsJsonAsync("api/Operation/StartTestCourse",tuple);
            HttpResponseMessage responseClassroomGroup = await client.PostAsJsonAsync("api/Classroom/GetClassroomGroupbyClassroomID", tuple);
            if (response.IsSuccessStatusCode)
            {
                TestCourseReturnModel testCourseReturnModel = await response.Content.ReadAsAsync<TestCourseReturnModel>();
                testCourseReturnModel.Qlist.Insert(0, new Quater { QuarterID = 0, QuarterDesc = "--Select Quarter--" });
                comboBox1.DataSource = testCourseReturnModel.Qlist;
                comboBox1.DisplayMember = "QuarterDesc";
                comboBox1.ValueMember = "QuarterID";
                if (testCourseReturnModel.Quater != null)
                    comboBox1.SelectedValue = testCourseReturnModel.Quater.QuarterID;

                comboBox2.DataSource = testCourseReturnModel.Clist;
                comboBox2.DisplayMember = "ClassTypeName";
                comboBox2.ValueMember = "ClassTypeID";

                comboBox3.DataSource = testCourseReturnModel.Flist;
                comboBox3.DisplayMember = "FileServerName";
                comboBox3.ValueMember = "FileServerID";
                if (responseClassroomGroup.IsSuccessStatusCode)
                {
                    ClassroomGroup classroomGroup = await responseClassroomGroup.Content.ReadAsAsync<ClassroomGroup>();
                    comboBox3.SelectedValue = classroomGroup.FileServerID;
                }
                SetDefaultNameforTestCourse(classroomView);

                comboBox4.DataSource = Helper.TwentyFourArray();
                comboBox5.DataSource = Helper.SixtyArray();
                comboBox5.SelectedIndex = 3;
                comboBox6.DataSource = Helper.SixtyArray();

                Show();
            }
            else
            {
                MessageBox.Show("failure!");
            }
        }

        private void SetDefaultNameforTestCourse(ClassroomView classroom)
        {
            string sClassroomName = classroom.ClassroomName;

            if (classroom != null)
            {
                string sTime = " at " + DateTime.Now.ToString("yyyy-MM-dd HH mm ss");
                textBox1.Text = "Test Course in " + sClassroomName + sTime;
            }
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                if (comboBox1.SelectedIndex == 0 || comboBox5.SelectedIndex == 0/*||string.IsNullOrWhiteSpace(textBox2.Text)*/)
                {
                    MessageBox.Show("illegal argument!");
                    return;
                }
                ClassSchedule classSchedule = new ClassSchedule();
                classSchedule.ClassStartTime = DateTime.Now;

                classSchedule.ClassName = textBox1.Text.Replace((char)92, ' ');
                classSchedule.ClassroomID = classroomView.ClassroomID;
                classSchedule.ClassID = -1;
                classSchedule.ClassLength = comboBox4.SelectedIndex * 3600 + comboBox5.SelectedIndex * 60 + comboBox6.SelectedIndex * 1;
                classSchedule.ClassTypeID = Convert.ToInt32(comboBox2.SelectedValue);
                classSchedule.IsPeriodicCourse = false;
                classSchedule.FileServerID = Convert.ToInt32(comboBox3.SelectedValue);
                classSchedule.InstructorName = textBox2.Text;
                classSchedule.Status = 'E';
                classSchedule.QuarterID = Convert.ToInt32(comboBox1.SelectedValue);
                classSchedule.IsPostDelay = false;
                //classSchedule.Email = null;

                SubmitStartTestCourse submitStartTestCourse = new SubmitStartTestCourse();
                submitStartTestCourse.ClassSchedule = classSchedule;
                submitStartTestCourse.SessionID = LoginInfo.sessionID;

                var client = Helper.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.PostAsJsonAsync("api/Operation/SubmitStartTestCourse", submitStartTestCourse);
                if (response.IsSuccessStatusCode)
                {
                    //ExecutionResult result= await response.Content.ReadAsAsync<ExecutionResult>();
                    MessageBox.Show("succeeded!");
                    Close();
                }
                else
                {
                    //MessageBox.Show(response.Content.ReadAsStringAsync().Result);
                    MessageBox.Show("failure!");
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                MessageBox.Show("service failure!");
            }
            finally
            {
                button1.Enabled = true;
            }
        }

        private String FormatDataDir(int classTypeID, String DataDir)
        {
            if (String.IsNullOrEmpty(DataDir) == false)
            {
                DataDir = DataDir.Replace(' ', '-');
                DataDir = @"TestCourse/" + DataDir;
            }
            return DataDir;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetDefaultNameforTestCourse(classroomView);
        }
    }
}
