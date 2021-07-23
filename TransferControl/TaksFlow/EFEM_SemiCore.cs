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
    class EFEM_SemiCore : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(EFEM_SemiCore));
        IUserInterfaceReport _TaskReport;
        public EFEM_SemiCore(IUserInterfaceReport TaskReport)
        {
            _TaskReport = TaskReport;
        }
        public void Excute(object input)
        {
            TaskFlowManagement.CurrentProcessTask TaskJob = (TaskFlowManagement.CurrentProcessTask)input;
            logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());

            string Message = "";
            Node Target = null;
            Node Position = null;
            Job Wafer;
            string Value = "";
            string tmp = "";
            string binaryStr = "";
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
                                if (NodeManagement.Get("ROBOT01") == null ? 
                                    false : NodeManagement.Get("ROBOT01").Enable)                      
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));

                                //Loadport Reset
                                for (int i = 1; i < 5; i++)
                                {
                                    string LoadportName = string.Format("LOADPORT{0}", i.ToString().PadLeft(2, '0'));
                                    if (NodeManagement.Get(LoadportName) == null ?
                                        false : NodeManagement.Get(LoadportName).Enable)
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(LoadportName, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
                                }

                                if(!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 1; i < 5; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", i.ToString().PadLeft(2, '0'));
                                        if (NodeManagement.Get(E84Name) != null)
                                        {
                                            if (NodeManagement.Get(E84Name).Enable)
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIStatus }));
                                        }
                                    }
                                }


                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                {
                                    for (int i = 1; i < 5; i++)
                                    {
                                        string E84Name = string.Format("E84{0}", i.ToString().PadLeft(2, '0'));
                                        if (NodeManagement.Get(E84Name) != null)
                                        {
                                            if (NodeManagement.Get(E84Name).Enable && !SystemConfig.Get().OfflineMode)
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDOStatus }));
                                        }
                                    }
                                }
                                break;

                            case 2:
                                if (!SystemConfig.Get().OfflineMode)
                                    for (int i = 1; i < 5; i++)
                                {
                                    string E84Name = string.Format("E84{0}", i.ToString().PadLeft(2, '0'));
                                    if (NodeManagement.Get(E84Name) != null)
                                    {
                                        if (NodeManagement.Get(E84Name).Enable && !NodeManagement.Get(E84Name).E84IOStatus["HO_AVBL"])
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.Reset }));
                                        }
                                    }
                                }

                                break;

                            case 3:
                                if (!SystemConfig.Get().OfflineMode)
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

                            case 4:
                                if (!SystemConfig.Get().OfflineMode)
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
                                for(int i = 1; i < 5; i++)
                                {
                                    string E84Name = string.Format("E84{0}", i.ToString().PadLeft(2, '0'));
                                    if (NodeManagement.Get(E84Name) != null)
                                    {
                                        if (NodeManagement.Get(E84Name).Enable)
                                        {
                                            NodeManagement.Get(NodeManagement.Get(E84Name).Associated_Node).E84Mode = 
                                                NodeManagement.Get(E84Name).E84Mode;
                                        }
                                        
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
                                if (NodeManagement.Get("ROBOT01") == null ? 
                                    false : NodeManagement.Get("ROBOT01").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_INIT, new Dictionary<string, string>() { { "@Target", "ROBOT01" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }

                                if (NodeManagement.Get("LOADPORT01") == null ?
                                    false : NodeManagement.Get("LOADPORT01").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_INIT, new Dictionary<string, string>() { { "@Target", "LOADPORT01" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");
                                        break;
                                    }

                                    if(NodeManagement.Get("E8401") == null ?
                                    false : NodeManagement.Get("E8401").Enable)
                                    {
                                        if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT,
                                            new Dictionary<string, string>()
                                            { { "@Target", "E8401" } }, 
                                            "",TaskJob.MainTaskId).Promise())
                                        {
                                            //中止Task
                                            AbortTask(TaskJob, null, "TASK_ABORT");
                                            break;
                                        }
                                    }
                                }

                                if (NodeManagement.Get("LOADPORT02") == null ?
                                    false : NodeManagement.Get("LOADPORT02").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_INIT, new Dictionary<string, string>() { { "@Target", "LOADPORT02" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }

                                    if (NodeManagement.Get("E8402") == null ?
                                    false : NodeManagement.Get("E8402").Enable)
                                    {
                                        if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT,
                                            new Dictionary<string, string>()
                                            { { "@Target", "E8402" } },
                                            "", TaskJob.MainTaskId).Promise())
                                        {
                                            //中止Task
                                            AbortTask(TaskJob, null, "TASK_ABORT");
                                            break;
                                        }
                                    }
                                }

                                if (NodeManagement.Get("LOADPORT03") == null ?
                                    false : NodeManagement.Get("LOADPORT03").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_INIT, new Dictionary<string, string>() { { "@Target", "LOADPORT03" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }

                                    if (NodeManagement.Get("E8403") == null ?
                                    false : NodeManagement.Get("E8403").Enable)
                                    {
                                        if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT,
                                            new Dictionary<string, string>()
                                            { { "@Target", "E8403" } },
                                            "", TaskJob.MainTaskId).Promise())
                                        {
                                            //中止Task
                                            AbortTask(TaskJob, null, "TASK_ABORT");
                                            break;
                                        }
                                    }
                                }

                                if (NodeManagement.Get("LOADPORT04") == null ?
                                    false : NodeManagement.Get("LOADPORT04").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_INIT, new Dictionary<string, string>() { { "@Target", "LOADPORT04" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }

                                    if (NodeManagement.Get("E8404") == null ?
                                    false : NodeManagement.Get("E8404").Enable)
                                    {
                                        if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.E84_INIT,
                                            new Dictionary<string, string>()
                                            { { "@Target", "E8404" } },
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
                                if(NodeManagement.Get("ROBOT01") != null)
                                    if (NodeManagement.Get("ROBOT01").Enable && !NodeManagement.Get("ROBOT01").InitialComplete)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "ROBOT01" }, "S0300015");
                                        return;
                                    }

                                if (NodeManagement.Get("LOADPORT01") != null)
                                    if (NodeManagement.Get("LOADPORT01").Enable && !NodeManagement.Get("LOADPORT01").InitialComplete)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT01" }, "S0300019");
                                        return;
                                    }
                                if (NodeManagement.Get("LOADPORT02") != null)
                                    if (NodeManagement.Get("LOADPORT02").Enable && !NodeManagement.Get("LOADPORT02").InitialComplete)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT02" }, "S0300020");
                                        return;
                                    }

                                if (NodeManagement.Get("LOADPORT03") != null)
                                    if (NodeManagement.Get("LOADPORT03").Enable && !NodeManagement.Get("LOADPORT03").InitialComplete)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT03" }, "S0300021");
                                        return;
                                    }

                                if (NodeManagement.Get("LOADPORT04") != null)
                                    if (NodeManagement.Get("LOADPORT04").Enable && !NodeManagement.Get("LOADPORT04").InitialComplete)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT04" }, "S0300022");
                                        return;
                                    }   

                                AckTask(TaskJob);
                                if (NodeManagement.Get("ROBOT01") == null ?
                                    false : NodeManagement.Get("ROBOT01").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_ORGSH, new Dictionary<string, string>() { { "@Target", "ROBOT01" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }

                                if (NodeManagement.Get("LOADPORT01") == null ?
                                    false : NodeManagement.Get("LOADPORT01").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT01", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                }

                                if (NodeManagement.Get("LOADPORT02") == null ?
                                    false : NodeManagement.Get("LOADPORT02").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT02", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                }

                                if (NodeManagement.Get("LOADPORT03") == null ?
                                    false : NodeManagement.Get("LOADPORT03").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT03", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                }

                                if (NodeManagement.Get("LOADPORT04") == null ?
                                    false : NodeManagement.Get("LOADPORT04").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT04", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                }

                                break;

                            default:

                                if(NodeManagement.Get("ROBOT01") != null)
                                    NodeManagement.Get("ROBOT01").OrgSearchComplete = NodeManagement.Get("ROBOT01").Enable;

                                if (NodeManagement.Get("LOADPORT01") != null)
                                    NodeManagement.Get("LOADPORT01").OrgSearchComplete = NodeManagement.Get("LOADPORT01").Enable;

                                if (NodeManagement.Get("LOADPORT02") != null)
                                    NodeManagement.Get("LOADPORT02").OrgSearchComplete = NodeManagement.Get("LOADPORT02").Enable;

                                if (NodeManagement.Get("LOADPORT03") != null)
                                    NodeManagement.Get("LOADPORT03").OrgSearchComplete = NodeManagement.Get("LOADPORT03").Enable;

                                if (NodeManagement.Get("LOADPORT04") != null)
                                    NodeManagement.Get("LOADPORT04").OrgSearchComplete = NodeManagement.Get("LOADPORT04").Enable;

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
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Stop, Value = "1" }));
                                }
                                break;
                            case 1:
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
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Continue }));
                                }
                                break;
                            case 1:

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
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Pause }));
                                }
                                break;
                            case 1:
 
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
                                if (!Target.IsPause)
                                {
                                    switch(Value)
                                    {
                                        case "1":
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "01" }));
                                            break;

                                        case "2":
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "02" }));
                                            break;
                                    }
                                    
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
                                //目標位置是否初始化完成
                                if (!Position.Type.Equals("LoadLock"))
                                {
                                    if (!Position.InitialComplete)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300168");
                                        return;
                                    }

                                    if (!Position.OrgSearchComplete)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300169");
                                        return;
                                    }
                                }

                                //Robot 是否初始化完成
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300015");
                                    return;
                                }

                                //Robot 是否歸原點完成
                                if (!Target.OrgSearchComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300041");
                                    return;
                                }


                                //是否需要重新Mapping
                                if (Position.Type.Equals("LOADPORT") && !Position.IsMapping)
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
                                if (Position.Type.Equals("LoadLock"))
                                {
                                    if(!TaskJob.Params["@BYPASS_CHECK"].Equals("TRUE"))
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
                                    Wafer = JobManagement.Get(Position.Name, Slot);
                                    //上Arm有五個Forks
                                    if (Arm.Equals("1"))
                                    {
                                        for(int i = 0; i<5; i++)
                                        {
                                            Wafer = JobManagement.Get(Position.Name, (Convert.ToInt32(Slot) + i).ToString());
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
                                //移動Wafer
                                string MethodName = Arm.Equals("1") ? Transaction.Command.RobotType.PutByRArm : Transaction.Command.RobotType.PutByLArm;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                break;

                            case 3:
                                string TargetName = Arm.Equals("1") ? Target.Name + "_R" : Target.Name + "_L";

                                //搬帳
                                for (int i = 0; i<5; i++)
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

                                    if (Position.Type.Equals("LoadLock"))
                                    {
                                        JobManagement.Remove(Wafer);
                                        MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "true");
                                    }

                                    //L Arm 只有一個Forks
                                    if (Arm.Equals("2")) break;
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
                                if (!Position.Type.Equals("LoadLock"))
                                {
                                    if (!Position.InitialComplete)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300168");
                                        return;
                                    }

                                    if (!Position.OrgSearchComplete)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300169");
                                        return;
                                    }
                                }
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
                                if (Position.Type.Equals("LOADPORT") && !Position.IsMapping)
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
                                if (Position.Type.Equals("LoadLock"))
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

                                    if(TaskJob.Params.ContainsKey("@IsTransCommand"))
                                        if (!TaskJob.Params["@IsTransCommand"].Equals("TRUE"))
                                            AckTask(TaskJob);
                                }
                                else
                                {
                                    Wafer = JobManagement.Get(Position.Name, Slot);

                                    for (int i = 0; i < 5; i++)
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
                                    AckTask(TaskJob);
                                }
                                break;
                            case 2:
                                string MethodName = Arm.Equals("1") ? Transaction.Command.RobotType.GetByRArm : Transaction.Command.RobotType.GetByLArm;
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));

                                break;
                            case 3:
                                if (Position.Type.Equals("LoadLock"))
                                {
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "TRUE");
                                }

                                //搬帳
                                for (int i = 0; i < 5; i++)
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
                                    if (!FromPosition.Type.Equals("LoadLock"))
                                    {
                                        if (!FromPosition.InitialComplete)
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = FromPosition.Name }, "S0300168");
                                            return;
                                        }

                                        if (!FromPosition.OrgSearchComplete)
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = FromPosition.Name }, "S0300169");
                                            return;
                                        }
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
                                    if (!ToPosition.Type.Equals("LoadLock"))
                                    {
                                        if (!ToPosition.InitialComplete)
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = ToPosition.Name }, "S0300168");
                                            return;
                                        }

                                        if (!ToPosition.OrgSearchComplete)
                                        {
                                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = ToPosition.Name }, "S0300169");
                                            return;
                                        }
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


                                //Get safety check
                                if (!TaskJob.Params["@FromPosition"].Contains("BF") && !TaskJob.Params["@FromPosition"].Contains("LL"))
                                {
                                    for (int i = 0; i < 5; i++)
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
                                        for (int i = 0; i < 5; i++)
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

                                if (FromPosition.Type.Equals("LoadLock"))
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + FromPosition.Name, "FALSE");

                                if (ToPosition.Type.Equals("LoadLock"))
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

                                if(TaskJob.TaskName == TaskFlowManagement.Command.ROBOT_PUTWAIT)
                                {
                                    string MethodName = Arm.Equals("1") ? Transaction.Command.RobotType.PutWaitByRArm : Transaction.Command.RobotType.PutWaitByLArm;
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = MethodName, Position = Position.Name, Arm = Arm, Slot = Slot }));
                                }
                                else
                                {
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

                                if(TaskJob.TaskName == TaskFlowManagement.Command.ROBOT_WAFER_HOLD)
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

                                if(!SystemConfig.Get().OfflineMode)
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
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300015");
                                    return;
                                }

                                AckTask(TaskJob);

                                break;
                            case 1:

                                break;
                            case 2:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.OrginSearch }));
                                break;


                            default:
                                Target.OrgSearchComplete = true;
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
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_INIT, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, null, "TASK_ABORT");

                                    break;
                                }
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
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300168");
                                    return;
                                }

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));

                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
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
                                if(!SystemConfig.Get().OfflineMode)
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
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300168");
                                    return;
                                }

                                AckTask(TaskJob);

                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.ForceInitialPos }));

                                break;

                            case 1:
                                if (!SystemConfig.Get().OfflineMode)
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                                break;

                            default:
                                Target.OrgSearchComplete = true;
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

                                //移除所有Wafer
                                foreach (Job eachJob in JobManagement.GetByNode(Target.Name))
                                    JobManagement.Remove(eachJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MappingLoad }));

                                break;

                            case 1:
                                if(TaskJob.TaskName.Equals(TaskFlowManagement.Command.LOADPORT_OPEN))
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
                        Node E84Node = NodeManagement.Get(Target.Name.Replace("LOADPORT","E84"));
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                AckTask(TaskJob);

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED}));

                                //同步化 E84 DI Status
                                if(E84Node != null)
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
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, string.Format("S04000{0}", Target.Name.Substring(3,2)));
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
                                            if(NodeManagement.Get(E84Name).E84IOStatus["VALID"])
                                            {
                                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, string.Format("S04000{0}",i.ToString().PadLeft(2,'0')));
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
        private void AckTask(TaskFlowManagement.CurrentProcessTask TaskJob)
        {

            if (TaskJob.State == TaskFlowManagement.CurrentProcessTask.TaskState.None)
            {
                TaskJob.State = TaskFlowManagement.CurrentProcessTask.TaskState.ACK;
                _TaskReport.On_TaskJob_Ack(TaskJob);
            }
        }
        private void AbortTask(TaskFlowManagement.CurrentProcessTask TaskJob, Node Node, string Message)
        {
            if (Node != null)
            {
                _TaskReport.On_Alarm_Happen(AlarmManagement.NewAlarm(Node, Message, TaskJob.MainTaskId));
            }
            _TaskReport.On_TaskJob_Aborted(TaskJob);
        }
        private void FinishTask(TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            _TaskReport.On_TaskJob_Finished(TaskJob);
        }
        private void ResetRobotInterLock()
        {
            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF1", "TRUE");
            MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_BF2", "TRUE");
        }
    }
}
