using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.TaksFlow
{
    public interface ITaskFlow
    {
        bool Excute(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport);
    }
}
