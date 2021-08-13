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
    class EFEM_MIC_2P : BaseTaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(EFEM_MIC_2P));
        //IUserInterfaceReport _TaskReport;
        private int ArmCount;
        public EFEM_MIC_2P(IUserInterfaceReport TaskReport) : base(TaskReport)
        {
            _TaskReport = TaskReport;
            ArmCount = 2;
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
                                string Name = "";
                                if (IsNodeEnabledOrNull("ROBOT01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));

                                if (IsNodeEnabledOrNull("ALIGNER01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Reset }));

                                //Loadport Reset
                                for (int i = 0; i < 2; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i+1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
                                }

                                break;
                            case 1:
                                if (IsNodeEnabledOrNull("ALIGNER01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetError, Value = "00" } ));
                                break;

                            case 2:
                                if (IsNodeEnabledOrNull("ALIGNER01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER01", "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus}));

                                for (int i = 0; i < 2; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
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
                                Name = "ALIGNER01";
                                if (IsNodeEnabledOrNull(Name))
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ALIGNER_INIT, 
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

                                for (int i = 0; i<2; i++)
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
                                }
                                break;

                            case 2:
                                for (int i = 0; i < 2; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
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
                                string Name = "";

                                if (IsNodeEnabledOrNull("ROBOT01"))
                                    if (!IsNodeInitialComplete(NodeManagement.Get("ROBOT01"), TaskJob)) return;

                                if(IsNodeEnabledOrNull("ALIGNER01"))
                                    if (!IsNodeInitialComplete(NodeManagement.Get("ALIGNER01"), TaskJob)) return;


                                for (int i = 0; i < 2; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                        if (!IsNodeInitialComplete(NodeManagement.Get(Name), TaskJob)) return;
                                }

                                AckTask(TaskJob);

                                if (IsNodeEnabledOrNull("ROBOT01"))
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_ORGSH, new Dictionary<string, string>() { { "@Target", Name } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }

                                for(int i = 0; i<2; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i+1).ToString().PadLeft(2,'0'));
                                    if (IsNodeEnabledOrNull(Name))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                    }
                                }

                                if (IsNodeEnabledOrNull("ALIGNER01"))
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.OrginSearch }));

                                break;

                            case 1:
                                for (int i = 0; i < 2; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    if (IsNodeEnabledOrNull(Name))
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                                }
                                break;

                            default:

                                OrgSearchCompleted("ROBOT01");

                                for (int i = 0; i < 2; i++)
                                {
                                    Name = string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0'));
                                    OrgSearchCompleted(Name);
                                }

                                OrgSearchCompleted("ALIGNER01");

                                FinishTask(TaskJob);

                                return;
                        }
                        break;
#region ROBOT
                    case TaskFlowManagement.Command.ROBOT_ABORT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                if (Target.IsPause)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Stop, Value = "1" }));
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_RESTR:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);

                                if (Target.IsPause)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Continue }));
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_HOLD:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                if (!Target.IsPause)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Pause }));
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.GET_CLAMP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                switch (Value)
                                {
                                    case "1":
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "01" }));
                                        break;

                                    case "2":
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "02" }));
                                        break;
                                }

                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_PUT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                string MethodName = "";
                                //目標位置是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Position, TaskJob))    return;

                                //Robot 是否初始化歸原點完成
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob))     return;

                                if(Position.Type.ToUpper().Equals("ALIGNER") && Arm.Equals("3"))
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300156");
                                    return;
                                }
                                
                                //是否需要重新Mapping
                                if (Position.Type.ToUpper().Equals("LOADPORT") && !Position.IsMapping)
                                {
                                    AckTask(TaskJob);
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_RE_MAPPING, new Dictionary<string, string>() { { "@Target", Position.Name } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        TaskJob.State = TaskFlowManagement.CurrentProcessTask.TaskState.Abort;
                                        AbortTask(TaskJob, null, "S0300001");//LOAD_PORT_NOT_READY

                                        break;
                                    }
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

                                    if (TaskJob.Params.ContainsKey("@IsTransCommand"))
                                        if (!TaskJob.Params["@IsTransCommand"].Equals("TRUE"))
                                            AckTask(TaskJob);
                                }
                                else
                                {
                                    //目的地不能有片子
                                    Wafer = null;
                                    
                                    if (Arm.Equals("1") && Arm.Equals("2"))
                                    {
                                        Wafer = JobManagement.Get(Position.Name, Slot);
                                    }
                                    else if(Arm.Equals("3"))
                                    {
                                        for(int i = 0; i<ArmCount; i++)
                                        {
                                            Wafer = JobManagement.Get(Position.Name, (Convert.ToInt32(Slot) + i).ToString() );
                                            if (Wafer != null) break;
                                        }
                                    }

                                    if (Wafer == null)
                                    {
                                        AckTask(TaskJob);
                                    }
                                    else
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300171");
                                        return;
                                    }
                                }
                                break;

                            case 2:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.Home }));

                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.PutWait, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                    }
                                }
                                break;

                            case 3:
                                //移動Wafer
                                MethodName = Arm.Equals("3") ? Transaction.Command.RobotType.DoublePut : Transaction.Command.RobotType.Put;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                break;
                            case 4:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferHold }));
                                    }
                                }
                                break;

                            case 5:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));                                    
                                }
                                break;

                            case 6:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                }
                                break;

                            case 7:
                                string TargetName = "";
                                
                                switch(Arm)
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
                                        Wafer.Slot = Slot;

                                        _TaskReport.On_Job_Location_Changed(Wafer);


                                        if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                        {
                                            JobManagement.Remove(Wafer);
                                        }
                                        break;
                                    case "3":
                                        for(int i = 0; i< ArmCount; i++)
                                        {
                                            TargetName = i== 0 ? Target.Name + "_R" : Target.Name + "_L";
                                            Wafer = JobManagement.Get(TargetName, "1");
                                            if (Wafer == null)
                                            {
                                                Wafer = JobManagement.Add();
                                                Wafer.MapFlag = true;
                                            }
                                            Wafer.LastNode = Wafer.Position;
                                            Wafer.LastSlot = Wafer.Slot;
                                            Wafer.Position = Position.Name;
                                            Wafer.Slot = (Convert.ToInt32(Slot) + (ArmCount-1- i)).ToString();

                                            _TaskReport.On_Job_Location_Changed(Wafer);

                                            if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                            {
                                                JobManagement.Remove(Wafer);
                                            }
                                        }
                                        break;
                                }
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
                                    AckTask(TaskJob);
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_RE_MAPPING, new Dictionary<string, string>() { { "@Target", Position.Name } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
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

                                    if (TaskJob.Params.ContainsKey("@IsTransCommand"))
                                        if (!TaskJob.Params["@IsTransCommand"].Equals("TRUE"))
                                            AckTask(TaskJob);
                                }
                                else
                                {
                                    //目的地需要有片子
                                    Wafer = JobManagement.Get(Position.Name, Slot);

                                    switch(Arm)
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
                                            for(int i = 0; i< ArmCount; i++)
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
                                            }
                                            break;
                                    }

                                    AckTask(TaskJob);
                                }
                                break;

                            case 4:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferRelease }));

                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.GetWait, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                    }
                                }
                                break;

                            case 5:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                                }
                                break;

                            case 6:
                                if (Position.Type.ToUpper().Equals("ALIGNER"))
                                {
                                    if (!SystemConfig.Get().OfflineMode)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                }
                                break;

                            case 2:
                                string MethodName = Arm.Equals("3") ? Transaction.Command.RobotType.DoubleGet : Transaction.Command.RobotType.Get;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));

                                break;
                            case 3:
                                if (Position.Type.ToUpper().Equals("LOADLOCK"))
                                {
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "TRUE");
                                }

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
                                        Wafer.LastNode = Wafer.Position;
                                        Wafer.LastSlot = Wafer.Slot;
                                        Wafer.Position = Arm.Equals("1") ? Target.Name + "_R" : Target.Name + "_L";
                                        Wafer.Slot = "1";

                                        _TaskReport.On_Job_Location_Changed(Wafer);
                                        break;
                                    case "3":
                                        for (int i = 0; i < ArmCount; i++)
                                        {
                                            Wafer = JobManagement.Get(Position.Name, (Convert.ToInt32(Slot) + i).ToString());
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
                                    if(!TaskJob.Params["@FromArm"].Equals("3"))
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
                                        for(int i = 0; i< ArmCount; i++)
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

                                AckTask(TaskJob);

                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_GET, new Dictionary<string, string>() { { "@Target", Target.Name }, { "@Position", TaskJob.Params["@FromPosition"] }, { "@Slot", TaskJob.Params["@FromSlot"] }, { "@BYPASS_CHECK", TaskJob.Params["@From_BYPASS_CHECK"] }, { "@IsTransCommand", "TRUE" } }, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, null, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_PUT, new Dictionary<string, string>() { { "@Target", Target.Name }, { "@Position", TaskJob.Params["@ToPosition"] }, { "@Slot", TaskJob.Params["@ToSlot"] }, { "@BYPASS_CHECK", TaskJob.Params["@To_BYPASS_CHECK"] }, { "@IsTransCommand", "TRUE" } }, "", TaskJob.MainTaskId).Promise())
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
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);


                                break;
                            case 1:

                                if (TaskJob.TaskName == TaskFlowManagement.Command.ROBOT_PUTWAIT)
                                {
                                    string MethodName = Transaction.Command.RobotType.PutWait;
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                }
                                else
                                {
                                    string MethodName = Transaction.Command.RobotType.GetWait;
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
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (TaskJob.TaskName == TaskFlowManagement.Command.ROBOT_WAFER_HOLD)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.WaferHold, Arm = Arm }));
                                else
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.WaferRelease, Arm = Arm }));
                                break;

                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = Arm.Equals("1") ? "008" : "009" }));
                                break;

                            case 2:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = Arm.Equals("1") ? "01" : "02" }));
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_MODE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = Value }));

                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetMode }));

                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_RETRACT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.ArmReturn }));

                                break;


                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_SPEED:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Speed, Value = Value }));

                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));

                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_SERVO:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Servo, Value = Value }));

                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));

                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_HOME:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Home }));

                                break;


                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!IsNodeInitialComplete(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                break;
                            case 1:

                                break;
                            case 2:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.OrginSearch }));
                                break;


                            default:
                                OrgSearchCompleted(Target.Name);

                                if(Target.OrgSearchComplete)
                                    ResetRobotInterLock();

                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_RESET:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));
                                break;

                            case 1:
                                //確認R軸 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "008" }));
                                break;

                            case 2:
                                //確認L軸 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "009" }));
                                break;

                            case 3:
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
                                //取得速度
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));
                                break;

                            case 7:
                                //取得模式
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetMode }));
                                break;

                            case 8:
                                //取得異常
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetError, Value = "00" }));
                                break;

                            case 9:
                                //Servo on
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Servo, Value = "1" }));
                                break;

                            case 10:
                                //更新Bobot狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));
                                break;

                            default:
                                ResetRobotInterLock();
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_INIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                //開啟R軸電磁閥
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "1" }));

                                break;

                            case 1:
                                //開啟L軸電磁閥
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "1" }));

                                break;

                            case 2:
                                //確認R軸 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "008" }));
                                break;

                            case 3:
                                //確認L軸 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "009" }));
                                break;

                            case 4:
                                //R軸 Presence 不存在,則關閉R軸電磁閥
                                if (!SystemConfig.Get().OfflineMode && !Target.R_Presence)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "0" }));

                                break;

                            case 5:
                                //L軸 Presence 不存在,則關閉L軸電磁閥
                                if (!SystemConfig.Get().OfflineMode && !Target.L_Presence)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "0" }));

                                break;

                            case 6:
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
                            case 7:
                                //取得R軸電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "01" }));
                                break;

                            case 8:
                                //取得L軸電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "02" }));
                                break;

                            case 9:
                                //取得速度
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));

                                break;

                            case 10:
                                //取得模式
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetMode }));

                                break;

                            case 11:
                                //取得異常
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetError, Value = "00" }));

                                break;

                            case 12:
                                //Servo on
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Servo, Value = "1" }));

                                break;

                            case 13:
                                //更新Bobot狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));

                                break;

                            default:
                                Target.InitialComplete = true;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_SAVE_LOG:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                //開啟R軸電磁閥
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "1" }));

                                break;

                            default:
                                Target.InitialComplete = true;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
