using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public interface ITaskFlowReport
    {
        void On_Task_Ack(TaskFlowManagement.CurrentProcessTask Task);
        void On_Task_Abort(TaskFlowManagement.CurrentProcessTask Task);
        void On_Task_Finished(TaskFlowManagement.CurrentProcessTask Task);
        void On_Alarm_Happen(AlarmManagement.Alarm Alarm);
    }
}
