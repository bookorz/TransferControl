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
        void On_TaskJob_Ack(string TaskID);
        void On_TaskJob_Finished(string TaskID);
        void On_TaskJob_Aborted(string TaskID, string Location, string ReportType,string Message);
        void On_Event_Trigger(string Type,string Source,string Name,string Value);
    }
}
