using log4net;
using TransferControl.Config;
using TransferControl.Controller;
using TransferControl.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using TransferControl.Parser;
using TransferControl.CommandConvert;
using TransferControl.Digital_IO;

namespace TransferControl.Engine
{
    public class MainControl : ICommandReport, IDIOTriggerReport, IJobReport
    {
        //git upload test4
        private static readonly ILog logger = LogManager.GetLogger(typeof(MainControl));

        public bool IsInitial = false;
        //DateTime StartTime = new DateTime();
        IUserInterfaceReport _UIReport;

        //int LapsedWfCount = 0;
        //int LapsedLotCount = 0;
        public string EqpState = "";
        public int NotchDirect = 270;

        public int SpinWaitTimeOut = 99999000;
        public DIO DIO;
        //public TaskFlowManagement TaskJob;
        public static MainControl Instance;
        public static Dictionary<string, string> IO_State = new Dictionary<string, string>();

        /// <summary>
        /// 建構子，傳入一個事件回報對象
        /// </summary>
        /// <param name="ReportTarget"></param>
        public MainControl(IUserInterfaceReport ReportUI)
        {
           // ArchiveLog.doWork(@"D:\log\", @"D:\log_backup\");//自動壓縮LOG檔案
            Instance = this;
            EqpState = "Idle";

            _UIReport = ReportUI;
            //初始化所有Controller
            AlarmManagement.InitialAlarm();
            DIO = new DIO(this);

            //初始化所有Node
            NodeManagement.LoadConfig();

            ControllerManagement.LoadConfig(this);

           


            //初始化Robot點位表
            PointManagement.LoadConfig();
            //初始化工作腳本

            
        }


        /// <summary>
        /// 對所有Controller連線
        /// </summary>






