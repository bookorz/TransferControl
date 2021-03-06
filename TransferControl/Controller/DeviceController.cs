﻿using TransferControl.Comm;
using TransferControl.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Collections.Concurrent;
using System.Threading;
using TransferControl.CommandConvert;
using TransferControl.Config;
using Newtonsoft.Json;

namespace TransferControl.Controller
{
    public class DeviceController : IConnectionReport, ITransactionReport, IController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(DeviceController));
        [JsonIgnore]
        public ICommandReport _ReportTarget;
        IConnection conn;

        CommandDecoder _Decoder;
        [JsonIgnore]
        public CommandEncoder Encoder;
        ConcurrentDictionary<string, Transaction> TransactionList = new ConcurrentDictionary<string, Transaction>();
        public string Name { get; set; }
        [JsonIgnore]
        public string Status = "Disconnected";
        private bool _IsConnected { get; set; }
        [JsonIgnore]
        public int TrxNo = 1;


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
            this.Vendor=Vendor;
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
            _Decoder = new CommandConvert.CommandDecoder(Vendor);

            Encoder = new CommandEncoder(Vendor);


            this.Name = DeviceName;
            this.Status = "";
            this._IsConnected = false;
            ThreadPool.QueueUserWorkItem(new WaitCallback(Start));
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
            conn.WaitForData(WaitForData);

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
                    if (!this.Vendor.ToUpper().Equals("MITSUBISHI_PLC"))
                    {
                        _ReportTarget.On_Message_Log("CMD", DeviceName + " Send:" + Txn.CommandEncodeStr.Replace("\r", ""));
                    }
                    if (this.Vendor.Equals("SMARTTAG8200") || this.Vendor.Equals("SMARTTAG8400"))
                    {
                        //if (Txn.Method == Transaction.Command.SmartTagType.GetLCDData)
                        //{
                        //    conn.WaitForData(true);
                        //}
                        ThreadPool.QueueUserWorkItem(new WaitCallback(conn.SendHexData), Txn.CommandEncodeStr);

                    }
                    else
                    {

                        ThreadPool.QueueUserWorkItem(new WaitCallback(conn.Send), Txn.CommandEncodeStr);
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

        public void sendWith4Byte(object text)
        {

            string cmd = "";
            for (int i = 0; i < text.ToString().Length; i = i + 12)
            {
                if ((text.ToString().Length - i) > 12)
                {
                    cmd = text.ToString().Substring(i, 12);
                }
                else
                {
                    cmd = text.ToString().Substring(i);
                }
                conn.SendHexData(cmd);
                System.Threading.Thread.Sleep(22);
            }
            //Console.WriteLine("Send:" + text);

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
                                        conn.Send("admin\r\n");
                                        continue;
                                    }
                                    if (ReturnMsg.Type == CommandReturnMessage.ReturnType.Password)
                                    {
                                        conn.Send("\r\n");
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
                                        conn.Send(ReturnMsg.FinCommand);
                                        //isWaiting = false;
                                        logger.Debug(DeviceName + " Send:" + ReturnMsg.FinCommand);
                                    }
                                }
                                else if (TransactionList.TryRemove(key, out Txn))
                                {
                                    // Node.InitialComplete = false;
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
                                                conn.Send(ReturnMsg.FinCommand);
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
                                            conn.Send(ReturnMsg.FinCommand);

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
                                            conn.Send(ReturnMsg.FinCommand);

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
                                                    conn.Send(ReturnMsg.FinCommand);
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
            _ReportTarget.On_Message_Log("CMD", DeviceName + " " + "Connected");

        }

        public void On_Connection_Connecting(string Msg)
        {
            this._IsConnected = false;
            this.Status = "Connecting";
            ChangeNodeConnectionStatus("Connecting");
            _ReportTarget.On_Controller_State_Changed(DeviceName, "Connecting");
            _ReportTarget.On_Message_Log("CMD", DeviceName + " " + "Connecting");
        }

        public void On_Connection_Disconnected(string Msg)
        {
            this._IsConnected = false;
            this.Status = "Disconnected";
            ChangeNodeConnectionStatus("Disconnected");
            _ReportTarget.On_Controller_State_Changed(DeviceName, "Disconnected");
            _ReportTarget.On_Message_Log("CMD", DeviceName + " " + "Disconnected");
            conn.Reconnect();
        }

        public void On_Connection_Error(string Msg)
        {
            this._IsConnected = false;
            //foreach (Transaction txn in TransactionList.Values.ToList())
            //{
            //    txn.SetTimeOutMonitor(false);
            //}
            //TransactionList.Clear();
            ChangeNodeConnectionStatus("Connection_Error");
            _ReportTarget.On_Controller_State_Changed(DeviceName, "Connection_Error");
            
            _ReportTarget.On_Message_Log("CMD", DeviceName + " " + "Connection_Error");
            conn.Reconnect();
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
                    Node Node = NodeManagement.GetByController(DeviceName, Txn.AdrNo);
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

        public void On_Transaction_BypassTimeOut(Transaction Txn)
        {
            logger.Debug(DeviceName + "(On_Transaction_BypassTimeOut Txn is timeout:" + Txn.CommandEncodeStr);
            _ReportTarget.On_Message_Log("CMD", DeviceName + "(On_Transaction_BypassTimeOut Txn is timeout:" + Txn.CommandEncodeStr);
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
