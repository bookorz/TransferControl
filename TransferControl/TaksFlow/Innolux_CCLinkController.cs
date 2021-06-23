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
    class Innolux_CCLinkController : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(Sanwa_CCLinkController));
        IUserInterfaceReport _TaskReport;

        public Innolux_CCLinkController(IUserInterfaceReport TaskReport)
        {
            _TaskReport = TaskReport;
        }
        public void Excute(object input)
        {
            TaskFlowManagement.CurrentProcessTask TaskJob = (TaskFlowManagement.CurrentProcessTask)input;
            logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());

            Node Target = null;
            string Stage = "";
            string Address = "";
            string WorkType = "";
            string Code = "";
            string Hand = "";
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
                        case "@Stage":
                            Stage = item.Value;
                            break;
                        case "@Code":
                            Code = item.Value;
                            break;
                        case "@Address":
                            Address = item.Value;
                            break;
                        case "@WorkType":
                            WorkType = item.Value;
                            break;
                        case "@Hand":
                            Hand = item.Value;
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
                                //if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                //{
                                //    //    //中止Task
                                //    AbortTask(TaskJob, Target, "TASK_ABORT");

                                //    break;
                                //}
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_POWER_ON, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }

                                //Robot Software 原点復帰_On
                                Target.SetIO("OUTPUT", 37, 1);

                                //Robot Software 原点
                                //等待變化    
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[37] == 1 , 30000);

                                //Robot Task 起動中
                                if (Target.GetIO("INPUT")[34] == 1)
                                {
                                    AbortTask(TaskJob, Target, "Robot Task 起動中(DI_34) On Error");
                                    break;
                                }

                                //Robot Software 原点復帰_Off
                                Target.SetIO("OUTPUT", 37, 0);

                                //Robot Task Cycle 停止
                                Target.SetIO("OUTPUT", 34, 1);

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
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_POWER_ON, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");

                                    break;
                                }

                                if (Target.GetIO("INPUT")[34] != 0)
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "Robot Task On Running(DI_34) OFF Error");
                                    break;
                                }

                                ///Robot Task 初期起動
                                Target.SetIO("OUTPUT", 33, 1);

                                //Robot Task 起動
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[34] == 1, 30000);

                                Target.SetIO("OUTPUT", 33, 0);

                                if (Target.GetIO("INPUT")[34] != 1)
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "Robot Task Running(DI_34) On Timeout");

                                    break;
                                }

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_ABORT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                //Robot Task 起動中
                                if (Target.GetIO("INPUT")[34] != 1)
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "Robot Task On Running(DI_34) On Error");
                                    break;
                                }

                                ///Robot Task Cycle 停止
                                Target.SetIO("OUTPUT", 34, 0);

                                ////Robot Task 起動中
                                ////等待變化    
                                //SpinWait.SpinUntil(() => Target.GetIO("INPUT")[34] == 0, 30000);

                                //if(Target.GetIO("INPUT")[34] == 1)
                                //{
                                //    //中止Task
                                //    AbortTask(TaskJob, Target, "Robot Task On Running(DI_34) Off Error");
                                //    //break;
                                //}

                                ///Robot Task Cycle 停止
                                //Target.SetIO("OUTPUT", 34, 0);

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

                                //Robot Task 起動中
                                if (Target.GetIO("INPUT")[34] != 1)
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "Robot Task Run On (DI_34) Error");
                                    break;
                                }

                                //Robot Task Hold 停止
                                Target.SetIO("OUTPUT", 35, 0);

                                //Robot Task Hold 停止中  
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[35] == 1, 10000);
                                if(Target.GetIO("INPUT")[35] == 0)
                                {

                                    AbortTask(TaskJob, Target, "Robot Task Hold Stop Off(DI_35) Error");
                                }

                                Target.SetIO("OUTPUT", 35, 1);

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
                                //Robot Task Hold 停止中  
                                if (Target.GetIO("INPUT")[35] != 1)
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "Robot Task Hold On (DI_34) Error");
                                    break;
                                }

                                //Robot Task Hold 解除
                                Target.SetIO("OUTPUT", 36, 1);

                                //Robot Task 起動中 
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[34] == 1, 10000);

                                if(Target.GetIO("INPUT")[34] == 0)
                                {

                                    AbortTask(TaskJob, Target, "Robot Task Run on(DI_34) Error");
                                }

                                //Robot Task Hold 解除
                                Target.SetIO("OUTPUT", 36, 0);

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
                                //TaskJob.Params.Add("@Command", "0");
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 1:
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

                    case TaskFlowManagement.Command.CCLINK_CMD_PARAM:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Stage") ? TaskJob.Params["@Stage"] : "0"), 2).PadLeft(2, '0');
                                Target.SetIO("OUTPUT", 7, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 8, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Code") ? TaskJob.Params["@Code"] : "0"), 2).PadLeft(5, '0');
                                Target.SetIO("OUTPUT", 9, Convert.ToByte(binaryStr[4].ToString()));
                                Target.SetIO("OUTPUT", 10, Convert.ToByte(binaryStr[3].ToString()));
                                Target.SetIO("OUTPUT", 11, Convert.ToByte(binaryStr[2].ToString()));
                                Target.SetIO("OUTPUT", 12, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 13, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Address") ? TaskJob.Params["@Address"] : "0"), 2).PadLeft(7, '0');
                                Target.SetIO("OUTPUT", 14, Convert.ToByte(binaryStr[6].ToString()));
                                Target.SetIO("OUTPUT", 15, Convert.ToByte(binaryStr[5].ToString()));
                                Target.SetIO("OUTPUT", 16, Convert.ToByte(binaryStr[4].ToString()));
                                Target.SetIO("OUTPUT", 17, Convert.ToByte(binaryStr[3].ToString()));
                                Target.SetIO("OUTPUT", 18, Convert.ToByte(binaryStr[2].ToString()));
                                Target.SetIO("OUTPUT", 19, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 20, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@WorkType") ? TaskJob.Params["@WorkType"] : "0"), 2).PadLeft(6, '0');
                                Target.SetIO("OUTPUT", 21, Convert.ToByte(binaryStr[5].ToString()));
                                Target.SetIO("OUTPUT", 22, Convert.ToByte(binaryStr[4].ToString()));
                                Target.SetIO("OUTPUT", 23, Convert.ToByte(binaryStr[3].ToString()));
                                Target.SetIO("OUTPUT", 24, Convert.ToByte(binaryStr[2].ToString()));
                                Target.SetIO("OUTPUT", 25, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 26, Convert.ToByte(binaryStr[0].ToString()));


                                binaryStr = Convert.ToString(Convert.ToInt32(TaskJob.Params.ContainsKey("@Hand") ? TaskJob.Params["@Hand"] : "0"), 2).PadLeft(2, '0');
                                Target.SetIO("OUTPUT", 27, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 28, Convert.ToByte(binaryStr[0].ToString()));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.ROBOT_GETWAIT:
                    case TaskFlowManagement.Command.ROBOT_GET:
                    case TaskFlowManagement.Command.ROBOT_PUTWAIT:
                    case TaskFlowManagement.Command.ROBOT_PUT:
                    case TaskFlowManagement.Command.ROBOT_SEARCH_SUBSTRATE_EDGE:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_PARAM, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");
                                }
                                break;
                            case 1:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_CMD_EXE, TaskJob.Params).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "TASK_ABORT");
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

                                //無異常
                                if (Target.GetIO("INPUT")[42] == 1 ||
                                    Target.GetIO("INPUT")[43] == 1 ||
                                    Target.GetIO("INPUT")[44] == 1 ||
                                    Target.GetIO("INPUT")[45] == 1 ||
                                    Target.GetIO("INPUT")[46] == 1 ||
                                    Target.GetIO("INPUT")[47] == 1)
                                {
                                    AbortTask(TaskJob, Target, "ALARM is happend");
                                    return;
                                }

                                //Check System Ready (System Task 動作中)
                                if (Target.GetIO("INPUT")[39] != 1)
                                {
                                    AbortTask(TaskJob, Target, "System Ready(DI_39) Error");
                                    return;
                                }

                                //Robot Controller Local/ Teaching Mode
                                if (Target.GetIO("INPUT")[36] != 0)
                                {
                                    AbortTask(TaskJob, Target, "Check Local/ Teaching Mode(DI_36) Error");
                                    return;
                                }

                                //Check Servo Power
                                if (Target.GetIO("INPUT")[32] != 1)
                                {
                                    AbortTask(TaskJob, Target, "Check  Servo Power(DI_32) Error");
                                    return;
                                }

                                //Check Robot Task 起動可
                                if (Target.GetIO("INPUT")[33] != 1)
                                {
                                    AbortTask(TaskJob, Target, "Task Can Start(DI_33) Error");
                                    return;
                                }

                                //Robot Task 啟動中
                                //if (Target.GetIO("INPUT")[34] != 0)
                                //{
                                //    //中止Task
                                //    AbortTask(TaskJob, Target, "Task On Running(DI_34) On");

                                //    break;
                                //}


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
                                //Robot Task 起動中
                                if (Target.GetIO("INPUT")[34] != 1)
                                {
                                    //中止Task
                                    AbortTask(TaskJob, Target, "Robot Task 起動中(DI_34) 訊號異常");
                                }
                                break;

                            case 1:

                                //動作実行中
                                if (Target.GetIO("INPUT")[30] != 0)
                                {
                                    AbortTask(TaskJob, Target, "動作実行中(DI_30)On Error");
                                    return;
                                }

                                //動作実行完了(確認上個流程結束)
                                if (Target.GetIO("INPUT")[31] != 0)
                                {
                                    AbortTask(TaskJob, Target, "動作実行完了(DI_31)On Error");
                                    return;
                                }


                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Mode") ? TaskJob.Params["@Mode"] : "0"), 2);
                                Target.SetIO("OUTPUT", 45, Convert.ToByte(binaryStr[0].ToString()));

                                binaryStr = Convert.ToString(Convert.ToInt16(TaskJob.Params.ContainsKey("@Command") ? TaskJob.Params["@Command"] : "0"), 2).PadLeft(6, '0');

                                Target.SetIO("OUTPUT", 1, Convert.ToByte(binaryStr[5].ToString()));
                                Target.SetIO("OUTPUT", 2, Convert.ToByte(binaryStr[4].ToString()));
                                Target.SetIO("OUTPUT", 3, Convert.ToByte(binaryStr[3].ToString()));
                                Target.SetIO("OUTPUT", 4, Convert.ToByte(binaryStr[2].ToString()));
                                Target.SetIO("OUTPUT", 5, Convert.ToByte(binaryStr[1].ToString()));
                                Target.SetIO("OUTPUT", 6, Convert.ToByte(binaryStr[0].ToString()));

                                SpinWait.SpinUntil(() => Target.GetIO("OUTPUT")[1] == Target.GetIO("OUTPUT_OLD")[1] &&
                                                        Target.GetIO("OUTPUT")[2] == Target.GetIO("OUTPUT_OLD")[2] &&
                                                        Target.GetIO("OUTPUT")[3] == Target.GetIO("OUTPUT_OLD")[3] &&
                                                        Target.GetIO("OUTPUT")[4] == Target.GetIO("OUTPUT_OLD")[4] &&
                                                        Target.GetIO("OUTPUT")[5] == Target.GetIO("OUTPUT_OLD")[5] &&
                                                        Target.GetIO("OUTPUT")[6] == Target.GetIO("OUTPUT_OLD")[6],
                                                        Timeout.Infinite);

                                //動作Start
                                Target.SetIO("OUTPUT", 0, 1);

                                //Command 一致
                                Target.SetIO("OUTPUT", 29, 1);

                                //動作実行中
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[28] == 0, 1000);

                                Target.SetIO("OUTPUT", 29, 0);

                                //動作実行中
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[30] == 1, 3000);

                                if (Target.GetIO("INPUT")[30] != 1)
                                {
                                    AbortTask(TaskJob, Target, "動作実行中(DI_30)On Timeout");
                                    return;
                                }

                                //動作Start
                                Target.SetIO("OUTPUT", 0, 0);

                                //動作実行完了
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[31] == 1, 30000);
                                if(Target.GetIO("INPUT")[31] != 1)
                                {
                                    AbortTask(TaskJob, Target, "動作実行完了(DI_31)On TimeOut");
                                    return;
                                }

                                Target.SetIO("OUTPUT", 31, 1);
                                SpinWait.SpinUntil(() => Target.GetIO("INPUT")[31] == 0, 30000);

                                Target.SetIO("OUTPUT", 31, 0);
                                if (Target.GetIO("INPUT")[31] != 0)
                                {
                                    AbortTask(TaskJob, Target, "Robot Task Finishing(DI_31)Off TimeOut");
                                    return;
                                }

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
                                Target.SetIO("OUTPUT", 32, 1);

                                SpinWait.SpinUntil(() => false, 1000);

                                Target.SetIO("OUTPUT", 32, 0);

                                break;

                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
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