        /// <summary>
        /// 命令傳送成功
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Txn"></param>
        /// <param name="Msg"></param>
        public void On_Command_Excuted(Node Node, Transaction Txn, CommandReturnMessage Msg)
        {
            //string Message = "";
            try
            {
                logger.Debug("On_Command_Excuted");

                //所有裝置
                switch (Txn.Method)
                {
                    case Transaction.Command.RobotType.Reset:
                        Node.HasAlarm = false;
                        AlarmManagement.Clear(Node.Name);
                        _UIReport.On_Node_State_Changed(Node, Node.State);
                        Node.IsExcuting = false;
                        break;
                    case Transaction.Command.RobotType.Pause:
                        Node.IsPause = true;
                        break;
                    case Transaction.Command.RobotType.Continue:
                        Node.IsPause = false;
                        break;
                }





                if (Node.Type.Equals("ROBOT") && !Txn.Position.Equals(""))//紀錄Robot最後位置
                {
                    Node.CurrentPosition = Txn.Position;
                }//分裝置別
                switch (Node.Type)
                {
                    //case "MITSUBISHI_PLC":
                    //    switch (Txn.Method)
                    //    {
                    //        case Transaction.Command.Mitsubishi_PLC.ReadBit:
                    //            string area = Txn.CommandEncodeStr.Substring(Txn.CommandEncodeStr.IndexOf("FB0000")+7,8);
                    //            Node.SetIO(area, Msg.Value);
                               
                    //            break;
                    //        case Transaction.Command.Mitsubishi_PLC.ReadWord:
                    //            area = Txn.CommandEncodeStr.Substring(Txn.CommandEncodeStr.IndexOf("FB0000") + 7, 8);
                    //            Node.SetIO(area, Msg.Value);
                    //            break;
                    //    }
                    //    break;
                    case "SMARTTAG":
                        switch (Txn.Method)
                        {
                            case Transaction.Command.SmartTagType.GetLCDData:
                                Node.FoupID = Msg.Value;
                                break;
                        }
                        break;
                    case "LOADPORT":
                        switch (Txn.Method)
                        {


                            case Transaction.Command.LoadPortType.Unload:
                            case Transaction.Command.LoadPortType.MappingUnload:
                            case Transaction.Command.LoadPortType.DoorUp:
                            case Transaction.Command.LoadPortType.InitialPos:
                            case Transaction.Command.LoadPortType.ForceInitialPos:


                                //LoadPort卸載時打開安全鎖
                                // Node.InterLock = true;
                                //標記尚未Mapping
                                Node.IsMapping = false;
                                Node.MappingResult = "";
                                //刪除所有帳
                                foreach (Job eachJob in JobManagement.GetByNode(Node.Name))
                                {
                                    JobManagement.Remove(eachJob);
                                }



                                Node.FoupID = "";

                                break;
                            case Transaction.Command.LoadPortType.GetLED:
                                MessageParser parser = new MessageParser(Node.Vendor);
                                foreach (KeyValuePair<string, string> each in parser.ParseMessage(Txn.Method, Msg.Value))
                                {
                                    switch (each.Key)
                                    {
                                        case "LOAD":

                                            break;
                                        case "UNLOAD":

                                            break;
                                        case "OPACCESS":
                                            Node.OPACCESS = each.Value == "2" ? true : false;
                                            break;
                                    }
                                }
                                break;
                            case Transaction.Command.LoadPortType.ReadStatus:
                                parser = new MessageParser(Node.Vendor);
                                Node.Status = parser.ParseMessage(Txn.Method, Msg.Value);
                                foreach (KeyValuePair<string, string> each in Node.Status)
                                {
                                    switch (each.Key)
                                    {
                                        case "SLOTPOS":
                                            Node.CurrentSlotPosition = each.Value;

                                            break;
                                        case "PIP":
                                            if (each.Value.Equals("TRUE"))
                                            {
                                                Node.Foup_Placement = true;
                                                Node.Foup_Presence = false;
                                            }
                                            else
                                            {
                                                Node.Foup_Placement = false;
                                                Node.Foup_Presence = true;
                                            }
                                            break;
                                        case "PRTST":
                                            if (each.Value.Equals("UNLK"))
                                            {
                                                Node.Foup_Lock = false;
                                            }
                                            else
                                            {
                                                Node.Foup_Lock = true;
                                            }
                                            break;
                                        case "Door Position":
                                            Node.Door_Position = each.Value;
                                            break;
                                        case "Y Axis Position":
                                            Node.Y_Axis_Position = each.Value;
                                            break;
                                        case "Z Axis Position":
                                            Node.Z_Axis_Position = each.Value;
                                            break;
                                        case "FOUP Clamp Status":
                                            if (each.Value.Equals("Open"))
                                            {
                                                Node.Foup_Lock = false;
                                            }
                                            else if (each.Value.Equals("Close"))
                                            {
                                                Node.Foup_Lock = true;
                                            }
                                            break;
                                        case "Latch Key Status":
                                            if (each.Value.Equals("Open"))
                                            {
                                                Node.Latch_Open = true;
                                            }
                                            else if (each.Value.Equals("Close"))
                                            {
                                                Node.Latch_Open = false;
                                            }
                                            break;
                                        case "Cassette Presence":
                                            if (each.Value.Equals("None"))
                                            {
                                                Node.Foup_Placement = false;
                                                Node.Foup_Presence = false;
                                            }
                                            else if (each.Value.Equals("Normal position"))
                                            {
                                                Node.Foup_Placement = true;
                                                Node.Foup_Presence = true;
                                            }
                                            else if (each.Value.Equals("Error load"))
                                            {
                                                Node.Foup_Placement = false;
                                                Node.Foup_Presence = false;
                                            }
                                            break;
                                        case "Wafer Protrusion Sensor":
                                            if (each.Value.Equals("Unblocked"))
                                            {
                                                Node.WaferProtrusionSensor = false;
                                            }
                                            else if (each.Value.Equals("Blocked"))
                                            {
                                                Node.WaferProtrusionSensor = true;
                                            }
                                            break;
                                    }
                                }


                                break;
                            case Transaction.Command.LoadPortType.GetMapping:
                            case Transaction.Command.LoadPortType.GetMappingDummy:
                                //產生Mapping資料
                                Node.LoadTime = DateTime.Now;
                                string Mapping = Msg.Value;


                                

                                Node.MappingResult = Mapping;

                                Node.IsMapping = true;


                                int currentIdx = 1;
                                for (int i = 0; i < Mapping.Length; i++)
                                {
                                    if (Node.CarrierType != null)
                                    {
                                        if (Node.CarrierType.Equals("OPEN"))
                                        {

                                            if ((i + 1) > 13)
                                            {
                                                continue;
                                            }

                                        }
                                    }
                                    Job wafer = JobManagement.Add();
                                    wafer.Slot = (i + 1).ToString();
                                    
                                    wafer.Position = Node.Name;
                                    wafer.AlignerFlag = false;
                                    wafer.MappingValue = Mapping[i].ToString();
                                    string Slot = (i + 1).ToString("00");


                                    switch (Mapping[i])
                                    {
                                        case '0':
                                            //wafer.Status = Job.MapStatus.Empty;

                                            //wafer.MapFlag = false;
                                            //wafer.ErrPosition = false;
                                            JobManagement.Remove(wafer);
                                            break;
                                        case '1':
                                            wafer.Status = Job.MapStatus.Normal;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = false;

                                            break;
                                        case '2':
                                        case 'E':
                                            wafer.Status = Job.MapStatus.Crossed;

                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;

                                            Node.IsMapping = false;
                                            break;
                                        default:
                                        case '?':
                                            wafer.Status = Job.MapStatus.Undefined;

                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;

                                            Node.IsMapping = false;
                                            break;
                                        case 'W':
                                            wafer.Status = Job.MapStatus.Double;

                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;

                                            Node.IsMapping = false;
                                            break;
                                    }


                                }
                                if (!Node.IsMapping)
                                {
                                    CommandReturnMessage rem = new CommandReturnMessage();
                                    rem.Value = "MAPERR";
                                    _UIReport.On_Command_Error(Node, Txn, rem);
                                }
                                break;

                        }


                        break;
                    case "ROBOT":
                        switch (Txn.Method)
                        {
                            case Transaction.Command.RobotType.Get:
                            case Transaction.Command.RobotType.DoubleGet:
                            case Transaction.Command.RobotType.GetWithoutBack:
                            case Transaction.Command.RobotType.WaitBeforeGet:
                            case Transaction.Command.RobotType.Put:
                            case Transaction.Command.RobotType.DoublePut:
                            case Transaction.Command.RobotType.PutWithoutBack:
                            case Transaction.Command.RobotType.WaitBeforePut:
                                Node.ArmExtend = Txn.Position;
                                break;

                            case Transaction.Command.RobotType.GetSpeed:
                                if ((Node.Vendor.Equals("ATEL_NEW") || Node.Vendor.Equals("SANWA")))
                                {
                                    if (Convert.ToInt16(Msg.Value) == 0)
                                    {
                                        Msg.Value = "100";
                                    }
                                }

                                Node.Speed = Msg.Value;
                                break;
                            case Transaction.Command.RobotType.GetError:
                                if (Msg.Value.Contains("00000000"))
                                {
                                    Node.HasAlarm = false;
                                    Node.LastError = "";
                                }
                                else
                                {
                                    Node.HasAlarm = true;
                                    Node.LastError = Msg.Value;
                                }
                                break;
                            case Transaction.Command.RobotType.GetStatus:
                                MessageParser parser = new MessageParser(Node.Vendor);
                                Dictionary<string, string> StatusResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                foreach (KeyValuePair<string, string> each in StatusResult)
                                {
                                    switch (each.Key)
                                    {
                                        case "Servo":
                                            Node.Servo = each.Value;
                                            break;
                                        case "R_Hold_Status":
                                            Node.R_Hold_Status = each.Value;
                                            break;
                                        case "L_Hold_Status":
                                            Node.L_Hold_Status = each.Value;
                                            break;
                                    }
                                }
                                break;
                            case Transaction.Command.RobotType.GetPosition:
                                parser = new MessageParser(Node.Vendor);
                                Dictionary<string, string> PositionResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                foreach (KeyValuePair<string, string> each in PositionResult)
                                {
                                    switch (each.Key)
                                    {
                                        case "X_Position":
                                            Node.X_Position = each.Value;
                                            break;
                                        case "R_Position":
                                            Node.R_Position = each.Value;
                                            break;
                                        case "L_Position":
                                            Node.L_Position = each.Value;
                                            break;
                                    }
                                }
                                break;
                            case Transaction.Command.RobotType.GetMode:
                                Node.Mode = Msg.Value;
                                break;
                            case Transaction.Command.RobotType.GetRIO:
                                parser = new MessageParser(Node.Vendor);
                                Dictionary<string, string> RioResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                foreach (KeyValuePair<string, string> each in RioResult)
                                {
                                    //Node.SetIO(each.Key, Msg.Value);
                                   
                                    switch (each.Key)
                                    {
                                        case "R_Presure_switch":
                                            if (each.Value.Equals("1"))
                                            {
                                                Node.R_Presence = true;
                                            }
                                            else
                                            {
                                                Node.R_Presence = false;
                                            }
                                            break;
                                        case "R_Present":
                                            if (each.Value.Equals("1"))
                                            {
                                                Node.R_Presence = true;
                                            }
                                            else
                                            {
                                                Node.R_Presence = false;
                                            }
                                            break;
                                        case "L_Present":
                                            if (each.Value.Equals("1"))
                                            {
                                                Node.L_Presence = true;
                                            }
                                            else
                                            {
                                                Node.L_Presence = false;
                                            }
                                            break;
                                        case "R_Hold_Status":
                                            Node.R_Hold_Status = each.Value;
                                            break;
                                        case "L_Hold_Status":
                                            Node.L_Hold_Status = each.Value;
                                            break;
                                        case "R_Clamp_Sensor":
                                            if (each.Value.Equals("1"))
                                            {
                                                Node.RArmClamp = true;
                                            }
                                            else
                                            {
                                                Node.RArmClamp = false;
                                            }
                                            break;
                                        case "R_UnClamp_Sensor":
                                            if (each.Value.Equals("1"))
                                            {
                                                Node.RArmUnClamp = true;
                                            }
                                            else
                                            {
                                                Node.RArmUnClamp = false;
                                            }
                                            break;
                                        case "L_Clamp_Sensor":
                                            if (each.Value.Equals("1"))
                                            {
                                                Node.LArmClamp = true;
                                            }
                                            else
                                            {
                                                Node.LArmClamp = false;
                                            }
                                            break;
                                        case "L_UnClamp_Sensor":
                                            if (each.Value.Equals("1"))
                                            {
                                                Node.LArmUnClamp = true;
                                            }
                                            else
                                            {
                                                Node.LArmUnClamp = false;
                                            }
                                            break;

                                    }
                                }

                                break;
                            case Transaction.Command.RobotType.GetSV:
                                parser = new MessageParser(Node.Vendor);
                                Dictionary<string, string> SVResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                foreach (KeyValuePair<string, string> each in SVResult)
                                {
                                    switch (each.Key)
                                    {
                                        case "R_Vacuum_Solenoid":
                                            Node.R_Vacuum_Solenoid = each.Value;
                                            break;
                                        case "L_Vacuum_Solenoid":
                                            Node.L_Vacuum_Solenoid = each.Value;
                                            break;
                                    }
                                }
                                break;
                            case Transaction.Command.RobotType.GetMapping:

                                //產生Mapping資料
                                string Mapping = Msg.Value.Replace(",", "").Substring(1);


                                Node port = NodeManagement.Get(Node.CurrentPosition);

                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    if (port.Name.Equals("LOADPORT01"))
                                    {
                                        Mapping = SystemConfig.Get().FakeDataP1;

                                    }
                                    if (port.Name.Equals("LOADPORT02"))
                                    {
                                        Mapping = SystemConfig.Get().FakeDataP2;

                                    }
                                    Msg.Value = Mapping;
                                }
                                port.MappingResult = Mapping;

                                port.IsMapping = true;


                                int currentIdx = 1;
                                for (int i = 0; i < Mapping.Length; i++)
                                {
                                    if (port.CarrierType != null)
                                    {
                                        if (port.CarrierType.Equals("OPEN"))
                                        {

                                            if ((i + 1) > 13)
                                            {
                                                continue;
                                            }

                                        }
                                    }
                                    Job wafer = JobManagement.Add();
                                    wafer.Slot = (i + 1).ToString();
                                  
                                    wafer.Position = port.Name;
                                    wafer.AlignerFlag = false;
                                    wafer.MappingValue = Mapping[i].ToString();
                                    string Slot = (i + 1).ToString("00");


                                    switch (Mapping[i])
                                    {
                                        case '0':
                                            //wafer.Status = Job.MapStatus.Empty;
                                            //wafer.MapFlag = false;
                                            //wafer.ErrPosition = false;
                                            //MappingData.Add(wafer);
                                            JobManagement.Remove(wafer);
                                            break;
                                        case '1':


                                            wafer.Status = Job.MapStatus.Normal;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = false;

                                            break;
                                        case '2':
                                        case 'E':
                                            wafer.Status = Job.MapStatus.Crossed;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;
                                            //MappingData.Add(wafer);
                                            port.IsMapping = false;
                                            break;
                                        default:
                                        case '?':
                                            wafer.Status = Job.MapStatus.Undefined;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;
                                            //MappingData.Add(wafer);
                                            port.IsMapping = false;
                                            break;
                                        case 'W':
                                            wafer.Status = Job.MapStatus.Double;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;
                                            //MappingData.Add(wafer);
                                            port.IsMapping = false;
                                            break;
                                    }
                                }
                                if (!port.IsMapping)
                                {
                                    CommandReturnMessage rem = new CommandReturnMessage();
                                    rem.Value = "MAPERR";
                                    _UIReport.On_Command_Error(Node, Txn, rem);
                                }

                                break;

                        }

                        break;
                    case "ALIGNER":
                    case "OCR":
                        switch (Txn.Method)
                        {
                            case Transaction.Command.RobotType.GetSpeed:
                                if (Msg.Value.Equals("0") && (Node.Vendor.Equals("ATEL_NEW") || Node.Vendor.Equals("SANWA")))
                                {
                                    Msg.Value = "100";
                                }

                                Node.Speed = Msg.Value;
                                break;
                            case Transaction.Command.RobotType.GetRIO:
                                MessageParser parser = new MessageParser(Node.Vendor);
                                Dictionary<string, string> RioResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                foreach (KeyValuePair<string, string> each in RioResult)
                                {
                                    //Node.SetIO(each.Key, Msg.Value);
                                    
                                    switch (each.Key)
                                    {
                                        case "R_Present":
                                            if (each.Value.Equals("1"))
                                            {
                                                if (!Node.R_Presence)
                                                {
                                                    logger.Debug(Node.Name + " Presence change 0 to 1");
                                                }
                                                Node.R_Presence = true;
                                            }
                                            else
                                            {
                                                if (Node.R_Presence)
                                                {
                                                    logger.Debug(Node.Name + " Presence change 1 to 0");
                                                }
                                                Node.R_Presence = false;
                                            }
                                            break;
                                        case "L_Present":
                                            if (each.Value.Equals("1"))
                                            {
                                                Node.L_Presence = true;
                                            }
                                            else
                                            {
                                                Node.L_Presence = false;
                                            }
                                            break;
                                        case "R-Hold Status":
                                            Node.R_Hold_Status = each.Value;
                                            break;
                                        case "L-Hold Status":
                                            Node.L_Hold_Status = each.Value;
                                            break;
                                    }
                                }
                                break;
                            case Transaction.Command.RobotType.GetSV:
                                parser = new MessageParser(Node.Vendor);
                                Dictionary<string, string> SVResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                foreach (KeyValuePair<string, string> each in SVResult)
                                {
                                    switch (each.Key)
                                    {
                                        case "R_Vacuum_Solenoid":
                                            Node.R_Vacuum_Solenoid = each.Value;
                                            break;
                                    }
                                }
                                break;
                        }

                        break;
                    case "FFU":
                        switch (Txn.Method)
                        {
                            case Transaction.Command.FFUType.GetStatus:
                                MessageParser parser = new MessageParser(Node.Vendor);
                                Dictionary<string, string> StatusResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                foreach (KeyValuePair<string, string> each in StatusResult)
                                {
                                    switch (each.Key)
                                    {
                                        case "Target_RPM":
                                            Node.Speed = each.Value;
                                            break;
                                        case "Alarm":
                                            Node.HasAlarm = each.Value.Equals("0") ? false : true;
                                            break;
                                    }
                                }
                                break;
                        }
                        break;
                }

                _UIReport.On_Command_Excuted(Node, Txn, Msg);



            }
            catch (Exception e)
            {
                logger.Error(Node.Controller + "-" + Node.AdrNo + "(On_Command_Excuted)" + e.Message + "\n" + e.StackTrace);
            }
        }

