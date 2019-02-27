using TransferControl.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Controller
{
    public interface IController
    {
        //void Connect();
        //void Close();
        void Start(object state);
        bool DoWork(Transaction Txn,bool WaitForData = false);
        string GetNextSeq();
        SANWA.Utility.Encoder GetEncoder();
        DeviceConfig GetConfig();
    }
}
