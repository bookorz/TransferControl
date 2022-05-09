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
/// Git Test
/// 華海清科 12吋 1 Robot 2 Loadport 2 Loadlock
/// Robot : 1 Arm 同時包含 Clamp & Vacumm
/// 取放片流程 : 
/// Vacuum => LP(下方取片) -> 翻轉Wafer -> BF 下方放片 
/// Clamp => LP 下方取片 -> BF 下方放片
/// Clamp => BF 下方取片 -> LP 下方放片
///               
/// </summary>

namespace TransferControl.TaksFlow
{
    class EFEM_HWATSING_2P : BaseTaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(EFEM_HWATSING_2P));

        private readonly int ArmCount;
        public EFEM_HWATSING_2P(IUserInterfaceReport TaskReport) : base(TaskReport)
        {
            _TaskReport = TaskReport;
            ArmCount = 1;


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
                    case TaskFlowManagement.Command.RESET_ALL:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
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

                            case 1:
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

                            case 2:
                                string Name = "";
                                if (IsNodeEnabledOrNull("ROBOT01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));


                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    //Loadport Reset
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));

                                    //E84 RESET
                                    if (!SystemConfig.Get().OfflineMode)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                        {
                                            if (NodeManagement.Get(E84Name).E84Mode == E84_Mode.AUTO)
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


                            case 3:
                                if (!SystemConfig.Get().OfflineMode)
                                {

                                    SpinWait.SpinUntil(() => false, 600);

                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));
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
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                                    }
                                }
                                break;



                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALL_INIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);
                                string Name = "ROBOT01";
                                if (IsNodeEnabledOrNull(Name))
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_INIT,
                                        new Dictionary<string, string>()
                                        { { "@Target", Name } }, "",
                                        TaskJob.MainTaskId).Promise()
                                        )
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }
                                break;

                            case 1:

                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                    {
                                        if (!TaskFlowManagement.Excute(
                                            TaskFlowManagement.Command.LOADPORT_INIT,
                                            new Dictionary<string, string>()
                                            { { "@Target", Name } }, "",
                                            TaskJob.MainTaskId).Promise()
                                            )
                                        {
                                            //中止Task
                                            AbortTask(TaskJob, null, "TASK_ABORT");
                                            return;
                                        }
                                    }

                                    Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                    {
                                        if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT,
                                            new Dictionary<string, string>()
                                            { { "@Target", Name} },
                                            "", TaskJob.MainTaskId).Promise())
                                        {
                                            //中止Task
                                            AbortTask(TaskJob, null, "TASK_ABORT");
                                            break;
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


                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                        if (!IsNodeInitialComplete(NodeManagement.Get(Name), TaskJob)) return;
                                }

                                Name = "ROBOT01";
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_ORGSH, new Dictionary<string, string>() { { "@Target", Name } }, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, null, "TASK_ABORT");

                                    break;
                                }

                                for (int i = 0; i < LoadportCount; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                }

                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                                    }
                                }

                                break;

                            default:

                                OrgSearchCompleted("ROBOT01");

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
                        if (!Sanwa_RobotAbort(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_RESTR:
                        if (!Sanwa_RobotReStart(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_HOLD:
                        if (!Sanwa_RobotHold(TaskJob, Target)) return;
                        break;

                    case TaskFlowManagement.Command.GET_CLAMP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
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

                                if (Position.Type.ToUpper().Equals("LOADPORT") && !Position.IsMapping)
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");
                                break;

                            case 1:
                                GetRobotPresence(TaskJob, Target);
                                break;

                            case 2:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 3:
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


                            case 4:
                                //移動Wafer
                                string arm = "1";

                                RobotPoint point = PointManagement.GetPoint(Target.Name, Position.Name );
                                //Arm 1 => Clamp
                                //Arm 2 => Vacuum                                
                                if(Arm.Equals("1"))
                                {
                                    arm = "0";
                                    point.Point = point.ClampPoint;
                                }
                                else
                                {
                                    arm = "1";
                                    point.Point = point.VacuumPoint;
                                }

                                Position.RobotError = true;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Get, Position = Position.Name, Arm = arm, Slot = Slot }));
                                break;

                            case 5:
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

                                if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "TRUE");

                                if(SystemConfig.Get().OfflineMode)
                                {
                                    if(Arm.Equals("1"))
                                    {
                                        Target.R_Presence = true;
                                    }
                                    else
                                    {
                                        Target.L_Presence = true;
                                    }
                                }
                                break;

                            case 6:
                                GetRobotPresence(TaskJob, Target);
                                break;

                            case 7:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 8:
                                if(!SystemConfig.Get().DummyMappingData)
                                {
                                    if (Arm.Equals("1"))
                                    {
                                        if (!Target.R_Presence)
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300179");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        if (!Target.L_Presence)
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S030017A");
                                            return;
                                        }
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
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");
                                }
                                break;

                            case 1:
                                GetRobotPresence(TaskJob, Target);
                                break;

                            case 2:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 3:
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
                                    Wafer = JobManagement.Get(Position.Name, Slot);

                                    if (Wafer != null)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300171");
                                        return;
                                    }
                                }
                                break;

                            case 4:
                                //移動Wafer
                                string arm = "0";

                                RobotPoint point = PointManagement.GetPoint(Target.Name, Position.Name);

                                //Arm 1 => Clamp
                                //Arm 2 => Vacuum  
                                if (Arm.Equals("1"))
                                {
                                    arm = "0";
                                    point.Point = point.ClampPoint;
                                }
                                else
                                {
                                    arm = "1";
                                    point.Point = point.VacuumPoint;
                                }

                                Position.RobotError = true;

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Put, Position = Position.Name, Arm = arm, Slot = Slot }));

                                break;

                            case 5:
                                Wafer = JobManagement.Get(Arm.Equals("1") ? Target.Name + "_R" : Target.Name + "_L", "1");
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
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "true");
                                }


                                if (SystemConfig.Get().OfflineMode)
                                {
                                    if (Arm.Equals("1"))
                                    {
                                        Target.R_Presence = false;
                                    }
                                    else
                                    {
                                        Target.L_Presence = false;
                                    }
                                }
                                break;

                            case 6:
                                GetRobotPresence(TaskJob, Target);
                                break;

                            case 7:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            default:
                                Position.RobotError = false;
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

                                    }
                                }

                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                //Get safety check
                                if (!TaskJob.Params["@FromPosition"].Contains("BF") && !TaskJob.Params["@FromPosition"].Contains("LL"))
                                {
                                    Wafer = JobManagement.Get(TaskJob.Params["@FromPosition"], TaskJob.Params["@FromSlot"]);
                                    if (Wafer == null)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300162");
                                        return;
                                    }
                                }

                                if (TaskJob.Params["@FromPosition"].Equals(TaskJob.Params["@ToPosition"]) && TaskJob.Params["@FromSlot"].Equals(TaskJob.Params["@ToSlot"]))
                                {

                                }
                                else
                                {
                                    Wafer = null;
                                    Wafer = JobManagement.Get(TaskJob.Params["@ToPosition"], TaskJob.Params["@ToSlot"]);

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
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300015");
                                    return;
                                }

                                if (!Target.OrgSearchComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300041");
                                    return;
                                }

                                AckTask(TaskJob);
                                break;

                            case 1:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 2:
                                string arm = Arm.Equals("1") ? "0" : "1";
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.PutWait, Position = Position.Name, Arm = arm, Slot = "1" }));
                                break;

                            case 3:
                                GetRobotPresence(TaskJob, Target);
                                break;

                            case 4:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }

                        break;

                    case TaskFlowManagement.Command.ROBOT_GETWAIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300015");
                                    return;
                                }

                                if (!Target.OrgSearchComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300041");
                                    return;
                                }

                                AckTask(TaskJob);

                                break;
                            case 1:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 2:
                                string arm = "0";

                                RobotPoint point = PointManagement.GetPoint(Target.Name, Position.Name);
                                //Arm 1 => Clamp
                                //Arm 2 => Vacuum                                
                                if (Arm.Equals("1"))
                                {
                                    arm = "0";
                                    point.Point = point.ClampPoint;
                                }
                                else
                                {
                                    arm = "1";
                                    point.Point = point.VacuumPoint;
                                }
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.GetWait, Position = Position.Name, Arm = arm, Slot = "1" }));
                                break;

                            case 3:
                                GetRobotPresence(TaskJob, Target);
                                break;

                            case 4:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_WAFER_RELEASE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300015");
                                    return;
                                }

                                if (!Target.OrgSearchComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300041");
                                    return;
                                }

                                AckTask(TaskJob);

                                if (Arm.Equals("1"))
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Flip, Value = "0" }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Flip, Value = "1" }));
                                }
                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.WaferRelease, Arm = Arm.Equals("1")? "0":"1" }));
                                break;
                            case 2:
                                GetRobotPresence(TaskJob, Target);
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_WAFER_HOLD:
                        switch(TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300015");
                                    return;
                                }

                                if (!Target.OrgSearchComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300041");
                                    return;
                                }

                                AckTask(TaskJob);
                                if(Arm.Equals("1"))
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Flip, Value = "0" }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Flip, Value = "1" }));
                                }

                                break;

                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.WaferHold, Arm = Arm.Equals("1") ? "0" : "1" }));
                                break;

                            case 2:
                                GetRobotPresence(TaskJob, Target);
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_MODE:
                        if (!Sanwa_RobotMode(TaskJob, Target, Value)) return;
                        break;

                    case TaskFlowManagement.Command.ROBOT_RETRACT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300015");
                                    return;
                                }

                                if (!Target.OrgSearchComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300041");
                                    return;
                                }

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.ArmReturn }));

                                break;


                            default:
                                MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF1", "TRUE");
                                MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF2", "TRUE");
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
                                GetRobotPresence(TaskJob, Target);
                                break;
                            case 2:
                                GetRobotPosition(TaskJob, Target);
                                break;
                            case 3:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.OrginSearch }));
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Initialize }));
                                break;
                            case 4:
                                GetRobotPresence(TaskJob, Target);
                                break;
                            case 5:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 6:

                                //Job Wafer = null;
                                //砍掉不必要的帳
                                if (JobManagement.Get("ROBOT01_R","1") != null && !Target.R_Presence)
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
                                if (JobManagement.Get("ROBOT01_L", "1") != null && !Target.L_Presence)
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
                                ResetRobotInterLock();

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
                        switch(TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                break;

                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));
                                break;

                            case 2:
                                GetRobotPresence(TaskJob, Target);
                                break;


                            case 3:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_INIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                break;
                            case 1:
                                //if (!SystemConfig.Get().OfflineMode)
                                //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Servo, Value = "1" }));
                                break;

                            case 2:
                                //if (!SystemConfig.Get().OfflineMode)
                                //{
                                //    if (SystemConfig.Get().DummyMappingData)
                                //    {
                                //        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = "1" }));
                                //    }
                                //    else
                                //    {
                                //        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = "0" }));
                                //    }
                                //}
                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = "1" }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = "0" }));
                                }

                                break;

                            case 3:
                                //if (!SystemConfig.Get().OfflineMode)
                                //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));
                                break;

                            case 4:
                                //if (!SystemConfig.Get().OfflineMode)
                                //   TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetMode }));
                                break;

                            case 5:
                                //if (!SystemConfig.Get().OfflineMode)
                                //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetError, Value = "00" }));
                                break;

                            case 6:
                                //if (!SystemConfig.Get().OfflineMode)
                                //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Servo, Value = "1" }));
                                break;

                            case 7:
                                //if (!SystemConfig.Get().OfflineMode)
                                //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Initialize }));
                                break;
                            case 8:
                                GetRobotPresence(TaskJob, Target);
                                break;

                            case 9:
                                GetRobotPosition(TaskJob, Target);
                                break;

                            case 10:
                                //if (!SystemConfig.Get().OfflineMode)
                                //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));
                                break;

                            default:
                                Target.InitialComplete = true;
                                FinishTask(TaskJob);
                                return;
                        }
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
                    #region CSTID
                    case TaskFlowManagement.Command.GET_CSTID:
                        if (!GetCSTID(TaskJob, Target, Value)) return;

                        break;
                    case TaskFlowManagement.Command.SET_CSTID:
                        if (!SetCSTID(TaskJob, Target, Value)) return;
                        break;
                    #endregion
                    #region E84
                    case TaskFlowManagement.Command.E84_INIT:
                        if (!Sanwa_E84INIT(TaskJob, Target)) return;
                        break;
                    case TaskFlowManagement.Command.RESET_E84:
                        if (!Sanwa_E84Reset(TaskJob, Target)) return;
                        break;
                    case TaskFlowManagement.Command.E84_MODE:
                    case TaskFlowManagement.Command.E84_TRANSREQ:
                        if (!Sanwa_E84Mode(TaskJob, Target,Value)) return;
                        break;
                    case TaskFlowManagement.Command.E84_SET_ALL_MODE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 0; i < LoadportCount; i++)
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
                                break;

                            case 1:
                                for (int i = 0; i < LoadportCount; i++)
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
                                    for (int i = 0; i < LoadportCount; i++)
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
                                    for (int i = 0; i < LoadportCount; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", (i + 1).ToString().PadLeft(2, '0'));
                                        if (IsNodeEnabledOrNull(E84Name))
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                                    }
                                }
                                break;

                            default:
                                for (int i = 0; i < LoadportCount; i++)
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
                        if (!Sanwa_E84SetTP1(TaskJob, Target, Value)) return;
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
        private void ResetRobotInterLock()
        {
            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF1", "TRUE");
            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF2", "TRUE");
        }

        public override void GetRobotPresence(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!SystemConfig.Get().OfflineMode)
            {
                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.GetPresence }));
            }
        }
        public override void AbortTask(TaskFlowManagement.CurrentProcessTask TaskJob, Node node, string Message)
        {
            if (node != null)
            {
                _TaskReport.On_Alarm_Happen(AlarmManagement.NewAlarm(node, Message, TaskJob.MainTaskId));
                //if(node.Vendor.Equals("SANWA_HWATSING_MC"))
                //{
                //    Node tempNode = new Node
                //    {
                //        Name = node.Name,
                //        Vendor = "SANWA_MC"
                //    };

                //    _TaskReport.On_Alarm_Happen(AlarmManagement.NewAlarm(tempNode, Message, TaskJob.MainTaskId));
                //}
                //else
                //{
                //    _TaskReport.On_Alarm_Happen(AlarmManagement.NewAlarm(node, Message, TaskJob.MainTaskId));
                //}
            }
            _TaskReport.On_TaskJob_Aborted(TaskJob);
        }

        public override bool Sanwa_RobotMode(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = Value }));

                    break;
                case 1:
                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetMode }));

                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool Sanwa_RobotSpeed(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Speed, Value = Value }));
                    Target.Speed = Value;

                    break;
                case 1:
                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));

                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public override bool Sanwa_RobotGetSpeed(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);
                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));

                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }

    }
}
