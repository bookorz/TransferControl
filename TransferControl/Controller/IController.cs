using TransferControl.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.CommandConvert;
using Newtonsoft.Json;

namespace TransferControl.Controller
{
    public interface IController
    {

        
        void SetReport(ICommandReport ReportTarget);
        string GetDeviceName();
        bool GetEnable();
        string GetControllerType();
        string GetConnectionType();
        string GetIPAdress();
        int GetPort();
        string GetVendor();
        void SetVendor(string Vendor);
        string GetPortName();
        int GetBaudRate();
        string GetStatus();
        void SetStatus(string Status);
        void Start(object state);
        void DoWork(Transaction Txn,bool WaitForData = false);
        string GetNextSeq();
        void Reconnect();
        CommandEncoder GetEncoder();
        CommandDecoder GetDecoder();

        string GetDeviceType();


    }
}