#endregion
#region ALIGNER
                    case TaskFlowManagement.Command.ALIGNER_RESET:
                        switch(TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Reset }));
                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetError, Value = "00" }));
                                break;
                            case 2:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                break;

                        }
                        break;

                    case TaskFlowManagement.Command.ALIGNER_INIT:
                        switch(TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                //開啟電磁閥
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SetSV, Value = "01", Val2 = "1" }));
                                break;

                            case 1:
                                //確認 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                break;

                            case 2:
                                //Presence 不存在,則關閉R軸電磁閥
                                if (!SystemConfig.Get().OfflineMode && !Target.R_Presence)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SetSV, Value = "01", Val2 = "0" }));

                                break;

                            case 3:
                                //設定模式(Normal or dry)
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    if (SystemConfig.Get().DummyMappingData)
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Mode, Value = "1" }));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Mode, Value = "0" }));
                                    }
                                }
                                break;
                            case 4:
                                //取得電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                                break;

                            case 5:
                                //取得速度
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSpeed }));
                                break;

                            case 6:
                                //取得模式
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetMode }));
                                break;

                            case 7:
                                //取得異常
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetError, Value = "00" }));
                                break;

                            case 8:
                                //Servo on
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Servo, Value = "1" }));

                                break;

                            case 9:
                                if (Target.WaferSize.Equals("200"))
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SetAlign, Value = "100000" }));
                                }
                                else if (Target.WaferSize.Equals("300"))
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SetAlign, Value = "150000" }));
                                }
                                break;

                            case 10:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));

                                break;
                            default:
                                Target.InitialComplete = true;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALIGNER_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!IsNodeInitialComplete(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.OrginSearch }));
                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.Home }));
                                break;

                            case 2:
                                //確認 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                break;

                            case 3:
                                //取得電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                                break;

                            case 4:
                                //取得速度
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSpeed }));
                                break;

                            case 5:
                                //取得模式
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetMode }));
                                break;

                            case 6:
                                //取得異常
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetError, Value = "00" }));
                                break;

                            case 7:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));
                                break;

                            default:
                                OrgSearchCompleted(Target.Name);
                                FinishTask(TaskJob);

                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALIGNER_HOME:
                        switch(TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.Home }));

                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALIGNER_SERVO:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Servo, Value = Value }));
                                break;
                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ALIGNER_SPEED:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Speed, Value = Value }));
                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALIGNER_MODE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Mode, Value = Value }));

                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetMode }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALIGNER_WAFER_HOLD:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferHold }));
                                break;

                            case 1:
                                //確認 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                break;

                            case 2:
                                //取得電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALIGNER_WAFER_RELEASE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferRelease }));
                                break;

                            case 1:
                                //確認 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                break;

                            case 2:
                                //取得電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALIGNER_SAVE_LOG:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SaveLog }));
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ALIGNER_ALIGN:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferHold }));
                                break;

                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.Align, Value = Value }));
                                break;

                            case 2:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferRelease }));
                                break;

                            case 3:
                                //確認 Presence
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                                break;

                            case 4:
                                //取得電磁閥最後狀態
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                                break;

                            case 5:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

