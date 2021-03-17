using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassMonitor3.Model
{
    public class UserLoginLog
    {
        public int ID { get; set; }
        public string SessionID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime LoginTime { get; set; }
        public string LoginIP { get; set; }
    }
}
