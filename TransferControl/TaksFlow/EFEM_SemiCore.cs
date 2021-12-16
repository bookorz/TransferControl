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
    class EFEM_SemiCore : BaseTaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(EFEM_SemiCore));
        //IUserInterfaceReport _TaskReport;
        private int Arm1ForkNum = 5;
        public EFEM_SemiCore(IUserInterfaceReport TaskReport):base(TaskReport)
        {
            _TaskReport = TaskReport;

            LoadportCount = 4;
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
            if(!SystemConfig.Get().OfflineMode)
                if (MainControl.Instance.DIO.GetIO("DIN", "SAFETYRELAY").ToUpper().Equals("TRUE"))
                {
                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM" }, "S0300170");
                    return;
                }


            try
            {
                switch (TaskJob.TaskName)
                {
                    case TaskFlowManagement.Command.RESET_ALL:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                string NodeName = "ROBOT01";
                                if(IsNodeEnabledOrNull(NodeName))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));

                                //Loadport Reset
                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    NodeName = string.Format("LOADPORT{0}", (i+1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(NodeName))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(NodeName, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
                                }

                                if(!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i+1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));    
                                    }
                                }
                                break;
                            case 2:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                                    }
                                }
                                break;
                            case 3:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                        {
                                            if(NodeManagement.Get(E84Name).E84Mode == E84_Mode.AUTO)
                                            {
                                                //確認天車完全移走
                                                if (!NodeManagement.Get(E84Name).E84IOStatus["VALID"] &&
                                                    !NodeManagement.Get(E84Name).E84IOStatus["CS_0"] &&
                                                    !NodeManagement.Get(E84Name).E84IOStatus["CS_1"] &&
                                                    !NodeManagement.Get(E84Name).E84IOStatus["AM_AVBL"] &&
                                                    !NodeManagement.Get(E84Name).E84IOStatus["TR_REQ"] &&
                                                    !NodeManagement.Get(E84Name).E84IOStatus["BUSY"] &&
                                                    !NodeManagement.Get(E84Name).E84IOStatus["COMPT"] &&
                                                    !NodeManagement.Get(E84Name).E84IOStatus["CONT"] &&
                                                    !NodeManagement.Get(E84Name).E84IOStatus["HO_AVBL"])
                                                {
                                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.Reset }));
                                                }
                                            }
                                        }
                                    }
                                }
                                break;

                            case 4:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                        {
                                            if (NodeManagement.Get(NodeManagement.Get(E84Name).Associated_Node).E84Mode == E84_Mode.AUTO)
                                            {
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetAutoMode }));
                                            }
                                        }
                                    }
                                }
                                break;

                            case 5:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));
                                    }
                                }
                                break;

                            default:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                                    }
                                }

                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALL_INIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                string NodeName = "ROBOT01";
                                if (IsNodeEnabledOrNull(NodeName))
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_INIT, 
                                        new Dictionary<string, string>()
                                        { { "@Target", NodeName } }, "", 
                                        TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }

                                for(int i = 0; i< LoadportCount; i++)
                                {
                                    NodeName = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(NodeName))
                                    {
                                        if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_INIT, 
                                            new Dictionary<string, string>()
                                            { { "@Target", NodeName } },
                                            "", TaskJob.MainTaskId).Promise())
                                        {
                                            //中止Task
                                            AbortTask(TaskJob, null, "TASK_ABORT");
                                            break;
                                        }

                                        NodeName = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(NodeName))
                                        {
                                            if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT,
                                                new Dictionary<string, string>()
                                                { { "@Target", NodeName} },
                                                "", TaskJob.MainTaskId).Promise())
                                            {
                                                //中止Task
                                                AbortTask(TaskJob, null, "TASK_ABORT");
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALL_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                string Name = "ROBOT01";
                                if (IsNodeEnabledOrNull(Name))
                                    if (!IsNodeInitialComplete(NodeManagement.Get(Name), TaskJob)) return;

                                for(int i = 0; i< LoadportCount; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i+1).ToString().PadLeft(2,'0'));
                                    if (IsNodeEnabledOrNull(Name))
                                        if (!IsNodeInitialComplete(NodeManagement.Get(Name), TaskJob)) return;
                                }

 
                                
                                Name = "ROBOT01";
                                if (IsNodeEnabledOrNull(Name))
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_ORGSH, new Dictionary<string, string>() { { "@Target", Name } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        return;
                                    }
                                }

                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    if(!SystemConfig.Get().OfflineMode)
                                    {
                                        Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                    }
                                }

                                break;

                            case 1:
                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                    {
                                        Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                                    }
                                }
                                break;

                            default:

                                //OrgSearchCompleted("ROBOT01");

                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    OrgSearchCompleted(Name);
                                }

                                FinishTask(TaskJob);

                                return;
                        }
                        break;
