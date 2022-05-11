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
/// <summary>
///               
/// </summary>

namespace TransferControl.TaksFlow
{
    class Sorter_2P1R : BaseTaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(Sorter_2P1R));


        public Sorter_2P1R(IUserInterfaceReport TaskReport) : base(TaskReport)
        {
            _TaskReport = TaskReport;

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
            string RobotSpeed = "100";
            string AlignerSpeed = "100";

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
                        case "@RobotSpeed":
                            RobotSpeed = item.Value;
                            break;
                        case "@AlignerSpeed":
                            AlignerSpeed = item.Value;
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
                    //Sorter 初始化
                    case TaskFlowManagement.Command.SORTER_INIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                //Robot reset
                                if (!SystemConfig.Get().OfflineMode)
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));

                                //Loadport reset
                                if (!SystemConfig.Get().OfflineMode)
                                    for (int i = 0; i < LoadportCount; i++)
                                        if (IsNodeEnabledOrNull(string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'))))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0')), "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
                                break;

                            case 1:
                                if (IsNodeEnabledOrNull("ROBOT01"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                    {
                                        if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_INIT, new Dictionary<string, string>() { { "@Target", "ROBOT01" } }, "", TaskJob.MainTaskId).Promise())
                                        {
                                            //中止Task
                                            AbortTask(TaskJob, NodeManagement.Get("ROBOT01"), "TASK_ABORT");
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        NodeManagement.Get("ROBOT01").InitialComplete = true;
                                    }

                                }
                                break;

                            case 2://開啟 Loadport 所有Event
                                if (!SystemConfig.Get().OfflineMode)
                                    for (int i = 0; i < LoadportCount; i++)
                                        if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT0{0}", i + 1), "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.Mode, Value = "0" }));

                                if (!SystemConfig.Get().OfflineMode)
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(NodeManagement.Get("ROBOT01").Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Speed, Value = RobotSpeed }));

                                break;

                            case 3:
                                if (!SystemConfig.Get().OfflineMode)
                                    for (int i = 0; i < LoadportCount; i++)
                                        if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT0{0}", i + 1), "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.SetAllEvent, Value = "1" }));

                                //取得速度
                                if (!SystemConfig.Get().OfflineMode)
                                    if (IsNodeEnabledOrNull("ROBOT01"))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(NodeManagement.Get("ROBOT01").Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));

                                break;

                            case 4:
                                if (!SystemConfig.Get().OfflineMode)
                                    for (int i = 0; i < LoadportCount; i++)
                                        if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT0{0}", i + 1), "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                                break;

                            case 5:
                                for (int i = 0; i < LoadportCount; i++)
                                    if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                        NodeManagement.Get(string.Format("LOADPORT0{0}", i + 1)).InitialComplete = true;
                                break;

                            case 6:
                                if (IsNodeEnabledOrNull("ROBOT01"))
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_ORGSH, new Dictionary<string, string>() { { "@Target", "ROBOT01" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("ROBOT01"), "TASK_ABORT");
                                        break;
                                    }
                                }
                                break;

                            case 7:
                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT0{0}", i + 1), "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));

                                        NodeManagement.Get(string.Format("LOADPORT0{0}", i + 1)).OrgSearchComplete = true;
                                    }
                                }
                                break;

                            case 8:
                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    if (IsNodeEnabledOrNull(string.Format("LOADPORT0{0}", i + 1)))
                                    {
                                        if (!SystemConfig.Get().OfflineMode)
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(string.Format("LOADPORT0{0}", i + 1), "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                                    }
                                }
                                break;

                            case 9:
                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    string NodeName = string.Format("LOADPORT0{0}", i + 1);
                                    if (IsNodeEnabledOrNull(NodeName))
                                    {
                                        if (!SystemConfig.Get().OfflineMode)
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(NodeName,"FINISHED",new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = NodeManagement.Get(NodeName).Foup_Placement && NodeManagement.Get(NodeName).Foup_Presence ? "0" : "1" }));
                                        else
                                            NodeManagement.Get(NodeName).Load_LED = NodeManagement.Get(NodeName).Foup_Placement && NodeManagement.Get(NodeName).Foup_Presence ? "FALSE" : "TRUE";
                                    }
                                }
                                break;

                            case 10:
                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    string NodeName = string.Format("LOADPORT0{0}", i + 1);
                                    if (IsNodeEnabledOrNull(NodeName))
                                    {
                                        if (!SystemConfig.Get().OfflineMode)
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(NodeName, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = "0" }));
                                        else
                                            NodeManagement.Get(NodeName).UnLoad_LED =  "FALSE";
                                    }
                                }
                                break;

                            case 11:
                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    string NodeName = string.Format("LOADPORT0{0}", i + 1);
                                    if (IsNodeEnabledOrNull(NodeName))
                                    {
                                        if (!SystemConfig.Get().OfflineMode)
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(NodeName, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = "2" }));
                                        }
                                        else
                                        {
                                            NodeManagement.Get(NodeName).OPACCESS = true;
                                            NodeManagement.Get(NodeName).AccessSW_LED = "BLINK";
                                        }

                                    }
                                }
                                break;

                            case 12:
                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    string NodeName = string.Format("LOADPORT0{0}", i + 1);
                                    if (IsNodeEnabledOrNull(NodeName))
                                    {
                                        if (!SystemConfig.Get().OfflineMode)
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(NodeName, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED}));
                                    }
                                }
                                break;


                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
