using SANWA.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Engine
{
    public interface IHostInterfaceReport
    {
        void On_TaskJob_Ack(TaskJobManagment.CurrentProceedTask Task);
        void On_TaskJob_Finished(TaskJobManagment.CurrentProceedTask Task);
        void On_TaskJob_Aborted(TaskJobManagment.CurrentProceedTask Task, string Location, string ReportType,string Message);
        void On_Event_Trigger(string Type,string Source,string Name,string Value);
        void On_Foup_Presence(string PortName,bool Presence);
    }
}