#region ROBOT
                    case TaskFlowManagement.Command.ROBOT_ABORT:
                        if(!Sanwa_RobotAbort(TaskJob, Target))  return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_RESTR:
                        if(!Sanwa_RobotReStart(TaskJob, Target))    return;   
                        break;

                    case TaskFlowManagement.Command.ROBOT_HOLD:
                        if(!Sanwa_RobotHold(TaskJob, Target))   return;
                        break;

                    case TaskFlowManagement.Command.GET_CLAMP:
                        if(!Sanwa_RobotGetClamp(TaskJob, Target, Value))    return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_PUT:
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


                                //是否需要重新Mapping
                                if (Position.Type.ToUpper().Equals("LOADPORT") && !Position.IsMapping)
                                {
                                    ////AckTask(TaskJob);
                                    //if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_RE_MAPPING, new Dictionary<string, string>() { { "@Target", Position.Name } }, "", TaskJob.MainTaskId).Promise())
                                    //{
                                    //    //中止Task
                                    //    TaskJob.State = TaskFlowManagement.CurrentProcessTask.TaskState.Abort;
                                    //    AbortTask(TaskJob, null, "S0300001");//LOAD_PORT_NOT_READY

                                    //    break;
                                    //}
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");

                                }
                                break;
                            case 1:
                                //Check presence
                                if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                {
                                    if(!TaskJob.Params["@BYPASS_CHECK"].ToUpper().Equals("TRUE"))
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
                                    Wafer = JobManagement.Get(Position.Name, Slot);
                                    //上Arm有五個Forks
                                    if (Arm.Equals("1"))
                                    {
                                        for(int i = 0; i< Arm1ForkNum; i++)
                                        {
                                            Wafer = JobManagement.Get(Position.Name, (Convert.ToInt32(Slot) + i).ToString());
                                            if (Wafer != null) break;
                                        }
                                    }

                                    if (Wafer != null)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300171");
                                        return;
                                    }
                                }
                                break;

                            case 2:
                                //移動Wafer
                                string MethodName = Arm.Equals("1") ? Transaction.Command.RobotType.PutByRArm : Transaction.Command.RobotType.PutByLArm;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                break;

                            case 3:
                                string TargetName = Arm.Equals("1") ? Target.Name + "_R" : Target.Name + "_L";

                                //搬帳
                                for (int i = 0; i< Arm1ForkNum; i++)
                                {
                                    Wafer = JobManagement.Get(TargetName, (i+1).ToString());
                                    if (Wafer == null)
                                    {
                                        Wafer = JobManagement.Add();
                                        Wafer.MapFlag = true;
                                    }

                                    Wafer.LastNode = Wafer.Position;
                                    Wafer.LastSlot = Wafer.Slot;
                                    Wafer.Position = Position.Name;
                                    Wafer.Slot = (Convert.ToInt16(Slot)+i) .ToString();

                                    _TaskReport.On_Job_Location_Changed(Wafer);

                                    if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                    {
                                        JobManagement.Remove(Wafer);
                                    }

                                    //L Arm 只有一個Forks
                                    if (Arm.Equals("2")) break;
                                }

                                if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "true");

                                break;

                            case 4:
                                //取得R軸電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "01" }));
                                break;

                            case 5:
                                //取得L軸電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "02" }));
                                break;

                            case 6:
                                //確認R軸 Presence                   
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "008" }));
                                break;

                            case 7:
                                //確認L軸 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "009" }));
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

                                if (Position.Type.Equals("LOADPORT") && !Position.IsMapping)
                                {
                                    //if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_RE_MAPPING, new Dictionary<string, string>() { { "@Target", Position.Name } }, "", TaskJob.MainTaskId).Promise())
                                    //{
                                    //    //中止Task
                                    //    AbortTask(TaskJob, null, "TASK_ABORT");

                                    //    break;
                                    //}
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");

                                }
                                break;
                            case 1:
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
                                    Wafer = JobManagement.Get(Position.Name, Slot);

                                    for (int i = 0; i < Arm1ForkNum; i++)
                                    {
                                        Wafer = JobManagement.Get(Position.Name, (Convert.ToInt32(Slot) + i).ToString());

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

                                        if (Arm.Equals("2")) break;
                                    }
                                }
                                break;
                            case 2:
                                string MethodName = Arm.Equals("1") ? Transaction.Command.RobotType.GetByRArm : Transaction.Command.RobotType.GetByLArm;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));

                                break;
                            case 3:
                                if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                {
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "TRUE");
                                }

                                //搬帳
                                for (int i = 0; i < Arm1ForkNum; i++)
                                {
                                    Wafer = JobManagement.Get(Position.Name, (Convert.ToInt32(Slot) + i).ToString());
                                    if (Wafer == null)
                                    {
                                        Wafer = JobManagement.Add();
                                        Wafer.MapFlag = true;
                                    }
                                    Wafer.LastNode = Wafer.Position;
                                    Wafer.LastSlot = Wafer.Slot;
                                    Wafer.Position = Arm.Equals("1") ? Target.Name + "_R" : Target.Name + "_L";
                                    Wafer.Slot = (i + 1).ToString();

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

                                    //L Arm 只有一個Forks
                                    if (Arm.Equals("2")) break;
                                }

                                break;

                            case 4:
                                //取得R軸電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "01" }));
                                break;

                            case 5:
                                //取得L軸電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "02" }));
                                break;

                            case 6:
                                //確認R軸 Presence                   
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "008" }));
                                break;

                            case 7:
                                //確認L軸 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "009" }));
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
                                    }
                                    else
                                    {
                                        if(!TaskJob.Params["@From_BYPASS_CHECK"].Equals("TRUE"))
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

                                        //
                                    }
                                }
                                if (ToPosition != null)
                                {
                                    if (!ToPosition.Type.ToUpper().Equals("LOADLOCK"))
                                    {
                                        if (!CheckNodeStatusOnTaskJob(ToPosition, TaskJob)) return;
                                    }
                                    else
                                    {
                                        if(!TaskJob.Params["@To_BYPASS_CHECK"].Equals("TRUE"))
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
                                    for (int i = 0; i < Arm1ForkNum; i++)
                                    {
                                        Wafer = JobManagement.Get(TaskJob.Params["@FromPosition"], (Convert.ToInt32(TaskJob.Params["@FromSlot"]) + i).ToString());
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

                                        //L Arm 只有一個Forks
                                        if (TaskJob.Params["@FromArm"].Equals("2")) break;
                                    }
                                }

                                if (TaskJob.Params["@FromPosition"].Equals(TaskJob.Params["@ToPosition"]) && TaskJob.Params["@FromSlot"].Equals(TaskJob.Params["@ToSlot"]))
                                {

                                }
                                else
                                {
                                    Wafer = JobManagement.Get(TaskJob.Params["@ToPosition"], TaskJob.Params["@ToSlot"]);
                                    //上Arm有五個Forks
                                    if (TaskJob.Params["@ToArm"].Equals("1"))
                                    {
                                        for (int i = 0; i < Arm1ForkNum; i++)
                                        {
                                            Wafer = JobManagement.Get(TaskJob.Params["@ToPosition"], (Convert.ToInt32(TaskJob.Params["@ToSlot"]) + i).ToString());
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

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_GET, new Dictionary<string, string>() { { "@Target", Target.Name }, { "@Position", TaskJob.Params["@FromPosition"] }, { "@Arm", TaskJob.Params["@FromArm"] },{ "@Slot", TaskJob.Params["@FromSlot"] }, { "@BYPASS_CHECK", TaskJob.Params["@From_BYPASS_CHECK"] }, { "@IsTransCommand", "TRUE" } }, "", TaskJob.MainTaskId).Promise())
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
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);

                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                break;
                            case 1:

                                if(!SystemConfig.Get().OfflineMode)
                                    if (TaskJob.TaskName == TaskFlowManagement.Command.ROBOT_PUTWAIT)
                                    {
                                        //需在Config檔中，新增Point2
                                        string MethodName = Arm.Equals("1") ? Transaction.Command.RobotType.PutWaitByRArm : Transaction.Command.RobotType.PutWaitByLArm;
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                    }
                                    else
                                    {
                                        //需在Config檔中，新增Point2
                                        string MethodName = Arm.Equals("1") ? Transaction.Command.RobotType.GetWaitByRArm : Transaction.Command.RobotType.GetWaitByLArm;
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                    }

                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_WAFER_RELEASE:
                    case TaskFlowManagement.Command.ROBOT_WAFER_HOLD:
                        if(!Sanwa_RobotWaferVAC(TaskJob, Target, Arm))  return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_MODE:
                        if(!Sanwa_RobotMode(TaskJob, Target, Value))    return;
                        break;
                    case TaskFlowManagement.Command.ROBOT_RETRACT:
                        if(!Sanwa_RobotRetract(TaskJob, Target))    return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_SPEED:
                        if(!Sanwa_RobotSpeed(TaskJob, Target, Value))   return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_SERVO:
                        if(!Sanwa_RobotServo(TaskJob, Target, Value))   return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_HOME:
                        if(!Sanwa_RobotHome(TaskJob, Target))   return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!IsNodeInitialComplete(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                break;

                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.OrginSearch }));
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
                        if(!Sanwa_RobotReset(TaskJob, Target))  return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_INIT:
                        if(!Sanwa_RobotINIT(TaskJob, Target))   return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_SAVE_LOG:
                        if(!Sanwa_RobotSaveLog(TaskJob, Target))    return;
                        break;