        public void On_Command_Finished(Node Node, Transaction Txn, CommandReturnMessage Msg)
        {

            //string Message = "";
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                //UpdateJobLocation(Node, Txn);
                UpdateNodeStatus(Node, Txn);
                Node.LastFinMethod = Txn.Method;


                MessageParser parser = new MessageParser(Node.Vendor);
                Dictionary<string, string> parseResult = null;

                logger.Debug("On_Command_Finished:" + Txn.Method + ":" + Txn.Method);
                switch (Node.Type)
                {
                    case "SHELF":
                        switch (Txn.Method)
                        {
                            case Transaction.Command.Shelf.GetFOUPPresence:
                                Node.Status = parser.ParseMessage(Txn.Method, Msg.Value);
                                break;
                        }
                        break;
                    case "PTZ":
                        switch (Txn.Method)
                        {
                            case Transaction.Command.PTZ.Transfer:
                            case Transaction.Command.PTZ.Home:
                                parseResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                string Mapping = parseResult["Mapping"];
                                Node.MappingResult = Mapping;

                                Node.IsMapping = true;
                                foreach (Job each in JobManagement.GetByNode(Node.Name))
                                {
                                    JobManagement.Remove(each);
                                }


                                int currentIdx = 1;
                                for (int i = 0; i < Mapping.Length; i++)
                                {
                                    Job wafer = JobManagement.Add();
                                    wafer.Slot = (i + 1).ToString();
                                   
                                    wafer.Position = Node.Name;
                                    wafer.AlignerFlag = false;
                                    wafer.MappingValue = Mapping[i].ToString();
                                    string Slot = (i + 1).ToString("00");


                                    switch (Mapping[i])
                                    {
                                        case '0':
                                            //wafer.Status = Job.MapStatus.Empty;
                                            //wafer.MapFlag = false;
                                            //wafer.ErrPosition = false;
                                            //MappingData.Add(wafer);
                                            JobManagement.Remove(wafer);
                                            break;
                                        case '1':
                                            wafer.Status = Job.MapStatus.Normal;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = false;

                                            break;
                                        case '2':
                                        case 'E':
                                            wafer.Status = Job.MapStatus.Crossed;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;
                                            //MappingData.Add(wafer);
                                            Node.IsMapping = false;
                                            break;
                                        default:
                                        case '?':
                                            wafer.Status = Job.MapStatus.Undefined;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;
                                            //MappingData.Add(wafer);
                                            Node.IsMapping = false;
                                            break;
                                        case 'W':
                                            wafer.Status = Job.MapStatus.Double;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;
                                            //MappingData.Add(wafer);
                                            Node.IsMapping = false;
                                            break;
                                    }


                                }
                                if (!Node.IsMapping)
                                {
                                    CommandReturnMessage rem = new CommandReturnMessage();
                                    rem.Value = "MAPERR";
                                    _UIReport.On_Command_Error(Node, Txn, rem);
                                }
                                break;
                        }
                        break;
                    case "ILPT":
                        switch (Txn.Method)
                        {
                            case Transaction.Command.ILPT.Load:
                                parseResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                string Mapping = parseResult["Mapping"];
                                Node.MappingResult = Mapping;

                                Node.IsMapping = true;


                                int currentIdx = 1;
                                for (int i = 0; i < Mapping.Length; i++)
                                {
                                    Job wafer = JobManagement.Add();
                                    wafer.Slot = (i + 1).ToString();
                                   
                                    wafer.Position = Node.Name;
                                    wafer.AlignerFlag = false;
                                    wafer.MappingValue = Mapping[i].ToString();
                                    string Slot = (i + 1).ToString("00");


                                    switch (Mapping[i])
                                    {
                                        case '0':
                                            //wafer.Status = Job.MapStatus.Empty;
                                            //wafer.MapFlag = false;
                                            //wafer.ErrPosition = false;
                                            //MappingData.Add(wafer);
                                            JobManagement.Remove(wafer);
                                            break;
                                        case '1':

                                            wafer.Status = Job.MapStatus.Normal;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = false;


                                            break;
                                        case '2':
                                        case 'E':
                                            wafer.Status = Job.MapStatus.Crossed;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;
                                            //MappingData.Add(wafer);
                                            Node.IsMapping = false;
                                            break;
                                        default:
                                        case '?':
                                            wafer.Status = Job.MapStatus.Undefined;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;
                                            //MappingData.Add(wafer);
                                            Node.IsMapping = false;
                                            break;
                                        case 'W':
                                            wafer.Status = Job.MapStatus.Double;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;
                                            //MappingData.Add(wafer);
                                            Node.IsMapping = false;
                                            break;
                                    }


                                }
                                if (!Node.IsMapping)
                                {
                                    CommandReturnMessage rem = new CommandReturnMessage();
                                    rem.Value = "MAPERR";
                                    _UIReport.On_Command_Error(Node, Txn, rem);
                                }
                                break;
                        }
                        break;
                    case "SMARTTAG":
                        switch (Txn.Method)
                        {
                            case Transaction.Command.SmartTagType.GetLCDData:

                                Node.FoupID = Msg.Value;
                                break;
                        }
                        break;
                    case "ROBOT":
                        //Node.InterLock = false;
                        Node.Busy = false;
                        switch (Txn.Method)
                        {
                            case Transaction.Command.RobotType.Home:
                            case Transaction.Command.RobotType.OrginSearch:
                            case Transaction.Command.RobotType.ArmReturn:
                                Node.State = "READY";
                                Node.ArmExtend = "";
                                Node.CurrentPosition = "";
                                break;
                            case Transaction.Command.RobotType.PutBack:
                            case Transaction.Command.RobotType.GetAfterWait:
                            case Transaction.Command.RobotType.Get:
                            case Transaction.Command.RobotType.Put:
                                Node.ArmExtend = "";
                                break;
                        }



                        break;
                    case "ALIGNER":
                        UpdateNodeStatus(Node, Txn);
                        if (Txn.Method.Equals(Transaction.Command.AlignerType.Align) || Txn.Method.Equals(Transaction.Command.AlignerType.AlignOption))
                        {

                            Node.Home_Position = false;
                        }
                        switch (Txn.Method)
                        {
                            case Transaction.Command.RobotType.Home:
                                Node.State = "READY";
                                Node.Home_Position = true;

                                break;
                            case Transaction.Command.RobotType.OrginSearch:
                                Node.State = "READY";
                                
                                Node.Home_Position = false;
                                break;
                        }
                        break;
                    case "OCR":
                        UpdateNodeStatus(Node, Txn);

                        //Update Wafer ID by OCR result
                        switch (Txn.Method)
                        {
                            case Transaction.Command.OCRType.Read:
                            case Transaction.Command.OCRType.ReadM12:
                            case Transaction.Command.OCRType.ReadT7:

                                Node.Status = parser.ParseMessage(Txn.Method, Msg.Value);

                                foreach (KeyValuePair<string, string> each in Node.Status)
                                {
                                    switch (each.Key)
                                    {
                                        case "WAFER_ID":
                                            Node.OCR_ID = each.Value;
                                            break;
                                        case "SCORE":
                                            Node.OCR_Score = each.Value;
                                            break;
                                        case "PASS":
                                            Node.OCR_Pass = each.Value.Equals("1") ? true : false;

                                            break;
                                    }
                                }

                                break;
                            case Transaction.Command.OCRType.ReadConfig:

                                switch (Node.Vendor)
                                {
                                    case "HST":
                                        OCRInfo result = new OCRInfo(Msg.Value);
                                        Node.OCR_ID = result.Result;
                                        Node.OCR_Score = result.Score;
                                        Node.OCR_Pass = result.Passed.Equals("1") ? true : false;

                                        break;
                                }

                                break;
                        }

                        break;
                    case "LOADPORT":
                        UpdateNodeStatus(Node, Txn);
                        switch (Txn.Method)
                        {
                            case Transaction.Command.LoadPortType.MappingLoad:
                            case Transaction.Command.LoadPortType.Load:

                                break;
                            case Transaction.Command.LoadPortType.Unload:
                            case Transaction.Command.LoadPortType.MappingUnload:
                            case Transaction.Command.LoadPortType.UnDock:
                                Node.IsLoad = false;
                                _UIReport.On_Node_State_Changed(Node, "UnLoad Complete");
                                break;
                            case Transaction.Command.LoadPortType.InitialPos:
                            case Transaction.Command.LoadPortType.ForceInitialPos:
                                Node.IsLoad = false;
                                _UIReport.On_Node_State_Changed(Node, "Ready To Load");
                                Node.State = "READY";
                                break;
                            case Transaction.Command.LoadPortType.GetMapping:
                            case Transaction.Command.LoadPortType.GetMappingDummy:
                                //產生Mapping資料.
                                Node.IsMapping = true;
                                Node.LoadTime = DateTime.Now;
                                string Mapping = Msg.Value;

                                if (Node.Vendor.Equals("SANWA_MC"))
                                {
                                    Mapping = Mapping.Split(new string[] { ","}, StringSplitOptions.None )[1]=="2"?"?????????????????????????": Mapping.Split(new string[] { "," }, StringSplitOptions.None)[1];
                                    if (Mapping.Equals("?????????????????????????"))
                                    {
                                        Node.IsMapping = false;
                                    }
                                }


                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    if (Node.Name.Equals("LOADPORT01"))
                                    {
                                        Mapping = SystemConfig.Get().FakeDataP1;

                                    }
                                    if (Node.Name.Equals("LOADPORT02"))
                                    {

                                        Mapping = SystemConfig.Get().FakeDataP2;

                                    }
                                    Msg.Value = Mapping;
                                }

                                Node.MappingResult = Mapping;

                                


                                int currentIdx = 1;
                                for (int i = 0; i < Mapping.Length; i++)
                                {
                                    if (Node.CarrierType != null)
                                    {
                                        if (Node.CarrierType.Equals("OPEN"))
                                        {

                                            if ((i + 1) > 13)
                                            {
                                                continue;
                                            }

                                        }
                                    }
                                    Job wafer = JobManagement.Add();
                                    wafer.Slot = (i + 1).ToString();
                                   
                                    wafer.Position = Node.Name;
                                    wafer.AlignerFlag = false;
                                    wafer.MappingValue = Mapping[i].ToString();
                                    string Slot = (i + 1).ToString("00");


                                    switch (Mapping[i])
                                    {
                                        case '0':
                                            //wafer.Status = Job.MapStatus.Empty;

                                            //wafer.MapFlag = false;
                                            //wafer.ErrPosition = false;
                                            JobManagement.Remove(wafer);
                                            break;
                                        case '1':
                                            wafer.Status = Job.MapStatus.Normal;
                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = false;

                                            break;
                                        case '2':
                                        case 'E':
                                            wafer.Status = Job.MapStatus.Crossed;

                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;

                                            Node.IsMapping = false;
                                            break;
                                        default:
                                        case '?':
                                            wafer.Status = Job.MapStatus.Undefined;

                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;

                                            Node.IsMapping = false;
                                            break;
                                        case 'W':
                                            wafer.Status = Job.MapStatus.Double;

                                            wafer.MapFlag = true;
                                            wafer.ErrPosition = true;

                                            Node.IsMapping = false;
                                            break;
                                    }


                                }
                                if (!Node.IsMapping)
                                {
                                    CommandReturnMessage rem = new CommandReturnMessage();
                                    rem.Value = "MAPERR";
                                    _UIReport.On_Command_Error(Node, Txn, rem);
                                }
                                break;
                            case Transaction.Command.LoadPortType.ReadStatus:
                                parser = new MessageParser(Node.Vendor);
                                Node.Status = parser.ParseMessage(Txn.Method, Msg.Value);
                                foreach (KeyValuePair<string, string> each in Node.Status)
                                {
                                    switch (each.Key)
                                    {
                                        case "SLOTPOS":
                                            Node.CurrentSlotPosition = each.Value;

                                            break;
                                        case "PIP":
                                            if (each.Value.Equals("TRUE"))
                                            {
                                                Node.Foup_Placement = true;
                                                Node.Foup_Presence = true;
                                            }
                                            else
                                            {
                                                Node.Foup_Placement = false;
                                                Node.Foup_Presence = false;
                                            }
                                            break;
                                        case "PRTST":
                                            if (each.Value.Equals("UNLK"))
                                            {
                                                Node.Foup_Lock = false;
                                            }
                                            else
                                            {
                                                Node.Foup_Lock = true;
                                            }
                                            break;
                                        case "Door Position":
                                            Node.Door_Position = each.Value;
                                            break;
                                        case "Y Axis Position":
                                            Node.Y_Axis_Position = each.Value;
                                            break;
                                        case "Z Axis Position":
                                            Node.Z_Axis_Position = each.Value;
                                            break;
                                        case "FOUP Clamp Status":
                                            if (each.Value.Equals("Open"))
                                            {
                                                Node.Foup_Lock = false;
                                            }
                                            else if (each.Value.Equals("Close"))
                                            {
                                                Node.Foup_Lock = true;
                                            }
                                            break;
                                        case "Latch Key Status":
                                            if (each.Value.Equals("Open"))
                                            {
                                                Node.Latch_Open = true;
                                            }
                                            else if (each.Value.Equals("Close"))
                                            {
                                                Node.Latch_Open = false;
                                            }
                                            break;
                                        case "Cassette Presence":
                                            if (each.Value.Equals("None"))
                                            {
                                                Node.Foup_Placement = false;
                                                Node.Foup_Presence = false;
                                            }
                                            else if (each.Value.Equals("Normal position"))
                                            {
                                                Node.Foup_Placement = true;
                                                Node.Foup_Presence = true;
                                            }
                                            else if (each.Value.Equals("Error load"))
                                            {
                                                Node.Foup_Placement = false;
                                                Node.Foup_Presence = false;
                                            }
                                            break;
                                        case "Wafer Protrusion Sensor":
                                            if (each.Value.Equals("Unblocked"))
                                            {
                                                Node.WaferProtrusionSensor = false;
                                            }
                                            else if (each.Value.Equals("Blocked"))
                                            {
                                                Node.WaferProtrusionSensor = true;
                                            }
                                            break;
                                    }
                                }


                                break;
                            default:
                                Node.IsLoad = false;
                                break;
                        }
                        break;
                }

