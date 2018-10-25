using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public interface ITaskJobReport
    {
        void On_Task_NoExcuted(string Id);
        void On_Task_Abort(string Id);
    }
}