#endregion
#region LOADPORT
                    case TaskFlowManagement.Command.LOADPORT_INIT:
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
                                if (!IsNodeInitialComplete(Target, TaskJob)) return;
                                 
                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));

                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                                break;

                            default:
                                OrgSearchCompleted(Target.Name);
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
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
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
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                                break;

                            default:
                                OrgSearchCompleted(Target.Name);
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_READ_STATUS:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
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
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Clamp }));

                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                                break;
                            default:
                                //Target.Foup_Lock = true;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_UNCLAMP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UnClamp }));

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

                    case TaskFlowManagement.Command.LOADPORT_DOCK:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Dock }));
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

                    case TaskFlowManagement.Command.LOADPORT_UNDOCK:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UnDock }));
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
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.VacuumON }));
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

                    case TaskFlowManagement.Command.LOADPORT_VAC_OFF:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.VacuumOFF }));
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

                    case TaskFlowManagement.Command.LOADPORT_UNLATCH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UnLatchDoor }));
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

                    case TaskFlowManagement.Command.LOADPORT_LATCH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.LatchDoor }));
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

                    case TaskFlowManagement.Command.LOADPORT_DOOR_OPEN:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.DoorOpen }));
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

                    case TaskFlowManagement.Command.LOADPORT_DOOR_CLOSE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.DoorClose }));
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

                    case TaskFlowManagement.Command.LOADPORT_DOOR_DOWN:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.DoorDown }));
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

                    case TaskFlowManagement.Command.LOADPORT_DOOR_UP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.DoorUp }));
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

                    case TaskFlowManagement.Command.LOADPORT_OPEN:
                    case TaskFlowManagement.Command.LOADPORT_OPEN_NOMAP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

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
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_CLOSE:
                    case TaskFlowManagement.Command.LOADPORT_CLOSE_NOMAP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                //if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Unload }));

                                break;

                            case 1:
                                //if (TaskJob.TaskName.Equals(TaskFlowManagement.Command.LOADPORT_CLOSE))
                                //{
                                //    if (SystemConfig.Get().DummyMappingData)
                                //    {
                                //        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                                //    }
                                //    else
                                //    {
                                //        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                //    }
                                //}


                                break;
                            case 2:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

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
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                }
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
                                if (CheckNodeStatusOnTaskJob(Target, TaskJob)) return;

                                AckTask(TaskJob);

                                //移除所有Wafer
                                foreach (Job eachJob in JobManagement.GetByNode(Target.Name))
                                    JobManagement.Remove(eachJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.RetryMapping }));

                                break;
                            case 1:
                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                }

                                break;

                            case 2:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_SET_OPACCESS_INDICATOR:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = Value }));


                                break;

                            case 2:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_SET_LOAD_INDICATOR:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = Value }));
                                break;

                            case 2:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_SET_UNLOAD_INDICATOR:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = Value }));
                                break;

                            case 2:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_READ_LED:
                        Node E84Node = NodeManagement.Get(Target.Name.Replace("LOADPORT", "E84"));
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));

                                //同步化 E84 DI Status
                                if (E84Node != null)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIStatus }));
                                break;

                            case 1:
                                //同步化 E84 DO Status 
                                if (E84Node != null)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDOStatus }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    #endregion
