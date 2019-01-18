using log4net;
using Newtonsoft.Json;
using SANWA.Utility;
using TransferControl.Config;
using TransferControl.Controller;
using TransferControl.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransferControl.Parser;
using SANWA.Utility.Config;
using DIOControl;

namespace TransferControl.Engine
{
    public class RouteControl : AlarmMapping, Controller.ICommandReport, IDIOTriggerReport, IJobReport, ITaskJobReport
    {
         //git upload test4
        private static readonly ILog logger = LogManager.GetLogger(typeof(RouteControl));
        
        public bool IsInitial = false;
        //DateTime StartTime = new DateTime();
        IUserInterfaceReport _UIReport;
        IHostInterfaceReport _HostReport;
        //int LapsedWfCount = 0;
        //int LapsedLotCount = 0;
        public string EqpState = "";
        public int NotchDirect = 270;

        public int SpinWaitTimeOut = 99999000;
        public DIO DIO;
        public TaskJobManagment TaskJob;
        public static RouteControl Instance;

        /// <summary>
        /// 建構子，傳入一個事件回報對象
        /// </summary>
        /// <param name="ReportTarget"></param>
        public RouteControl(IUserInterfaceReport ReportUI, IHostInterfaceReport ReportHost = null)
        {
            Instance = this;
            EqpState = "Idle";
           
            _UIReport = ReportUI;
            _HostReport = ReportHost;
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
            TaskJob = new TaskJobManagment(this);

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
        public void On_Command_Excuted(Node Node, Transaction Txn, ReturnMessage Msg)
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
                                    Node.InterLock = true;
                                    //標記尚未Mapping
                                    Node.IsMapping = false;
                                    //刪除所有帳
                                    foreach (Job eachJob in Node.JobList.Values)
                                    {
                                        JobManagement.Remove(eachJob.Job_Id);
                                    }
                                    Node.JobList.Clear();
                                    Node.ReserveList.Clear();
                                    JobManagement.ClearAssignJobByPort(Node.Name);


                                    break;

                                case Transaction.Command.LoadPortType.ReadStatus:
                                    MessageParser parser = new MessageParser(Node.Brand);
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
                                            case "Cassette Presence":
                                                if (each.Value.Equals("None"))
                                                {
                                                    Node.Foup_Placement = false;
                                                    Node.Foup_Presence = true;
                                                }
                                                else if (each.Value.Equals("Normal position"))
                                                {
                                                    Node.Foup_Placement = true;
                                                    Node.Foup_Presence = false;
                                                }
                                                else if (each.Value.Equals("Error load"))
                                                {
                                                    Node.Foup_Placement = false;
                                                    Node.Foup_Presence = false;
                                                }
                                                break;
                                        }
                                    }
                                    if (_HostReport != null)
                                    {
                                        _HostReport.On_Event_Trigger("SIGSTAT", "PORT", Node.Name, "ALL");
                                    }

                                    break;
                                case Transaction.Command.LoadPortType.GetMapping:
                                    //產生Mapping資料
                                    string Mapping = Msg.Value;
                                    //string Mapping = "1111111111111111111111111";
                                    //if (!Mapping.Equals("0000000000000000000000000"))
                                    //{
                                    //    Mapping = "0000000110000000000000000";
                                    //}
                                    //WaferAssignUpdate.UpdateLoadPortMapping(Node.Name, Msg.Value);
                                    //if (Node.Name.Equals("LOADPORT02"))
                                    //{
                                    //    //Mapping = "1111111111111111111111111";
                                    //    Mapping = SystemConfig.Get().MappingData;
                                    //}
                                    
                                    Node.MappingResult = Mapping;
                                    
                                    Node.IsMapping = true;
                                    if (_HostReport != null)
                                    {
                                        _HostReport.On_Event_Trigger("MAPDT", "", Node.Name, Msg.Value);
                                    }
                                    int currentIdx = 1;
                                    for (int i = 0; i < Mapping.Length; i++)
                                    {
                                        Job wafer = RouteControl.CreateJob();
                                        wafer.Slot = (i + 1).ToString();
                                        wafer.FromPort = Node.Name;
                                        wafer.FromPortSlot = wafer.Slot;
                                        wafer.Position = Node.Name;
                                        wafer.AlignerFlag = false;
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
                                    
                                    break;

                            }


