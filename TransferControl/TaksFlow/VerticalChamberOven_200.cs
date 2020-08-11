using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransferControl.Engine;
using TransferControl.Management;

namespace TransferControl.TaksFlow
{
    class VerticalChamberOven_200 : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(VerticalChamberOven_200));
        private const int DEF_OUT_DATA_ADDRESS = 0x500;
        private const int DEF_IN_DATA_ADDRESS = 0x500;
        private const int DEF_WOUT_DATA_ADDRESS = 0x6500;
        private const int DEF_WIN_DATA_ADDRESS = 0x6000;
        int CstRobotStation = 9;
        int RetryCount = 10;
        int RetryInterval = 100;
        int CurrentIndex = 0;
        string binaryStr = "";
        IUserInterfaceReport _TaskReport;
        public VerticalChamberOven_200(IUserInterfaceReport TaskReport)
        {
            _TaskReport = TaskReport;
        }

        public void Excute(object input)
        {
            TaskFlowManagement.CurrentProcessTask TaskJob = (TaskFlowManagement.CurrentProcessTask)input;
            if (TaskJob.TaskName != TaskFlowManagement.Command.CCLINK_GET_IO && TaskJob.TaskName != TaskFlowManagement.Command.CCLINK_SET_IO)
            {
                logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());

            }
            if (TaskJob.CurrentIndex == 0 && TaskJob.TaskName.ToString().IndexOf("CCLINK") == -1)
            {

                _TaskReport.On_Message_Log("CMD", TaskJob.TaskName.ToString() + " " + (TaskJob.Params.ContainsKey("@Target") ? TaskJob.Params["@Target"] : "") + " Executing");
            }
            string Message = "";
            Node Target = null;
            Node Position = null;
            string tmp = "";
            string Value = "";


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
                            Position = NodeManagement.Get(item.Value);
                            break;
                        case "@Value":
                            Value = item.Value;
                            break;
                    }
                }
            }
            try
            {

                switch (TaskJob.TaskName)
                {
                    case TaskFlowManagement.Command.LOADPORT_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));

                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", Target.Name } }, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_RESET:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_UNCLAMP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UnClamp }));

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
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Clamp }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_LIFT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Lift }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_UNLIFT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UnLift }));

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
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.RetryMapping }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_TWKDN:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.TweekDn }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_TWKUP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.TweekUp }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_SLOT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MoveToSlot, Value = Value }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_SET_SPEED:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetSpeed, Value = Value }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_READ_STATUS:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.CLOSE_FOUP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Unload }));

                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", Target.Name } }, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.OPEN_FOUP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.LoadWithLift }));

                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", Target.Name } }, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_SHOME:

                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "0");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
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
                                TaskJob.Params.Add("@Command", "1");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
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
                                TaskJob.Params.Add("@Command", "2");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

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
                                if (Convert.ToInt32(TaskJob.Params["@Position"]) == 22)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", "SMIF1" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                        break;
                                    }
                                    if (!NodeManagement.Get("SMIF1").Status["POS"].Equals("3") || !NodeManagement.Get("SMIF1").Status["LPS"].Equals("TRUE") || !NodeManagement.Get("SMIF1").Status["LLS"].Equals("TRUE"))
                                    {
                                        _TaskReport.On_Message_Log("CMD", "SMIF1 presence error");
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");
                                    }
                                }
                                else if (Convert.ToInt32(TaskJob.Params["@Position"]) == 23)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", "SMIF2" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                        break;
                                    }
                                    if (!NodeManagement.Get("SMIF2").Status["POS"].Equals("3") || !NodeManagement.Get("SMIF2").Status["LPS"].Equals("TRUE") || !NodeManagement.Get("SMIF2").Status["LLS"].Equals("TRUE"))
                                    {
                                        _TaskReport.On_Message_Log("CMD", "SMIF2 presence error");
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");
                                    }
                                }
                                break;
                            case 1:
                                if (!CheckPresence(Target, Convert.ToInt32(TaskJob.Params["@Position"]), 0))
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }

                                TaskJob.Params.Add("@Command", "3");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 5:
                                if (!CheckPresence(Target, Convert.ToInt32(TaskJob.Params["@Position"]), 1))
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 6:
                                if (Convert.ToInt32(TaskJob.Params["@Position"]) == 22)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", "SMIF1" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                        break;
                                    }

                                }
                                else if (Convert.ToInt32(TaskJob.Params["@Position"]) == 23)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", "SMIF2" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                        break;
                                    }

                                }

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
                                TaskJob.Params.Add("@Command", "4");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

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
                                if (Convert.ToInt32(TaskJob.Params["@Position"]) == 22)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", "SMIF1" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                        break;
                                    }
                                    if (!NodeManagement.Get("SMIF1").Status["POS"].Equals("3") || !NodeManagement.Get("SMIF1").Status["LPS"].Equals("FALSE") || !NodeManagement.Get("SMIF1").Status["LLS"].Equals("TRUE"))
                                    {
                                        _TaskReport.On_Message_Log("CMD", "SMIF1 presence error");
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");
                                    }
                                }
                                else if (Convert.ToInt32(TaskJob.Params["@Position"]) == 23)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", "SMIF2" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                        break;
                                    }
                                    if (!NodeManagement.Get("SMIF2").Status["POS"].Equals("3") || !NodeManagement.Get("SMIF2").Status["LPS"].Equals("FALSE") || !NodeManagement.Get("SMIF2").Status["LLS"].Equals("TRUE"))
                                    {
                                        _TaskReport.On_Message_Log("CMD", "SMIF2 presence error");
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");
                                    }
                                }
                                break;
                            case 1:
                                if (!CheckPresence(Target, Convert.ToInt32(TaskJob.Params["@Position"]), 1))
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                
                                TaskJob.Params.Add("@Command", "5");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 5:
                                if (!CheckPresence(Target, Convert.ToInt32(TaskJob.Params["@Position"]), 0))
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 6:
                                if (Convert.ToInt32(TaskJob.Params["@Position"]) == 22)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", "SMIF1" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                        break;
                                    }

                                }
                                else if (Convert.ToInt32(TaskJob.Params["@Position"]) == 23)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_READ_STATUS, new Dictionary<string, string>() { { "@Target", "SMIF2" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                        break;
                                    }

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
                                TaskJob.Params.Add("@Command", "6");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_WAFER_HOLD:

                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "12");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
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
                                TaskJob.Params.Add("@Command", "13");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
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
                                TaskJob.Params.Add("@Command", "16");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.CCLINK_SET_MODE:
                        string modeStr = TaskJob.Params["@Mode"];
                        binaryStr = Convert.ToString(Convert.ToUInt16(modeStr), 2).PadLeft(2, '0');
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                //REQ_MODE_1 (DI_09) 
                                Target.SetIO("OUTPUT", 9 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                //REQ_MODE_2 (DI_10) 
                                Target.SetIO("OUTPUT", 10 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }

                        break;
                    case TaskFlowManagement.Command.CCLINK_SET_SPEED:
                        string SpeedStr = TaskJob.Params["@Speed"];
                        double Speed = (int)Math.Round(Convert.ToDouble(SpeedStr));
                        int digit10 = (int)(Speed / Math.Pow(10, 1) % 10);
                        binaryStr = Convert.ToString(digit10 == 0 ? 10 : digit10, 2).PadLeft(4, '0');
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                //REQ_SPEED0 (DI_32)                                
                                Target.SetIO("OUTPUT", 32 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                //REQ_SPEED0 (DI_33)                                
                                Target.SetIO("OUTPUT", 33 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                //REQ_SPEED0 (DI_34)                                
                                Target.SetIO("OUTPUT", 34 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                //REQ_SPEED0 (DI_35)                                
                                Target.SetIO("OUTPUT", 35 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }

                        break;
                    case TaskFlowManagement.Command.CCLINK_CMD_PARAM:

                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Position") ? TaskJob.Params["@Position"] : "0"), 2).PadLeft(11, '0');

                                Target.SetIO("OUTPUT", 42 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                Target.SetIO("OUTPUT", 43 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                Target.SetIO("OUTPUT", 44 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                Target.SetIO("OUTPUT", 45 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                Target.SetIO("OUTPUT", 46 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                Target.SetIO("OUTPUT", 47 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                Target.SetIO("OUTPUT", 48 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                Target.SetIO("OUTPUT", 49 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                Target.SetIO("OUTPUT", 50 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                Target.SetIO("OUTPUT", 51 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 52 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Slot") ? TaskJob.Params["@Slot"] : "1", 2)).PadLeft(10, '0');
                                Target.SetIO("OUTPUT", 53 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                Target.SetIO("OUTPUT", 54 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                Target.SetIO("OUTPUT", 55 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                Target.SetIO("OUTPUT", 56 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                Target.SetIO("OUTPUT", 57 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                Target.SetIO("OUTPUT", 58 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                Target.SetIO("OUTPUT", 59 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                Target.SetIO("OUTPUT", 60 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                Target.SetIO("OUTPUT", 61 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 62 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Arm") ? TaskJob.Params["@Arm"] : "1", 2)).PadLeft(2, '0');
                                Target.SetIO("OUTPUT", 63 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 64 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Option") ? TaskJob.Params["@Option"] : "0", 2)).PadLeft(2, '0');
                                Target.SetIO("OUTPUT", 65 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 66 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@AL") ? TaskJob.Params["@AL"] : "0", 2));
                                Target.SetIO("OUTPUT", 67 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                Target.SetIO("OUTPUT", 68 + (CstRobotStation - 1) * 32, 0);

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@APD") ? TaskJob.Params["@APD"] : "0", 2));
                                Target.SetIO("OUTPUT", 69 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));



                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.CCLINK_CMD_EXE:

                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_POWER_ON, TaskJob.Params, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Command") ? TaskJob.Params["@Command"] : "0"), 2).PadLeft(6, '0');

                                Target.SetIO("OUTPUT", 36 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                Target.SetIO("OUTPUT", 37 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                Target.SetIO("OUTPUT", 38 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                Target.SetIO("OUTPUT", 39 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                Target.SetIO("OUTPUT", 40 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 41 + (CstRobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                SpinWait.SpinUntil(() => false, 500);
                                //REQ_INPUT(DI_01)
                                Target.SetIO("OUTPUT", 1 + (CstRobotStation - 1) * 32, 1);
                                //STS_INPUT (DO_03) 
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[3 + (CstRobotStation - 1) * 32] == 1, 1000);

                                if (Target.GetIO("INPUT")[3 + (CstRobotStation - 1) * 32] == 0)//Check STS_INPUT (DO_03) 
                                {
                                    AbortTask(TaskJob, Target, "STS_INPUT (DO_03)    not match to 1");
                                    //REQ_INPUT(DI_01)
                                    Target.SetIO("OUTPUT", 1 + (CstRobotStation - 1) * 32, 0);

                                    return;
                                }
                                //REQ_EXEC (DI_02) 
                                Target.SetIO("OUTPUT", 2 + (CstRobotStation - 1) * 32, 1);
                                //STS_INPUT (DO_03) 
                                //SpinWait.SpinUntil(() => Target.GetIO("INPUT")[3] == '1', 15000);
                                //if (Target.GetIO("INPUT")[3] == '0')//Check STS_INPUT (DO_03) 
                                //{
                                //    AbortTask(TaskJob, Target, "STS_INPUT (DO_03)   not match to 1");
                                //    return;
                                //}
                                //STS_EXEC (DO_04)
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[4 + (CstRobotStation - 1) * 32] == 1, 1000);

                                if (Target.GetIO("INPUT")[4 + (CstRobotStation - 1) * 32] == 0)//Check STS_EXEC (DO_04)
                                {
                                    AbortTask(TaskJob, Target, "STS_EXEC (DO_04)   not match to 1");
                                    //REQ_INPUT(DI_01)
                                    Target.SetIO("OUTPUT", 1 + (CstRobotStation - 1) * 32, 0);
                                    //REQ_EXEC (DI_02) 
                                    Target.SetIO("OUTPUT", 2 + (CstRobotStation - 1) * 32, 0);
                                    return;
                                }
                                //REQ_INPUT(DI_01)
                                Target.SetIO("OUTPUT", 1 + (CstRobotStation - 1) * 32, 0);
                                //REQ_EXEC (DI_02) 
                                Target.SetIO("OUTPUT", 2 + (CstRobotStation - 1) * 32, 0);
                                //STS_EXEC (DO_04)
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[4 + (CstRobotStation - 1) * 32] == 0, 60000);
                                if (Target.GetIO("INPUT")[4 + (CstRobotStation - 1) * 32] == 1)//Check STS_EXEC (DO_04)
                                {
                                    AbortTask(TaskJob, Target, "STS_EXEC (DO_04)   not match to 0 (Motion TimeOut)");
                                    return;
                                }



                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.CCLINK_POWER_ON:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                if (GetIO(Target, "INPUT", 0 + (CstRobotStation - 1) * 32) != 1)//Check  STS_REDAY  (DO_00) 
                                {
                                    AbortTask(TaskJob, Target, "STS_REDAY not match");
                                    return;
                                }

                                if (GetIO(Target, "INPUT", 6 + (CstRobotStation - 1) * 32) == 1)//Check  STS_ERROR (DO_06) 
                                {
                                    AbortTask(TaskJob, Target, "ALARM is happend STS_ERROR (DO_06) ");
                                    return;
                                }

                                //REQ_REMOTE (DI_00)                          
                                Target.SetIO("OUTPUT", 0 + (CstRobotStation - 1) * 32, 1);
                                //Wait for  STS_REMOTE (DO_01) STS_WAIT (DO_02) 
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[1 + (CstRobotStation - 1) * 32] == 1 && Target.GetIO("INPUT")[2 + (CstRobotStation - 1) * 32] == 1, 3000);
                                if (Target.GetIO("INPUT")[1 + (CstRobotStation - 1) * 32] == 0 || Target.GetIO("INPUT")[2 + (CstRobotStation - 1) * 32] == 0)//Check STS_REMOTE (DO_01) STS_WAIT (DO_02) 
                                {
                                    AbortTask(TaskJob, Target, "STS_REMOTE (DO_01) STS_WAIT (DO_02)  not match");
                                    return;
                                }
                                //REQ_SERVO (DI_08) 
                                Target.SetIO("OUTPUT", 8 + (CstRobotStation - 1) * 32, 1);
                                if (Target.GetIO("INPUT")[9 + (CstRobotStation - 1) * 32] == 0)//Check STS_SERVO (DO_09) 
                                {
                                    AbortTask(TaskJob, Target, "STS_REMOTE (DO_01) STS_WAIT (DO_02)  not match");
                                    return;
                                }
                                //REQ_ENTER (DI_07)
                                Target.SetIO("OUTPUT", 7 + (CstRobotStation - 1) * 32, 1);
                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_RESET:

                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (GetIO(Target, "INPUT", 0 + (CstRobotStation - 1) * 32) != 1)//Check  STS_REDAY  (DO_00) 
                                {
                                    AbortTask(TaskJob, Target, "STS_REDAY not match");
                                    return;
                                }
                                //REQ_REMOTE (DI_00)                          
                                Target.SetIO("OUTPUT", 0 + (CstRobotStation - 1) * 32, 1);
                                //REQ_RESET (DI_06) 

                                Target.SetIO("OUTPUT", 6 + (CstRobotStation - 1) * 32, 1);
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[6 + (CstRobotStation - 1) * 32] == 0, 15000);
                                Target.SetIO("OUTPUT", 6 + (CstRobotStation - 1) * 32, 0);
                                if (GetIO(Target, "INPUT", 6 + (CstRobotStation - 1) * 32) == 1)//Wait for  STS_ERROR (DO_06) 
                                {
                                    AbortTask(TaskJob, Target, "TimeOut");
                                    return;
                                }

                                break;

                            default:
                                FinishTask(TaskJob);
                                return;

                        }

                        break;
                    //case TaskFlowManagement.Command.CCLINK_SET_IO:

                    //    switch (TaskJob.CurrentIndex)
                    //    {
                    //        case 0:

                    //            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@Target"], "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + Convert.ToInt16(TaskJob.Params["@IO_NUM"]), PLC_Bit = Convert.ToByte(TaskJob.Params["@IO_VAL"]) }));
                    //            break;

                    //        default:
                    //            FinishTask(TaskJob);
                    //            return;
                    //    }

                    //    break;
                    //case TaskFlowManagement.Command.CCLINK_GET_IO:

                    //    switch (TaskJob.CurrentIndex)
                    //    {
                    //        case 0:
                    //            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@Target"], "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.ReadBit, PLC_Station = 1, PLC_Area = TaskJob.Params["@IO_AREA"], PLC_StartAddress = DEF_OUT_DATA_ADDRESS, PLC_Len = 384 }));
                    //            break;
                    //        default:
                    //            FinishTask(TaskJob);
                    //            return;
                    //    }

                    //    break;
                    default:
                        throw new NotSupportedException();
                }
                if (TaskJob.CheckList.Count != 0)
                {
                    foreach (TaskFlowManagement.ExcutedCmd eachCmd in TaskJob.CheckList)
                    {
                        eachCmd.Txn.TaskObj = TaskJob;
                        NodeManagement.Get(eachCmd.NodeName).SendCommand(eachCmd.Txn);
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
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " " + (TaskJob.Params.ContainsKey("@Target") ? TaskJob.Params["@Target"] : "") + " exception: " + e.StackTrace);
                AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), e.StackTrace);
                return;
            }
            return;
        }
        private void AbortTask(TaskFlowManagement.CurrentProcessTask TaskJob, Node Node, string Message)
        {
            _TaskReport.On_Alarm_Happen(AlarmManagement.NewAlarm(Node, Message));

            _TaskReport.On_TaskJob_Aborted(TaskJob);
            _TaskReport.On_Message_Log("CMD", TaskJob.TaskName.ToString() + " " + (TaskJob.Params.ContainsKey("@Target") ? TaskJob.Params["@Target"] : "") + " Aborted");

        }
        private void FinishTask(TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            if (TaskJob.TaskName.ToString().IndexOf("CCLINK") == -1)
            {
                _TaskReport.On_Message_Log("CMD", TaskJob.TaskName.ToString() + " " + (TaskJob.Params.ContainsKey("@Target") ? TaskJob.Params["@Target"] : "") + " Finished");
            }
            _TaskReport.On_TaskJob_Finished(TaskJob);

        }

        private byte GetIO(Node Target, string Area, int Pos)
        {

            return Target.GetIO(Area)[Pos];
        }

        private bool CheckPresence(Node Target, int Pos, int State)
        {
            bool result = true;
            string OrgPos = Pos.ToString();
            if (Pos <= 12)
            {
                Pos = (Pos - 1) * 2;
            }
            else if (Pos == 22 || Pos == 23)
            {
                return true;
            }
            else if (Pos >= 18)
            {
                Pos = (Pos - 5 - 1) * 2;
            }
            else
            {
                Pos = (Pos + 4 - 1) * 2;
            }

            if (Target.GetIO("PRESENCE")[Pos] != State)
            {
                _TaskReport.On_Message_Log("CMD", "Shelf_" + OrgPos + "_1 presence error");
                result = false;
            }
            if (Target.GetIO("PRESENCE")[Pos + 1] != State)
            {
                _TaskReport.On_Message_Log("CMD", "Shelf_" + OrgPos + "_2 presence error");
                result = false;
            }
            return result;
        }
        public class IN
        {
            public const int B1_1 = 1;
            public const int B1_2 = 1;
            public const int B2_1 = 1;
            public const int B2_2 = 1;
            public const int B3_1 = 1;
            public const int B3_2 = 1;
            public const int B4_1 = 1;
            public const int B4_2 = 1;
            public const int B5_1 = 1;
            public const int B5_2 = 1;
            public const int B6_1 = 1;
            public const int B6_2 = 1;
            public const int B7_1 = 1;
            public const int B7_2 = 1;
            public const int B8_1 = 1;
            public const int B8_2 = 1;
            public const int B9_1 = 1;
            public const int B9_2 = 1;
            public const int B10_1 = 1;
            public const int B10_2 = 1;
            public const int B11_1 = 1;
            public const int B11_2 = 1;
            public const int B12_1 = 1;
            public const int B12_2 = 1;
            public const int B13_1 = 1;
            public const int B13_2 = 1;
            public const int B14_1 = 1;
            public const int B14_2 = 1;
            public const int B15_1 = 1;
            public const int B15_2 = 1;
            public const int B16_1 = 1;
            public const int B16_2 = 1;
            public const int B17_1 = 1;
            public const int B17_2 = 1;
            public const int B18_1 = 1;
            public const int B18_2 = 1;
            public const int B19_1 = 1;
            public const int B19_2 = 1;
            public const int B20_1 = 1;
            public const int B20_2 = 1;
            public const int B21_1 = 1;
            public const int B21_2 = 1;
            public const int Cassette_In_SW_LED = 1;
            public const int Cassette_Out_SW_LED = 1;
            public const int Tx_Pause_SW_LED_Front = 1;
            public const int Tx_Pause_SW_LED_Rear = 1;
        }
        public class OUT
        {
            public const int Light_Tower_RED = 1;
            public const int Light_Tower_Yellow = 1;
            public const int Light_Tower_Green = 1;
            public const int Light_Tower_Blue = 1;
            public const int Light_Tower_Buzzer_1 = 1;
            public const int Buzzer_2 = 1;
            public const int Pod1_Lock_Free = 1;
            public const int Pod2_Lock_Free = 1;
            public const int Cassette_In_SW_LED = 1;
            public const int Cassette_Out_SW_LED = 1;
            public const int Tx_Pause_SW_LED_Front = 1;
            public const int Tx_Pause_SW_LED_Rear = 1;
        }
    }
}
