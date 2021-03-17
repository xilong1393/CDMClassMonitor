using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model.TestCourseModel
{
    public class ClassSchedule
    {
        public int ClassScheduleID { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public bool IsPeriodicCourse { get; set; }
        public DateTime ClassStartTime { get; set; }
        public int ClassLength { get; set; }
        public DateTime? ClassStartDate { get; set; }
        public DateTime? ClassEndDate { get; set; }
        public int FileServerID { get; set; }
        public string ClassDataDir { get; set; }
        public char Status { get; set; }
        public int ClassTypeID { get; set; }
        public int? WeekDayID { get; set; }
        public int ClassroomID { get; set; }
        public string InstructorName { get; set; }
        public int QuarterID { get; set; }
        public bool IsPostDelay { get; set; }
        public bool IsPodcast { get; set; }
        public string Email { get; set; }
        public class DDatePeriod
        {
            public DateTime StartDate { get; set; }

            public DateTime EndDate { get; set; }
        }
    }
}
