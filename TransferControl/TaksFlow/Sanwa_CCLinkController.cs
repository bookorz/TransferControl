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

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Slot") ? TaskJob.Params["@Slot"] : "1", 2)).PadLeft(10, '0');
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

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Arm") ? TaskJob.Params["@Arm"] : "1", 2)).PadLeft(2, '0');
                                Target.SetIO("OUTPUT", 63 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 64 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Option") ? TaskJob.Params["@Option"] : "0", 2)).PadLeft(2, '0');
                                Target.SetIO("OUTPUT", 65 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 66 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@AL") ? TaskJob.Params["@AL"] : "0", 2));
                                Target.SetIO("OUTPUT", 67 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));
                                Target.SetIO("OUTPUT", 68 + (RobotStation - 1) * 32, 0);

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@APD") ? TaskJob.Params["@APD"] : "0", 2));
                                Target.SetIO("OUTPUT", 69 + (RobotStation - 1) * 32, Convert.ToByte(binaryStr[0].ToString()));



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
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[3 + (RobotStation - 1) * 32] == 1, 1000);

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
