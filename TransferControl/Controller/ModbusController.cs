using log4net;
using Modbus.Device;
using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Net.Sockets;
using System.Threading;
using TransferControl.CommandConvert;
using TransferControl.Management;

namespace TransferControl.Controller
{
    public class ModbusController : ITransactionReport, IController
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ModbusController));
        ConcurrentDictionary<string, Transaction> TransactionList = new ConcurrentDictionary<string, Transaction>();
        public ICommandReport _ReportTarget;
        IModbusMaster conn;
        public CommandEncoder Encoder;
        public int TrxNo = 1;
        public string Name { get; set; }
        public string Status = "Disconnected";
        private bool _IsConnected { get; set; }
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
        public CommandEncoder GetEncoder()
        {
            return Encoder;
        }
        public void DoWork(Transaction Txn, bool WaitForData = false)
        {
            
            SpinWait.SpinUntil(() => conn!=null, 999999999);
            CommandReturnMessage msg = new CommandReturnMessage();
            lock (conn)
            {
               
                logger.Info(DeviceName + " Send:" + Txn.ModbusMethod);
                //Txn.SetTimeOut(1000);
                //Txn.SetTimeOutReport(this);
                //Txn.SetTimeOutMonitor(true);
                switch (Txn.ModbusMethod)
                {
                    case Transaction.Command.ModbusMethod.ReadHoldingRegisters:
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            ushort[] regs = conn.ReadHoldingRegisters(Txn.ModbusSlaveID, Txn.ModbusStartAddress, Txn.ModbusNumOfPoints);
                            msg.Type = CommandReturnMessage.ReturnType.Excuted;
                            foreach (ushort each in regs)
                            {
                                msg.Value += "," + each.ToString();
                            }
                            msg.Value = msg.Value.Substring(1);
                            _ReportTarget.On_Command_Excuted(NodeManagement.Get(Txn.NodeName), Txn, msg);
                        }).Start();
                        
                        break;
                    case Transaction.Command.ModbusMethod.WriteSingleRegister:
                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            conn.WriteSingleRegister(Txn.ModbusSlaveID, Txn.ModbusRegisterAddress, Txn.ModbusValue);
                            msg.Type = CommandReturnMessage.ReturnType.Excuted;
                            _ReportTarget.On_Command_Excuted(NodeManagement.Get(Txn.NodeName), Txn, msg);
                        }).Start();

                       
                        break;
                }
            }
           
        }

        public void On_Transaction_TimeOut(Transaction Txn)
        {
            //logger.Debug(DeviceName + "(On_Transaction_TimeOut Txn is timeout:" + Txn.CommandEncodeStr);

            //string key = "";

            //key = Txn.AdrNo + Txn.Method;
           


            //Txn.SetTimeOutMonitor(false);
            //if (TransactionList.TryRemove(key, out Txn))
            //{
            //    Node Node = NodeManagement.GetByController(DeviceName, Txn.AdrNo);
            //    if (Node.State.Equals("Pause"))
            //    {
            //        logger.Debug("Txn timeout,but state is pause. ignore this.");
            //        TransactionList.TryAdd(key, Txn);
            //        return;
            //    }
            //    if (Node != null)
            //    {
            //        _ReportTarget.On_Command_TimeOut(Node, Txn);
            //    }
            //    else
            //    {
            //        logger.Debug(DeviceName + "(On_Transaction_TimeOut Get Node fail.");
            //    }
            //}
            //else
            //{
            //    logger.Debug(DeviceName + "(On_Transaction_TimeOut TryRemove Txn fail.");
            //}
        }
        public void On_Transaction_BypassTimeOut(Transaction Txn)
        {
            throw new NotImplementedException();
        }



        public void Reconnect()
        {
            
        }

        public void SetReport(ICommandReport ReportTarget)
        {
            _ReportTarget = ReportTarget;
            

            Encoder = new CommandEncoder(Vendor);

            this.Name = DeviceName;
            this.Status = "";
            this._IsConnected = false;
        }



        public void Start(object state)
        {
            _ReportTarget.On_Controller_State_Changed(DeviceName, "Connecting");
            switch (this.ConnectionType.ToUpper())
            {
                case "SOCKET":
                   
                    conn = ModbusIpMaster.CreateIp(new TcpClient(this.IPAdress, this.Port));
                    _ReportTarget.On_Controller_State_Changed(DeviceName, "Connected");
                    break;
                case "COMPORT":
                    SerialPort serialPort = new SerialPort(); //Create a new SerialPort object.
                    serialPort.PortName = this.PortName;
                    serialPort.BaudRate = this.BaudRate;
                    serialPort.DataBits = 8;
                    serialPort.Parity = Parity.None;
                    serialPort.StopBits = StopBits.One;
                    serialPort.Open();
                    conn = ModbusSerialMaster.CreateRtu(serialPort);
                    _ReportTarget.On_Controller_State_Changed(DeviceName, "Connected");
                    break;
            }
        }
        public string GetDeviceName()
        {
            return this.Name;
        }
        public string GetConnectionType()
        {
            return this.ConnectionType;
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
        public string GetControllerType()
        {
            return this.ControllerType;
        }
        public void SetStatus(string Status)
        {
            this.Status = Status;
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
    }
}