#region ROBOT
                    case TaskFlowManagement.Command.GET_CLAMP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_ABORT:
                        if (!Sanwa_RobotAbort(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_RESTR:
                        if (!Sanwa_RobotReStart(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_HOLD:
                        if (!Sanwa_RobotHold(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_GET:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);

                                //目標位置是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Position, TaskJob)) return;

                                //Robot 是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                if (Position.Type.ToUpper().Equals("LOADPORT") && !Position.IsMapping)
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");
                                break;

                            case 1:
                                GetRobotStatus(TaskJob, Target);

                                break;

                            case 2:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 3:
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
                                break;


                            case 4:
                                Position.RobotError = true;
                                string MethodName = Arm.Equals("3") ? Transaction.Command.RobotType.DoubleGet : Transaction.Command.RobotType.Get;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));

                                break;

                            case 5:
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
                                        }
                                        break;
                                }


                                if (SystemConfig.Get().OfflineMode)
                                {
                                    switch(Arm)
                                    {
                                        case "1":
                                            Target.R_Hold_Status = "1";
                                            break;
                                        case "2":
                                            Target.L_Hold_Status = "1";
                                            break;
                                        case "3":
                                            Target.R_Hold_Status = "1";
                                            Target.L_Hold_Status = "1";
                                            break;
                                    }
                                }
                                break;

                            case 6:
                                if (!SystemConfig.Get().OfflineMode && !SystemConfig.Get().DummyMappingData)
                                    if (Target.RobotArmType.Equals(Robot_ArmType.USE_RARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "008" }));

                                break;

                            case 7:
                                if (!SystemConfig.Get().OfflineMode && !SystemConfig.Get().DummyMappingData)
                                    if (Target.RobotArmType.Equals(Robot_ArmType.USE_LARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "009" }));

                                break;

                            case 8:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 9:
                                GetRobotStatus(TaskJob, Target);

                                break;

                            case 10:
                                if (!SystemConfig.Get().DummyMappingData)
                                {
                                    switch (Arm)
                                    {
                                        case "1":
                                            if(!Target.R_Hold_Status.Equals("1"))
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300179");
                                                return;
                                            }
                                            break;
                                        case "2":
                                            if(!Target.L_Hold_Status.Equals("1"))
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S030017A");
                                                return;
                                            }
                                            break;
                                        case "3":
                                            if(!Target.R_Hold_Status.Equals("1") || 
                                                !Target.L_Hold_Status.Equals("1"))
                                            {
                                                if(!Target.R_Hold_Status.Equals("1"))
                                                {
                                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300179");
                                                    return;
                                                }

                                                if (!Target.L_Hold_Status.Equals("1"))
                                                {
                                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S030017A");
                                                    return;
                                                }

                                            }
                                            break;
                                    }
                                }

                                break;

                            default:
                                Position.RobotError = false;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_PUT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                //目標位置是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Position, TaskJob)) return;

                                //Robot 是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                //是否需要重新Mapping
                                if (Position.Type.ToUpper().Equals("LOADPORT") && !Position.IsMapping)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");
                                    return;
                                }

                                break;

                            case 1:
                                GetRobotStatus(TaskJob, Target);
                                break;

                            case 2:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 3:
                                //目的地不能有片子
                                Wafer = null;
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

                                if (Wafer != null)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300171");
                                    return;
                                }

                                break;

                            case 4:
                                //移動Wafer
                                Position.RobotError = true;

                                //移動Wafer
                                string MethodName = Arm.Equals("3") ? Transaction.Command.RobotType.DoublePut : Transaction.Command.RobotType.Put;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));

                                break;

                            case 5:

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
                                            JobManagement.Remove(Wafer);
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
                                                JobManagement.Remove(Wafer);
                                        }
                                        break;
                                }

                                if (SystemConfig.Get().OfflineMode)
                                {
                                    switch (Arm)
                                    {
                                        case "1":
                                            Target.R_Hold_Status = "0";
                                            break;
                                        case "2":
                                            Target.L_Hold_Status = "0";
                                            break;
                                        case "3":
                                            Target.R_Hold_Status = "0";
                                            Target.L_Hold_Status = "0";
                                            break;
                                    }
                                }
                                break;

                            case 6:
                                if (!SystemConfig.Get().OfflineMode && !SystemConfig.Get().DummyMappingData)
                                    if (Target.RobotArmType.Equals(Robot_ArmType.USE_RARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "008" }));

                                break;

                            case 7:
                                if (!SystemConfig.Get().OfflineMode && !SystemConfig.Get().DummyMappingData)
                                    if (Target.RobotArmType.Equals(Robot_ArmType.USE_LARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "009" }));

                                break;

                            case 8:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 9:
                                GetRobotStatus(TaskJob, Target);
                                break;

                            default:
                                Position.RobotError = false;
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
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);

                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;


                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.ArmReturn }));

                                break;


                            default:
                                FinishTask(TaskJob);
                                return;
                        }
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
                    case TaskFlowManagement.Command.ROBOT_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!IsNodeInitialComplete(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                break;
                            case 1:
                                GetRobotStatus(TaskJob, Target);
                                break;

                            case 2:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 3:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Initialize }));
                                break;

                            case 4:
                                GetRobotRArmHoldStatus(TaskJob, Target);
                                break;

                            case 5:
                                GetRobotLArmHoldStatus(TaskJob, Target);
                                break;

                            case 6:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 7:
                                GetRobotStatus(TaskJob, Target);
                                break;

                            case 8:
                                if (JobManagement.Get("ROBOT01_R", "1") != null && Target.R_Hold_Status.Equals("0"))
                                {
                                    Wafer = JobManagement.Get("ROBOT01_R", "1");
                                    JobManagement.Remove(Wafer);
                                    Wafer.LastNode = Wafer.Position;
                                    Wafer.LastSlot = Wafer.Slot;
                                    Wafer.Position = "";
                                    Wafer.Slot = "";

                                    _TaskReport.On_Job_Location_Changed(Wafer);
                                }


                                //砍掉不必要的帳
                                if (JobManagement.Get("ROBOT01_L", "1") != null && Target.R_Hold_Status.Equals("0"))
                                {
                                    Wafer = JobManagement.Get("ROBOT01_L", "1");
                                    JobManagement.Remove(Wafer);
                                    Wafer.LastNode = Wafer.Position;
                                    Wafer.LastSlot = Wafer.Slot;
                                    Wafer.Position = "";
                                    Wafer.Slot = "";

                                    _TaskReport.On_Job_Location_Changed(Wafer);


                                }



                                break;

                            default:
                                OrgSearchCompleted(Target.Name);

                                string Name;
                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                        NodeManagement.Get(Name).RobotError = false;
                                }

                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_RESET:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                break;

                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));
                                break;

                            case 2:
                                GetRobotStatus(TaskJob, Target);
                                break;


                            case 3:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 4:
                                GetRobotRArmHoldStatus(TaskJob, Target);

                                break;

                            case 5:
                                GetRobotLArmHoldStatus(TaskJob, Target);

                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_INIT:
                        if (!Sanwa_RobotINIT(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_SAVE_LOG:
                        if (!Sanwa_RobotSaveLog(TaskJob, Target)) return;
                        break;

#endregion
#region LOADPORT
                    case TaskFlowManagement.Command.LOADPORT_INIT:
                        if (!TDK_LoadportINIT(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_ORGSH:
                        if (!TDK_LoadportORG(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_RESET:
                        if (!TDK_LoadportReset(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_FORCE_ORGSH:
                        if (!TDK_LoadportForceORG(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_READ_STATUS:
                        if (!TDK_LoadportReadStatus(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_CLAMP:
                        if (!TDK_LoadportClamp(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_UNCLAMP:
                        if (!TDK_LoadportUnclamp(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOCK:
                        if (!TDK_LoadportDock(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_UNDOCK:
                        if (!TDK_LoadportUndock(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_VAC_ON:
                        if (!TDK_LoadportVACOn(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_VAC_OFF:
                        if (!TDK_LoadportVACOff(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_UNLATCH:
                        if (!TDK_LoadportUnlatch(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_LATCH:
                        if (!TDK_LoadportLatch(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOOR_OPEN:
                        if (!TDK_LoadportDoorOpen(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOOR_CLOSE:
                        if (!TDK_LoadportDoorClose(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOOR_DOWN:
                        if (!TDK_LoadportDoorDown(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOOR_UP:
                        if (!TDK_LoadportDoorUp(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_ALL_OPEN:
                        if (!TDK_LoadportAllOpen(TaskJob)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_OPEN:
                    case TaskFlowManagement.Command.LOADPORT_OPEN_NOMAP:
                        if (!TDK_LoadportOpen(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_ALL_CLOSE:
                        if (!TDK_LoadportAllClose(TaskJob)) return;
                        break;
                    case TaskFlowManagement.Command.LOADPORT_CLOSE:
                    case TaskFlowManagement.Command.LOADPORT_CLOSE_NOMAP:
                        if (!TDK_LoadportClose(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_GET_MAPDT:
                        if (!TDK_LoadportGetMapData(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_RE_MAPPING:
                        if (!TDK_LoadportReMapping(TaskJob, Target)) return;
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
                        if (!TDK_LoadportReadLed(TaskJob, Target)) return;
                        break;
                    #endregion
#region NOTIFY
                    case TaskFlowManagement.Command.NOTIFY_CARRIER_OUT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.NOTIFY_SEMIAUTO_FINISHED:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
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
                            CtrlNode.SendCmdFinished = false;
                            CtrlNode.SendCommand(eachCmd.Txn);
                            CtrlNode.SendCmdFinished = true;
                        }
                        else
                        {
                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = eachCmd.NodeName }, "S0300175");
                            return;
                        }

                        //if (eachCmd.Txn.Method == Transaction.Command.LoadPortType.GetMappingDummy)
                        //{
                        //    if (!SystemConfig.Get().OfflineMode)
                        //        break;
                        //}
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
            catch (Exception e)
            {
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " exception: " + e.StackTrace);
                AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), e.StackTrace);
                return;
            }
            return;
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
                        if (Target.RobotArmType.Equals(Robot_ArmType.USE_RARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "1" }));

                    break;

                case 2:
                    //開啟L軸電磁閥
                    if (!SystemConfig.Get().OfflineMode)
                        if (Target.RobotArmType.Equals(Robot_ArmType.USE_LARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "1" }));

                    break;

                case 3:
                    SpinWait.SpinUntil(() => false, 500);
                    break;

                case 4:
                    //確認R軸 Presence                   
                    GetRobotRArmHoldStatus(TaskJob, Target);
                    break;

                case 5:
                    //確認L軸 Presence
                    GetRobotLArmHoldStatus(TaskJob, Target);
                    break;

                case 6:
                    //R軸 Presence 不存在,則關閉R軸電磁閥
                    if (!SystemConfig.Get().OfflineMode)
                        if (Target.RobotArmType.Equals(Robot_ArmType.USE_RARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                            if(Target.R_Hold_Status.Equals("0"))
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "0" }));

                    break;

                case 7:
                    //L軸 Presence 不存在,則關閉L軸電磁閥
                    if (!SystemConfig.Get().OfflineMode)
                        if (Target.RobotArmType.Equals(Robot_ArmType.USE_LARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                            if (Target.L_Hold_Status.Equals("0"))
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "0" }));

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
                    //if (!SystemConfig.Get().OfflineMode)
                    //    if (Target.RobotArmType.Equals(Robot_ArmType.USE_RARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                    //        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "01" }));
                    break;

                case 10:
                    //取得L軸電磁閥最後狀態
                    //if (!SystemConfig.Get().OfflineMode)
                    //    if (Target.RobotArmType.Equals(Robot_ArmType.USE_LARM) || Target.RobotArmType.Equals(Robot_ArmType.USE_RLARM))
                    //        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "02" }));
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
                    GetRobotStatus(TaskJob, Target);

                    break;
                case 16:
                    //確認R軸 Presence                   
                    GetRobotRArmHoldStatus(TaskJob, Target);
                    break;

                case 17:
                    GetRobotLArmHoldStatus(TaskJob, Target);
                    break;

                default:
                    Target.InitialComplete = true;
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
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
                    if(Arm.Equals("1"))
                    {
                        GetRobotRArmHoldStatus(TaskJob, Target);
                    }
                    else
                    {
                        GetRobotLArmHoldStatus(TaskJob, Target);
                    }

                    break;

                case 3:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = Arm.Equals("1") ? "01" : "02" }));
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool TDK_LoadportINIT(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.Mode, Value = "0" }));
                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.SetAllEvent, Value = "1" }));
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));
                    break;

                default:
                    Target.InitialComplete = true;
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool TDK_LoadportORG(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!IsNodeInitialComplete(Target, TaskJob)) return false;
                    AckTask(TaskJob);

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = Target.Foup_Placement && Target.Foup_Presence ? "0" : "1" }));
                    else
                        Target.Load_LED = Target.Foup_Placement && Target.Foup_Presence ? "FALSE" : "TRUE";
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = "0" }));
                    else
                        Target.UnLoad_LED = "FALSE";
                    break;


                case 4:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if(Target.Foup_Placement && Target.Foup_Presence)
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = "2" }));
                    }
                    else
                    {
                        if (Target.Foup_Placement && Target.Foup_Presence)
                        {
                            Target.OPACCESS = true;
                            Target.AccessSW_LED = "BLINK";
                        }
                    }

                    break;

                case 5:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));
                    break;

                default:

                    if (SystemConfig.Get().OfflineMode)
                        Target.Foup_Lock = false;

                    OrgSearchCompleted(Target.Name);
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool TDK_LoadportForceORG(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!IsNodeInitialComplete(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.ForceInitialPos }));

                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = Target.Foup_Placement && Target.Foup_Presence ? "0" : "1" }));
                    else
                        Target.Load_LED = Target.Foup_Placement && Target.Foup_Presence ? "FALSE" : "TRUE";
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value =  "0" }));
                    else
                        Target.UnLoad_LED = "FALSE";
                    break;


                case 4:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = "2" }));
                    }
                    else
                    {
                        Target.OPACCESS = true;
                        Target.AccessSW_LED = "BLINK";
                    }
                    break;

                case 5:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));

                    break;

                default:
                    if (SystemConfig.Get().OfflineMode)
                        Target.Foup_Lock = false;

                    OrgSearchCompleted(Target.Name);
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool TDK_LoadportClamp(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Clamp }));

                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = "1" }));
                    else
                        Target.Load_LED = "TRUE";
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = "0" }));
                    else
                        Target.UnLoad_LED = "FALSE";
                    break;

                case 4:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if(Target.OPACCESS)
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = "0" }));
                    }
                    else
                    {
                        Target.OPACCESS = false;
                        Target.AccessSW_LED = "FALSE";
                    }
                    break;

                case 5:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));
                    break;

                default:
                    if (SystemConfig.Get().OfflineMode)
                        Target.Foup_Lock = true;

                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool TDK_LoadportUnclamp(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    Target.IsMapping = false;

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Unload }));
                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = "0" }));
                    else
                        Target.Load_LED = "FALSE";
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = "1" }));
                    else
                        Target.UnLoad_LED = "TRUE";
                    break;

                case 4:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (Target.OPACCESS)
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = "0" }));
                    }
                    else
                    {
                        Target.OPACCESS = false;
                        Target.AccessSW_LED = "FALSE";
                    }
                    break;

                case 5:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));
                    break;

                default:
                    if (SystemConfig.Get().OfflineMode)
                        Target.Foup_Lock = false;

                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool TDK_LoadportDock(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Dock }));
                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = "1" }));
                    else
                        Target.Load_LED = "TRUE";
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = "0" }));
                    else
                        Target.UnLoad_LED = "FALSE";
                    break;

                case 4:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (Target.OPACCESS)
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = "0" }));
                    }
                    else
                    {
                        Target.OPACCESS = false;
                        Target.AccessSW_LED = "FALSE";
                    }
                    break;

                case 5:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));
                    break;

                default:

                    if (SystemConfig.Get().OfflineMode)
                        Target.Foup_Lock = true;

                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool TDK_LoadportUndock(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UntilUnDock }));

                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = "0" }));
                    else
                        Target.Load_LED = "FALSE";
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = "1" }));
                    else
                        Target.UnLoad_LED = "TRUE";
                    break;

                case 4:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (Target.OPACCESS)
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = "0" }));
                    }
                    else
                    {
                        Target.OPACCESS = false;
                        Target.AccessSW_LED = "FALSE";
                    }
                    break;

                case 5:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));
                    break;

                default:
                    if (SystemConfig.Get().OfflineMode)
                        Target.Foup_Lock = true;

                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool TDK_LoadportOpen(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    //移除所有Wafer
                    foreach (Job eachJob in JobManagement.GetByNode(Target.Name))
                        JobManagement.Remove(eachJob);

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MappingLoad }));
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
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                        }
                    }
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = "1" }));
                    else
                        Target.Load_LED = "TRUE";
                    break;

                case 4:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = "0" }));
                    else
                        Target.UnLoad_LED = "FALSE";
                    break;

                case 5:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (Target.OPACCESS)
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = "0" }));
                    }
                    else
                    {
                        Target.OPACCESS = false;
                        Target.AccessSW_LED = "FALSE";
                    }
                    break;

                case 6:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));
                    break;

                default:
                    if (SystemConfig.Get().OfflineMode)
                        Target.Foup_Lock = true;

                    Target.LoadTime = DateTime.Now;
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool TDK_LoadportClose(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    Target.IsMapping = false;

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UntilDoorCloseVacOFF }));

                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = "0" }));
                    else
                        Target.Load_LED = "FALSE";
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = "1" }));
                    else
                        Target.UnLoad_LED = "TRUE";
                    break;

                case 4:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (Target.OPACCESS)
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = "0" }));
                    }
                    else
                    {
                        Target.OPACCESS = false;
                        Target.AccessSW_LED = "FALSE";
                    }
                    break;

                case 5:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));
                    break;

                default:
                    if (SystemConfig.Get().OfflineMode)
                        Target.Foup_Lock = true;

                    FinishTask(TaskJob);
                    return false;
            }
            return true;
        }
    }
}
