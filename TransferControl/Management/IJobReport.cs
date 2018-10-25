using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public interface IJobReport
    {
        void On_Job_Position_Changed(Job Job);
    }
}
