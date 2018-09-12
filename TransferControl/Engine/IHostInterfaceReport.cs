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
        
        void On_TaskJob_Finished(string TaskID);
        void On_TaskJob_Aborted(string TaskID, string Message);
    }
}
