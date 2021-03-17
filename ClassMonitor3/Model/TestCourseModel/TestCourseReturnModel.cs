using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model.TestCourseModel
{
    public class TestCourseReturnModel
    {
        public List<Quater> Qlist { get; set; }
        public List<FileServer> Flist { get; set; }
        public List<ClassType> Clist { get; set; }
        public Classroom Classroom { get; set; }
        public Quater Quater { get; set; }
        public TestCourseReturnModel(List<Quater> qlist, List<FileServer> flist, List<ClassType> clist, Classroom classroom, Quater quater)
        {
            Qlist = qlist;
            Flist = flist;
            Clist = clist;
            Classroom = classroom;
            Quater = quater;
        }
    }
}
