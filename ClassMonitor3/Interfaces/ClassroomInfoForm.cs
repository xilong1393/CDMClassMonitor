using ClassMonitor3.Model;
using System.Windows.Forms;

namespace ClassMonitor3.Interfaces
{
    public partial class ClassroomInfoForm : Form
    {
        public ClassroomInfoForm(ClassroomInfoView classroomInfoView)
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = classroomInfoView;
        }
    }
}
