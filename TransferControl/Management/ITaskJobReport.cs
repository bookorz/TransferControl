using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public interface ITaskJobReport
    {
        void On_Task_NoExcuted(TaskJobManagment.CurrentProceedTask Task);
        void On_Task_Abort(TaskJobManagment.CurrentProceedTask Task);
    }
}
