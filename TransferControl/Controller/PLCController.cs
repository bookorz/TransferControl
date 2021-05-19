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
    public class PLCController : ITransactionReport, IController
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

        //private string ch1Send = "";
        //private string ch2Send = "";
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

                if (Txn.CommandType.Equals(""))
                {
                    Txn.CommandType = _Decoder.GetMessage(Txn.CommandEncodeStr)[0].CommandType;
                }

                if (Txn.Method.Equals(Transaction.Command.LoadPortType.Reset))
                {
                    Txn.SetTimeOut(15000);
                }
                else
                {
                    Txn.SetTimeOut(Txn.AckTimeOut);
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

            _Decoder = new CommandDecoder(Vendor);

            Encoder = new CommandEncoder(Vendor);


            this.Name = DeviceName;
            this.Status = "";

            ThreadPool.QueueUserWorkItem(new WaitCallback(Start), NodeManagement.Get("CCLinkController"));
        }


        public void Start(object node)
        {
            //PLC內X與Y區的起始位置為500(16進位) => 1280十進位
            int AddrOffset = 1280;
            //int CstRobotStation = 9;
            //int VipRobotStation = 13;
            Node Target = (Node)node;

            McProtocolTcp PLC = new McProtocolTcp(this.IPAdress, this.Port);

            this._IsConnected = true;
            byte[] result = new byte[512];
            int[] WResult = new int[32];
            bool isInit = false;

            while (true)
            {
                try
                {

                    if (!isInit)
                    {

                        PLC.Open();
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
                    }

                    if (!Target.GetIO("OUTPUT").SequenceEqual(Target.GetIO("OUTPUT_OLD")))
                    {
                        //Dictionary<int, byte> changedList = new Dictionary<int, byte>();
                        for (int i = 0; i < Target.GetIO("OUTPUT").Length; i++)
                        {
                            if (Target.GetIO("OUTPUT")[i] != Target.GetIO("OUTPUT_OLD")[i])
                            {

                                Target.SetIO("OUTPUT_OLD", i, Target.GetIO("OUTPUT")[i]);

                                Target.Mode = "OUTPUT";
                                Target.Position = i.ToString();
                                _ReportTarget.On_Node_State_Changed(Target, Target.GetIO("OUTPUT")[i].ToString());
                                PLC.SetBitDevice(PlcDeviceType.Y, i + AddrOffset, 1, new byte[] { Target.GetIO("OUTPUT")[i] });
                            }
                        }
                    }

                    result = new byte[512];
                    PLC.GetBitDevice(PlcDeviceType.X, AddrOffset, 512, result);
                    Target.SetIO("INPUT", result);
                    if (!Target.GetIO("INPUT").SequenceEqual(Target.GetIO("INPUT_OLD")))
                    {
                        for (int i = 0; i < Target.GetIO("INPUT").Length; i++)
                        {
                            if (Target.GetIO("INPUT")[i] != Target.GetIO("INPUT_OLD")[i])
                            {
                                Target.Mode = "INPUT";
                                Target.Position = i.ToString();
                                _ReportTarget.On_Node_State_Changed(Target, Target.GetIO("INPUT")[i].ToString());
                            }
                        }

                        ////6為ERROR Bit(for CST Robot)
                        //if (Target.GetIO("INPUT")[6 + (CstRobotStation - 1) * 32] == 1 && Target.GetIO("INPUT_OLD")[6 + (CstRobotStation - 1) * 32] == 0)
                        //{
                        //    string errAry = "";
                        //    for (int i = 32; i <= 64; i++)
                        //    {
                        //        errAry += Target.GetIO("INPUT")[i + (CstRobotStation - 1) * 32].ToString();
                        //    }

                        //    string error = new String(errAry.Reverse().ToArray());
                        //    error = Convert.ToInt32(error, 2).ToString("X");
                        //    _ReportTarget.On_Alarm_Happen(AlarmManagement.NewAlarm(new Node() { Name = "CSTRobot", Vendor = "SANWA_MC", Type = "ROBOT" }, error));
                        //}

                        ////6為ERROR Bit(for VIP Robot)
                        //if (Target.GetIO("INPUT")[6 + (VipRobotStation - 1) * 32] == 1 && Target.GetIO("INPUT_OLD")[6 + (VipRobotStation - 1) * 32] == 0)
                        //{
                        //    string errAry = "";
                        //    for (int i = 32; i <= 64; i++)
                        //    {
                        //        errAry += Target.GetIO("INPUT")[i + (VipRobotStation - 1) * 32].ToString();
                        //    }

                        //    string error = new String(errAry.Reverse().ToArray());
                        //    error = Convert.ToInt32(error, 2).ToString("X");
                        //    //_ReportTarget.On_Alarm_Happen(AlarmManagement.NewAlarm(Target, error, ""));
                        //    _ReportTarget.On_Alarm_Happen(AlarmManagement.NewAlarm(new Node() { Name = "VIPRobot", Vendor = "SANWA_MC", Type = "ROBOT" }, error));

                        //}

                        Target.SetIO("INPUT_OLD", Target.GetIO("INPUT"));

                    }
                }
                catch (Exception e)
                {
                    _ReportTarget.On_Message_Log("IO", "Lost connection with PLC");
                    SpinWait.SpinUntil(() => false, 5000);
                    try
                    {
                        if (isInit)
                        {
                            PLC.Open();
                        }
                    }
                    catch (Exception eeee)
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
                             .Select(x => Convert.ToInt32(BitConverter.ToInt16(bytes, x)))
                             .ToArray();
        }

        private byte[] ConvertToBit(int[] WResult)
        {
            string BitStr = "";
            byte[] result = new byte[WResult.Length*16];
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