                            break;
                        case "ROBOT":
                            switch (Txn.Method)
                            {
                                case Transaction.Command.RobotType.GetError:
                                    if (Msg.Value.Equals("00000000"))
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
                                case Transaction.Command.RobotType.GetMapping:
                                    //產生Mapping資料
                                    string Mapping = Msg.Value;
                                    //string Mapping = SystemConfig.Get().MappingData;
                                    //WaferAssignUpdate.UpdateLoadPortMapping(Node.Name, Msg.Value);
                                    Node.MappingResult = Mapping;
                                    Node Port = NodeManagement.Get(Node.CurrentPosition);
                                    if (Port != null)
                                    {
                                        int currentIdx = 1;
                                        for (int i = 0; i < Mapping.Length; i++)
                                        {
                                            Job wafer = RouteControl.CreateJob();
                                            wafer.Slot = (i + 1).ToString();
                                            wafer.FromPort = Port.Name;
                                            wafer.FromPortSlot = wafer.Slot;
                                            wafer.Position = Port.Name;
                                            wafer.AlignerFlag = false;
                                            string Slot = (i + 1).ToString("00");
                                            switch (Mapping[i])
                                            {
                                                case '0':
                                                    wafer.Job_Id = "No wafer";
                                                    wafer.Host_Job_Id = wafer.Job_Id;
                                                    //MappingData.Add(wafer);
                                                    break;
                                                case '1':
                                                    while (true)
                                                    {
                                                        wafer.Job_Id = "Wafer" + currentIdx.ToString("00");
                                                        wafer.Host_Job_Id = wafer.Job_Id;
                                                        wafer.MapFlag = true;
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
                                                    //MappingData.Add(wafer);
                                                    break;
                                                case '?':
                                                    wafer.Job_Id = "Undefined";
                                                    wafer.Host_Job_Id = wafer.Job_Id;
                                                    wafer.MapFlag = true;
                                                    //MappingData.Add(wafer);
                                                    break;
                                                case 'W':
                                                    wafer.Job_Id = "Double";
                                                    wafer.Host_Job_Id = wafer.Job_Id;
                                                    wafer.MapFlag = true;
                                                    //MappingData.Add(wafer);
                                                    break;
                                            }
                                            if (!Port.AddJob(wafer.Slot, wafer))
                                            {
                                                Job org = Port.GetJob(wafer.Slot);
                                                JobManagement.Remove(org.Job_Id);
                                                Port.RemoveJob(wafer.Slot);
                                                Port.AddJob(wafer.Slot, wafer);
                                            }

                                        }
                                        Port.IsMapping = true;
                                    }
                                    break;
                            }
                            
                            break;
                        case "ALIGNER":
                        case "OCR":
                            switch (Txn.Method)
                            {
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
                                            case "R-Hold Status":
                                                Node.R_Hold_Status = each.Value;
                                                break;
                                            case "L-Hold Status":
                                                Node.L_Hold_Status = each.Value;
                                                break;
                                        }
                                    }
                                    break;
                            }
                            
                            break;

                    }

                    _UIReport.On_Command_Excuted(Node, Txn, Msg);

                    TaskJobManagment.CurrentProceedTask Task;
                    if (TaskJob.IsTask(Txn.FormName,out Task))//如果是帶TaskID才檢查
                    {
                        string ErrorMessage = "";
                        string Report = "";
                        string Location = "";
                        if (!TaskJob.CheckTask(Txn.FormName, Node.Name, "CMD", Txn.Method, "Excuted", out ErrorMessage, out Report,out Location))
                        {//還沒做完
                            if (Report.Equals("ACK"))
                            {
                                if (_HostReport != null)
                                {
                                    _HostReport.On_TaskJob_Ack(Task);
                                }
                            }
                            if (!ErrorMessage.Equals(""))
                            {//做完但沒通過檢查
                                TaskJob.Remove(Txn.FormName);
                                if (_HostReport != null)
                                {

                                    _HostReport.On_TaskJob_Aborted(Task, Location, Report, ErrorMessage);
                                }
                                _UIReport.On_TaskJob_Aborted(Task, Node.Name, Report, ErrorMessage);
                            }
                            //檢查到不是Task，不做事
                        }
                        else
                        {//做完且通過檢查，開始進行下一個Task
                            
                            if (!TaskJob.Excute(Txn.FormName, out ErrorMessage,out Task))
                            {//如果沒有可以執行的Task，回報完成

                                if (ErrorMessage.Equals(""))
                                {
                                    if (_HostReport != null)
                                    {
                                        _HostReport.On_TaskJob_Finished(Task);
                                    }
                                    _UIReport.On_TaskJob_Finished(Task);
                                }
                                else
                                {
                                    if (_HostReport != null)
                                    {
                                        _HostReport.On_TaskJob_Aborted(Task, Location, Report, ErrorMessage);
                                    }
                                    _UIReport.On_TaskJob_Aborted(Task, Node.Name, Report, ErrorMessage);
                                }
                            }
                            else
                            {
                                if (Report.Equals("ACK"))
                                {
                                    if (_HostReport != null)
                                    {
                                        _HostReport.On_TaskJob_Ack(Task);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(Node.Controller + "-" + Node.AdrNo + "(On_Command_Excuted)" + e.Message + "\n" + e.StackTrace);
            }
        }

        public void On_Command_Finished(Node Node, Transaction Txn, ReturnMessage Msg)
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

                                    Node.FoupID = Msg.Value;
                                    break;
                            }
                            break;
                        case "ROBOT":
                            Node.InterLock = false;
                            Node.Busy = false;
                            switch (Txn.Method)
                            {
                                case Transaction.Command.RobotType.Home:
                                case Transaction.Command.RobotType.OrginSearch:
                                    Node.State = "Ready";
                                    break;
                            }

                            

                            break;
                        case "ALIGNER":
                        case "OCR":
                            UpdateNodeStatus(Node, Txn);
                            if (Node.Type.Equals("OCR"))
                            {
                                //Update Wafer ID by OCR result
                                if (Txn.Method.Equals(Transaction.Command.OCRType.Read))
                                {
                                    if (Txn.TargetJobs.Count != 0)
                                    {
                                        string[] OCRResult;

                                        OCRResult = Msg.Value.Replace("[", "").Replace("]", "").Split(',');

                                        Txn.TargetJobs[0].Host_Job_Id = OCRResult[0];


                                        switch (Node.Brand)
                                        {
                                            case "HST":
                                                if (OCRResult.Length >= 3)
                                                {
                                                    Txn.TargetJobs[0].OCRScore = OCRResult[2];
                                                }
                                                else
                                                {
                                                    Txn.TargetJobs[0].OCRScore = "0";
                                                }

                                                break;
                                            case "COGNEX":
                                                if (OCRResult.Length >= 2)
                                                {
                                                    Txn.TargetJobs[0].OCRScore = OCRResult[1];
                                                }
                                                else
                                                {
                                                    Txn.TargetJobs[0].OCRScore = "0";
                                                }
                                                break;
                                        }

                                        _UIReport.On_Job_Location_Changed(Txn.TargetJobs[0]);
                                    }
                                }
                                else if (Txn.Method.Equals(Transaction.Command.OCRType.ReadConfig))
                                {

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
                                }
                            }
                            else if (Node.Type.ToUpper().Equals("ALIGNER"))
                            {
                                if (Txn.Method.Equals(Transaction.Command.AlignerType.Align) || Txn.Method.Equals(Transaction.Command.AlignerType.AlignOption))
                                {
                                    if (Txn.TargetJobs.Count != 0)
                                    {
                                        Txn.TargetJobs[0].ProcessFlag = true;
                                    }
                                }
                            }
                            
                            break;
                        case "LOADPORT":
                            UpdateNodeStatus(Node, Txn);
                            switch (Txn.Method)
                            {
                                case Transaction.Command.LoadPortType.MappingLoad:
                                    IO_State_Change(Node.Name, "Foup_Lock", true);
                                    break;
                                case Transaction.Command.LoadPortType.Unload:
                                case Transaction.Command.LoadPortType.MappingUnload:
                                case Transaction.Command.LoadPortType.UnDock:
                                    IO_State_Change(Node.Name, "Foup_Lock", false);
                                    _UIReport.On_Node_State_Changed(Node, "UnLoad Complete");
                                    break;
                                case Transaction.Command.LoadPortType.InitialPos:
                                case Transaction.Command.LoadPortType.ForceInitialPos:
                                    _UIReport.On_Node_State_Changed(Node, "Ready To Load");
                                    IO_State_Change(Node.Name, "Foup_Lock", false);
                                    Node.State = "Ready";
                                    break;
                                case Transaction.Command.LoadPortType.Clamp:
                                    //IO_State_Change(Node.Name, "Foup_Lock", true);
                                    break;
                                case Transaction.Command.LoadPortType.UnClamp:
                                    //IO_State_Change(Node.Name, "Foup_Lock", false);
                                    break;
                            }
                            break;
                    }

                    _UIReport.On_Command_Finished(Node, Txn, Msg);



                    if (!Node.Type.Equals("LOADPORT"))//LoadPort 只能在Mapping完成後關閉安全鎖
                    {
                        Node.InterLock = false;
                    }
                    TaskJobManagment.CurrentProceedTask Task;
                    if (TaskJob.IsTask(Txn.FormName,out Task))//如果是帶TaskID才檢查
                    {
                        string ErrorMessage = "";
                        string Report = "";
                        string Location = "";
                        if (!TaskJob.CheckTask(Txn.FormName, Node.Name, "CMD", Txn.Method, "Finished", out ErrorMessage, out Report, out Location))
                        {//還沒做完
                            if (Report.Equals("ACK"))
                            {
                                if (_HostReport != null)
                                {
                                    _HostReport.On_TaskJob_Ack(Task);
                                }
                            }
                            if (!ErrorMessage.Equals(""))
                            {//做完但沒通過檢查
                                TaskJob.Remove(Txn.FormName);
                                if (_HostReport != null)
                                {
                                    _HostReport.On_TaskJob_Aborted(Task, Location, Report, ErrorMessage);
                                }
                                _UIReport.On_TaskJob_Aborted(Task, Node.Name, Report, ErrorMessage);
                            }
                            //檢查到不是Task，不做事
                        }
                        else
                        {//做完且通過檢查，開始進行下一個Task
                            
                            if (!TaskJob.Excute(Txn.FormName, out ErrorMessage,out Task))
                            {//如果沒有可以執行的Task，回報完成
                                if (Report.Equals("ACK"))
                                {
                                    if (_HostReport != null)
                                    {
                                        _HostReport.On_TaskJob_Ack(Task);
                                    }
                                }
                                if (ErrorMessage.Equals(""))
                                {
                                    if (_HostReport != null)
                                    {
                                        _HostReport.On_TaskJob_Finished(Task);
                                    }
                                    _UIReport.On_TaskJob_Finished(Task);
                                }
                                else
                                {
                                    if (_HostReport != null)
                                    {
                                        _HostReport.On_TaskJob_Aborted(Task, Location, Report, ErrorMessage);
                                    }
                                }
                            }
                            else
                            {
                                if (Report.Equals("ACK"))
                                {
                                    if (_HostReport != null)
                                    {
                                        _HostReport.On_TaskJob_Ack(Task);
                                    }
                                }
                            }
                        }
                    }
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

                            if (Node.Phase.Equals("2"))
                            {
                                

                                Node NextRobot = NodeManagement.GetNextRobot(Txn.TargetJobs[0].Destination);

                                if (NextRobot != null)
                                {
                                    if (Txn.TargetJobs[0].ProcessFlag)
                                    {
                                        //扣掉待搬送數量
                                        NextRobot.WaitForCarryCount--;
                                        //logger.Debug(NextRobot.Name + " WaitForCarryCount:" + NextRobot.Status.WaitForCarryCount);

                                    }
                                }
                                else
                                {
                                    logger.Error(Txn.TargetJobs[0].Job_Id + "找不到目的地搬送Robot");
                                }

                                Node.PutOut = false;
                                Node.GetMutex = true;
                                Node.UnLockByJob = "";
                                //4port use only
                                Node.PutAvailable = true;
                            }

                            break;
                        case Transaction.Command.RobotType.Put:
                        case Transaction.Command.RobotType.PutBack:
                            Node.CurrentPoint = Txn.Point;
                            if (Node.Phase.Equals("2"))
                            {
                                
                                Node.GetAvailable = true;
                                Node.GetMutex = true;
                                Node.UnLockByJob = "";
                                Node.PutAvailable = true;
                                Node.PutOut = false;


                            }

                            break;
                        case Transaction.Command.RobotType.WaitBeforeGet:
                        case Transaction.Command.RobotType.WaitBeforePut:
                        case Transaction.Command.RobotType.PutWithoutBack:
                            Node.CurrentPoint = Txn.Point;
                            Node.PutOutArm = Txn.Arm;
                            Node.PutOut = true;
                            //4Port use only

                            Node.GetAvailable = true;
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
                            Node.Available = true;
                            Node.UnLockByJob = "";

                            Node.GetAvailable = false;
                            Node.GetMutex = false;

                            // Node.PutAvailable = true;
                            break;
                    }
                    break;
                case "LOADPORT":
                    switch (Txn.Method)
                    {
                        case Transaction.Command.LoadPortType.MappingLoad:

                            Node.InterLock = false;
                            _UIReport.On_Node_State_Changed(Node, "Load Complete");
                            break;
                        case Transaction.Command.LoadPortType.Unload:
                            _UIReport.On_Node_State_Changed(Node, "UnLoad Complete");
                            break;
                        default:
                            Node.InterLock = true;
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
            TaskJobManagment.CurrentProceedTask Task = TaskJob.Remove(Txn.FormName);
            if (!Node.IsPause)
            {
                logger.Debug("Transaction TimeOut:" + Txn.CommandEncodeStr);
                Node.HasAlarm = true;
                if (_HostReport != null)
                {
                    _HostReport.On_TaskJob_Aborted(Task, Node.Name, "ABS", "TimeOut");
                }
                _UIReport.On_TaskJob_Aborted(Task, Node.Name, "ABS", "TimeOut");
                _UIReport.On_Command_TimeOut(Node, Txn);
            }
        }
        /// <summary>
        /// 事件觸發
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Msg"></param>
        public void On_Event_Trigger(Node Node, ReturnMessage Msg)
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
                                IO_State_Change(Node.Name, "Access_SW", true);
                                break;
                            case "MANOF":
                                IO_State_Change(Node.Name, "Access_SW", false);
                                break;
                            case "SMTON":
                                IO_State_Change(Node.Name, "Foup_Presence", false);
                                break;
                            case "PODOF":
                                IO_State_Change(Node.Name, "Foup_Presence", true);
                                IO_State_Change(Node.Name, "Foup_Placement", false);
                                if (_HostReport != null)
                                {
                                    _HostReport.On_Foup_Presence(Node.Name,false);
                                }
                                break;
                            case "PODON":
                                IO_State_Change(Node.Name, "Foup_Presence", false);
                                IO_State_Change(Node.Name, "Foup_Placement", true);
                                if (_HostReport != null)
                                {
                                    _HostReport.On_Foup_Presence(Node.Name, true);
                                }
                                break;
                            case "ABNST":
                                IO_State_Change(Node.Name, "Foup_Placement", false);
                                break;
                            case "POD_ARRIVED":
                                IO_State_Change(Node.Name, "Foup_Presence", true);
                                IO_State_Change(Node.Name, "Foup_Placement", true);
                                break;

                            case "POD_REMOVED":
                                IO_State_Change(Node.Name, "Foup_Presence", false);
                                IO_State_Change(Node.Name, "Foup_Placement", false);
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
                    _UIReport.On_Node_State_Changed(Node, "Alarm");

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
            foreach(Node each in find)
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
                
            }
            logger.Debug(Device_ID + " " + Status);
            _UIReport.On_Controller_State_Changed(Device_ID, Status);
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

            _UIReport.On_Node_State_Changed(Node, Status);
        }
        /// <summary>
        /// 命令執行發生錯誤
        /// </summary>
        /// <param name="Node"></param>
        /// <param name="Txn"></param>
        /// <param name="Msg"></param>
        public void On_Command_Error(Node Node, Transaction Txn, ReturnMessage Msg)
        {
            Node.InitialComplete = false;
            Node.OrgSearchComplete = false;
            Node.HasAlarm = true;
            TaskJobManagment.CurrentProceedTask Task = TaskJob.Remove(Txn.FormName);
            if (Msg.Value.Equals(""))
            {
                Msg.Value = Msg.Command;
            }
            if (_HostReport != null)
            {
                _HostReport.On_TaskJob_Aborted(Task, "", "ABS", Msg.Value);
            }
            _UIReport.On_TaskJob_Aborted(Task, Node.Name, "ABS", Msg.Value);
            _UIReport.On_Command_Error(Node, Txn, Msg);
            _UIReport.On_Node_State_Changed(Node, "Alarm");

        }

        public void On_Data_Chnaged(string Parameter, string Value)
        {
            if (_HostReport != null)
            {
                string DIO_Data = DIO.GetALL();
                _HostReport.On_Event_Trigger("SIGSTAT", "SYSTEM", Parameter, DIO_Data);
            }
            _UIReport.On_Data_Chnaged(Parameter, Value);
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
            if (ErrorCode.Equals("00100007"))
            {
                ControllerManagement.ClearTransactionList();
                TaskJob.Clear();
            }
            _UIReport.On_Alarm_Happen(DIOName, ErrorCode);
        }

        private void IO_State_Change(string Source, string Attr, object Value)
        {
            try
            {
                Node src = NodeManagement.Get(Source);
                if (src != null)
                {

                    src.GetType().GetProperty(Attr).SetValue(src, Value);
                    if (_HostReport != null)
                    {
                        _HostReport.On_Event_Trigger("SIGSTAT", "PORT", Source, "ALL");
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        public void On_Job_Position_Changed(Job Job)
        {
            _UIReport.On_Job_Location_Changed(Job);
        }

        public static Job CreateJob()
        {
            return new Job(RouteControl.Instance);
        }

        public void On_Task_NoExcuted(TaskJobManagment.CurrentProceedTask Task)
        {
            string ErrorMessage = "";
            string Report = "";
            string Location = "";
            if (!TaskJob.CheckTask(Task.Id, "", "","", "", out ErrorMessage, out Report, out Location))
            {//還沒做完
                if (Report.Equals("ACK"))
                {
                    if (_HostReport != null)
                    {
                        _HostReport.On_TaskJob_Ack(Task);
                    }
                }
                if (!ErrorMessage.Equals(""))
                {//做完但沒通過檢查
                    TaskJob.Remove(Task.Id);
                    if (_HostReport != null)
                    {

                        _HostReport.On_TaskJob_Aborted(Task, Location, Report, ErrorMessage);
                    }
                    _UIReport.On_TaskJob_Aborted(Task, "", Report, ErrorMessage);
                }
                //檢查到不是Task，不做事
            }
            else
            {//做完且通過檢查，開始進行下一個Task
                
                if (!TaskJob.Excute(Task.Id, out ErrorMessage,out Task))
                {//如果沒有可以執行的Task，回報完成

                    if (ErrorMessage.Equals(""))
                    {
                        if (_HostReport != null)
                        {
                            _HostReport.On_TaskJob_Finished(Task);
                        }
                        _UIReport.On_TaskJob_Finished(Task);
                    }
                    else
                    {
                        if (_HostReport != null)
                        {
                            _HostReport.On_TaskJob_Aborted(Task, Location, Report, ErrorMessage);
                        }
                        _UIReport.On_TaskJob_Aborted(Task, "", Report, ErrorMessage);
                    }
                }
                else
                {
                    if (Report.Equals("ACK"))
                    {
                        if (_HostReport != null)
                        {
                            _HostReport.On_TaskJob_Ack(Task);
                        }
                    }
                }
            }
        }

        public void On_Task_Abort(TaskJobManagment.CurrentProceedTask Task)
        {
            //TaskJob.Remove(Id);
            _HostReport.On_TaskJob_Aborted(Task, "SYSTEM", "ABS", "S0300170");
        }
    }
}
