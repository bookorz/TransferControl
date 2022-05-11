using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TransferControl.Config;
using TransferControl.Engine;
using TransferControl.Management;

namespace TransferControl.TaksFlow
{
    class EFEM_Demo_2P : BaseTaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(EFEM_Demo_2P));
        //IUserInterfaceReport _TaskReport;

        public EFEM_Demo_2P(IUserInterfaceReport TaskReport) : base(TaskReport)
        {
            _TaskReport = TaskReport;
            ArmCount = 2;

            LoadportCount = 2;
        }
        public override void Excute(object input)
        {
            TaskFlowManagement.CurrentProcessTask TaskJob = (TaskFlowManagement.CurrentProcessTask)input;
            logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());

            Node Target = null;
            Node Position = null;
            Job Wafer;
            string Value = "";

            string Arm = "";
            string Slot = "";

            Node FromPosition = null;
            Node ToPosition = null;
            if (TaskJob.Params != null)
            {
                foreach (KeyValuePair<string, string> item in TaskJob.Params)
                {
                    switch (item.Key)
                    {
                        case "@Target":
                            Target = NodeManagement.Get(item.Value);
                            break;
                        case "@Position":
                        case "@FromPosition":
                        case "@ToPosition":
                            Position = NodeManagement.Get(item.Value);
                            if (item.Key.Equals("@FromPosition"))
                                FromPosition = NodeManagement.Get(item.Value);
                            if (item.Key.Equals("@ToPosition"))
                                ToPosition = NodeManagement.Get(item.Value);
                            break;
                        case "@Value":
                            Value = item.Value;
                            break;
                        case "@Arm":
                        case "@FromArm":
                        case "@ToArm":
                            Arm = item.Value;
                            break;
                        case "@Slot":
                        case "@FromSlot":
                        case "@ToSlot":
                            Slot = item.Value;
                            break;
                    }
                }
            }

            if (!SystemConfig.Get().OfflineMode)
                if (MainControl.Instance.DIO.GetIO("DIN", "SAFETYRELAY").ToUpper().Equals("TRUE"))
                {
                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM" }, "S0300170");
                    return;
                }

            try
            {
                switch (TaskJob.TaskName)
                {
                    #region All
                    //Loadport 狀態尚未新增
                    case TaskFlowManagement.Command.EFEM_INIT:
                        switch(TaskJob.CurrentIndex)
                        {
                            //Reset
                            case 0:
                                if (IsNodeEnabledOrNull("ROBOT01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));

                                if (IsNodeEnabledOrNull("ALIGNER01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Reset }));

                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT0{0}", i + 1), "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
                                }
                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Servo, Value = "1" }));

                                    if (IsNodeEnabledOrNull("ALIGNER01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Servo, Value = "1" }));

                                    if (IsNodeEnabledOrNull("OCR01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("OCR01", "EXCUTED", new Transaction { Method = Transaction.Command.OCRType.Online }));
                                }
                                break;

                            case 2:
                                //開啟電磁閥
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    //Aligner
                                    if (IsNodeEnabledOrNull("ALIGNER01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SetSV, Value = "01", Val2 = "1" }));

                                    //Robot(R)
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "1" }));
                                }
                                break;

                            case 3:
                                //開啟電磁閥
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    //Robot(L)
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "1" }));
                                }
                                break;

                            case 4:
                                if (!SystemConfig.Get().OfflineMode)
                                    SpinWait.SpinUntil(() => false, 500);
                                break;

                            case 5:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    //確認Robot R軸  
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        GetRobotPresence(TaskJob, NodeManagement.Get("ROBOT01"), "RPresence");

                                    //Aligner Presence
                                    if (IsNodeEnabledOrNull("ALIGNER01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                }
                                break;

                            case 6:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    //確認Robot L軸 Presence 
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        GetRobotPresence(TaskJob, NodeManagement.Get("ROBOT01"), "LPresence");
                                }
                                break;

                            case 7:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if(IsNodeEnabledOrNull("ROBOT01"))
                                    {
                                        //Robot R軸 Presence 不存在,則關閉R軸電磁閥
                                        if (!NodeManagement.Get("ROBOT01").R_Presence)
                                        {
                                            Wafer = JobManagement.Get("ROBOT01_R", "1");
                                            if (Wafer != null)
                                            {
                                                Wafer.LastNode = Wafer.Position;
                                                Wafer.LastSlot = Wafer.Slot;
                                                Wafer.Position = "";
                                                Wafer.Slot = Wafer.Slot;

                                                JobManagement.Remove(Wafer);
                                                _TaskReport.On_Job_Location_Changed(Wafer);
                                            }

                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "0" }));
                                        }
                                    }

                                    if(IsNodeEnabledOrNull("ALIGNER01"))
                                    {
                                        //ALIGNER R軸 Presence 不存在,則關閉L軸電磁閥
                                        if (!NodeManagement.Get("ALIGNER01").R_Presence)
                                        {
                                            Wafer = JobManagement.Get("ALIGNER01", "1");
                                            if (Wafer != null)
                                            {
                                                Wafer.LastNode = Wafer.Position;
                                                Wafer.LastSlot = Wafer.Slot;
                                                Wafer.Position = "";
                                                Wafer.Slot = Wafer.Slot;

                                                JobManagement.Remove(Wafer);
                                                _TaskReport.On_Job_Location_Changed(Wafer);
                                            }

                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SetSV, Arm = "01", Value = "0" }));
                                        }
                                    }
                                }
                                break;
                            case 8:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                    {
                                        //Robot R軸 Presence 不存在,則關閉L軸電磁閥
                                        if (!NodeManagement.Get("ROBOT01").L_Presence)
                                        {
                                            Wafer = JobManagement.Get("ROBOT01_L", "1");
                                            if (Wafer != null)
                                            {
                                                Wafer.LastNode = Wafer.Position;
                                                Wafer.LastSlot = Wafer.Slot;
                                                Wafer.Position = "";
                                                Wafer.Slot = Wafer.Slot;

                                                JobManagement.Remove(Wafer);
                                                _TaskReport.On_Job_Location_Changed(Wafer);
                                            }

                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "0" }));
                                        }
                                    }
                                }
                                break;

                            case 9:
                                if (IsNodeEnabledOrNull("ROBOT01"))
                                    NodeManagement.Get("ROBOT01").InitialComplete = true;

                                if (IsNodeEnabledOrNull("ALIGNER01"))
                                    NodeManagement.Get("ALIGNER01").InitialComplete = true;

                                if (IsNodeEnabledOrNull("LOADPORT01"))
                                    NodeManagement.Get("LOADPORT01").InitialComplete = true;

                                if (IsNodeEnabledOrNull("LOADPORT02"))
                                    NodeManagement.Get("LOADPORT02").InitialComplete = true;

                                if (!SystemConfig.Get().OfflineMode)
                                    SpinWait.SpinUntil(() => false, 500);

                                break;
                            //ORG
                            case 10:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "FINISHED", new Transaction { Method = Transaction.Command.RobotType.OrginSearch }));
                                }
                                break;

                            case 11:
                                if(!SystemConfig.Get().OfflineMode)
                                {
                                    if (IsNodeEnabledOrNull("ALIGNER01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.OrginSearch }));

                                    if(IsNodeEnabledOrNull("ROBOT01"))
                                        GetRobotPresence(TaskJob, NodeManagement.Get("ROBOT01"), "RSolenoid");
                                }

                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT0{0}", i + 1), "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.ForceInitialPos }));
                                }

                                break;

                            case 12:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (IsNodeEnabledOrNull("ALIGNER01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.Home }));

                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        GetRobotPresence(TaskJob, NodeManagement.Get("ROBOT01"), "LSolenoid");


                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT0{0}", i + 1), "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetStatus }));
                                    }

                                    break;
                                }
                                break;

                            case 13:
                                if(!SystemConfig.Get().OfflineMode)
                                {
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        GetRobotPresence(TaskJob, NodeManagement.Get("ROBOT01"), "RPresence");

                                    if (IsNodeEnabledOrNull("ALIGNER01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                                }
                                break;

                            case 14:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        GetRobotPresence(TaskJob, NodeManagement.Get("ROBOT01"), "LPresence");

                                    if (IsNodeEnabledOrNull("ALIGNER01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                }
                                break;

                            case 15:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        GetRobotPosition(TaskJob, NodeManagement.Get("ROBOT01"));
                                }
                                break;
                            case 16:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));

                                    if (IsNodeEnabledOrNull("ALIGNER01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));
                                }
                                break;

                            default:
                                OrgSearchCompleted("ROBOT01");
                                OrgSearchCompleted("ALIGNER01");
                                OrgSearchCompleted("LOADPORT01");
                                OrgSearchCompleted("LOADPORT02");

                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.RESET_ALL:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (IsNodeEnabledOrNull("ROBOT01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));

                                if (IsNodeEnabledOrNull("ALIGNER01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Reset }));

                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT0{0}", i + 1), "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
                                }
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    #endregion
                    #region Robot
                    case TaskFlowManagement.Command.ROBOT_ABORT:
                        if (!Sanwa_RobotAbort(TaskJob, Target)) return;
                        break;
                    case TaskFlowManagement.Command.ROBOT_RESTR:
                        if (!Sanwa_RobotReStart(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_HOLD:
                        if (!Sanwa_RobotHold(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.GET_CLAMP:
                        if (!Sanwa_RobotGetClamp(TaskJob, Target, Value)) return;
                        break;
                    case TaskFlowManagement.Command.ROBOT_PUT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                if (TaskJob.Params.ContainsKey("@IsTransCommand"))
                                    if (!TaskJob.Params["@IsTransCommand"].Equals("TRUE"))
                                        AckTask(TaskJob);

                                string MethodName = "";
                                //目標位置是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Position, TaskJob)) return;

                                //Robot 是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                if (Position.Type.ToUpper().Equals("ALIGNER") && Arm.Equals("3"))
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300156");
                                    return;
                                }

                                //是否需要重新Mapping
                                if (Position.Type.ToUpper().Equals("LOADPORT") && !Position.IsMapping)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");
                                }
                                break;
                            case 1:
                                //取得R軸電磁閥最後狀態
                                //GetRobotPresence(TaskJob, Target, "RSolenoid");
                                break;

                            case 2:
                                //取得L軸電磁閥最後狀態
                                //GetRobotPresence(TaskJob, Target, "LSolenoid");
                                break;

                            case 3:
                                //確認R軸 Presence                   
                                //GetRobotPresence(TaskJob, Target, "RPresence");
                                break;

                            case 4:
                                //確認L軸 Presence
                                //GetRobotPresence(TaskJob, Target, "LPresence");
                                break;
                            case 5:
                                //if (!SystemConfig.Get().OfflineMode)
                                //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));

                                break;
                            case 6:
                                //確認Robot
                                GetRobotPosition(TaskJob, Target);

                                break;

                            case 7:
                                //Check presence
                                if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                {
                                    if (!TaskJob.Params["@BYPASS_CHECK"].Equals("TRUE"))
                                    {
                                        if (MainControl.Instance.DIO.GetIO("DIN", Position.Name + "_DOOR_OPEN").ToUpper().Equals("FALSE"))
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300163");
                                            return;
                                        }

                                        if (MainControl.Instance.DIO.GetIO("DIN", Position.Name + "_ARM_EXTEND_ENABLE").ToUpper().Equals("FALSE"))
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300164");
                                            return;
                                        }
                                    }

                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "false");
                                }
                                else
                                {
                                    //目的地不能有片子
                                    Wafer = null;
                                    bool ErrorSlot = false;

                                    if (Arm.Equals("1") || Arm.Equals("2"))
                                    {
                                        Wafer = JobManagement.Get(Position.Name, Slot);
                                    }
                                    else if (Arm.Equals("3"))
                                    {
                                        for (int i = 0; i < ArmCount; i++)
                                        {
                                            Wafer = JobManagement.Get(Position.Name, (Convert.ToInt32(Slot) - i).ToString());
                                            if (Wafer != null) break;
                                        }
                                    }

                                    if (Wafer != null || ErrorSlot)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300171");
                                        return;
                                    }
                                }
                                break;

                            case 8:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.Home }));

                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.PutWait, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                    }
                                }
                                break;

                            case 9:
                                //移動Wafer
                                MethodName = Arm.Equals("3") ? Transaction.Command.RobotType.DoublePut : Transaction.Command.RobotType.Put;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                break;

                            case 10:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferHold }));
                                }
                                break;

                            case 11:
                                string TargetName = "";

                                switch (Arm)
                                {
                                    case "1":
                                    case "2":
                                        TargetName = Arm.Equals("1") ? Target.Name + "_R" : Target.Name + "_L";
                                        Wafer = JobManagement.Get(TargetName, "1");
                                        if (Wafer == null)
                                        {
                                            Wafer = JobManagement.Add();
                                            Wafer.MapFlag = true;
                                        }
                                        Wafer.LastNode = Wafer.Position;
                                        Wafer.LastSlot = Wafer.Slot;
                                        Wafer.Position = Position.Name;
                                        Wafer.Slot = Convert.ToInt32(Slot).ToString();

                                        _TaskReport.On_Job_Location_Changed(Wafer);


                                        if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                        {
                                            JobManagement.Remove(Wafer);
                                        }
                                        break;
                                    case "3":
                                        for (int i = 0; i < ArmCount; i++)
                                        {
                                            TargetName = i == 0 ? Target.Name + "_R" : Target.Name + "_L";
                                            Wafer = JobManagement.Get(TargetName, "1");
                                            if (Wafer == null)
                                            {
                                                Wafer = JobManagement.Add();
                                                Wafer.MapFlag = true;
                                            }
                                            Wafer.LastNode = Wafer.Position;
                                            Wafer.LastSlot = Wafer.Slot;
                                            Wafer.Position = Position.Name;
                                            Wafer.Slot = (Convert.ToInt32(Slot) - i).ToString();

                                            _TaskReport.On_Job_Location_Changed(Wafer);

                                            if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                            {
                                                JobManagement.Remove(Wafer);
                                            }
                                        }
                                        break;
                                }

                                if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "true");

                                break;

                            case 12:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                                }
                                break;

                            case 13:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                }
                                break;

                            case 14:
                                //取得R軸電磁閥最後狀態
                                GetRobotPresence(TaskJob, Target, "RSolenoid");
                                break;

                            case 15:
                                //取得L軸電磁閥最後狀態
                                GetRobotPresence(TaskJob, Target, "LSolenoid");
                                break;

                            case 16:
                                //確認R軸 Presence                   
                                GetRobotPresence(TaskJob, Target, "RPresence");
                                break;

                            case 17:
                                //確認L軸 Presence
                                GetRobotPresence(TaskJob, Target, "LPresence");
                                break;

                            case 18:
                                //確認Robot
                                GetRobotPosition(TaskJob, Target);

                                break;
                            case 19:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_GET:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (TaskJob.Params.ContainsKey("@IsTransCommand"))
                                    if (!TaskJob.Params["@IsTransCommand"].Equals("TRUE"))
                                        AckTask(TaskJob);

                                //目標位置是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Position, TaskJob)) return;

                                //Robot 是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                if (Position.Type.ToUpper().Equals("ALIGNER") && Arm.Equals("3"))
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300158");
                                    return;
                                }


                                if (Position.Type.ToUpper().Equals("LOADPORT") && !Position.IsMapping)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");
                                    return;
                                }

                                break;

                            case 1:
                                //取得R軸電磁閥最後狀態
                                //GetRobotPresence(TaskJob, Target, "RSolenoid");
                                break;

                            case 2:
                                //取得L軸電磁閥最後狀態
                                //GetRobotPresence(TaskJob, Target, "LSolenoid");
                                break;

                            case 3:
                                //確認R軸 Presence                   
                                //GetRobotPresence(TaskJob, Target, "RPresence");
                                break;

                            case 4:
                                //確認L軸 Presence
                                //GetRobotPresence(TaskJob, Target, "LPresence");
                                break;

                            case 5:
                                //確認Robot
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 6:
                                //Check presence
                                if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                {
                                    if (!TaskJob.Params["@BYPASS_CHECK"].Equals("TRUE"))
                                    {
                                        if (MainControl.Instance.DIO.GetIO("DIN", Position.Name + "_DOOR_OPEN").ToUpper().Equals("FALSE"))
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300163");
                                            return;
                                        }

                                        if (MainControl.Instance.DIO.GetIO("DIN", Position.Name + "_ARM_EXTEND_ENABLE").ToUpper().Equals("FALSE"))
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300164");
                                            return;
                                        }
                                    }

                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "false");
                                }
                                else
                                {
                                    //目的地需要有片子
                                    Wafer = JobManagement.Get(Position.Name, Slot);

                                    switch (Arm)
                                    {
                                        case "1":
                                        case "2":
                                            Wafer = JobManagement.Get(Position.Name, Slot);
                                            if (Wafer == null)
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300162");
                                                return;
                                            }
                                            else if (!Wafer.MapFlag || Wafer.ErrPosition)
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300162");
                                                return;
                                            }
                                            break;

                                        case "3":
                                            for (int i = 0; i < ArmCount; i++)
                                            {
                                                Wafer = JobManagement.Get(Position.Name, (Convert.ToInt32(Slot) - i).ToString());

                                                if (Wafer == null)
                                                {
                                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300162");
                                                    return;
                                                }
                                                else if (!Wafer.MapFlag || Wafer.ErrPosition)
                                                {
                                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300162");
                                                    return;
                                                }
                                            }
                                            break;
                                    }
                                }
                                break;

                            case 7:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferRelease }));

                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.GetWait, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                    }
                                }
                                break;

                            case 8:
                                string MethodName = Arm.Equals("3") ? Transaction.Command.RobotType.DoubleGet : Transaction.Command.RobotType.Get;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));

                                break;

                            case 9:
                                switch (Arm)
                                {
                                    case "1":
                                    case "2":
                                        Wafer = JobManagement.Get(Position.Name, Slot);
                                        if (Wafer == null)
                                        {
                                            Wafer = JobManagement.Add();
                                            Wafer.MapFlag = true;
                                        }
                                        Wafer.LastNode = Wafer.Position ?? Position.Name;
                                        Wafer.LastSlot = Wafer.Slot ?? "1";
                                        Wafer.Position = Arm.Equals("1") ? Target.Name + "_R" : Target.Name + "_L";
                                        Wafer.Slot = "1";

                                        _TaskReport.On_Job_Location_Changed(Wafer);
                                        break;
                                    case "3":
                                        for (int i = 0; i < ArmCount; i++)
                                        {
                                            Wafer = JobManagement.Get(Position.Name, (Convert.ToInt32(Slot) - i).ToString());
                                            if (Wafer == null)
                                            {
                                                Wafer = JobManagement.Add();
                                                Wafer.MapFlag = true;
                                            }
                                            Wafer.LastNode = Wafer.Position;
                                            Wafer.LastSlot = Wafer.Slot;
                                            Wafer.Position = i == 0 ? Target.Name + "_R" : Target.Name + "_L";
                                            Wafer.Slot = "1";

                                            _TaskReport.On_Job_Location_Changed(Wafer);

                                            while (true)
                                            {
                                                Job ghost = JobManagement.Get(Wafer.LastNode, Wafer.LastSlot);
                                                if (ghost != null)
                                                {
                                                    JobManagement.Remove(ghost);
                                                }
                                                else
                                                {
                                                    break;
                                                }
                                            }
                                        }
                                        break;
                                }

                                if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "TRUE");
                                break;

                            case 10:
                                //Aligner Solenoid
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                                }
                                break;

                            case 11:
                                //Aligner Presence
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                }
                                break;

                            case 12:
                                //取得Robot R軸電磁閥最後狀態
                                GetRobotPresence(TaskJob, Target, "RSolenoid");
                                break;

                            case 13:
                                //取得Robot L軸電磁閥最後狀態
                                GetRobotPresence(TaskJob, Target, "LSolenoid");
                                break;

                            case 14:
                                //確認Robot R軸 Presence                   
                                GetRobotPresence(TaskJob, Target, "RPresence");
                                break;

                            case 15:
                                //確認Robot L軸 Presence
                                GetRobotPresence(TaskJob, Target, "LPresence");
                                break;

                            case 16:
                                //確認Robot
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 17:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));
                                break;


                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_CARRY:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                if (FromPosition != null)
                                {
                                    if (!FromPosition.Type.ToUpper().Equals("LOADLOCK"))
                                    {
                                        if (!CheckNodeStatusOnTaskJob(FromPosition, TaskJob)) return;

                                        if (FromPosition.Type.ToUpper().Equals("ALIGNER") && TaskJob.Params["@FromArm"].Equals("3"))
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300158");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (!TaskJob.Params["@From_BYPASS_CHECK"].ToUpper().Equals("TRUE"))
                                        {
                                            if (MainControl.Instance.DIO.GetIO("DIN", FromPosition.Name + "_DOOR_OPEN").ToUpper().Equals("FALSE"))
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300163");
                                                return;
                                            }

                                            if (MainControl.Instance.DIO.GetIO("DIN", FromPosition.Name + "_ARM_EXTEND_ENABLE").ToUpper().Equals("FALSE"))
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = FromPosition.Name }, "S0300164");
                                                return;
                                            }
                                        }

                                    }
                                }

                                if (ToPosition != null)
                                {
                                    if (!ToPosition.Type.ToUpper().Equals("LOADLOCK"))
                                    {
                                        if (!CheckNodeStatusOnTaskJob(ToPosition, TaskJob)) return;

                                        if (ToPosition.Type.ToUpper().Equals("ALIGNER") && TaskJob.Params["@ToArm"].Equals("3"))
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300156");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (!TaskJob.Params["@To_BYPASS_CHECK"].Equals("TRUE"))
                                        {
                                            if (MainControl.Instance.DIO.GetIO("DIN", ToPosition.Name + "_DOOR_OPEN").ToUpper().Equals("FALSE"))
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300163");
                                                return;
                                            }

                                            if (MainControl.Instance.DIO.GetIO("DIN", ToPosition.Name + "_ARM_EXTEND_ENABLE").ToUpper().Equals("FALSE"))
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = ToPosition.Name }, "S0300164");
                                                return;
                                            }
                                        }

                                        //MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + ToPosition.Name, "FALSE");
                                    }
                                }

                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                //Get safety check
                                if (!TaskJob.Params["@FromPosition"].Contains("BF") && !TaskJob.Params["@FromPosition"].Contains("LL"))
                                {
                                    if (!TaskJob.Params["@FromArm"].Equals("3"))
                                    {
                                        Wafer = JobManagement.Get(TaskJob.Params["@FromPosition"], TaskJob.Params["@FromSlot"]);
                                        if (Wafer == null)
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300162");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < ArmCount; i++)
                                        {
                                            Wafer = JobManagement.Get(TaskJob.Params["@FromPosition"], (Convert.ToInt32(TaskJob.Params["@FromSlot"]) - i).ToString());
                                            if (Wafer == null)
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300162");
                                                return;
                                            }
                                            else if (!Wafer.MapFlag || Wafer.ErrPosition)
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300162");
                                                return;
                                            }
                                        }
                                    }
                                }

                                if (TaskJob.Params["@FromPosition"].Equals(TaskJob.Params["@ToPosition"]) && TaskJob.Params["@FromSlot"].Equals(TaskJob.Params["@ToSlot"]))
                                {

                                }
                                else
                                {
                                    Wafer = null;
                                    if (!TaskJob.Params["@FromArm"].Equals("3"))
                                    {
                                        Wafer = JobManagement.Get(TaskJob.Params["@ToPosition"], TaskJob.Params["@ToSlot"]);
                                    }
                                    else
                                    {
                                        for (int i = 0; i < ArmCount; i++)
                                        {
                                            Wafer = JobManagement.Get(TaskJob.Params["@ToPosition"], (Convert.ToInt32(TaskJob.Params["@ToSlot"]) - i).ToString());
                                            if (Wafer != null) break;
                                        }

                                    }

                                    //Put safety check
                                    if (Wafer != null)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300171");
                                        return;
                                    }
                                }

                                if (FromPosition.Type.ToUpper().Equals("LOADLOCK"))
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + FromPosition.Name, "FALSE");

                                if (ToPosition.Type.ToUpper().Equals("LOADLOCK"))
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + ToPosition.Name, "FALSE");



                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_GET, new Dictionary<string, string>() { { "@Target", Target.Name }, { "@Position", TaskJob.Params["@FromPosition"] }, { "@Arm", TaskJob.Params["@FromArm"] }, { "@Slot", TaskJob.Params["@FromSlot"] }, { "@BYPASS_CHECK", TaskJob.Params["@From_BYPASS_CHECK"] }, { "@IsTransCommand", "TRUE" } }, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, null, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_PUT, new Dictionary<string, string>() { { "@Target", Target.Name }, { "@Position", TaskJob.Params["@ToPosition"] }, { "@Arm", TaskJob.Params["@ToArm"] }, { "@Slot", TaskJob.Params["@ToSlot"] }, { "@BYPASS_CHECK", TaskJob.Params["@To_BYPASS_CHECK"] }, { "@IsTransCommand", "TRUE" } }, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, null, "TASK_ABORT");

                                    break;
                                }
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_PUTWAIT:
                    case TaskFlowManagement.Command.ROBOT_GETWAIT:
                        if (!Sanwa_RobotWait(TaskJob, Target, Position, Arm, Slot)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_WAFER_RELEASE:
                    case TaskFlowManagement.Command.ROBOT_WAFER_HOLD:
                        if (!Sanwa_RobotWaferVAC(TaskJob, Target, Arm)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_MODE:
                        if (!Sanwa_RobotMode(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_RETRACT:
                        if (!Sanwa_RobotRetract(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_SPEED:
                        if (!Sanwa_RobotSpeed(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_GET_SPEED:
                        if (!Sanwa_RobotGetSpeed(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_SERVO:
                        if (!Sanwa_RobotServo(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_HOME:
                        if (!Sanwa_RobotHome(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!IsNodeInitialComplete(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                break;

                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Home }));
                                break;

                            case 2:
                                if(!Target.R_Presence)
                                {
                                    Wafer = JobManagement.Get(Target.Name + "_R", "1");
                                    if(Wafer != null)
                                    {
                                        Wafer.LastNode = Wafer.Position;
                                        Wafer.LastSlot = Wafer.Slot;
                                        Wafer.Position = "";
                                        Wafer.Slot = Wafer.Slot;

                                        JobManagement.Remove(Wafer);
                                        _TaskReport.On_Job_Location_Changed(Wafer);
                                    }

                                }

                                if(!Target.L_Presence)
                                {
                                    Wafer = JobManagement.Get(Target.Name + "_L", "1");
                                    if(Wafer != null)
                                    {
                                        Wafer.LastNode = Wafer.Position;
                                        Wafer.LastSlot = Wafer.Slot;
                                        Wafer.Position = "";
                                        Wafer.Slot = Wafer.Slot;

                                        JobManagement.Remove(Wafer);

                                        _TaskReport.On_Job_Location_Changed(Wafer);
                                    }
                                }
                                break;

                            case 3:
                                //確認Robot
                                if (!SystemConfig.Get().OfflineMode)
                                    GetRobotPosition(TaskJob, Target);
                                break;


                            default:
                                OrgSearchCompleted(Target.Name);

                                if (Target.OrgSearchComplete)
                                    ResetRobotInterLock();

                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_RESET:
                        if (!Sanwa_RobotReset(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_INIT:
                        if (!Sanwa_RobotINIT(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_SAVE_LOG:
                        if (!Sanwa_RobotSaveLog(TaskJob, Target)) return;
                        break;

                    #endregion
                    #region ALIGNER
                    case TaskFlowManagement.Command.ALIGNER_RESET:
                        if (!Sanwa_AlignerReset(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_INIT:
                        if(!Sanwa_AlignerINIT(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_ORGSH:
                        if (!Sanwa_AlignerORG(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_HOME:
                        if (!Sanwa_AlignerHome(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_SERVO:
                        if (!Sanwa_AlignerServo(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_SPEED:
                        if(!Sanwa_AlignerSpeed(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_GET_SPEED:
                        if (!Sanwa_AlignerGetSpeed(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_MODE:
                        if (!Sanwa_AlignerMode(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_WAFER_HOLD:
                        if (!Sanwa_AlignerWaferHold(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_WAFER_RELEASE:
                        if (!Sanwa_AlignerWaferRelease(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_SAVE_LOG:
                        if (!Sanwa_AlignerSaveLog(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_ALIGN:
                        if (!Sanwa_AlignerAlign(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.ALIGNER_GET_CLAMP:
                        if(!Sanwa_AlignerGetClamp(TaskJob, Target)) return;
                        break;

                    #endregion
                    #region LOADPORT
                    case TaskFlowManagement.Command.LOADPORT_INIT:
                        ///待新增
                        switch(TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (SystemConfig.Get().DummyMappingData)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.Mode, Value = "3" }));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.Mode, Value = "0" }));
                                    }
                                }
                                break;

                            case 2:
                                //確認狀態
                                GetLoadportStatus(TaskJob, Target);
                                break;

                            default:
                                Target.InitialComplete = true;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300168");
                                    return;
                                }

                                AckTask(TaskJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));

                                break;

                            case 1:
                                //確認狀態
                                GetLoadportStatus(TaskJob, Target);
                                break;

                            default:
                                Target.OrgSearchComplete = true;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_RESET:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));

                                break;
                            case 1:
                                //確認狀態
                                //GetLoadportStatus(TaskJob, Target);
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_FORCE_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!IsNodeInitialComplete(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.ForceInitialPos }));

                                break;

                            case 1:
                                GetLoadportStatus(TaskJob, Target);
                                break;

                            default:
                                OrgSearchCompleted(Target.Name);
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_READ_STATUS:
                        switch(TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                break;
                            case 1:
                                GetLoadportStatus(TaskJob, Target);
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_CLAMP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Clamp }));

                                break;

                            case 1:
                                GetLoadportStatus(TaskJob, Target);

                                break;
                            default:
                                Target.Foup_Lock = true;
                                FinishTask(TaskJob);
                                return;
                        }

                        break;

                    case TaskFlowManagement.Command.LOADPORT_UNCLAMP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UnClamp }));

                                break;

                            case 1:
                                GetLoadportStatus(TaskJob, Target);
                                break;

                            default:
                                Target.Foup_Lock = false;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOCK:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Dock }));
                                break;

                            case 1:
                                GetLoadportStatus(TaskJob, Target);
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_UNDOCK:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UntilUnDock }));

                                break;

                            case 1:
                                GetLoadportStatus(TaskJob, Target);

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_OPEN:
                    case TaskFlowManagement.Command.LOADPORT_OPEN_NOMAP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                //移除所有Wafer
                                foreach (Job eachJob in JobManagement.GetByNode(Target.Name))
                                    JobManagement.Remove(eachJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MappingLoad, Value = "0" }));

                                break;

                            case 1:
                                if (TaskJob.TaskName.Equals(TaskFlowManagement.Command.LOADPORT_OPEN))
                                {
                                    if (SystemConfig.Get().DummyMappingData)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                    }
                                }
                                break;

                            case 2:
                                GetLoadportStatus(TaskJob, Target);

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    //case TaskFlowManagement.Command.LOADPORT_ALL_CLOSE:
                    //    if (!TDK_LoadportAllClose(TaskJob)) return;
                    //    break;

                    case TaskFlowManagement.Command.LOADPORT_CLOSE:
                    case TaskFlowManagement.Command.LOADPORT_CLOSE_NOMAP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                //避免尚未關完門就把Robot伸過來
                                NodeManagement.Get(Target.Name).IsMapping = false;

                                AckTask(TaskJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Unload, Value = "0" }));

                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MappingUnload, Value = "0" }));
                                break;


                            case 1:
                                //if (SystemConfig.Get().DummyMappingData)
                                //{
                                //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                                //}
                                //else
                                //{
                                //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                //}
                                break;

                            case 2:
                                GetLoadportStatus(TaskJob, Target);

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_GET_MAPDT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                }
                                break;

                            case 1:
                                GetLoadportStatus(TaskJob, Target);
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_RE_MAPPING:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300168");
                                    return;
                                }

                                if (!Target.OrgSearchComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300169");
                                    return;
                                }

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.RetryMapping }));

                                break;

                            case 1:
                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                }

                                break;
                            case 2:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));

                                break;

                            case 3:
                                GetLoadportStatus(TaskJob, Target);
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_SET_OPACCESS_INDICATOR:
                        if (!TDK_LoadportSetOPAccessIndicator(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_SET_LOAD_INDICATOR:
                        if (!TDK_LoadportSetLoadIndicator(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_SET_UNLOAD_INDICATOR:
                        if (!TDK_LoadportSetUnloadIndicator(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_READ_LED:
                        //if (!TDK_LoadportReadLed(TaskJob, Target)) return;
                        break;

                    #endregion
                    #region CSTID
                    case TaskFlowManagement.Command.GET_CSTID:
                        if (!GetCSTID(TaskJob, Target)) return;
                        break;
                    case TaskFlowManagement.Command.SET_CSTID:
                        if (!SetCSTID(TaskJob, Target, Value)) return;
                        break;
                    #endregion
                    #region E84
                    case TaskFlowManagement.Command.E84_INIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                //AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));
                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.RESET_E84:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.Reset }));
                                break;

                            case 1:
                                //    if (NodeManagement.Get(NodeManagement.Get(Target.Name).Associated_Node).E84Mode == E84_Mode.AUTO)
                                //        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetAutoMode }));
                                break;

                            case 2:
                                TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT, new Dictionary<string, string>() { { "@Target", Target.Name } }, "", TaskJob.MainTaskId).Promise();
                                break;

                            default:

                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.E84_MODE:
                    case TaskFlowManagement.Command.E84_TRANSREQ:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (Target.E84IOStatus["VALID"])
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, string.Format("S04000{0}", Target.Name.Substring(3, 2)));
                                        return;
                                    }
                                }

                                break;
                            case 1:

                                //AckTask(TaskJob);
                                if (Value.Equals("1")) //set auto
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetAutoMode }));
                                }
                                else //set manual
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetManualMode }));
                                }
                                break;

                            case 2:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT, new Dictionary<string, string>() { { "@Target", Target.Name } }, "", TaskJob.MainTaskId).Promise();
                                break;

                            default:

                                if (TaskJob.TaskName == TaskFlowManagement.Command.E84_MODE)
                                    NodeManagement.Get(Target.Associated_Node).E84Mode = Value.Equals("1") ? E84_Mode.AUTO : E84_Mode.MANUAL;

                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.E84_SET_ALL_MODE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                        {
                                            if (NodeManagement.Get(E84Name).E84IOStatus["VALID"])
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, string.Format("S04000{0}", i.ToString().PadLeft(2, '0')));
                                                return;
                                            }
                                        }
                                    }
                                }

                                //AckTask(TaskJob);
                                break;

                            case 1:
                                for (int i = 0; i < 4; i++)
                                {
                                    string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(E84Name))
                                    {
                                        if (Value.Equals("1")) //set auto
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetAutoMode }));
                                        }
                                        else //set manual
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetManualMode }));
                                        }
                                    }
                                }

                                break;

                            case 2:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));
                                    }
                                }
                                break;

                            case 3:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < 4; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                                    }
                                }
                                break;

                            default:
                                for (int i = 0; i < 4; i++)
                                {
                                    string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(E84Name))
                                    {
                                        NodeManagement.Get(NodeManagement.Get(E84Name).Associated_Node).E84Mode = Value.Equals("1") ? E84_Mode.AUTO : E84_Mode.MANUAL;
                                    }
                                }

                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.E84_SETTP1:
                        if (!Sanwa_E84SetTP1(TaskJob, Target,Value)) return;
                        break;
                    case TaskFlowManagement.Command.E84_SETTP2:
                        if (!Sanwa_E84SetTP2(TaskJob, Target, Value)) return;
                        break;
                    case TaskFlowManagement.Command.E84_SETTP3:
                        if (!Sanwa_E84SetTP3(TaskJob, Target, Value)) return;
                        break;
                    case TaskFlowManagement.Command.E84_SETTP4:
                        if (!Sanwa_E84SetTP4(TaskJob, Target, Value)) return;
                        break;
                    case TaskFlowManagement.Command.E84_SETTP5:
                        if (!Sanwa_E84SetTP5(TaskJob, Target, Value)) return;
                        break;
                    case TaskFlowManagement.Command.E84_SETTP6:
                        if (!Sanwa_E84SetTP6(TaskJob, Target, Value)) return;
                        break;
                    #endregion
                    #region OCR
                    case TaskFlowManagement.Command.OCR_ONLINE:
                        switch(TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!SystemConfig.Get().OfflineMode)
                                    if (IsNodeEnabledOrNull(Target.Name))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.OCRType.Online }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.OCR_OFFLINE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!SystemConfig.Get().OfflineMode)
                                    if (IsNodeEnabledOrNull(Target.Name))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.OCRType.Offline }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.OCR_READ:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (IsNodeEnabledOrNull(Target.Name))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.OCRType.Read }));
                                break;

                            case 1:
                                Node postion = NodeManagement.Get(Target.Associated_Node);
                                if(IsNodeEnabledOrNull(postion.Name))
                                {
                                    Wafer = JobManagement.Get(postion.Name,"1");
                                    Wafer.OCRPass = Target.OCR_Pass;
                                    Wafer.OCRResult = Target.OCR_ID;
                                    Wafer.OCRScore = Target.OCR_Score;

                                    Wafer.OCRFlag = true;

                                    _TaskReport.On_Job_Location_Changed(Wafer);
                                }
                                    

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    #endregion

                    default:
                        throw new NotSupportedException();
                }
                if (TaskJob.CheckList.Count != 0)
                {
                    foreach (TaskFlowManagement.ExcutedCmd eachCmd in TaskJob.CheckList)
                    {
                        Node CtrlNode = NodeManagement.Get(eachCmd.NodeName);
                        eachCmd.Txn.TaskObj = TaskJob;

                        if (CtrlNode.Connected)
                        {
                            if (!CtrlNode.SendCommand(eachCmd.Txn))
                            {
                                ///通訊傳送失敗
                                logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" +
                                    TaskJob.CurrentIndex.ToString() + "SendCommand Return false(1)");

                                //等30秒後
                                SpinWait.SpinUntil(() => !CtrlNode.IsExcuting, 30000);
                                if (!CtrlNode.IsExcuting)
                                {
                                    logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() +
                                        "SendCommand Again" + "if (!CtrlNode.IsExcuting)");
                                }
                                else
                                {
                                    logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() +
                                        "SendCommand Again" + "Timeout");
                                }

                                if (!CtrlNode.SendCommand(eachCmd.Txn))
                                {
                                    ///通訊傳送失敗
                                    logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" +
                                        TaskJob.CurrentIndex.ToString() + "SendCommand Return false(2)");

                                    CtrlNode.IsExcuting = false;
                                }
                            }
                        }
                        else if (SystemConfig.Get().OfflineMode)
                        {
                            //離線版本
                            CtrlNode.SendCommand(eachCmd.Txn);
                        }
                        else
                        {
                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = eachCmd.NodeName }, "S0300175");
                            return;
                        }
                        if (eachCmd.Txn.Method == Transaction.Command.LoadPortType.GetMappingDummy)
                        {
                            if (!SystemConfig.Get().OfflineMode)
                                break;
                        }
                    }
                }
                else
                {//recursive
                    if (!TaskJob.HasError && !TaskJob.Finished)
                    {
                        TaskJob.CurrentIndex++;
                        this.Excute(TaskJob);

                        return;
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " exception: " + e.StackTrace);
                AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), e.StackTrace);
            }
        }

        private void GetRobotPresence(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target,string type)
        {
            if(!SystemConfig.Get().OfflineMode)
            {
                switch (type)
                {
                    case "RSolenoid":
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "01" }));
                        break;
                    case "LSolenoid":
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "02" }));
                        break;
                    case "RPresence":
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "008" }));
                        break;
                    case "LPresence":
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "009" }));
                        break;
                    default:
                        break;
                }
            }
        }
        public override bool Sanwa_RobotWaferVAC(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Arm)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);

                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;


                    if (TaskJob.TaskName == TaskFlowManagement.Command.ROBOT_WAFER_HOLD)
                    {
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = Arm.Equals("1") ? "01" : "02", Value = "1" }));
                    }
                    else
                    {
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = Arm.Equals("1") ? "01" : "02", Value = "0" }));
                    }
                    break;

                case 1:
                    SpinWait.SpinUntil(() => false, 500);
                    break;

                case 2:
                    //取得R軸電磁閥最後狀態
                    GetRobotPresence(TaskJob, Target, "RSolenoid");
                    break;

                case 3:
                    //取得L軸電磁閥最後狀態
                    GetRobotPresence(TaskJob, Target, "LSolenoid");
                    break;

                case 4:
                    //確認R軸 Presence                   
                    GetRobotPresence(TaskJob, Target, "RPresence");
                    break;

                case 5:
                    //確認L軸 Presence
                    GetRobotPresence(TaskJob, Target, "LPresence");
                    break;



                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool Sanwa_RobotINIT(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);
                    //Servo on
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Servo, Value = "1" }));

                    break;

                case 1:
                    //開啟R軸電磁閥
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "1" }));

                    break;

                case 2:
                    //開啟L軸電磁閥
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "1" }));

                    break;

                case 3:
                    SpinWait.SpinUntil(() => false, 500);
                    break;

                case 4:
                    //確認R軸 Presence                   
                    if (IsNodeEnabledOrNull(Target.Name))
                        GetRobotPresence(TaskJob, Target, "RPresence");
                    break;

                case 5:
                    //確認L軸 Presence
                    if (IsNodeEnabledOrNull(Target.Name))
                        GetRobotPresence(TaskJob, Target, "LPresence");

                    break;

                case 6:
                    //R軸 Presence 不存在,則關閉R軸電磁閥
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if(!Target.R_Presence)
                        {
                            Job Wafer = JobManagement.Get(Target.Name + "_R", "1");
                            if (Wafer != null)
                            {
                                Wafer.LastNode = Wafer.Position;
                                Wafer.LastSlot = Wafer.Slot;
                                Wafer.Position = "";
                                Wafer.Slot = Wafer.Slot;

                                JobManagement.Remove(Wafer);
                                _TaskReport.On_Job_Location_Changed(Wafer);
                            }

                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "0" }));
                        }
                    }
                    break;

                case 7:
                    //L軸 Presence 不存在,則關閉L軸電磁閥
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if(!Target.L_Presence)
                        {
                            Job Wafer = JobManagement.Get(Target.Name + "_L", "1");
                            if (Wafer != null)
                            {
                                Wafer.LastNode = Wafer.Position;
                                Wafer.LastSlot = Wafer.Slot;
                                Wafer.Position = "";
                                Wafer.Slot = Wafer.Slot;

                                JobManagement.Remove(Wafer);
                                _TaskReport.On_Job_Location_Changed(Wafer);
                            }

                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "0" }));
                        }

                    }
                    break;

                case 8:
                    //設定模式(Normal or dry)
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (SystemConfig.Get().DummyMappingData)
                        {
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = "1" }));
                        }
                        else
                        {
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = "0" }));
                        }
                    }
                    break;

                case 9:
                    //取得R軸電磁閥最後狀態
                    if (IsNodeEnabledOrNull(Target.Name))
                        GetRobotPresence(TaskJob, Target, "RSolenoid");
                    break;

                case 10:
                    //取得L軸電磁閥最後狀態
                    if (IsNodeEnabledOrNull(Target.Name))
                        GetRobotPresence(TaskJob, Target, "LSolenoid");
                    break;

                case 11:
                    //if (!SystemConfig.Get().OfflineMode)
                    //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Speed, Value = "100" }));
                    break;

                case 12:
                    //取得速度
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));

                    break;

                case 13:
                    //取得模式
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetMode }));

                    break;

                case 14:
                    //取得異常
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetError, Value = "00" }));

                    break;

                case 15:
                    //更新Bobot狀態
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));

                    break;
                case 16:
                    //確認R軸 Presence                   
                    if (IsNodeEnabledOrNull(Target.Name))
                        GetRobotPresence(TaskJob, Target, "RPresence");
                    break;

                case 17:
                    //確認L軸 Presence
                    if (IsNodeEnabledOrNull(Target.Name))
                        GetRobotPresence(TaskJob, Target, "LPresence");
                    break;

                case 18:
                    //位置資訊
                    if (IsNodeEnabledOrNull(Target.Name))
                        GetRobotPosition(TaskJob, Target);
                    break;


                default:
                    Target.InitialComplete = true;
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }

        private void ResetRobotInterLock()
        {
            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF1", "TRUE");
            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF2", "TRUE");
        }
    }
}
