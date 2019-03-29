using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Operation
{
    public interface IXfeStateReport
    {
        void On_Transfer_Complete(XfeControl xfe);
    }
}
