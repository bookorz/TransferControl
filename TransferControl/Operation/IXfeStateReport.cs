using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Operation
{
    public interface IXfeStateReport
    {
        void On_Transfer_Complete(XfeCrossZone xfe);
        void On_LoadPort_Complete(Node Port);
        void On_UnLoadPort_Complete(Node Port);
    }
}
