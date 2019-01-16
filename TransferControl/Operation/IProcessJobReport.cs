using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Operation
{
    public interface IProcessJobReport
    {
        void On_PJ_Complete(ProcessJob PJ);
    }
}
