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
using TransferControl.Config;
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
     
        private string ch1Send = "";
        private string ch2Send = "";
        [JsonIgnore]
        public string Status = "Disconnected";
        [JsonIgnore]
        public int TrxNo = 1;
        CommandDecoder _Decoder;
        private bool _IsConnected { get; set; }
        public void DoWork(Transaction orgTxn, bool WaitForData = false)
        {
            Transaction Txn = null;


            Txn = orgTxn;


            if (Txn.CommandEncodeStr.Equals("GetMappingDummy"))
            {
                string mappingData = "";
                if (Txn.NodeName.Equals("LOADPORT01"))
                {
                    //mappingData = "1111111111111111111111111";
                    mappingData = SystemConfig.Get().FakeDataP1;
                }
                else if (Txn.NodeName.Equals("LOADPORT02"))
                {
                    mappingData = SystemConfig.Get().FakeDataP2;
                }
                else if (Txn.NodeName.Equals("LOADPORT03"))
                {
                    mappingData = SystemConfig.Get().FakeDataP3;
                }
                else if (Txn.NodeName.Equals("LOADPORT04"))
                {
                    mappingData = SystemConfig.Get().FakeDataP4;
                }

                CommandReturnMessage cm = new CommandReturnMessage();
                //cm.CommandType = Transaction.Command.LoadPortType.GetMapping;
                cm.Value = mappingData;
                //Txn.Method= Transaction.Command.LoadPortType.GetMapping;
                _ReportTarget.On_Command_Excuted(NodeManagement.Get(Txn.NodeName), Txn, cm);

            }
            //conn.WaitForData(WaitForData);

            // lock (TransactionList)
            List<CommandReturnMessage> msgList = _Decoder.GetMessage(Txn.CommandEncodeStr);


            if (!Txn.NodeType.Equals("OCR"))
            {

                if (msgList.Count != 0)
                {
                    Txn.Type = msgList[0].Command;
                    //Txn.CommandType = msgList[0].CommandType;
                }
            }
            else
            {
                Txn.Type = "";
                //Txn.CommandType = "";
            }
            string key = "";
            if (Vendor.ToUpper().Equals("KAWASAKI"))
            {

                key = Txn.Seq;
            }
            if (DeviceType.ToUpper().Equals("SMARTTAG"))
            {
                key = "00";
            }
            else if (Vendor.ToUpper().Equals("HST") || Vendor.ToUpper().Equals("COGNEX"))
            {
                key = "1" + Txn.Type;
            }

            else if (Vendor.ToUpper().Equals("SANWA") || Vendor.ToUpper().Equals("ATEL_NEW"))
            {
                key = Txn.AdrNo + Txn.Type;
                //支援同時多發命令
                for (int seq = 0; seq <= 99; seq++)
                {
                    string tmpKey = key + seq.ToString("00");

                    if (!TransactionList.ContainsKey(tmpKey))
                    {
                        key = tmpKey;
                        break;
                    }
                    if (seq == 99)
                    {
                        logger.Error("seq is run out!");
                    }
                }
            }
            else
            {
                if (Vendor.ToUpper().Equals("SANWA_MC"))
                {

                    if (orgTxn.CommandEncodeStr.Contains("MCR:"))
                    {
                        Txn.CommandType = "CMD";
                    }
                    if (Txn.Method == Transaction.Command.LoadPortType.Reset)
                    {
                        key = "0";
                    }
                    else
                    {
                        key = Txn.AdrNo;
                    }
                    //}
                    //else
                    //{
                    //    key = "0" + msgList[0].Command;
                    //}

                }
                else
                {
                    key = Txn.AdrNo + Txn.Type;
                }
            }

            if (TransactionList.TryAdd(key, Txn) || Txn.Method.Equals("Stop"))
            {




                Txn.SetTimeOutReport(this);
                Txn.SetTimeOutMonitor(true);




                //if (Vendor.ToUpper().Equals("ACDT"))
                //{
                //    byte[] byteAry = Encoding.UTF8.GetBytes(Txn.CommandEncodeStr);


                //    logger.Debug(DeviceName + " Send:" + BitConverter.ToString(byteAry) + " Wafer:" + waferids);
                //}
                //else
                //{

                if (Txn.CommandType.Equals(""))
                {
                    Txn.CommandType = _Decoder.GetMessage(Txn.CommandEncodeStr)[0].CommandType;
                }
                //if (Txn.CommandType.Equals("GET") || Txn.CommandType.IndexOf("FS") != -1)
                //{

                //}
                if (Txn.Method.Equals(Transaction.Command.LoadPortType.Reset))
                {
                    Txn.SetTimeOut(15000);
                }
                else
                {
                    Txn.SetTimeOut(Txn.AckTimeOut);
                }
                try
                {
                    //logger.Info(DeviceName + " Send:" + Txn.CommandEncodeStr.Replace("\r", ""));
                    //if (!this.Vendor.ToUpper().Equals("MITSUBISHI_PLC"))
                    //{
                    //    _ReportTarget.On_Message_Log("CMD", DeviceName + " Send:" + Txn.CommandEncodeStr.Replace("\r", ""));
                    //}
                    //if (this.Vendor.Equals("SMARTTAG8200") || this.Vendor.Equals("SMARTTAG8400"))
                    //{
                    //    //if (Txn.Method == Transaction.Command.SmartTagType.GetLCDData)
                    //    //{
                    //    //    conn.WaitForData(true);
                    //    //}
                    //    ThreadPool.QueueUserWorkItem(new WaitCallback(conn.SendHexData), Txn.CommandEncodeStr);

                    //}
                    //else
                    //{

                    //    ThreadPool.QueueUserWorkItem(new WaitCallback(conn.Send), Txn.CommandEncodeStr);
                    //}
                    if (Txn.NodeName.ToUpper().Equals("SMIF1"))
                    {
                        ch1Send = Txn.CommandEncodeStr;
                    }
                    else if (Txn.NodeName.ToUpper().Equals("SMIF2"))
                    {
                        ch2Send = Txn.CommandEncodeStr;
                    }


                }
                catch (Exception eex)
                {
                    logger.Error(eex.StackTrace);
                    _ReportTarget.On_Message_Log("CMD", DeviceName + " Err:" + eex.StackTrace);
                    Txn.SetTimeOutMonitor(false);
                    Transaction tmp;
                    TransactionList.TryRemove(key, out tmp);
                    CommandReturnMessage rm = new CommandReturnMessage();
                    rm.Value = "ConnectionError";
                    _ReportTarget.On_Command_Error(NodeManagement.Get(Txn.NodeName), Txn, rm);
                }
            }
            else
            {
                Transaction workingTxn;
                TransactionList.TryRemove(key, out workingTxn);
                logger.Debug(DeviceName + "(DoWork " + IPAdress + ":" + Port.ToString() + ":" + Txn.CommandEncodeStr + ") Same type command " + workingTxn.CommandEncodeStr + " is already excuting.");
                _ReportTarget.On_Message_Log("CMD", DeviceName + "(DoWork " + IPAdress + ":" + Port.ToString() + ":" + Txn.CommandEncodeStr + ") Same type command " + workingTxn.CommandEncodeStr + " is already excuting.");
                Txn.SetTimeOutMonitor(false);
                CommandReturnMessage rm = new CommandReturnMessage();
                rm.Value = "AlreadyExcuting";
                _ReportTarget.On_Command_Error(NodeManagement.Get(Txn.NodeName), Txn, rm);
            }



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
            logger.Debug(DeviceName + "(On_Transaction_TimeOut Txn is timeout:" + Txn.Method);
            _ReportTarget.On_Message_Log("CMD", DeviceName + "(On_Transaction_TimeOut Txn is timeout:" + Txn.Method);
            string key = "";
            if (Vendor.ToUpper().Equals("KAWASAKI"))
            {
                key = Txn.Seq;

            }
            else if (Vendor.ToUpper().Equals("HST") || Vendor.ToUpper().Equals("COGNEX"))
            {
                key = "1";
            }

            else if (Vendor.ToUpper().Equals("SANWA") || Vendor.ToUpper().Equals("ATEL_NEW"))
            {
                key = Txn.AdrNo + Txn.Method;
                for (int seq = 0; seq <= 99; seq++)
                {
                    string tmpKey = key + seq.ToString("00");

                    if (TransactionList.ContainsKey(tmpKey))
                    {
                        key = tmpKey;
                        break;
                    }
                    if (seq == 99)
                    {
                        logger.Error("seq is run out!");
                    }
                }
            }
            else
            {
                key = Txn.AdrNo;

            }
            Txn.SetTimeOutMonitor(false);

            if (TransactionList.ContainsKey(key))
            {
                if (TransactionList.TryRemove(key, out Txn))
                {
                    //Node Node = NodeManagement.GetByController(DeviceName, Txn.AdrNo);
                    Node Node = NodeManagement.Get(Txn.NodeName);
                    Node.IsExcuting = false;
                    if (Node.State.Equals("Pause"))
                    {
                        logger.Debug("Txn timeout,but state is pause. ignore this.");
                        TransactionList.TryAdd(key, Txn);
                        return;
                    }
                    //if (Node != null)
                    //{
                    //    _ReportTarget.On_Command_TimeOut(Node, Txn);
                    //}
                    //else
                    //{
                    //    logger.Debug(DeviceName + "(On_Transaction_TimeOut Get Node fail.");
                    //}
                }
                else
                {
                    logger.Debug(DeviceName + "(On_Transaction_TimeOut TryRemove Txn fail.");
                }
            }
            _ReportTarget.On_Command_TimeOut(NodeManagement.Get(Txn.NodeName), Txn);
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
                    if (!ch1Send.Equals(""))
                    {
                        int[] SendDataBytes = ByteArrayToIntArray(Encoding.ASCII.GetBytes(ch1Send));

                        PLC.WriteDeviceBlock(PlcDeviceType.D, 24848, SendDataBytes.Length, SendDataBytes);
                        PLC.WriteDeviceBlock(PlcDeviceType.D, 24834, 1, new int[] { SendDataBytes.Length });
                        PLC.SetBitDevice(PlcDeviceType.Y, 1776, 1, new byte[] { 1 });
                        ch1Send = "";
                    }
                    if (!ch2Send.Equals(""))
                    {
                        int[] SendDataBytes = ByteArrayToIntArray(Encoding.ASCII.GetBytes(ch2Send));

                        PLC.WriteDeviceBlock(PlcDeviceType.D, 25104, SendDataBytes.Length, SendDataBytes);
                        PLC.WriteDeviceBlock(PlcDeviceType.D, 25090, 1, new int[] { SendDataBytes.Length });
                        PLC.SetBitDevice(PlcDeviceType.Y, 1778, 1, new byte[] { 1 });
                        ch2Send = "";
                    }
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
                            rData1 += dec.ToString("X4").Substring(2, 2) + dec.ToString("X4").Substring(0,2);
                        }
                        rData1 = Encoding.ASCII.GetString(StringToByteArray(rData1)).Trim('\0');
                        //On_Connection_Message(rData1);

                        ThreadPool.QueueUserWorkItem(new WaitCallback(On_Connection_Message), rData1);

                    }
                    if (RecieveIndex_2 != WResult[1])
                    {
                        RecieveIndex_2 = WResult[1];
                        int[] WResult1 = new int[90];
                        PLC.ReadDeviceBlock(PlcDeviceType.D, 25616, 90, WResult1);
                        string rData2 = "";
                        foreach (int dec in WResult1)
                        {
                            rData2 += dec.ToString("X4").Substring(2, 2) + dec.ToString("X4").Substring(0, 2);
                        }

                        rData2 = Encoding.ASCII.GetString(StringToByteArray(rData2)).Trim('\0');
                        //On_Connection_Message(rData2);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(On_Connection_Message), rData2);


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
                    try
                    {
                        PLC.Open();
                    }catch(Exception eeee)
                    {
                        _ReportTarget.On_Message_Log("IO", eeee.StackTrace);
                    }
                  
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
        public static int[] ByteArrayToIntArray(byte[] bytes)
        {
            if (bytes.Length % 2 != 0)
            {
                bytes = bytes.Concat(new byte[] { 0 }).ToArray();
            }
            return Enumerable.Range(0, bytes.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToInt32(BitConverter.ToInt16(bytes,x)))
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
        public void On_Connection_Message(object MsgObj)
        {
            try
            {
                string Msg = (string)MsgObj;
                //if (Vendor.ToUpper().Equals("ACDT"))
                //{
                //    byte[] byteAry = Encoding.ASCII.GetBytes(Msg);


                //    logger.Debug(DeviceName + " Recieve:" + BitConverter.ToString(byteAry));
                //}
                //else
                //{

                //}
                //Node Target = null;

                List<CommandReturnMessage> ReturnMsgList = _Decoder.GetMessage(Msg);
                foreach (CommandReturnMessage ReturnMsg in ReturnMsgList)
                {
                    //logger.Info(DeviceName + " Recieve:" + ReturnMsg.OrgMsg);
                    if (!this.Vendor.ToUpper().Equals("MITSUBISHI_PLC"))
                    {
                        _ReportTarget.On_Message_Log("CMD", DeviceName + " Recieve:" + ReturnMsg.OrgMsg);
                    }
                    try
                    {
                        Transaction Txn = null;
                        Node Node = null;
                        if (ReturnMsg != null)
                        {
                            lock (TransactionList)
                            {
                                if (ReturnMsg.Command != null)
                                {
                                    if (ReturnMsg.Command.Equals("PAUSE"))
                                    {
                                        foreach (Transaction t in TransactionList.Values)
                                        {
                                            t.SetTimeOutMonitor(false);
                                        }
                                    }
                                    if (ReturnMsg.Command.Equals("CONT_"))
                                    {
                                        foreach (Transaction t in TransactionList.Values)
                                        {
                                            t.SetTimeOutMonitor(true);
                                        }
                                    }
                                }


                                string key = "";
                                if (Vendor.ToUpper().Equals("KAWASAKI"))
                                {
                                    key = ReturnMsg.Seq;

                                }
                                else if (Vendor.ToUpper().Equals("HST") || Vendor.ToUpper().Equals("COGNEX"))
                                {
                                    key = "1" + ReturnMsg.Command;
                                }

                                else if (Vendor.ToUpper().Equals("SANWA") || Vendor.ToUpper().Equals("ATEL_NEW"))
                                {
                                    key = ReturnMsg.NodeAdr + ReturnMsg.Command;
                                    for (int seq = 0; seq <= 99; seq++)
                                    {
                                        string tmpKey = key + seq.ToString("00");

                                        if (TransactionList.ContainsKey(tmpKey))
                                        {
                                            key = tmpKey;
                                            break;
                                        }
                                        if (seq == 99)
                                        {
                                            logger.Error("seq is run out!");
                                        }
                                    }
                                }
                                else if (Vendor.ToUpper().Equals("SANWA_MC"))
                                {
                                    //if (ReturnMsg.Command.Equals("MCR__"))
                                    //{
                                    if (ReturnMsg.Command.Equals("RESET"))
                                    {
                                        key = "0";
                                    }
                                    else
                                    {
                                        key = ReturnMsg.NodeAdr;
                                    }
                                    //}
                                    //else
                                    //{
                                    //    key = "0" + ReturnMsg.Command;
                                    //}
                                }
                                else if (DeviceType.Equals("SMARTTAG"))
                                {
                                    key = "00";
                                }
                                else
                                {
                                    key = (ReturnMsg.NodeAdr + ReturnMsg.Command).Equals("") ? "0" : ReturnMsg.NodeAdr + ReturnMsg.Command;
                                }

                                if (Vendor.ToUpper().Equals("KAWASAKI"))
                                {
                                    if (TransactionList.TryGetValue(key, out Txn))
                                    {
                                        Node = NodeManagement.Get(Txn.NodeName);

                                        if (!Txn.CommandType.Equals("CMD"))
                                        {
                                            if (ReturnMsg.Type.Equals(CommandReturnMessage.ReturnType.Excuted))
                                            {
                                                continue;
                                            }
                                            else if (ReturnMsg.Type.Equals(CommandReturnMessage.ReturnType.Finished))
                                            {
                                                ReturnMsg.Type = CommandReturnMessage.ReturnType.Excuted;
                                            }
                                        }

                                    }
                                    else
                                    {
                                        logger.Debug("Transaction not exist:key=" + key);
                                        return;
                                    }
                                }
                                else if (Vendor.ToUpper().Equals("TDK"))
                                {
                                    if (TransactionList.TryGetValue(key, out Txn))
                                    {
                                        Node = NodeManagement.Get(Txn.NodeName);
                                        if (Txn.CommandType.Equals("SET") && ReturnMsg.Type.Equals(CommandReturnMessage.ReturnType.Excuted))
                                        {
                                            //continue;
                                        }
                                    }
                                    else
                                    {
                                        Node = NodeManagement.GetByController(DeviceName, ReturnMsg.NodeAdr);
                                    }
                                }
                                else
                                {
                                    //if (ReturnMsg.NodeAdr.Equals("") || ReturnMsg.Command.Equals("RESET") || ReturnMsg.Command.Equals("SP___") || ReturnMsg.Command.Equals("PAUSE") || ReturnMsg.Command.Equals("CONT_") || ReturnMsg.Command.Equals("STOP_") || ReturnMsg.Command.Equals("TGEVT"))
                                    //{
                                    //    Node = NodeManagement.GetFirstByController(DeviceName);
                                    //}
                                    //else
                                    //{
                                    //    Node = NodeManagement.GetByController(DeviceName, ReturnMsg.NodeAdr);
                                    //}
                                    Node = NodeManagement.GetByController(DeviceName, ReturnMsg.NodeAdr.Equals("") ? "0" : ReturnMsg.NodeAdr);
                                    if (Node == null)
                                    {
                                        Node = NodeManagement.GetOCRByController(DeviceName);
                                    }
                                    if (Node == null)
                                    {
                                        Node = NodeManagement.GetFirstByController(DeviceName);
                                    }
                                }
                                //lock (TransactionList)
                                //{
                                //lock (Node)
                                //{
                                //Target = Node;
                                if (Node.Vendor.ToUpper().Equals("COGNEX"))
                                {
                                    if (ReturnMsg.Type == CommandReturnMessage.ReturnType.UserName)
                                    {
                                        //conn.Send("admin\r\n");
                                        continue;
                                    }
                                    if (ReturnMsg.Type == CommandReturnMessage.ReturnType.Password)
                                    {
                                        //conn.Send("\r\n");
                                        continue;
                                    }
                                }
                                if (ReturnMsg.Type == CommandReturnMessage.ReturnType.Event)
                                {

                                    //_ReportTarget.On_Event_Trigger(Node, ReturnMsg);
                                }
                                else if ((ReturnMsg.Type == CommandReturnMessage.ReturnType.Information && Node.Vendor.ToUpper().Equals("TDK") && !TransactionList.ContainsKey(key)))
                                {
                                    if (ReturnMsg.Type.Equals(CommandReturnMessage.ReturnType.Information))
                                    {
                                        //ThreadPool.QueueUserWorkItem(new WaitCallback(conn.Send), ReturnMsg.FinCommand);
                                        //conn.Send(ReturnMsg.FinCommand);
                                        //isWaiting = false;
                                        logger.Debug(DeviceName + " Send:" + ReturnMsg.FinCommand);
                                    }
                                }
                                else if (TransactionList.TryRemove(key, out Txn))
                                {
                                    // Node.InitialComplete = false;
                                    Node = NodeManagement.Get(Txn.NodeName);
                                    switch (ReturnMsg.Type)
                                    {
                                        case CommandReturnMessage.ReturnType.Excuted:
                                            if (!Txn.CommandType.Equals("CMD") && !Txn.CommandType.Equals("MOV") && !Txn.CommandType.Equals("HCS"))
                                            {
                                                logger.Debug("Txn timmer stoped.");
                                                Txn.SetTimeOutMonitor(false);
                                                lock (Node.ExcuteLock)
                                                {
                                                    Node.IsExcuting = false;
                                                }

                                            }
                                            else
                                            {
                                                //if (Txn.Method.Equals(Transaction.Command.LoadPortType.Reset))
                                                //{
                                                //    logger.Debug("Txn timmer stoped.");
                                                //    Txn.SetTimeOutMonitor(false);

                                                //}
                                                //else if (Txn.Method.Equals(Transaction.Command.RobotType.OrginSearch))
                                                //{
                                                //    logger.Debug("Txn timmer stoped.");
                                                //    Txn.SetTimeOutMonitor(false);
                                                //    //Node.IsExcuting = false;
                                                //    TransactionList.TryAdd(key, Txn);
                                                //}
                                                //else
                                                //{
                                                Txn.SetTimeOutMonitor(false);
                                                Txn.SetTimeOut(Txn.MotionTimeOut);
                                                Txn.SetTimeOutMonitor(true);
                                                TransactionList.TryAdd(key, Txn);

                                                //}
                                            }
                                            //_ReportTarget.On_Command_Excuted(Node, Txn, ReturnMsg);
                                            break;
                                        case CommandReturnMessage.ReturnType.Finished:
                                            logger.Debug("Txn timmer stoped.");
                                            Txn.SetTimeOutMonitor(false);
                                            lock (Node.ExcuteLock)
                                            {
                                                Node.IsExcuting = false;
                                            }
                                            //_ReportTarget.On_Command_Finished(Node, Txn, ReturnMsg);
                                            break;
                                        case CommandReturnMessage.ReturnType.Error:
                                            logger.Debug("Txn timmer stoped.");
                                            Txn.SetTimeOutMonitor(false);
                                            lock (Node.ExcuteLock)
                                            {
                                                Node.IsExcuting = false;
                                            }
                                            //_ReportTarget.On_Command_Error(Node, Txn, ReturnMsg);
                                            if (Vendor.ToUpper().Equals("TDK") || DeviceType.ToUpper().Equals("SMARTTAG"))
                                            {
                                                //conn.Send(ReturnMsg.FinCommand);
                                                logger.Debug(DeviceName + " Send:" + ReturnMsg.FinCommand);
                                            }
                                            break;
                                        case CommandReturnMessage.ReturnType.Information:
                                            logger.Debug("Txn timmer stoped.");
                                            Txn.SetTimeOutMonitor(false);
                                            lock (Node.ExcuteLock)
                                            {
                                                Node.IsExcuting = false;
                                            }
                                            if (Vendor.ToUpper().Equals("TDK") && Txn.CommandType.Equals("SET"))
                                            {
                                                ReturnMsg.Type = CommandReturnMessage.ReturnType.Excuted;

                                            }
                                            else
                                            {
                                                ReturnMsg.Type = CommandReturnMessage.ReturnType.Finished;
                                                //Node.IsExcuting = false;
                                            }
                                            SpinWait.SpinUntil(() => false, 50);
                                            //ThreadPool.QueueUserWorkItem(new WaitCallback(conn.Send), ReturnMsg.FinCommand);
                                            //conn.Send(ReturnMsg.FinCommand);

                                            logger.Debug(DeviceName + " Send:" + ReturnMsg.FinCommand);

                                            break;
                                    }
                                }
                                else
                                {
                                    if (ReturnMsg.Type != null)
                                    {
                                        if (ReturnMsg.Type.Equals(CommandReturnMessage.ReturnType.Information))
                                        {
                                            //ThreadPool.QueueUserWorkItem(new WaitCallback(conn.Send), ReturnMsg.FinCommand);
                                            //conn.Send(ReturnMsg.FinCommand);

                                            logger.Debug(DeviceName + " Send:" + ReturnMsg.FinCommand);
                                        }
                                        else
                                        {
                                            if (ReturnMsg.Type == CommandReturnMessage.ReturnType.Error)
                                            {
                                                Txn = TransactionList.First().Value;

                                                logger.Debug("Txn timmer stoped.");
                                                Txn.SetTimeOutMonitor(false);
                                                lock (Node.ExcuteLock)
                                                {
                                                    Node.IsExcuting = false;
                                                }
                                                //_ReportTarget.On_Command_Error(Node, Txn, ReturnMsg);
                                                if (Vendor.ToUpper().Equals("TDK") || DeviceType.ToUpper().Equals("SMARTTAG"))
                                                {
                                                    //conn.Send(ReturnMsg.FinCommand);
                                                    logger.Debug(DeviceName + " Send:" + ReturnMsg.FinCommand);
                                                }

                                                TransactionList.TryRemove(TransactionList.First().Key, out Txn);
                                            }
                                            else
                                            {

                                                logger.Debug(DeviceName + "(On_Connection_Message Txn is not found. msg:" + Msg);
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        logger.Debug(DeviceName + "(On_Connection_Message Return type is null. msg:" + Msg);
                                        continue;
                                    }
                                }
                                //}
                            }
                            switch (ReturnMsg.Type)
                            {
                                case CommandReturnMessage.ReturnType.Information:
                                case CommandReturnMessage.ReturnType.Event:
                                    Transaction t = new Transaction();
                                    t.NodeName = Node.Name;
                                    t.NodeType = Node.Type;
                                    t.Value = ReturnMsg.Value;
                                    t.CommandEncodeStr = ReturnMsg.OrgMsg;
                                    t.Method = ReturnMsg.Command;

                                    //TransactionRecord.AddDetail(TransactionRecord.GetUUID(), Node.Name,Node.Type,ReturnMsg.Type,ReturnMsg.Value);
                                    _ReportTarget.On_Event_Trigger(Node, ReturnMsg);
                                    break;
                                case CommandReturnMessage.ReturnType.Excuted:

                                    _ReportTarget.On_Command_Excuted(Node, Txn, ReturnMsg);
                                    if (Txn.CommandType.Equals("CMD") && !Node.Type.Equals("LOADPORT"))
                                    {
                                        _ReportTarget.On_Node_State_Changed(Node, "Busy");
                                    }
                                    break;
                                case CommandReturnMessage.ReturnType.Finished:

                                    //if (Node.Type.Equals("LOADPORT"))
                                    //{
                                    //    Node.InterLock = false;
                                    //}

                                    _ReportTarget.On_Command_Finished(Node, Txn, ReturnMsg);
                                    if (!Node.Type.Equals("LOADPORT"))
                                    {
                                        _ReportTarget.On_Node_State_Changed(Node, "StandBy");
                                    }


                                    break;
                                case CommandReturnMessage.ReturnType.Error:

                                    //if (Node.Type.Equals("LOADPORT"))
                                    //{
                                    //    Node.InterLock = false;
                                    //}
                                    _ReportTarget.On_Command_Error(Node, Txn, ReturnMsg);

                                    break;

                            }


                        }
                        else
                        {
                            logger.Debug(DeviceName + "(On_Connection_Message Message decode fail:" + Msg);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(DeviceName + "(On_Connection_Message " + IPAdress + ":" + Port.ToString() + ")" + e.Message + "\n" + e.StackTrace);
                    }
                }

            }
            catch (Exception e)
            {
                logger.Error(DeviceName + "(On_Connection_Message " + IPAdress + ":" + Port.ToString() + ")" + e.Message + "\n" + e.StackTrace);
            }

        }
    }
}
