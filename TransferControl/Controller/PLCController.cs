using InControls.PLC.Mitsubishi;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransferControl.CommandConvert;
using TransferControl.Management;

namespace TransferControl.Controller
{
    public class PLCController :   ITransactionReport, IController
    {
        
        [JsonIgnore]
        public CommandEncoder Encoder;
        ConcurrentDictionary<string, Transaction> TransactionList = new ConcurrentDictionary<string, Transaction>();
        private static readonly ILog logger = LogManager.GetLogger(typeof(PLCController));
        [JsonIgnore]
        public ICommandReport _ReportTarget;
        public string Name { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string ControllerType { get; set; }
        public string Vendor { get; set; }
        public string IPAdress { get; set; }
        public int Port { get; set; }
        public string ConnectionType { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public bool Enable { get; set; }
        [JsonIgnore]
        public string Status = "Disconnected";
        [JsonIgnore]
        public int TrxNo = 1;
        CommandDecoder _Decoder;
        private bool _IsConnected { get; set; }
        public void DoWork(Transaction Txn, bool WaitForData = false)
        {
            throw new NotImplementedException();
        }
        public string GetNextSeq()
        {
            string result = "";
            lock (this)
            {
                result = TrxNo.ToString("000");
                if (TrxNo >= 999)
                {
                    TrxNo = 1;
                }
                else
                {
                    TrxNo++;
                }
            }
            return result;
        }

        public CommandEncoder GetEncoder()
        {
            return Encoder;
        }
        public string GetControllerType()
        {
            return this.ControllerType;
        }
        public string GetConnectionType()
        {
            return this.ConnectionType;
        }
        public string GetDeviceName()
        {
            return this.Name;
        }
        public bool GetEnable()
        {
            return this.Enable;
        }
        public string GetIPAdress()
        {
            return this.IPAdress;
        }
        public int GetPort()
        {
            return this.Port;
        }
        public string GetVendor()
        {
            return this.Vendor;
        }
        public void SetVendor(string Vendor)
        {
            this.Vendor = Vendor;
        }
        public string GetPortName()
        {
            return this.PortName;
        }
        public int GetBaudRate()
        {
            return this.BaudRate;
        }
        public string GetStatus()
        {
            return this.Status;
        }
        public void SetStatus(string Status)
        {
            this.Status = Status;
        }

        public void On_Transaction_BypassTimeOut(Transaction Txn)
        {
            throw new NotImplementedException();
        }

        public void On_Transaction_TimeOut(Transaction Txn)
        {
            throw new NotImplementedException();
        }

        public void Reconnect()
        {
            throw new NotImplementedException();
        }

        public void SetReport(ICommandReport ReportTarget)
        {
            _ReportTarget = ReportTarget;


           
            _Decoder = new CommandConvert.CommandDecoder(Vendor);

            Encoder = new CommandEncoder(Vendor);


            this.Name = DeviceName;
            this.Status = "";
            
            ThreadPool.QueueUserWorkItem(new WaitCallback(Start), NodeManagement.Get("CSTROBOT"));
        }


        public void Start(object node)
        {
            int AddrOffset = 1280;
            int CstRobotStation = 9;
            Node Target = (Node)node;
            //SpinWait.SpinUntil(() => Target.GetController().GetStatus().Equals("Connected"), 9999999);
            //McProtocolTcp PLC = new McProtocolTcp("192.168.3.39", 2000);
            McProtocolTcp PLC = new McProtocolTcp(this.IPAdress, this.Port);
            PLC.Open();
            this._IsConnected = true;
            byte[] result = new byte[512];
            int[] WResult = new int[32];

            //INIT
            result = new byte[512];
            PLC.GetBitDevice(PlcDeviceType.Y, AddrOffset, 512, result);

            Target.SetIO("OUTPUT", result);
            result = new byte[512];
            Target.SetIO("OUTPUT_OLD", result);
            result = new byte[512];
            PLC.GetBitDevice(PlcDeviceType.X, AddrOffset, 512, result);
            Target.SetIO("INPUT", result);
            result = new byte[512];
            Target.SetIO("INPUT_OLD", result);
            WResult = new int[32];
            PLC.ReadDeviceBlock(PlcDeviceType.D, 24576, 32, WResult);
            result = ConvertToBit(WResult);
            Target.SetIO("PRESENCE", result);
            result = new byte[512];
            Target.SetIO("PRESENCE_OLD", result);
            WResult = new int[2];
            PLC.ReadDeviceBlock(PlcDeviceType.D, 25856, 2, WResult);
            int RecieveIndex_1 = WResult[0];
            int RecieveIndex_2 = WResult[1];


            //PLC.SetBitDevice(PlcDeviceType.Y, new Dictionary<int, byte>() { { 1280,1}, { 1281, 1 }, { 1285, 1 } });

            while (true)
            {
                try
                {
                    //SpinWait.SpinUntil(() => false, 10);
                    WResult = new int[90];
                    PLC.ReadDeviceBlock(PlcDeviceType.D, 25856, 2, WResult);
                    if (RecieveIndex_1 != WResult[0])
                    {
                        RecieveIndex_1 = WResult[0];
                        int[] WResult1 = new int[90];
                        PLC.ReadDeviceBlock(PlcDeviceType.D, 25360, 90, WResult1);
                        string rData1 = "";
                        foreach(int dec in WResult1)
                        {
                            rData1 += dec.ToString("X4");
                        }
                        rData1 = Encoding.ASCII.GetString(StringToByteArray(rData1));
                    }
                    if (RecieveIndex_2 != WResult[1])
                    {
                        RecieveIndex_2 = WResult[1];
                        int[] WResult1 = new int[90];
                        PLC.ReadDeviceBlock(PlcDeviceType.D, 25616, 90, WResult1);
                        string rData2 = "";
                        foreach (int dec in WResult1)
                        {
                            rData2 += dec.ToString("X4");
                        }
                        rData2 = Encoding.ASCII.GetString(StringToByteArray(rData2));

                    }
                    if (!Target.GetIO("OUTPUT").SequenceEqual(Target.GetIO("OUTPUT_OLD")))
                    {
                        Dictionary<int, byte> changedList = new Dictionary<int, byte>();
                        for (int i = 0; i < Target.GetIO("OUTPUT").Length; i++)
                        {
                            if (Target.GetIO("OUTPUT")[i] != Target.GetIO("OUTPUT_OLD")[i])
                            {
                                changedList.Add(i + AddrOffset, Target.GetIO("OUTPUT")[i]);
                                //_TaskReport.On_Message_Log("IO", "Y Area [" + (i + AddrOffset).ToString("X4") + "] " + Target.GetIO("OUTPUT_OLD")[i] + "->" + Target.GetIO("OUTPUT")[i]);
                                Target.SetIO("OUTPUT_OLD", i, Target.GetIO("OUTPUT")[i]);
                                //UpdateUI("OUTPUT", i, Target.GetIO("OUTPUT")[i], Target);
                                _ReportTarget.On_DIO_Data_Chnaged(i.ToString(), Target.GetIO("OUTPUT")[i].ToString(), "OUTPUT");
                            }
                        }
                        PLC.SetBitDevice(PlcDeviceType.Y, changedList);


                    }

                    PLC.GetBitDevice(PlcDeviceType.X, AddrOffset, 512, result);
                    Target.SetIO("INPUT", result);
                    if (!Target.GetIO("INPUT").SequenceEqual(Target.GetIO("INPUT_OLD")))
                    {
                        for (int i = 0; i < Target.GetIO("INPUT").Length; i++)
                        {

                            if (Target.GetIO("INPUT")[i] != Target.GetIO("INPUT_OLD")[i])
                            {
                                //_TaskReport.On_Message_Log("IO", "X Area [" + (i + AddrOffset).ToString("X4") + "] " + Target.GetIO("INPUT_OLD")[i] + "->" + Target.GetIO("INPUT")[i]);
                                //UpdateUI("INPUT", i, Target.GetIO("INPUT")[i], Target);
                                _ReportTarget.On_DIO_Data_Chnaged(i.ToString(), Target.GetIO("INPUT")[i].ToString(), "INPUT");
                            }
                        }
                        if (Target.GetIO("INPUT")[6 + (CstRobotStation - 1) * 32] == 1 && Target.GetIO("INPUT_OLD")[6 + (CstRobotStation - 1) * 32] == 0)
                        {
                            string errAry = "";
                            for (int i = 32; i <= 64; i++)
                            {
                                errAry += Target.GetIO("INPUT")[i + (CstRobotStation - 1) * 32].ToString();
                            }

                            string error = new String(errAry.Reverse().ToArray());
                            error = Convert.ToInt32(error, 2).ToString("X");
                            _ReportTarget.On_Alarm_Happen(AlarmManagement.NewAlarm(Target, error, ""));
                        }
                        Target.SetIO("INPUT_OLD", Target.GetIO("INPUT"));

                    }

                    WResult = new int[32];
                    PLC.ReadDeviceBlock(PlcDeviceType.D, 24576, 32, WResult);
                    result = ConvertToBit(WResult);
                    Target.SetIO("PRESENCE", result);
                    if (!Target.GetIO("PRESENCE").SequenceEqual(Target.GetIO("PRESENCE_OLD")))
                    {
                        for (int i = 0; i < Target.GetIO("PRESENCE").Length; i++)
                        {

                            if (Target.GetIO("PRESENCE")[i] != Target.GetIO("PRESENCE_OLD")[i])
                            {
                                //_TaskReport.On_Message_Log("IO", "X Area [" + (i + AddrOffset).ToString("X4") + "] " + Target.GetIO("INPUT_OLD")[i] + "->" + Target.GetIO("INPUT")[i]);
                                //UpdateUI("PRESENCE", i, Target.GetIO("PRESENCE")[i], Target);
                                _ReportTarget.On_DIO_Data_Chnaged(i.ToString(), Target.GetIO("PRESENCE")[i].ToString(), "PRESENCE");
                            }
                        }

                        Target.SetIO("PRESENCE_OLD", Target.GetIO("PRESENCE"));

                    }
                }
                catch (Exception e)
                {
                    _ReportTarget.On_Message_Log("IO", "Lost connection with PLC");
                    SpinWait.SpinUntil(() => false, 5000);
                }
            }
        }
        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
        private byte[] ConvertToBit(int[] WResult)
        {
            string BitStr = "";
            byte[] result = new byte[512];
            for (int i = 0; i < WResult.Length; i++)
            {
                BitStr += new String(Convert.ToString(Convert.ToInt16(WResult[i]), 2).PadLeft(16, '0').Reverse().ToArray());
            }

            for (int i = 0; i < BitStr.Length; i++)
            {
                result[i] = Convert.ToByte(BitStr[i].ToString());
            }
            return result;
        }
    }
}
