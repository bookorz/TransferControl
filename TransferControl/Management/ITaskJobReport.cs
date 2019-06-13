using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public interface ITaskJobReport
    {
        void On_Task_Ack(TaskJobManagment.CurrentProceedTask Task);
        void On_Task_Abort(TaskJobManagment.CurrentProceedTask Task, string Location, string ReportType, string Message);
        void On_Task_Finished(TaskJobManagment.CurrentProceedTask Task);
    }
}
