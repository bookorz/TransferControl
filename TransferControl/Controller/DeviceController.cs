using TransferControl.Comm;
using TransferControl.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Collections.Concurrent;
using SANWA.Utility;
using System.Threading;
using SANWA.Utility.Config;

namespace TransferControl.Controller
{
    public class DeviceController : IConnectionReport, ITransactionReport
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DeviceController));
        public ICommandReport _ReportTarget;
        IConnection conn;

        SANWA.Utility.Decoder _Decoder;
        public SANWA.Utility.Encoder Encoder;
        ConcurrentDictionary<string, Transaction> TransactionList = new ConcurrentDictionary<string, Transaction>();
        public string Name { get; set; }
        public string Status = "Disconnected";
        private bool _IsConnected { get; set; }
        public int TrxNo = 1;
        bool WaitingForSync = false;
        string ReturnForSync = "";
        string ReturnTypeForSync = "";

        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string Vendor { get; set; }
        public string IPAdress { get; set; }
        public int Port { get; set; }
        public string ConnectionType { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public bool Enable { get; set; }

        public void SetReport(ICommandReport ReportTarget)
        {
            _ReportTarget = ReportTarget;


            switch (ConnectionType.ToUpper())
            {
                case "SOCKET":
                    //conn = new SocketClient(Config, this);
                    conn = new SocketClient(this, this);
                    break;
                case "COMPORT":
                    conn = new ComPortClient(this, this);
                    break;

            }
            _Decoder = new SANWA.Utility.Decoder(Vendor);

            Encoder = new SANWA.Utility.Encoder(Vendor);


            this.Name = DeviceName;
            this.Status = "";
            this._IsConnected = false;
        }

        public DeviceController GetConfig()
        {
            return this;
        }

        public bool IsConnected()
        {
            return this._IsConnected;
        }

        public void ClearTransactionList()
        {
            foreach (Transaction each in TransactionList.Values)
            {
                each.SetTimeOutMonitor(false);
            }
            TransactionList.Clear();
        }

        public void Reconnect()
        {
            try
            {
                conn.Reconnect();
            }
            catch (Exception e)
            {

                logger.Error(DeviceName + "(DisconnectServer " + IPAdress + ":" + Port.ToString() + ")" + e.Message + "\n" + e.StackTrace);

            }
        }

        //public void Connect()
        //{
        //    try
        //    {
        //        Close();
        //        conn.Connect();
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Error(_Config.DeviceName + "(ConnectToServer " + _Config.IPAdress + ":" + _Config.Port.ToString() + ")" + e.Message + "\n" + e.StackTrace);
        //    }

        //}

        public void Start(object state)
        {
            try
            {

                conn.Start();
            }
            catch (Exception e)
            {
                logger.Error(DeviceName + "(ConnectToServer " + IPAdress + ":" + Port.ToString() + ")" + e.Message + "\n" + e.StackTrace);
            }

        }

        public string DoWorkSync(string Cmd, string Type, int Timeout = 30000)
        {
            string result = "";
            WaitingForSync = true;
            ReturnTypeForSync = Type;
            conn.Send(Cmd);

            SpinWait.SpinUntil(() => !WaitingForSync, Timeout);
            if (WaitingForSync)
            {
                result = "Command time out!";
            }
            else
            {
                result = ReturnForSync;
                ReturnForSync = "";
                ReturnTypeForSync = "";
            }
            WaitingForSync = false;
            return result;
        }

        public bool DoWork(Transaction Txn, bool WaitForData = false)
        {
            conn.WaitForData(WaitForData);
            bool result = false;
            // lock (TransactionList)
            if (Txn.Method.Equals(Transaction.Command.LoadPortType.Reset))
            {
                logger.Debug("Txn timmer stoped.");
                Txn.SetTimeOutMonitor(false);

            }

            if (!Txn.NodeType.Equals("OCR"))
            {
                List<ReturnMessage> msgList = _Decoder.GetMessage(Txn.CommandEncodeStr);

                if (msgList.Count != 0)
                {
                    Txn.Type = msgList[0].Command;
                    Txn.CommandType = msgList[0].CommandType;
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
            else if (Vendor.ToUpper().Equals("HST") || Vendor.ToUpper().Equals("COGNEX"))
            {
                key = "1" + Txn.Type;
            }
            else if (Vendor.ToUpper().Equals("ASYST") || Vendor.ToUpper().Equals("SMARTTAG") || Vendor.ToUpper().Equals("ACDT"))
            {
                key = Txn.AdrNo;
            }
            else
            {
                key = Txn.AdrNo + Txn.Type;
                //支援同時多發命令
                for (int seq = 0; seq <= 99; seq++)
                {
                    key = key + seq.ToString("00");
                    if (!TransactionList.ContainsKey(key))
                    {
                        break;
                    }
                    if (seq == 99)
                    {
                        logger.Error("seq is run out!");
                    }
                }
            }
            

            if (TransactionList.TryAdd(key, Txn) || Txn.Method.Equals("Stop"))
            {
                

                    

                Txn.SetTimeOutReport(this);
                Txn.SetTimeOutMonitor(true);
                TransactionRecord.New(Txn);


                string waferids = "";
                foreach (Job each in Txn.TargetJobs)
                {
                    waferids += each.Job_Id + " ";
                }
                //if (Vendor.ToUpper().Equals("ACDT"))
                //{
                //    byte[] byteAry = Encoding.UTF8.GetBytes(Txn.CommandEncodeStr);
                        

                //    logger.Debug(DeviceName + " Send:" + BitConverter.ToString(byteAry) + " Wafer:" + waferids);
                //}
                //else
                //{
                    logger.Debug(DeviceName + " Send:" + Txn.CommandEncodeStr.Replace("\r", "") + " Wafer:" + waferids);
               //}
                Txn.CommandType = _Decoder.GetMessage(Txn.CommandEncodeStr)[0].CommandType;
                if (Txn.CommandType.Equals("GET") || Txn.CommandType.IndexOf("FS") != -1)
                {
                    Txn.SetTimeOut(1000);
                }


                if (this.Vendor.Equals("SMARTTAG"))
                {
                    result = sendWith4Byte(Txn.CommandEncodeStr);
                }
                else
                {
                    result = conn.Send(Txn.CommandEncodeStr);
                }
            }
            else
            {
                Transaction workingTxn;
                TransactionList.TryGetValue(key, out workingTxn);
                logger.Debug(DeviceName + "(DoWork " + IPAdress + ":" + Port.ToString() + ":" + Txn.CommandEncodeStr + ") Same type command " + workingTxn.CommandEncodeStr + " is already excuting.");

                result = false;
            }



            //}
            return result;
        }

        public bool sendWith4Byte(string text)
        {
            bool result = true;
            string cmd = "";
            for (int i = 0; i < text.Length; i = i + 12)
            {
                if ((text.Length - i) > 12)
                {
                    cmd = text.Substring(i, 12);
                }
                else
                {
                    cmd = text.Substring(i);
                }
                result = result & conn.SendHexData(cmd);
                System.Threading.Thread.Sleep(22);
            }
            //Console.WriteLine("Send:" + text);
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
                    logger.Debug(DeviceName + " Recieve:" + Msg.Replace("\r", ""));
                //}


                List<ReturnMessage> ReturnMsgList = _Decoder.GetMessage(Msg);
                foreach (ReturnMessage ReturnMsg in ReturnMsgList)
                {

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
                                if (WaitingForSync)
                                {
                                    if (ReturnMsg.Type.Equals(ReturnMessage.ReturnType.Error))
                                    {
                                        ReturnForSync = Msg;
                                        WaitingForSync = false;
                                        return;
                                    }
                                    else if (ReturnTypeForSync.Equals("CMD"))
                                    {
                                        if (ReturnMsg.Type.Equals(ReturnMessage.ReturnType.Finished))
                                        {
                                            ReturnForSync = Msg;
                                            WaitingForSync = false;
                                            return;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        if (ReturnMsg.Type.Equals(ReturnMessage.ReturnType.Excuted))
                                        {
                                            ReturnForSync = Msg;
                                            WaitingForSync = false;
                                            return;
                                        }
                                        else
                                        {
                                            continue;
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
                                else if (Vendor.ToUpper().Equals("ASYST") || Vendor.ToUpper().Equals("SMARTTAG") || Vendor.ToUpper().Equals("ACDT"))
                                {
                                    key = ReturnMsg.NodeAdr;
                                }
                                else
                                {
                                    key = ReturnMsg.NodeAdr + ReturnMsg.Command;
                                    for (int seq = 0; seq <= 99; seq++)
                                    {
                                        key = key + seq.ToString("00");
                                        if (TransactionList.ContainsKey(key))
                                        {
                                            break;
                                        }
                                        if (seq == 99)
                                        {
                                            logger.Error("seq is run out!");
                                        }
                                    }
                                }
                                if (Vendor.ToUpper().Equals("KAWASAKI"))
                                {
                                    //if (TransactionList.TryGetValue(key, out Txn))
                                    //{
                                    //    Node = NodeManagement.Get(Txn.NodeName);
                                    //    if (!Txn.CommandType.Equals("GET") && !Txn.CommandType.Equals("SET") && !Txn.CommandType.Equals("CMD"))
                                    //    {
                                    //        Txn.CommandType = Encoder.GetCommandType(Txn.CommandType);
                                    //    }
                                    //    if (!Txn.CommandType.Equals("CMD"))
                                    //    {
                                    //        if (ReturnMsg.Type.Equals(ReturnMessage.ReturnType.Excuted))
                                    //        {
                                    //            continue;
                                    //        }
                                    //        else if (ReturnMsg.Type.Equals(ReturnMessage.ReturnType.Finished))
                                    //        {
                                    //            ReturnMsg.Type = ReturnMessage.ReturnType.Excuted;
                                    //        }
                                    //    }

                                    //}
                                    //else
                                    //{
                                    //    logger.Debug("Transaction not exist:key=" + key);
                                    //    return;
                                    //}
                                }
                                else if (Vendor.ToUpper().Equals("TDK"))
                                {
                                    if (TransactionList.TryGetValue(key, out Txn))
                                    {
                                        Node = NodeManagement.Get(Txn.NodeName);
                                        if (Txn.CommandType.Equals("SET") && ReturnMsg.Type.Equals(ReturnMessage.ReturnType.Excuted))
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Node = NodeManagement.GetByController(DeviceName, ReturnMsg.NodeAdr);
                                    }
                                }
                                else
                                {

                                    Node = NodeManagement.GetByController(DeviceName, ReturnMsg.NodeAdr);
                                    if (Node == null)
                                    {
                                        Node = NodeManagement.GetOCRByController(DeviceName);
                                    }
                                }
                                //lock (TransactionList)
                                //{
                                lock (Node)
                                {

                                    if (ReturnMsg.Type == ReturnMessage.ReturnType.Event)
                                    {
                                        //_ReportTarget.On_Event_Trigger(Node, ReturnMsg);
                                    }
                                    else if (TransactionList.TryRemove(key, out Txn))
                                    {
                                        // Node.InitialComplete = false;
                                        switch (ReturnMsg.Type)
                                        {
                                            case ReturnMessage.ReturnType.Excuted:
                                                if (!Txn.CommandType.Equals("CMD") && !Txn.CommandType.Equals("MOV") && !Txn.CommandType.Equals("HCS"))
                                                {
                                                    logger.Debug("Txn timmer stoped.");
                                                    Txn.SetTimeOutMonitor(false);
                                                    Node.IsExcuting = false;
                                                }
                                                else
                                                {
                                                    if (Txn.Method.Equals(Transaction.Command.LoadPortType.Reset))
                                                    {
                                                        logger.Debug("Txn timmer stoped.");
                                                        Txn.SetTimeOutMonitor(false);
                                                        Node.IsExcuting = false;
                                                    }
                                                    else if (Txn.Method.Equals(Transaction.Command.RobotType.OrginSearch) || Txn.Method.Equals(Transaction.Command.LoadPortType.InitialPos))
                                                    {
                                                        logger.Debug("Txn timmer stoped.");
                                                        Txn.SetTimeOutMonitor(false);
                                                        //Node.IsExcuting = false;
                                                        TransactionList.TryAdd(key, Txn);
                                                    }
                                                    else
                                                    {
                                                        Txn.SetTimeOutMonitor(false);
                                                        Txn.SetTimeOut(30000);
                                                        Txn.SetTimeOutMonitor(true);
                                                        TransactionList.TryAdd(key, Txn);
                                                    }
                                                }
                                                //_ReportTarget.On_Command_Excuted(Node, Txn, ReturnMsg);
                                                break;
                                            case ReturnMessage.ReturnType.Finished:
                                                logger.Debug("Txn timmer stoped.");
                                                Txn.SetTimeOutMonitor(false);
                                                Node.IsExcuting = false;
                                                //_ReportTarget.On_Command_Finished(Node, Txn, ReturnMsg);
                                                break;
                                            case ReturnMessage.ReturnType.Error:
                                                logger.Debug("Txn timmer stoped.");
                                                Txn.SetTimeOutMonitor(false);
                                                Node.IsExcuting = false;
                                                //_ReportTarget.On_Command_Error(Node, Txn, ReturnMsg);
                                                if (Vendor.ToUpper().Equals("TDK") || Vendor.ToUpper().Equals("SMARTTAG"))
                                                {
                                                    conn.Send(ReturnMsg.FinCommand);
                                                    logger.Debug(DeviceName + "Send:" + ReturnMsg.FinCommand);
                                                }
                                                break;
                                            case ReturnMessage.ReturnType.Information:
                                                logger.Debug("Txn timmer stoped.");
                                                Txn.SetTimeOutMonitor(false);
                                                if (Vendor.ToUpper().Equals("TDK") && Txn.CommandType.Equals("SET"))
                                                {
                                                    ReturnMsg.Type = ReturnMessage.ReturnType.Excuted;
                                                    Node.IsExcuting = false;
                                                }
                                                else
                                                {
                                                    ReturnMsg.Type = ReturnMessage.ReturnType.Finished;
                                                    Node.IsExcuting = false;
                                                }
                                                SpinWait.SpinUntil(() => false, 50);
                                                //ThreadPool.QueueUserWorkItem(new WaitCallback(conn.Send), ReturnMsg.FinCommand);
                                                conn.Send(ReturnMsg.FinCommand);
                                                logger.Debug(DeviceName + "Send:" + ReturnMsg.FinCommand);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        if (ReturnMsg.Type != null)
                                        {
                                            if (ReturnMsg.Type.Equals(ReturnMessage.ReturnType.Information))
                                            {
                                                //ThreadPool.QueueUserWorkItem(new WaitCallback(conn.Send), ReturnMsg.FinCommand);
                                                conn.Send(ReturnMsg.FinCommand);
                                                logger.Debug(DeviceName + "Send:" + ReturnMsg.FinCommand);
                                            }
                                            else
                                            {
                                                if (ReturnMsg.Type.ToUpper().Equals("ERROR"))
                                                {
                                                    Txn = TransactionList.First().Value;

                                                    logger.Debug("Txn timmer stoped.");
                                                    Txn.SetTimeOutMonitor(false);
                                                    Node.IsExcuting = false;
                                                    //_ReportTarget.On_Command_Error(Node, Txn, ReturnMsg);
                                                    if (Vendor.ToUpper().Equals("TDK") || Vendor.ToUpper().Equals("SMARTTAG"))
                                                    {
                                                        conn.Send(ReturnMsg.FinCommand);
                                                        logger.Debug(DeviceName + "Send:" + ReturnMsg.FinCommand);
                                                    }

                                                    TransactionList.TryRemove(TransactionList.First().Key, out Txn);
                                                }
                                                else
                                                {

                                                    logger.Debug(DeviceName + "(On_Connection_Message Txn is not found. msg:" + Msg);
                                                    return;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            logger.Debug(DeviceName + "(On_Connection_Message Return type is null. msg:" + Msg);
                                            return;
                                        }
                                    }
                                }
                            }
                            switch (ReturnMsg.Type)
                            {
                                case ReturnMessage.ReturnType.Information:
                                case ReturnMessage.ReturnType.Event:
                                    Transaction t = new Transaction();
                                    t.NodeName = Node.Name;
                                    t.NodeType = Node.Type;
                                    t.Value = ReturnMsg.Value;
                                    t.CommandEncodeStr = ReturnMsg.OrgMsg;
                                    t.Method = ReturnMsg.Command;
                                    TransactionRecord.New(t, ReturnMsg.Type);
                                    //TransactionRecord.AddDetail(TransactionRecord.GetUUID(), Node.Name,Node.Type,ReturnMsg.Type,ReturnMsg.Value);
                                    _ReportTarget.On_Event_Trigger(Node, ReturnMsg);
                                    break;
                                case ReturnMessage.ReturnType.Excuted:
                                    TransactionRecord.Update(Txn, ReturnMsg);
                                    _ReportTarget.On_Command_Excuted(Node, Txn, ReturnMsg);
                                    if (Txn.CommandType.Equals("CMD") && !Node.Type.Equals("LOADPORT"))
                                    {
                                        _ReportTarget.On_Node_State_Changed(Node, "Busy");
                                    }
                                    break;
                                case ReturnMessage.ReturnType.Finished:
                                    TransactionRecord.Update(Txn, ReturnMsg);
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
                                case ReturnMessage.ReturnType.Error:
                                    TransactionRecord.Update(Txn, ReturnMsg);
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

        private void ChangeNodeConnectionStatus(string status)
        {
            var Nodes = from each in NodeManagement.GetList()
                        where each.Controller.Equals(this.Name)
                        select each;
            foreach (Node eachNode in Nodes)
            {
                eachNode.ConnectionStatus = status;
            }
        }

        public void On_Connection_Connected(object Msg)
        {
            this._IsConnected = true;
            this.Status = "Connected";
            ChangeNodeConnectionStatus("Connected");
            _ReportTarget.On_Controller_State_Changed(DeviceName, "Connected");

        }

        public void On_Connection_Connecting(string Msg)
        {
            this._IsConnected = false;
            this.Status = "Connecting";
            ChangeNodeConnectionStatus("Connecting");
            _ReportTarget.On_Controller_State_Changed(DeviceName, "Connecting");

        }

        public void On_Connection_Disconnected(string Msg)
        {
            this._IsConnected = false;
            this.Status = "Disconnected";
            ChangeNodeConnectionStatus("Disconnected");
            _ReportTarget.On_Controller_State_Changed(DeviceName, "Disconnected");

        }

        public void On_Connection_Error(string Msg)
        {
            this._IsConnected = false;
            foreach (Transaction txn in TransactionList.Values.ToList())
            {
                txn.SetTimeOutMonitor(false);
            }
            TransactionList.Clear();
            ChangeNodeConnectionStatus("Connection_Error");
            _ReportTarget.On_Controller_State_Changed(DeviceName, "Connection_Error");
        }

        public void On_Transaction_TimeOut(Transaction Txn)
        {
            logger.Debug(DeviceName + "(On_Transaction_TimeOut Txn is timeout:" + Txn.CommandEncodeStr);

            string key = "";
            if (Vendor.ToUpper().Equals("KAWASAKI"))
            {
                key = Txn.Seq;

            }
            else if (Vendor.ToUpper().Equals("HST") || Vendor.ToUpper().Equals("COGNEX"))
            {
                key = "1";
            }
            else if (Vendor.ToUpper().Equals("ASYST") || Vendor.ToUpper().Equals("SMARTTAG") || Vendor.ToUpper().Equals("ACDT"))
            {
                key = Txn.AdrNo;

            }
            else
            {
                key = Txn.AdrNo + Txn.Method;
                for (int seq = 0; seq <= 99; seq++)
                {
                    key = key + seq.ToString("00");
                    if (TransactionList.ContainsKey(key))
                    {
                        break;
                    }
                    if (seq == 99)
                    {
                        logger.Error("seq is run out!");
                    }
                }
            }

            Txn.SetTimeOutMonitor(false);
            if (TransactionList.TryRemove(key, out Txn))
            {
                Node Node = NodeManagement.GetByController(DeviceName, Txn.AdrNo);
                if (Node.State.Equals("Pause"))
                {
                    logger.Debug("Txn timeout,but state is pause. ignore this.");
                    TransactionList.TryAdd(key, Txn);
                    return;
                }
                if (Node != null)
                {
                    _ReportTarget.On_Command_TimeOut(Node, Txn);
                }
                else
                {
                    logger.Debug(DeviceName + "(On_Transaction_TimeOut Get Node fail.");
                }
            }
            else
            {
                logger.Debug(DeviceName + "(On_Transaction_TimeOut TryRemove Txn fail.");
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

        public SANWA.Utility.Encoder GetEncoder()
        {
            return Encoder;
        }

        public void On_Transaction_BypassTimeOut(Transaction Txn)
        {
            logger.Debug(DeviceName + "(On_Transaction_BypassTimeOut Txn is timeout:" + Txn.CommandEncodeStr);

            string key = "";
            if (Vendor.ToUpper().Equals("KAWASAKI"))
            {
                key = Txn.Seq;

            }
            else if (Vendor.ToUpper().Equals("HST") || Vendor.ToUpper().Equals("COGNEX"))
            {
                key = "1";
            }
            else if (Vendor.ToUpper().Equals("ASYST") || Vendor.ToUpper().Equals("SMARTTAG") || Vendor.ToUpper().Equals("ACDT"))
            {
                key = Txn.AdrNo;

            }
            else
            {
                key = Txn.AdrNo + Txn.Method;
                for (int seq = 0; seq <= 99; seq++)
                {
                    key = key + seq.ToString("00");
                    if (TransactionList.ContainsKey(key))
                    {
                        break;
                    }
                    if (seq == 99)
                    {
                        logger.Error("seq is run out!");
                    }
                }
            }

            Txn.SetTimeOutMonitor(false);
            if (TransactionList.TryRemove(key, out Txn))
            {
                Node Node = NodeManagement.GetByController(DeviceName, Txn.AdrNo);
                if (Node.State.Equals("Pause"))
                {
                    logger.Debug("Txn timeout,but state is pause. ignore this.");
                    TransactionList.TryAdd(key, Txn);
                    return;
                }
                if (Node != null)
                {
                    //_ReportTarget.On_Command_TimeOut(Node, Txn);
                }
                else
                {
                    logger.Debug(DeviceName + "(On_Transaction_TimeOut Get Node fail.");
                }
            }
            else
            {
                logger.Debug(DeviceName + "(On_Transaction_TimeOut TryRemove Txn fail.");
            }
        }
    }
}
