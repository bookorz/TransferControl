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
    public class RouteControl : AlarmMapping, ICommandReport, IDIOTriggerReport, IJobReport, ITaskFlowReport
    {
        //git upload test4
        private static readonly ILog logger = LogManager.GetLogger(typeof(RouteControl));

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
        public static RouteControl Instance;

        /// <summary>
        /// 建構子，傳入一個事件回報對象
        /// </summary>
        /// <param name="ReportTarget"></param>
        public RouteControl(IUserInterfaceReport ReportUI)
        {
            ArchiveLog.doWork(@"D:\log\", @"D:\log_backup\");//自動壓縮LOG檔案
            Instance = this;
            EqpState = "Idle";

            _UIReport = ReportUI;
            //初始化所有Controller
            DIO = new DIO(this);



            ControllerManagement.LoadConfig(this);

            //初始化所有Node
            NodeManagement.LoadConfig();



            //初始化命令參數轉換表
            CmdParamManagement.Initialize();
            //初始化Robot點位表
            PointManagement.LoadConfig();
            //初始化工作腳本
            TaskFlowManagement.SetReport(this);

        }


        /// <summary>
        /// 對所有Controller連線
        /// </summary>
        public void ConnectAll()
        {
            ControllerManagement.ConnectAll();
            DIO.Connect();


        }





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
                        AlarmManagement.Remove(Node.Name);
                        AlarmManagement.Remove("SYSTEM");
                        _UIReport.On_Node_State_Changed(Node, Node.State);
                        break;
                    case Transaction.Command.RobotType.Pause:
                        //Node.IsPause = true;
                        break;
                    case Transaction.Command.RobotType.Continue:
                        //Node.IsPause = false;
                        break;
                }



                Job TargetJob = null;
                if (Txn.TargetJobs.Count != 0)
                {
                    TargetJob = Txn.TargetJobs[0];
                    if (Node.Type.Equals("ROBOT") && !Txn.Position.Equals(""))//紀錄Robot最後位置
                    {
                        Node.CurrentPosition = Txn.Position;
                    }//分裝置別
                    switch (Node.Type)
                    {
                        case "SMARTTAG":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.SmartTagType.GetLCDData:
                                    Node.Carrier.CarrierID = Msg.Value;
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
                                    //紀錄快照
                                    if (Node.IsMapping)
                                    {
                                        Node.MappingDataSnapshot = "";
                                        for (int i = 1; i <= 25; i++)
                                        {
                                            Job slot = null;
                                            if (Node.JobList.TryGetValue(i.ToString(), out slot))
                                            {
                                                if (slot.MappingValue.Equals(""))
                                                {
                                                    Node.MappingDataSnapshot += "0";
                                                }
                                                else
                                                {
                                                    Node.MappingDataSnapshot += slot.MappingValue;
                                                }
                                            }
                                        }
                                    }

                                    //LoadPort卸載時打開安全鎖
                                    // Node.InterLock = true;
                                    //標記尚未Mapping
                                    Node.IsMapping = false;
                                    Node.MappingResult = "";
                                    //刪除所有帳
                                    foreach (Job eachJob in Node.JobList.Values)
                                    {
                                        JobManagement.Remove(eachJob.Job_Id);
                                    }
                                    Node.JobList.Clear();
                                    Node.ReserveList.Clear();
                                    JobManagement.ClearAssignJobByPort(Node.Name);
                                    Node.FoupID = "";

                                    break;
                                case Transaction.Command.LoadPortType.GetLED:
                                    MessageParser parser = new MessageParser(Node.Brand);
                                    foreach (KeyValuePair<string, string> each in parser.ParseMessage(Txn.Method, Msg.Value))
                                    {
                                        switch (each.Key)
                                        {
                                            case "LOAD":

                                                break;
                                            case "UNLOAD":

                                                break;
                                            case "OPACCESS":
                                                Node.OPACCESS = each.Value == "2"?true:false;
                                                break;
                                        }
                                    }
                                    break;
                                case Transaction.Command.LoadPortType.ReadStatus:
                                    parser = new MessageParser(Node.Brand);
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
                                        }
                                    }


                                    break;
                                case Transaction.Command.LoadPortType.GetMapping:
                                case Transaction.Command.LoadPortType.GetMappingDummy:
                                    //產生Mapping資料
                                    Node.LoadTime = DateTime.Now;
                                    string Mapping = Msg.Value;
                                    // string Mapping = "1111111111111111111111111";
                                    //if (!Mapping.Equals("0000000000000000000000000"))
                                    //{
                                    //    Mapping = "0000000110000000000000000";
                                    //}
                                    // WaferAssignUpdate.UpdateLoadPortMapping(Node.Name, Msg.Value);
                                    if (SystemConfig.Get().MappingDataCheck && Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_burnin)
                                    {
                                        if (!Node.MappingDataSnapshot.Equals(""))
                                        {
                                            if (!Node.MappingDataSnapshot.Equals(Mapping))
                                            {
                                                CommandReturnMessage rem = new CommandReturnMessage();
                                                rem.Value = "MAPCHKERR";
                                                _UIReport.On_Command_Error(Node, Txn, rem);
                                                Mapping = Node.MappingDataSnapshot;
                                            }
                                        }
                                    }

                                    if (SystemConfig.Get().FakeData)
                                    {
                                        if (Node.Name.Equals("LOADPORT01"))
                                        {
                                            Mapping = "1111111111111000000000000";
                                            //Mapping = "1000000000000000000000000";

                                            //Mapping = SystemConfig.Get().MappingData;
                                        }
                                        if (Node.Name.Equals("LOADPORT02"))
                                        {
                                            //Mapping = "1111111110000000000000000";
                                            Mapping = "0000000000000000000000000";
                                            //Mapping = SystemConfig.Get().MappingData;
                                        }
                                        Msg.Value = Mapping;
                                    }

                                    //if (Node.Name.Equals("LOADPORT04"))
                                    //{
                                    //    Mapping = "1111111111111111111100000";
                                    //    //Mapping = SystemConfig.Get().MappingData;
                                    //}

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
                                        Job wafer = RouteControl.CreateJob();
                                        wafer.Slot = (i + 1).ToString();
                                        wafer.FromPort = Node.Name;
                                        wafer.FromPortSlot = wafer.Slot;
                                        wafer.Position = Node.Name;
                                        wafer.AlignerFlag = false;
                                        wafer.MappingValue = Mapping[i].ToString();
                                        string Slot = (i + 1).ToString("00");


                                        switch (Mapping[i])
                                        {
                                            case '0':
                                                wafer.Job_Id = "No wafer";
                                                wafer.Host_Job_Id = wafer.Job_Id;
                                                wafer.MapFlag = false;
                                                wafer.ErrPosition = false;
                                                //MappingData.Add(wafer);
                                                break;
                                            case '1':
                                                while (true)
                                                {
                                                    wafer.Job_Id = "Wafer" + currentIdx.ToString("000");
                                                    wafer.Host_Job_Id = wafer.Job_Id;
                                                    wafer.MapFlag = true;
                                                    wafer.ErrPosition = false;
                                                    if (JobManagement.Add(wafer.Job_Id, wafer))
                                                    {

                                                        //MappingData.Add(wafer);
                                                        break;
                                                    }
                                                    currentIdx++;
                                                }

                                                break;
                                            case '2':
                                            case 'E':
                                                wafer.Job_Id = "Crossed";
                                                wafer.Host_Job_Id = wafer.Job_Id;
                                                wafer.MapFlag = true;
                                                wafer.ErrPosition = true;
                                                //MappingData.Add(wafer);
                                                Node.IsMapping = false;
                                                break;
                                            default:
                                            case '?':
                                                wafer.Job_Id = "Undefined";
                                                wafer.Host_Job_Id = wafer.Job_Id;
                                                wafer.MapFlag = true;
                                                wafer.ErrPosition = true;
                                                //MappingData.Add(wafer);
                                                Node.IsMapping = false;
                                                break;
                                            case 'W':
                                                wafer.Job_Id = "Double";
                                                wafer.Host_Job_Id = wafer.Job_Id;
                                                wafer.MapFlag = true;
                                                wafer.ErrPosition = true;
                                                //MappingData.Add(wafer);
                                                Node.IsMapping = false;
                                                break;
                                        }
                                        if (!Node.AddJob(wafer.Slot, wafer))
                                        {
                                            Job org = Node.GetJob(wafer.Slot);
                                            JobManagement.Remove(org.Job_Id);
                                            Node.RemoveJob(wafer.Slot);
                                            Node.AddJob(wafer.Slot, wafer);
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
                                    if ((Node.Brand.Equals("ATEL_NEW") || Node.Brand.Equals("SANWA")))
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
                                    }
                                    else
                                    {
                                        Node.HasAlarm = true;
                                    }
                                    break;
                                case Transaction.Command.RobotType.GetStatus:
                                    MessageParser parser = new MessageParser(Node.Brand);
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
                                    parser = new MessageParser(Node.Brand);
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
                                        }
                                    }
                                    break;
                                case Transaction.Command.RobotType.GetRIO:
                                    parser = new MessageParser(Node.Brand);
                                    Dictionary<string, string> RioResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                    foreach (KeyValuePair<string, string> each in RioResult)
                                    {
                                        Node.IO.Remove(each.Key);
                                        Node.IO.Add(each.Key, each.Value);
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
                                    parser = new MessageParser(Node.Brand);
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

                                    //string Mapping = SystemConfig.Get().MappingData;
                                    //WaferAssignUpdate.UpdateLoadPortMapping(Node.Name, Msg.Value);
                                    Node port = NodeManagement.Get(Node.CurrentPosition);
                                    if (SystemConfig.Get().MappingDataCheck && Recipe.Get(SystemConfig.Get().CurrentRecipe).is_use_burnin)
                                    {
                                        if (!port.MappingDataSnapshot.Equals(Mapping) && !port.MappingDataSnapshot.Equals(""))
                                        {
                                            CommandReturnMessage rem = new CommandReturnMessage();
                                            rem.Value = "MAPCHKERR";
                                            _UIReport.On_Command_Error(Node, Txn, rem);
                                            Mapping = port.MappingDataSnapshot;
                                        }
                                    }
                                    if (SystemConfig.Get().FakeData)
                                    {
                                        if (port.Name.Equals("LOADPORT01"))
                                        {
                                            Mapping = "1110000000000000000000000";
                                            //Mapping = "1000000000000000000000000";

                                            //Mapping = SystemConfig.Get().MappingData;
                                        }
                                        if (port.Name.Equals("LOADPORT02"))
                                        {
                                            Mapping = "1111111110000000000000000";
                                            //Mapping = "0000000000000000000000000";
                                            //Mapping = SystemConfig.Get().MappingData;
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
                                        Job wafer = RouteControl.CreateJob();
                                        wafer.Slot = (i + 1).ToString();
                                        wafer.FromPort = port.Name;
                                        wafer.FromPortSlot = wafer.Slot;
                                        wafer.Position = port.Name;
                                        wafer.AlignerFlag = false;
                                        wafer.MappingValue = Mapping[i].ToString();
                                        string Slot = (i + 1).ToString("00");


                                        switch (Mapping[i])
                                        {
                                            case '0':
                                                wafer.Job_Id = "No wafer";
                                                wafer.Host_Job_Id = wafer.Job_Id;
                                                wafer.MapFlag = false;
                                                wafer.ErrPosition = false;
                                                //MappingData.Add(wafer);
                                                break;
                                            case '1':
                                                while (true)
                                                {
                                                    wafer.Job_Id = "Wafer" + currentIdx.ToString("000");
                                                    wafer.Host_Job_Id = wafer.Job_Id;
                                                    wafer.MapFlag = true;
                                                    wafer.ErrPosition = false;
                                                    if (JobManagement.Add(wafer.Job_Id, wafer))
                                                    {

                                                        //MappingData.Add(wafer);
                                                        break;
                                                    }
                                                    currentIdx++;
                                                }

                                                break;
                                            case '2':
                                            case 'E':
                                                wafer.Job_Id = "Crossed";
                                                wafer.Host_Job_Id = wafer.Job_Id;
                                                wafer.MapFlag = true;
                                                wafer.ErrPosition = true;
                                                //MappingData.Add(wafer);
                                                port.IsMapping = false;
                                                break;
                                            default:
                                            case '?':
                                                wafer.Job_Id = "Undefined";
                                                wafer.Host_Job_Id = wafer.Job_Id;
                                                wafer.MapFlag = true;
                                                wafer.ErrPosition = true;
                                                //MappingData.Add(wafer);
                                                port.IsMapping = false;
                                                break;
                                            case 'W':
                                                wafer.Job_Id = "Double";
                                                wafer.Host_Job_Id = wafer.Job_Id;
                                                wafer.MapFlag = true;
                                                wafer.ErrPosition = true;
                                                //MappingData.Add(wafer);
                                                port.IsMapping = false;
                                                break;
                                        }
                                        if (!port.AddJob(wafer.Slot, wafer))
                                        {
                                            Job org = port.GetJob(wafer.Slot);
                                            JobManagement.Remove(org.Job_Id);
                                            port.RemoveJob(wafer.Slot);
                                            port.AddJob(wafer.Slot, wafer);
                                        }

                                    }
                                    if (!port.IsMapping)
                                    {
                                        CommandReturnMessage rem = new CommandReturnMessage();
                                        rem.Value = "MAPERR";
                                        _UIReport.On_Command_Error(Node, Txn, rem);
                                    }
                                    //Node.MappingResult = Mapping;
                                    //Node Port = NodeManagement.Get(Node.CurrentPosition);
                                    //if (Port != null)
                                    //{
                                    //    int currentIdx = 1;
                                    //    for (int i = 0; i < Mapping.Length; i++)
                                    //    {
                                    //        Job wafer = RouteControl.CreateJob();
                                    //        wafer.Slot = (i + 1).ToString();
                                    //        wafer.FromPort = Port.Name;
                                    //        wafer.FromPortSlot = wafer.Slot;
                                    //        wafer.Position = Port.Name;
                                    //        wafer.AlignerFlag = false;
                                    //        string Slot = (i + 1).ToString("00");
                                    //        switch (Mapping[i])
                                    //        {
                                    //            case '0':
                                    //                wafer.Job_Id = "No wafer";
                                    //                wafer.Host_Job_Id = wafer.Job_Id;
                                    //                //MappingData.Add(wafer);
                                    //                break;
                                    //            case '1':
                                    //                while (true)
                                    //                {
                                    //                    wafer.Job_Id = "Wafer" + currentIdx.ToString("00");
                                    //                    wafer.Host_Job_Id = wafer.Job_Id;
                                    //                    wafer.MapFlag = true;
                                    //                    if (JobManagement.Add(wafer.Job_Id, wafer))
                                    //                    {

                                    //                        //MappingData.Add(wafer);
                                    //                        break;
                                    //                    }
                                    //                    currentIdx++;
                                    //                }

                                    //                break;
                                    //            case '2':
                                    //            case 'E':
                                    //                wafer.Job_Id = "Crossed";
                                    //                wafer.Host_Job_Id = wafer.Job_Id;
                                    //                wafer.MapFlag = true;
                                    //                //MappingData.Add(wafer);
                                    //                break;
                                    //            case '?':
                                    //                wafer.Job_Id = "Undefined";
                                    //                wafer.Host_Job_Id = wafer.Job_Id;
                                    //                wafer.MapFlag = true;
                                    //                //MappingData.Add(wafer);
                                    //                break;
                                    //            case 'W':
                                    //                wafer.Job_Id = "Double";
                                    //                wafer.Host_Job_Id = wafer.Job_Id;
                                    //                wafer.MapFlag = true;
                                    //                //MappingData.Add(wafer);
                                    //                break;
                                    //        }
                                    //        if (!Port.AddJob(wafer.Slot, wafer))
                                    //        {
                                    //            Job org = Port.GetJob(wafer.Slot);
                                    //            JobManagement.Remove(org.Job_Id);
                                    //            Port.RemoveJob(wafer.Slot);
                                    //            Port.AddJob(wafer.Slot, wafer);
                                    //        }

                                    //    }
                                    //    Port.IsMapping = true;
                                    //}
                                    break;

                            }

                            break;
                        case "ALIGNER":
                        case "OCR":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.RobotType.GetSpeed:
                                    if (Msg.Value.Equals("0") && (Node.Brand.Equals("ATEL_NEW") || Node.Brand.Equals("SANWA")))
                                    {
                                        Msg.Value = "100";
                                    }

                                    Node.Speed = Msg.Value;
                                    break;
                                case Transaction.Command.RobotType.GetRIO:
                                    MessageParser parser = new MessageParser(Node.Brand);
                                    Dictionary<string, string> RioResult = parser.ParseMessage(Txn.Method, Msg.Value);
                                    foreach (KeyValuePair<string, string> each in RioResult)
                                    {
                                        Node.IO.Remove(each.Key);
                                        Node.IO.Add(each.Key, each.Value);
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
                                    parser = new MessageParser(Node.Brand);
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
                                    MessageParser parser = new MessageParser(Node.Brand);
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

                    TaskFlowManagement.Next(Node, Txn, "Excuted");
                }
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
                Job TargetJob = null;
                if (Txn.TargetJobs.Count != 0)
                {
                    TargetJob = Txn.TargetJobs[0];
                    logger.Debug("On_Command_Finished:" + Txn.Method + ":" + Txn.Method);
                    switch (Node.Type)
                    {
                        case "SMARTTAG":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.SmartTagType.GetLCDData:

                                    Node.Carrier.CarrierID = Msg.Value;
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
                                if (Txn.TargetJobs.Count != 0)
                                {
                                    Txn.TargetJobs[0].ProcessFlag = true;
                                }
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
                                    Node.OrgSearchComplete = true;
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
                                    MessageParser parser = new MessageParser(Node.Brand);
                                    Node.Status = parser.ParseMessage(Txn.Method, Msg.Value);
                                    Node Aligner = NodeManagement.Get(Node.Associated_Node);
                                    if (Aligner == null)
                                    {
                                        logger.Error("Aligner is not found, Associated_Node setting incorrect.");
                                        break;
                                    }
                                    Job j = Aligner.JobList["1"];
                                    foreach (KeyValuePair<string, string> each in Node.Status)
                                    {
                                        switch (each.Key)
                                        {
                                            case "WAFER_ID":
                                                if (!j.OCRPass && !j.OCR_M12_Pass && !j.OCR_T7_Pass)
                                                {
                                                    j.Host_Job_Id = each.Value;
                                                }
                                                switch (Txn.Method)
                                                {
                                                    case Transaction.Command.OCRType.Read:
                                                        j.OCRResult = each.Value;
                                                        break;
                                                    case Transaction.Command.OCRType.ReadM12:
                                                        j.OCR_M12_Result = each.Value;
                                                        break;
                                                    case Transaction.Command.OCRType.ReadT7:
                                                        j.OCR_T7_Result = each.Value;
                                                        break;
                                                }
                                                break;
                                            case "SCORE":
                                                switch (Txn.Method)
                                                {
                                                    case Transaction.Command.OCRType.Read:
                                                        j.OCRScore = each.Value;
                                                        break;
                                                    case Transaction.Command.OCRType.ReadM12:
                                                        j.OCR_M12_Score = each.Value;
                                                        break;
                                                    case Transaction.Command.OCRType.ReadT7:
                                                        j.OCR_T7_Score = each.Value;
                                                        break;
                                                }
                                                break;
                                            case "PASS":
                                                switch (Txn.Method)
                                                {
                                                    case Transaction.Command.OCRType.Read:
                                                        j.OCRPass = each.Value.Equals("1") ? true : false;
                                                        break;
                                                    case Transaction.Command.OCRType.ReadM12:
                                                        j.OCR_M12_Pass = each.Value.Equals("1") ? true : false;
                                                        break;
                                                    case Transaction.Command.OCRType.ReadT7:
                                                        j.OCR_T7_Pass = each.Value.Equals("1") ? true : false;
                                                        break;
                                                }
                                                break;
                                        }
                                    }
                                    //if (Txn.TargetJobs.Count != 0)
                                    //{
                                    //    string[] OCRResult;

                                    //    OCRResult = Msg.Value.Replace("[", "").Replace("]", "").Split(',');

                                    //    //Txn.TargetJobs[0].Host_Job_Id = OCRResult[0];

                                    //    NodeManagement.Get(Node.Associated_Node).JobList.First().Value.Host_Job_Id = OCRResult[0];

                                    //    switch (Txn.Method)
                                    //    {
                                    //        case Transaction.Command.OCRType.Read:
                                    //            NodeManagement.Get(Node.Associated_Node).JobList.First().Value.OCRResult = OCRResult[0];
                                    //            break;
                                    //        case Transaction.Command.OCRType.ReadM12:
                                    //            NodeManagement.Get(Node.Associated_Node).JobList.First().Value.OCR_M12_Result = OCRResult[0];
                                    //            break;
                                    //        case Transaction.Command.OCRType.ReadT7:
                                    //            NodeManagement.Get(Node.Associated_Node).JobList.First().Value.OCR_T7_Result = OCRResult[0];
                                    //            break;
                                    //    }
                                    //    switch (Node.Brand)
                                    //    {
                                    //        case "HST":
                                    //            if (OCRResult.Length >= 3)
                                    //            {


                                    //                switch (Txn.Method)
                                    //                {
                                    //                    case Transaction.Command.OCRType.Read:
                                    //                        NodeManagement.Get(Node.Associated_Node).JobList.First().Value.OCRScore = OCRResult[2];
                                    //                        break;
                                    //                    case Transaction.Command.OCRType.ReadM12:
                                    //                        NodeManagement.Get(Node.Associated_Node).JobList.First().Value.OCR_M12_Score = OCRResult[2];
                                    //                        break;
                                    //                    case Transaction.Command.OCRType.ReadT7:
                                    //                        NodeManagement.Get(Node.Associated_Node).JobList.First().Value.OCR_T7_Score = OCRResult[2];
                                    //                        break;
                                    //                }
                                    //            }
                                    //            else
                                    //            {

                                    //                switch (Txn.Method)
                                    //                {
                                    //                    case Transaction.Command.OCRType.Read:
                                    //                        NodeManagement.Get(Node.Associated_Node).JobList.First().Value.OCRScore = "0";
                                    //                        break;
                                    //                    case Transaction.Command.OCRType.ReadM12:
                                    //                        NodeManagement.Get(Node.Associated_Node).JobList.First().Value.OCR_M12_Score = "0";
                                    //                        break;
                                    //                    case Transaction.Command.OCRType.ReadT7:
                                    //                        NodeManagement.Get(Node.Associated_Node).JobList.First().Value.OCR_T7_Score = "0";
                                    //                        break;
                                    //                }
                                    //            }
                                    //            if (OCRResult[0].IndexOf("*") == -1)
                                    //            {
                                    //                Node.OCRSuccess = true;
                                    //            }
                                    //            else
                                    //            {
                                    //                Node.OCRSuccess = false;
                                    //            }
                                    //            break;
                                    //        case "COGNEX":
                                    //            if (OCRResult.Length >= 3)
                                    //            {
                                    //                Txn.TargetJobs[0].OCRScore = OCRResult[1];
                                    //                if (!OCRResult[2].Equals("0.000"))
                                    //                {
                                    //                    Node.OCRSuccess = true;
                                    //                }
                                    //                else
                                    //                {
                                    //                    Node.OCRSuccess = false;
                                    //                }
                                    //            }
                                    //            else
                                    //            {
                                    //                Txn.TargetJobs[0].OCRScore = "0";
                                    //            }

                                    //            break;
                                    //    }

                                    //    _UIReport.On_Job_Location_Changed(Txn.TargetJobs[0]);
                                    //}
                                    break;
                                case Transaction.Command.OCRType.ReadConfig:


                                    if (Txn.TargetJobs.Count != 0)
                                    {
                                        switch (Node.Brand)
                                        {
                                            case "HST":
                                                OCRInfo result = new OCRInfo(Msg.Value);

                                                Txn.TargetJobs[0].OcrCodeList.Add(result);
                                                Txn.TargetJobs[0].OcrCodeList.Sort((x, y) => { return x.Score.CompareTo(y.Score); });
                                                var find = from Ocr in Txn.TargetJobs[0].OcrCodeList
                                                           where Ocr.Passed.Equals("1")
                                                           select Ocr;
                                                if (find.Count() != 0)
                                                {
                                                    Txn.TargetJobs[0].Host_Job_Id = find.First().Result;
                                                }

                                                break;
                                        }
                                    }

                                    _UIReport.On_Job_Location_Changed(Txn.TargetJobs[0]);
                                    break;
                            }

                            break;
                        case "LOADPORT":
                            UpdateNodeStatus(Node, Txn);
                            switch (Txn.Method)
                            {
                                case Transaction.Command.LoadPortType.MappingLoad:
                                case Transaction.Command.LoadPortType.Load:
                                    Node.IsLoad = true;
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
                                default:
                                    Node.IsLoad = false;
                                    break;
                            }
                            break;
                    }

                    _UIReport.On_Command_Finished(Node, Txn, Msg);


                    TaskFlowManagement.Next(Node, Txn, "Finished");

                }
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
            TaskFlowManagement.CurrentProcessTask Task = TaskFlowManagement.TaskRemove(Txn.TaskId);
            if (Task == null)
            {
                Task = new TaskFlowManagement.CurrentProcessTask();
            }
            Task.HasError = true;
            Task.Finished = true;
            if (!Node.IsPause)
            {
                logger.Debug("Transaction TimeOut:" + Txn.CommandEncodeStr);
                Node.HasAlarm = true;
                _UIReport.On_TaskJob_Aborted(Task, Node.Name, "On_Command_Error", "TimeOut");
                _UIReport.On_Command_TimeOut(Node, Txn);
            }
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
                switch (Node.Type.ToUpper())
                {
                    case "LOADPORT":
                        switch (Msg.Command)
                        {
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
                                foreach (Job eachJob in Node.JobList.Values)
                                {
                                    JobManagement.Remove(eachJob.Job_Id);
                                }
                                Node.JobList.Clear();
                                Node.ReserveList.Clear();
                                JobManagement.ClearAssignJobByPort(Node.Name);
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
                                //IO_State_Change(Node.Name, "Foup_Presence", false);
                                //IO_State_Change(Node.Name, "Foup_Placement", true);
                                break;

                            case "POD_REMOVED":
                                //IO_State_Change(Node.Name, "Foup_Presence", true);
                                //IO_State_Change(Node.Name, "Foup_Placement", false);
                                break;
                        }
                        break;
                }
                if (Msg.Command.Equals("ERROR"))
                {
                    Node.HasAlarm = true;
                    Node.InitialComplete = false;
                    Node.OrgSearchComplete = false;
                    _UIReport.On_Command_Error(Node, new Transaction(), Msg);
                    _UIReport.On_Node_State_Changed(Node, "ALARM");

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
                       where node.Controller.Equals(Device_ID)
                       select node;
            foreach (Node each in find)
            {
                switch (Status)
                {
                    case "Connected":
                        each.Connected = true;
                        if (each.PoolTask != null)
                        {
                            if (!each.PoolTask.Trim().Equals(""))
                            {
                                each.PoolStart(each.PoolTask);
                            }
                        }
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
            TaskFlowManagement.CurrentProcessTask Task = TaskFlowManagement.TaskRemove(Txn.TaskId);
            if (Task != null)
            {
                Task.HasError = true;
                Task.Finished = true;
                if (Msg.Value.Equals(""))
                {
                    Msg.Value = Msg.Command;
                }
            }
            _UIReport.On_TaskJob_Aborted(Task, Node.Name, "On_Command_Error", Msg.Value);
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
                        foreach(Node n in NodeManagement.GetList())
                        {
                            n.InitialComplete = false;
                            n.OrgSearchComplete = false;
                        }
                    }
                    break;
            }
            _UIReport.On_Data_Chnaged(Parameter, Value, Type);
        }

        public void On_Connection_Error(string DIOName, string ErrorMsg)
        {
            _UIReport.On_Connection_Error(DIOName, ErrorMsg);
        }

        public void On_Connection_Status_Report(string DIOName, string Status)
        {
            _UIReport.On_Connection_Status_Report(DIOName, Status);
        }

        public void On_Alarm_Happen(string DIOName, string ErrorCode)
        {
            //if (ErrorCode.Equals("00100007"))
            //{
            //    ControllerManagement.ClearTransactionList();
            //    TaskJob.Clear();
            //}
            _UIReport.On_Alarm_Happen(DIOName, ErrorCode);
        }



        public void On_Job_Position_Changed(Job Job)
        {

            _UIReport.On_Job_Location_Changed(Job);
        }

        public static Job CreateJob()
        {
            return new Job(RouteControl.Instance);
        }



        public void On_Task_Abort(TaskFlowManagement.CurrentProcessTask Task, string Location, string ReportType, string Message)
        {
            //TaskJob.Remove(Id);
            logger.Debug("On_Task_Abort");
            _UIReport.On_TaskJob_Aborted(Task, Location, ReportType, Message);
        }

        public void On_Task_Finished(TaskFlowManagement.CurrentProcessTask Task)
        {
            logger.Debug("On_Task_Finished");
            _UIReport.On_TaskJob_Finished(Task);
        }

        public void On_Task_Ack(TaskFlowManagement.CurrentProcessTask Task)
        {
            _UIReport.On_TaskJob_Ack(Task);
        }
    }
}
