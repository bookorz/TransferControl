using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Comm
{
    interface IConnection
    {
        void Send(object Message);
        void SendHexData(object Message);
        void Start();
        void WaitForData(bool Enable);
        void Reconnect();
    }
}