#region CSTID
                    case TaskFlowManagement.Command.GET_CSTID:
                        if (Target.Type.ToUpper().Equals("SMARTTAG"))
                        {
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:

                                    AckTask(TaskJob);

                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.SmartTagType.Hello }));

                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.SmartTagType.GetLCDData }));

                                    break;
                                default:
                                    FinishTask(TaskJob);
                                    return;
                            }
                        }
                        else if (Target.Type.ToUpper().Equals("RFID"))
                        {
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    AckTask(TaskJob);

                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RFIDType.GetCarrierID }));
                                    break;
                                default:
                                    FinishTask(TaskJob);
                                    return;
                            }
                        }
                        break;
                    case TaskFlowManagement.Command.SET_CSTID:
                        if (Target.Type.ToUpper().Equals("SMARTTAG"))
                        {
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    AckTask(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.SmartTagType.Hello }));
                                    break;
                                case 1:
                                    if (Target.Vendor.ToUpper().Equals("SMARTTAG8200"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.SmartTagType.SelectLCDData }));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.SmartTagType.SetLCDData, Value = Value }));

                                    break;
                                default:
                                    FinishTask(TaskJob);
                                    return;
                            }
                        }
                        else if (Target.Type.ToUpper().Equals("RFID"))
                        {
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    AckTask(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RFIDType.SetCarrierID, Value = Value }));
                                    break;
                                default:
                                    FinishTask(TaskJob);
                                    return;
                            }
                        }
                        break;
                    #endregion