#endregion
#region LOADPORT
                    case TaskFlowManagement.Command.LOADPORT_INIT:
                        if(!TDK_LoadportINIT(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_ORGSH:
                        if(!TDK_LoadportORG(TaskJob, Target))   return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_RESET:
                        if(!TDK_LoadportReset(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_FORCE_ORGSH:
                        if(!TDK_LoadportForceORG(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_READ_STATUS:
                        if(!TDK_LoadportReadStatus(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_CLAMP:
                        if(!TDK_LoadportClamp(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_UNCLAMP:
                        //if(!TDK_LoadportUnclamp(TaskJob, Target))   return;
                        if(!TDK_LoadportClose(TaskJob, Target))     return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOCK:
                        if(!TDK_LoadportDock(TaskJob, Target))  return;
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
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_VAC_ON:
                        if(!TDK_LoadportVACOn(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_VAC_OFF:
                        if(!TDK_LoadportVACOff(TaskJob, Target))    return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_UNLATCH:
                        if(!TDK_LoadportUnlatch(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_LATCH:
                        if(!TDK_LoadportLatch(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOOR_OPEN:
                        if(!TDK_LoadportDoorOpen(TaskJob, Target))  return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOOR_CLOSE:
                        if(!TDK_LoadportDoorClose(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOOR_DOWN:
                        if(!TDK_LoadportDoorDown(TaskJob, Target))  return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_DOOR_UP:
                        if(!TDK_LoadportDoorUp(TaskJob, Target))    return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_ALL_OPEN:
                        if(!TDK_LoadportAllOpen(TaskJob))   return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_OPEN:
                    case TaskFlowManagement.Command.LOADPORT_OPEN_NOMAP:
                        if(!TDK_LoadportOpen(TaskJob, Target))  return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_ALL_CLOSE:
                        if(!TDK_LoadportAllClose(TaskJob))  return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_CLOSE:
                    case TaskFlowManagement.Command.LOADPORT_CLOSE_NOMAP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                //避免尚未關完門就把Robot伸過來
                                NodeManagement.Get(Target.Name).IsMapping = false;

                                AckTask(TaskJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UntilDoorCloseVacOFF }));

                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_GET_MAPDT:
                        if(!TDK_LoadportGetMapData(TaskJob, Target))    return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_RE_MAPPING:
                        if(!TDK_LoadportReMapping(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_SET_OPACCESS_INDICATOR:
                        if(!TDK_LoadportSetOPAccessIndicator(TaskJob, Target, Value))   return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_SET_LOAD_INDICATOR:
                        if(!TDK_LoadportSetLoadIndicator(TaskJob, Target, Value))   return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_SET_UNLOAD_INDICATOR:
                        if(!TDK_LoadportSetUnloadIndicator(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.LOADPORT_READ_LED:
                        if(!TDK_LoadportReadLed(TaskJob, Target))   return;
                        break;

#endregion
#region CSTID
                    case TaskFlowManagement.Command.GET_CSTID:
                        if(!GetCSTID(TaskJob, Target))  return;
                        break;
                    case TaskFlowManagement.Command.SET_CSTID:
                        if(!SetCSTID(TaskJob, Target,Value))    return;
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

                                if(TaskJob.TaskName == TaskFlowManagement.Command.E84_MODE)
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
                                    string E84Name = string.Format("E84{0}", (i+1).ToString().PadLeft(2, '0'));
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
            catch (Exception e)
            {
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " exception: " + e.StackTrace);
                AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), e.StackTrace);
                return;
            }
            return;
        }
        public override bool Sanwa_RobotGetClamp(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }

        private void ResetRobotInterLock()
        {
            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF1", "TRUE");
            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF2", "TRUE");

            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF1S", "TRUE");
            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF2S", "TRUE");
        }
    }
}