                _UIReport.On_Command_Finished(Node, Txn, Msg);





            }
            catch (Exception e)
            {
                logger.Error(Node.Controller + "-" + Node.AdrNo + "(On_Command_Finished)" + e.Message + "\n" + e.StackTrace);
            }
            //watch.Stop();
            //var elapsedMs = watch.ElapsedMilliseconds;
            //logger.Debug("On_Command_Finished ProcessTime:" + elapsedMs.ToString());




        }
        /// <summary>
        /// 更新Node狀態
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Txn"></param>
        private void UpdateNodeStatus(Node Node, Transaction Txn)
        {

            switch (Node.Type)
            {
                case "ROBOT":

                    switch (Txn.Method)
                    {

                        case Transaction.Command.RobotType.Get:
                        case Transaction.Command.RobotType.GetAfterWait:
                            Node.CurrentPoint = Txn.Point;



                            break;
                        case Transaction.Command.RobotType.Put:
                        case Transaction.Command.RobotType.PutBack:
                            Node.CurrentPoint = Txn.Point;


                            break;
                        case Transaction.Command.RobotType.WaitBeforeGet:
                        case Transaction.Command.RobotType.WaitBeforePut:
                        case Transaction.Command.RobotType.PutWithoutBack:
                            Node.CurrentPoint = Txn.Point;
                            //4Port use only


                            break;
                        case Transaction.Command.RobotType.GetWait:
                        case Transaction.Command.RobotType.PutWait:
                            Node.CurrentPoint = Txn.Point;

                            break;
                    }
                    break;
                case "ALIGNER":

                    switch (Txn.Method)
                    {
                        case Transaction.Command.AlignerType.WaferHold:
                            Node.IsWaferHold = true;
                            break;
                        case Transaction.Command.AlignerType.WaferRelease:
                            Node.IsWaferHold = false;
                            break;
                        case Transaction.Command.AlignerType.Retract:
                        case Transaction.Command.AlignerType.Home:


                            // Node.PutAvailable = true;
                            break;
                    }
                    break;
                case "LOADPORT":
                    switch (Txn.Method)
                    {
                        case Transaction.Command.LoadPortType.MappingLoad:

                            // Node.InterLock = false;
                            _UIReport.On_Node_State_Changed(Node, "Load Complete");
                            break;
                        case Transaction.Command.LoadPortType.Unload:
                            _UIReport.On_Node_State_Changed(Node, "UnLoad Complete");
                            break;
                        default:
                            // Node.InterLock = true;
                            break;
                    }
                    break;
            }
            //logger.Debug(JsonConvert.SerializeObject(Node));

        }

        /// <summary>
        /// 命令超時
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Txn"></param>
        public void On_Command_TimeOut(Node Node, Transaction Txn)
        {

            logger.Debug("Transaction TimeOut:" + Txn.CommandEncodeStr);
            Node.HasAlarm = true;
            _UIReport.On_Alarm_Happen(AlarmManagement.NewAlarm(new Node() { Vendor = "SYSTEM", Name = Node.Name }, "S0300177", Txn.TaskObj.MainTaskId));
            _UIReport.On_Command_TimeOut(Node, Txn);

            
        }
        /// <summary>
        /// 事件觸發
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Msg"></param>
        public void On_Event_Trigger(Node Node, CommandReturnMessage Msg)
        {
            try
            {
                logger.Debug("On_Event_Trigger");
                if (Msg.Command.Equals("INPUT"))
                {

                    string IO_Name = Msg.Value.Split(',')[0];
                    string IO_Value = Msg.Value.Split(',')[1];
                    Dictionary<string, string> param = new Dictionary<string, string>();

                    lock (IO_State)
                    {

                        if (IO_State.ContainsKey(IO_Name))
                        {
                            IO_State.Remove(IO_Name);
                            IO_State.Add(IO_Name, IO_Value);
                        }
                        else
                        {
                            IO_State.Add(IO_Name, IO_Value);
                        }
                        switch (IO_Name)
                        {
                            case "CTU-Present":
                                NodeManagement.Get("CTU").R_Presence = IO_Value.Equals("1") ? true : false;
                                break;
                            case "PTZ-Present":
                                NodeManagement.Get("PTZ").R_Presence = IO_Value.Equals("1") ? true : false;

                                break;
                        }
                    }
                }
                switch (Node.Type.ToUpper())
                {
                    case "LOADPORT":
                        switch (Msg.Command)
                        {
                            case "STS__":
                                if (Msg.Value.Equals("R-Present,1"))
                                {
                                    Node.Foup_Presence = true;
                                    Node.Foup_Placement = true;
                                }
                                else if (Msg.Value.Equals("R-Present,0"))
                                {
                                    Node.Foup_Presence = false;
                                    Node.Foup_Placement = false;
                                }
                                break;
                            case "MANSW":
                                //IO_State_Change(Node.Name, "Access_SW", true);
                                break;
                            case "MANOF":
                                //IO_State_Change(Node.Name, "Access_SW", false);
                                break;
                            case "SMTON":
                                //IO_State_Change(Node.Name, "Foup_Presence", false);
                                CarrierManagement.Remove(Node.Carrier);
                                Node.Foup_Presence = false;
                                Node.Foup_Placement = false;
                                //IO_State_Change(Node.Name, "Foup_Presence", true);
                                //IO_State_Change(Node.Name, "Foup_Placement", false);
                                Node.IsMapping = false;
                                Node.MappingResult = "";
                                //刪除所有帳
                                foreach (Job eachJob in JobManagement.GetByNode(Node.Name))
                                {
                                    JobManagement.Remove(eachJob);
                                }
                                Node.FoupID = "";
                                break;
                            case "PODOF":


                                break;
                            case "PODON":
                                CarrierManagement.Add().SetLocation(Node.Name);
                                Node.Foup_Presence = true;
                                Node.Foup_Placement = true;
                                //IO_State_Change(Node.Name, "Foup_Presence", false);
                                //IO_State_Change(Node.Name, "Foup_Placement", true);

                                break;
                            case "ABNST":
                                //IO_State_Change(Node.Name, "Foup_Placement", false);
                                break;
                            case "POD_ARRIVED":
                                Node.Foup_Presence = true;
                                Node.Foup_Placement = true;
                                break;

                            case "POD_REMOVED":
                                Node.Foup_Presence = false;
                                Node.Foup_Placement = false;
                                break;
                            case "INPUT":
                                string IO_Name = Msg.Value.Split(',')[0];
                                string IO_Value = Msg.Value.Split(',')[1];
                                _UIReport.On_DIO_Data_Chnaged(IO_Name, IO_Value.Equals("1") ? "TRUE" : "FALSE", "DIN");
                                break;
                        }
                        break;
                }
                if (Msg.Command.Equals("ERROR"))
                {
                   
                   
                    //_UIReport.On_Command_Error(Node, new Transaction(), Msg);
                    _UIReport.On_Node_State_Changed(Node, "ALARM");

                    _UIReport.On_Alarm_Happen( AlarmManagement.NewAlarm(Node, Msg.Value));
                }
                else
                {
                    _UIReport.On_Event_Trigger(Node, Msg);
                }
            }
            catch (Exception e)
            {
                logger.Error(Node.Controller + "-" + Node.AdrNo + "(On_Command_Finished)" + e.Message + "\n" + e.StackTrace);
            }

        }
        /// <summary>
        /// Controller狀態變更
        /// </summary>
        /// <param name="Device_ID"></param>
        /// <param name="Status"></param>
        public void On_Controller_State_Changed(string Device_ID, string Status)
        {
            var find = from node in NodeManagement.GetList()
                       where node.Controller.ToUpper().Equals(Device_ID.ToUpper())
                       select node;
            foreach (Node each in find)
            {
                switch (Status)
                {
                    case "Connected":
                        each.Connected = true;

                        break;
                    case "Disconnected":
                    case "Connection_Error":
                        each.Connected = false;
                        
                        break;
                }
                _UIReport.On_Node_Connection_Changed(each.Name, Status);
            }
            logger.Debug(Device_ID + " " + Status);

        }


        /// <summary>
        /// Node機況變更
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Status"></param>
        public void On_Node_State_Changed(Node Node, string Status)
        {

            //StateRecord.NodeStateUpdate(Node.Name, Node.State, Status);
            Node.State = Status;
            if (Node.HasAlarm)
            {
                Status = "ALARM";
            }
            _UIReport.On_Node_State_Changed(Node, Status);
        }
        /// <summary>
        /// 命令執行發生錯誤
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Txn"></param>
        /// <param name="Msg"></param>
        public void On_Command_Error(Node Node, Transaction Txn, CommandReturnMessage Msg)
        {
            //Node.InitialComplete = false;
            //Node.OrgSearchComplete = false;
            Node.HasAlarm = true;
 

            _UIReport.On_Alarm_Happen( AlarmManagement.NewAlarm(Node, Msg.Value,Txn.TaskObj.MainTaskId));
       
            _UIReport.On_Command_Error(Node, Txn, Msg);
            _UIReport.On_Node_State_Changed(Node, "ALARM");

           
            
        }

        public void On_Data_Chnaged(string Parameter, string Value, string Type)
        {
            switch (Parameter.ToUpper())
            {
                case "SAFETYRELAY":

                    if (Value.ToUpper().Equals("FALSE"))
                    {
                        foreach (Node n in NodeManagement.GetList())
                        {
                            n.InitialComplete = false;
                            n.OrgSearchComplete = false;
                        }
                    }
                    break;
            }
            _UIReport.On_DIO_Data_Chnaged(Parameter, Value, Type);
        }

        public void On_Connection_Error(string DIOName, string ErrorMsg)
        {

            _UIReport.On_Alarm_Happen( AlarmManagement.NewAlarm(new Node() { Name= DIOName ,Vendor="SANWA",Type="DIO"}, "00200001"));
            _UIReport.On_Connection_Error(DIOName, ErrorMsg);
        }

        public void On_Connection_Status_Report(string DIOName, string Status)
        {
            _UIReport.On_Connection_Status_Report(DIOName, Status);
        }

        public void On_DIO_Alarm_Happen(string DIOName, string ErrorCode)
        {
       
            _UIReport.On_Alarm_Happen( AlarmManagement.NewAlarm(new Node() { Name = DIOName, Vendor = "SANWA", Type = "DIO" }, ErrorCode));


        }



        public void On_Job_Position_Changed(Job Job)
        {

            _UIReport.On_Job_Location_Changed(Job);
        }


        public void On_Message_Log(string Type, string Message)
        {
            _UIReport.On_Message_Log(Type, Message);
        }

        
    }
}