#region E84
                    case TaskFlowManagement.Command.E84_INIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);

                                //同步化 E84 DI Status 
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIStatus }));
                                break;

                            case 1:

                                //同步化 E84 DO Status 
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDOStatus }));
                                break;
                            case 2:

                                //同步化 E84 Operate Status 
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetOperateStatus }));
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
                                TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT, new Dictionary<string, string>() { { "@Target", Target.Name } }, "", TaskJob.MainTaskId).Promise();
                                break;

                            default:

                                NodeManagement.Get(NodeManagement.Get(Target.Name).Associated_Node).E84Mode = Target.E84Mode;

                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.E84_MODE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (Target.E84IOStatus["VALID"])
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, string.Format("S04000{0}", Target.Name.Substring(3, 2)));
                                    return;
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

                                TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT, new Dictionary<string, string>() { { "@Target", Target.Name } }, "", TaskJob.MainTaskId).Promise();
                                break;



                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.E84_SET_ALL_MODE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                for (int i = 1; i < 5; i++)
                                {
                                    string E84Name = string.Format("E84{0}", i.ToString().PadLeft(2, '0'));
                                    if (NodeManagement.Get(E84Name) != null)
                                    {
                                        if (NodeManagement.Get(E84Name).Enable)
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
                                for (int i = 1; i < 5; i++)
                                {
                                    string E84Name = string.Format("E84{0}", i.ToString().PadLeft(2, '0'));
                                    if (NodeManagement.Get(E84Name) != null)
                                    {
                                        if (NodeManagement.Get(E84Name).Enable)
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
                                }

                                break;

                            case 2:
                                for (int i = 1; i < 5; i++)
                                {
                                    string E84Name = string.Format("E84{0}", i.ToString().PadLeft(2, '0'));
                                    if (NodeManagement.Get(E84Name) != null)
                                    {
                                        if (NodeManagement.Get(E84Name).Enable)
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIStatus }));
                                    }
                                }
                                break;

                            case 3:
                                for (int i = 1; i < 5; i++)
                                {
                                    string E84Name = string.Format("E84{0}", i.ToString().PadLeft(2, '0'));
                                    if (NodeManagement.Get(E84Name) != null)
                                    {
                                        if (NodeManagement.Get(E84Name).Enable)
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDOStatus }));
                                    }
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
    }
}

