using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Threading;
using System.Threading.Tasks;
using TransferControl.Engine;
using TransferControl.Management;

namespace TransferControl.TaksFlow
{
    class Sanwa_CCLinkController : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(Sanwa_CCLinkController));
        IUserInterfaceReport _TaskReport;
        public Sanwa_CCLinkController(IUserInterfaceReport TaskReport)
        {
            _TaskReport = TaskReport;
        }
        public void Excute(object input)
        {
            TaskFlowManagement.CurrentProcessTask TaskJob = (TaskFlowManagement.CurrentProcessTask)input;
            logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());

            //string Message = "";
            Node Target = null;
            Node Position = null;
            //string tmp = "";
            string Value = "";
            int RobotStation = -1;
            string binaryStr = "";

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
                        case "@RobotStation":
                            RobotStation = Convert.ToInt32(item.Value);
                            break;
                    }
                }
            }
            try
            {

                switch (TaskJob.TaskName)
                {
                    case TaskFlowManagement.Command.ROBOT_SHOME:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "0");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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
                                TaskJob.Params.Add("@Command", "3");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_GETWAIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "4");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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
                                TaskJob.Params.Add("@Command", "5");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_PUTWAIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "6");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_SEARCH_SUBSTRATE_EDGE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "7");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_FORK_MOTION:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "8");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_WAFER_EXCHANGE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "9");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_MAPPING:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "10");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_MULTI_PANEL_ENABLE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "14");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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
                     
                    case TaskFlowManagement.Command.ROBOT_RHOME:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "15");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "16");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_SET_MOTION_OFFSET:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "17");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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
                        
                    case TaskFlowManagement.Command.ROBOT_CHECK_PANEL:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "18");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_GET_USE_SELECTOR:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "19");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_SET_POINT_DATA:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "20");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_SAVE_POINT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "21");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_SAVE_LOG:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "22");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_GET_USE_RL_SELECTOR:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "23");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_GET_STEP_BY_STEP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "24");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_PUT_STEP_BY_STEP:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "25");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.ROBOT_FORK_ORGSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "26");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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
                        
                    case TaskFlowManagement.Command.ROBOT_PUSH:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                TaskJob.Params.Add("@Command", "27");
                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_MODE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_SET_SPEED, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 3:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 4:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
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

                    case TaskFlowManagement.Command.CCLINK_SET_MODE:
                        string modeStr = TaskJob.Params["@Mode"];
                        binaryStr = Convert.ToString(Convert.ToUInt16(modeStr), 2).PadLeft(2, '0');
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                //REQ_MODE_1 (DI_09) 
                                Target.SetIO("OUTPUT", 9 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                //REQ_MODE_2 (DI_10) 
                                Target.SetIO("OUTPUT", 10 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
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
                                Target.SetIO("OUTPUT", 32 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                //REQ_SPEED0 (DI_33)                                
                                Target.SetIO("OUTPUT", 33 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                //REQ_SPEED0 (DI_34)                                
                                Target.SetIO("OUTPUT", 34 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                //REQ_SPEED0 (DI_35)                                
                                Target.SetIO("OUTPUT", 35 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
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
                                if(TaskJob.Params["@Command"].Equals("8") ||        //FORK_ 
                                    TaskJob.Params["@Command"].Equals("12") ||      //WHLD_ 
                                    TaskJob.Params["@Command"].Equals("13") ||      //WRLS_
                                     TaskJob.Params["@Command"].Equals("18") ||     //PNSTS
                                    TaskJob.Params["@Command"].Equals("26"))        //FOKCB
                                {
                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Arm") ? TaskJob.Params["@Arm"] : "1"), 2).PadLeft(2, '0');
                                    Target.SetIO("OUTPUT", 42 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 43 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Fork") ? TaskJob.Params["@Fork"] : "0"), 2).PadLeft(2, '0');
                                    Target.SetIO("OUTPUT", 44 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 45 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Vac") ? TaskJob.Params["@Vac"] : "0"), 2).PadLeft(2, '0');
                                    Target.SetIO("OUTPUT", 46 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 47 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    Target.SetIO("OUTPUT", 48 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 49 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 50 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 51 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 52 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 53 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 54 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 55 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 56 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 57 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 58 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 59 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 60 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 61 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 62 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 63 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 64 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 65 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 66 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 67 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 68 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 69 + (RobotStation - 1) * 32, 0);
                                }
                                else if(TaskJob.Params["@Command"].Equals("0") ||   //SHOME
                                    TaskJob.Params["@Command"].Equals("1") ||       //HOME_
                                    TaskJob.Params["@Command"].Equals("2") ||       //RET__
                                    TaskJob.Params["@Command"].Equals("7") ||       //ATX__ 
                                    TaskJob.Params["@Command"].Equals("10") ||      //MAP__
                                    TaskJob.Params["@Command"].Equals("15") ||      //RHOME
                                    TaskJob.Params["@Command"].Equals("16") ||      //ORG__
                                    TaskJob.Params["@Command"].Equals("21") ||      //SAVEP 
                                    TaskJob.Params["@Command"].Equals("22"))        //LOGSV
                                {
                                    binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Position") ? TaskJob.Params["@Position"] : "0"), 2).PadLeft(11, '0');
                                    Target.SetIO("OUTPUT", 42 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                    Target.SetIO("OUTPUT", 43 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 44 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 45 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 46 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 47 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 48 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 49 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 50 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 51 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 52 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    if(TaskJob.Params.ContainsKey("@Arm"))
                                    {
                                        binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params["@Arm"]), 2).PadLeft(2, '0');
                                        Target.SetIO("OUTPUT", 53 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                        Target.SetIO("OUTPUT", 54 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                    }
                                    else if(TaskJob.Params.ContainsKey("@Col"))
                                    {
                                        binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params["@Col"]), 2).PadLeft(2, '0');
                                        Target.SetIO("OUTPUT", 53 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                        Target.SetIO("OUTPUT", 54 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                    }
                                    else
                                    {
                                        Target.SetIO("OUTPUT", 53 + (RobotStation - 1) * 32, 0);
                                        Target.SetIO("OUTPUT", 54 + (RobotStation - 1) * 32, 0);
                                    }

                                    Target.SetIO("OUTPUT", 55 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 56 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 57 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 58 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 59 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 60 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 61 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 62 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 63 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 64 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 65 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 66 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 67 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 68 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 69 + (RobotStation - 1) * 32, 0);
                                }
                                else if(TaskJob.Params["@Command"].Equals("14"))
                                {
                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Arm") ? TaskJob.Params["@Arm"] : "1"), 2).PadLeft(2, '0');
                                    Target.SetIO("OUTPUT", 42 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 43 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = TaskJob.Params.ContainsKey("@Selector") ? TaskJob.Params["@Selector"].PadLeft(16, '0') : "0000000000000000";
                                    Target.SetIO("OUTPUT", 44 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[15].ToString()));
                                    Target.SetIO("OUTPUT", 45 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[14].ToString()));
                                    Target.SetIO("OUTPUT", 46 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[13].ToString()));
                                    Target.SetIO("OUTPUT", 47 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[12].ToString()));
                                    Target.SetIO("OUTPUT", 48 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[11].ToString()));
                                    Target.SetIO("OUTPUT", 49 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                    Target.SetIO("OUTPUT", 50 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 51 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 52 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 53 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 54 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 55 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 56 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 57 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 58 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 59 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    Target.SetIO("OUTPUT", 60 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 61 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 62 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 63 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 64 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 65 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 66 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 67 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 68 + (RobotStation - 1) * 32, 0);
                                    Target.SetIO("OUTPUT", 69 + (RobotStation - 1) * 32, 0);
                                }
                                else if(TaskJob.Params["@Command"].Equals("17"))    //AOFST
                                {
                                    binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Position") ? TaskJob.Params["@Position"] : "0"), 2).PadLeft(11, '0');
                                    Target.SetIO("OUTPUT", 42 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                    Target.SetIO("OUTPUT", 43 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 44 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 45 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 46 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 47 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 48 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 49 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 50 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 51 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 52 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = "";
                                    for (int i = 0; i < 27; i++) binaryStr += "0";
                                    if(TaskJob.Params.ContainsKey("@Offset"))
                                        binaryStr = Convert.ToString(Math.Abs(Convert.ToInt32(TaskJob.Params["@Offset"])), 2).PadLeft(27,'0');

                                    Target.SetIO("OUTPUT", 53 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[26].ToString()));
                                    Target.SetIO("OUTPUT", 54 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[25].ToString()));
                                    Target.SetIO("OUTPUT", 55 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[24].ToString()));
                                    Target.SetIO("OUTPUT", 56 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[23].ToString()));
                                    Target.SetIO("OUTPUT", 57 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[22].ToString()));
                                    Target.SetIO("OUTPUT", 58 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[21].ToString()));
                                    Target.SetIO("OUTPUT", 59 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[20].ToString()));
                                    Target.SetIO("OUTPUT", 60 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[19].ToString()));
                                    Target.SetIO("OUTPUT", 61 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[18].ToString()));
                                    Target.SetIO("OUTPUT", 62 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[17].ToString()));
                                    Target.SetIO("OUTPUT", 63 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[16].ToString()));
                                    Target.SetIO("OUTPUT", 64 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[15].ToString()));
                                    Target.SetIO("OUTPUT", 65 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[14].ToString()));
                                    Target.SetIO("OUTPUT", 66 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[13].ToString()));
                                    Target.SetIO("OUTPUT", 67 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[12].ToString()));
                                    Target.SetIO("OUTPUT", 68 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[11].ToString()));
                                    Target.SetIO("OUTPUT", 69 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                    Target.SetIO("OUTPUT", 70 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 71 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 72 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 73 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 74 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 75 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 76 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 77 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 78 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 79 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = TaskJob.Params.ContainsKey("@Offset") ? (Convert.ToInt32(TaskJob.Params["@Offset"]) < 0 ? "1" : "0"): "0";
                                    Target.SetIO("OUTPUT", 80 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                }
                                else if (TaskJob.Params["@Command"].Equals("20"))   //PDATA
                                {
                                    binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Position") ? TaskJob.Params["@Position"] : "0"), 2).PadLeft(11, '0');
                                    Target.SetIO("OUTPUT", 42 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                    Target.SetIO("OUTPUT", 43 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 44 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 45 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 46 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 47 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 48 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 49 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 50 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 51 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 52 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@FieldOffset") ? TaskJob.Params["@FieldOffset"] : "0"), 2).PadLeft(9, '0');
                                    Target.SetIO("OUTPUT", 53 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 54 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 55 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 56 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 57 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 58 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 59 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 60 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 61 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = "";
                                    for (int i = 0; i < 27; i++) binaryStr += "0";
                                    if (TaskJob.Params.ContainsKey("@Offset"))
                                        binaryStr = Convert.ToString(Math.Abs(Convert.ToInt32(TaskJob.Params["@Offset"])), 2).PadLeft(27, '0');

                                    Target.SetIO("OUTPUT", 62 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[26].ToString()));
                                    Target.SetIO("OUTPUT", 63 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[25].ToString()));
                                    Target.SetIO("OUTPUT", 64 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[24].ToString()));
                                    Target.SetIO("OUTPUT", 65 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[23].ToString()));
                                    Target.SetIO("OUTPUT", 66 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[22].ToString()));
                                    Target.SetIO("OUTPUT", 67 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[21].ToString()));
                                    Target.SetIO("OUTPUT", 68 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[20].ToString()));
                                    Target.SetIO("OUTPUT", 69 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[19].ToString()));
                                    Target.SetIO("OUTPUT", 70 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[18].ToString()));
                                    Target.SetIO("OUTPUT", 71 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[17].ToString()));
                                    Target.SetIO("OUTPUT", 72 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[16].ToString()));
                                    Target.SetIO("OUTPUT", 73 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[15].ToString()));
                                    Target.SetIO("OUTPUT", 74 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[14].ToString()));
                                    Target.SetIO("OUTPUT", 75 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[13].ToString()));
                                    Target.SetIO("OUTPUT", 76 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[12].ToString()));
                                    Target.SetIO("OUTPUT", 77 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[11].ToString()));
                                    Target.SetIO("OUTPUT", 78 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                    Target.SetIO("OUTPUT", 79 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 80 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 81 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 82 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 83 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 84 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 85 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 86 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 87 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 88 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = TaskJob.Params.ContainsKey("@Offset") ? (Convert.ToInt32(TaskJob.Params["@Offset"]) < 0 ? "1" : "0") : "0";
                                    Target.SetIO("OUTPUT", 89 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                }
                                else if(TaskJob.Params["@Command"].Equals("24") ||  //GETST 
                                    TaskJob.Params["@Command"].Equals("25"))        //PUTST
                                {
                                    binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Position") ? TaskJob.Params["@Position"] : "0"), 2).PadLeft(11, '0');

                                    Target.SetIO("OUTPUT", 42 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                    Target.SetIO("OUTPUT", 43 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 44 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 45 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 46 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 47 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 48 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 49 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 50 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 51 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 52 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Slot") ? TaskJob.Params["@Slot"] : "1"), 2).PadLeft(10, '0');

                                    Target.SetIO("OUTPUT", 53 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 54 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 55 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 56 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 57 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 58 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 59 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 60 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 61 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 62 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Arm") ? TaskJob.Params["@Arm"] : "1"), 2).PadLeft(2, '0');
                                    Target.SetIO("OUTPUT", 63 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 64 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Step") ? TaskJob.Params["@Step"] : "0"), 2).PadLeft(4, '0');
                                    Target.SetIO("OUTPUT", 65 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 66 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 67 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 68 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                }
                                else
                                {
                                    binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Position") ? TaskJob.Params["@Position"] : "0"), 2).PadLeft(11, '0');

                                    Target.SetIO("OUTPUT", 42 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                    Target.SetIO("OUTPUT", 43 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 44 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 45 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 46 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 47 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 48 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 49 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 50 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 51 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 52 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Slot") ? TaskJob.Params["@Slot"] : "1"), 2).PadLeft(10, '0');

                                    Target.SetIO("OUTPUT", 53 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                    Target.SetIO("OUTPUT", 54 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                    Target.SetIO("OUTPUT", 55 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                    Target.SetIO("OUTPUT", 56 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                    Target.SetIO("OUTPUT", 57 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                    Target.SetIO("OUTPUT", 58 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                    Target.SetIO("OUTPUT", 59 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                    Target.SetIO("OUTPUT", 60 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                    Target.SetIO("OUTPUT", 61 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 62 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Arm") ? TaskJob.Params["@Arm"] : "1"), 2).PadLeft(2, '0');

                                    Target.SetIO("OUTPUT", 63 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 64 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Option") ? TaskJob.Params["@Option"] : "0", 2)).PadLeft(2, '0');

                                    Target.SetIO("OUTPUT", 65 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                    Target.SetIO("OUTPUT", 66 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@AL") ? TaskJob.Params["@AL"] : "0", 2));

                                    Target.SetIO("OUTPUT", 67 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    Target.SetIO("OUTPUT", 68 + (RobotStation - 1) * 32, 0);

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@APD") ? TaskJob.Params["@APD"] : "0", 2));

                                    Target.SetIO("OUTPUT", 69 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@PUSH") ? TaskJob.Params["@PUSH"] : "0", 2));

                                    Target.SetIO("OUTPUT", 70 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                    if(TaskJob.Params.ContainsKey("@Selector"))
                                    {
                                        binaryStr = TaskJob.Params["@Selector"].PadLeft(16, '0');
                                        Target.SetIO("OUTPUT", 70 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[15].ToString()));
                                        Target.SetIO("OUTPUT", 71 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[14].ToString()));
                                        Target.SetIO("OUTPUT", 72 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[13].ToString()));
                                        Target.SetIO("OUTPUT", 73 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[12].ToString()));
                                        Target.SetIO("OUTPUT", 74 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[11].ToString()));
                                        Target.SetIO("OUTPUT", 75 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                        Target.SetIO("OUTPUT", 76 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                        Target.SetIO("OUTPUT", 77 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                        Target.SetIO("OUTPUT", 78 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                        Target.SetIO("OUTPUT", 79 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                        Target.SetIO("OUTPUT", 80 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                        Target.SetIO("OUTPUT", 81 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                        Target.SetIO("OUTPUT", 82 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                        Target.SetIO("OUTPUT", 83 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                        Target.SetIO("OUTPUT", 84 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                        Target.SetIO("OUTPUT", 85 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                    }

                                    if (TaskJob.Params.ContainsKey("@Selector2"))
                                    {
                                        binaryStr = TaskJob.Params["@Selector2"].PadLeft(16, '0');
                                        Target.SetIO("OUTPUT", 86 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[15].ToString()));
                                        Target.SetIO("OUTPUT", 87 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[14].ToString()));
                                        Target.SetIO("OUTPUT", 88 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[13].ToString()));
                                        Target.SetIO("OUTPUT", 89 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[12].ToString()));
                                        Target.SetIO("OUTPUT", 90 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[11].ToString()));
                                        Target.SetIO("OUTPUT", 91 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[10].ToString()));
                                        Target.SetIO("OUTPUT", 92 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[9].ToString()));
                                        Target.SetIO("OUTPUT", 93 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[8].ToString()));
                                        Target.SetIO("OUTPUT", 94 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[7].ToString()));
                                        Target.SetIO("OUTPUT", 95 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[6].ToString()));
                                        Target.SetIO("OUTPUT", 96 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                        Target.SetIO("OUTPUT", 97 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                        Target.SetIO("OUTPUT", 98 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                        Target.SetIO("OUTPUT", 99 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                        Target.SetIO("OUTPUT", 100 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                        Target.SetIO("OUTPUT", 101 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                    }


                                }
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
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_POWER_ON, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Command") ? TaskJob.Params["@Command"] : "0"), 2).PadLeft(6, '0');

                                Target.SetIO("OUTPUT", 36 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[5].ToString()));
                                Target.SetIO("OUTPUT", 37 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[4].ToString()));
                                Target.SetIO("OUTPUT", 38 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[3].ToString()));
                                Target.SetIO("OUTPUT", 39 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[2].ToString()));
                                Target.SetIO("OUTPUT", 40 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 41 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                SpinWait.SpinUntil(() => false, 500);
                                //REQ_INPUT(DI_01)
                                Target.SetIO("OUTPUT", 1 + (RobotStation - 1) * 32, 1);
                                //STS_INPUT (DO_03) 
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[3 + (RobotStation - 1) * 32] == 1, 3000);

                                if (Target.GetIO("INPUT")[3 + (RobotStation - 1) * 32] == 0)//Check STS_INPUT (DO_03) 
                                {
                                    AbortTask(TaskJob, Target, "STS_INPUT (DO_03)    not match to 1");
                                    //REQ_INPUT(DI_01)
                                    Target.SetIO("OUTPUT", 1 + (RobotStation - 1) * 32, 0);

                                    return;
                                }
                                //REQ_EXEC (DI_02) 
                                Target.SetIO("OUTPUT", 2 + (RobotStation - 1) * 32, 1);
                                //STS_INPUT (DO_03) 
                                //SpinWait.SpinUntil(() => Target.GetIO("INPUT")[3] == '1', 15000);
                                //if (Target.GetIO("INPUT")[3] == '0')//Check STS_INPUT (DO_03) 
                                //{
                                //    AbortTask(TaskJob, Target, "STS_INPUT (DO_03)   not match to 1");
                                //    return;
                                //}
                                //STS_EXEC (DO_04)
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[4 + (RobotStation - 1) * 32] == 1, 1000);

                                if (Target.GetIO("INPUT")[4 + (RobotStation - 1) * 32] == 0)//Check STS_EXEC (DO_04)
                                {
                                    AbortTask(TaskJob, Target, "STS_EXEC (DO_04)   not match to 1");
                                    //REQ_INPUT(DI_01)
                                    Target.SetIO("OUTPUT", 1 + (RobotStation - 1) * 32, 0);
                                    //REQ_EXEC (DI_02) 
                                    Target.SetIO("OUTPUT", 2 + (RobotStation - 1) * 32, 0);
                                    return;
                                }
                                //REQ_INPUT(DI_01)
                                Target.SetIO("OUTPUT", 1 + (RobotStation - 1) * 32, 0);
                                //REQ_EXEC (DI_02) 
                                Target.SetIO("OUTPUT", 2 + (RobotStation - 1) * 32, 0);
                                //STS_EXEC (DO_04)
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[4 + (RobotStation - 1) * 32] == 0, 60000);
                                if (Target.GetIO("INPUT")[4 + (RobotStation - 1) * 32] == 1)//Check STS_EXEC (DO_04)
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
                                //Check  STS_REDAY  (DO_00) 
                                if (Target.GetIO("INPUT")[0 + (RobotStation - 1) * 32] != 1)
                                {
                                    AbortTask(TaskJob, Target, "STS_REDAY not match");
                                    return;
                                }

                                if (Target.GetIO("INPUT")[6 + (RobotStation - 1) * 32] == 1)
                                {
                                    AbortTask(TaskJob, Target, "ALARM is happend STS_ERROR (DO_06) ");
                                    return;
                                }

                                //REQ_REMOTE (DI_00)                          
                                Target.SetIO("OUTPUT", 0 + (RobotStation - 1) * 32, 1);

                                //Wait for  STS_REMOTE (DO_01) STS_WAIT (DO_02) 
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[1 + (RobotStation - 1) * 32] == 1 && Target.GetIO("INPUT")[2 + (RobotStation - 1) * 32] == 1, 3000);

                                if (Target.GetIO("INPUT")[1 + (RobotStation - 1) * 32] == 0 || Target.GetIO("INPUT")[2 + (RobotStation - 1) * 32] == 0)//Check STS_REMOTE (DO_01) STS_WAIT (DO_02) 
                                {
                                    AbortTask(TaskJob, Target, "STS_REMOTE (DO_01) STS_WAIT (DO_02)  not match");
                                    return;
                                }
                                //REQ_SERVO (DI_08) 
                                Target.SetIO("OUTPUT", 8 + (RobotStation - 1) * 32, 1);
                                if (Target.GetIO("INPUT")[9 + (RobotStation - 1) * 32] == 0)//Check STS_SERVO (DO_09) 
                                {
                                    AbortTask(TaskJob, Target, "STS_REMOTE (DO_01) STS_WAIT (DO_02)  not match");
                                    return;
                                }
                                //REQ_ENTER (DI_07)
                                Target.SetIO("OUTPUT", 7 + (RobotStation - 1) * 32, 1);
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
                                //Check  STS_REDAY  (DO_00) 
                                if (Target.GetIO("INPUT")[0 + (RobotStation - 1) * 32] != 1)
                                {
                                    AbortTask(TaskJob, Target, "STS_REDAY not match");
                                    return;
                                }
                                //REQ_REMOTE (DI_00)                          
                                Target.SetIO("OUTPUT", 0 + (RobotStation - 1) * 32, 1);
                                //REQ_RESET (DI_06) 

                                Target.SetIO("OUTPUT", 6 + (RobotStation - 1) * 32, 1);
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[6 + (RobotStation - 1) * 32] == 0, 15000);
                                Target.SetIO("OUTPUT", 6 + (RobotStation - 1) * 32, 0);

                                if (Target.GetIO("INPUT")[6 + (RobotStation - 1) * 32] == 1)//Wait for  STS_ERROR (DO_06) 
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
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " exception: " + e.StackTrace);
                AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), e.StackTrace);
                return;
            }
            return;
        }
        private void AbortTask(TaskFlowManagement.CurrentProcessTask TaskJob, Node Node, string Message)
        {
            _TaskReport.On_Alarm_Happen(AlarmManagement.NewAlarm(Node, Message));

            _TaskReport.On_TaskJob_Aborted(TaskJob);
            _TaskReport.On_Message_Log("CMD", TaskJob.TaskName.ToString() + " Aborted");

        }
        private void FinishTask(TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            if (TaskJob.TaskName.ToString().IndexOf("CCLINK") == -1)
            {
                _TaskReport.On_Message_Log("CMD", TaskJob.TaskName.ToString() + " Finished");
            }
            _TaskReport.On_TaskJob_Finished(TaskJob);

        }
    }


}
