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
        IUserInterfaceReport _TaskReport;
        public VerticalChamberOven_200(IUserInterfaceReport TaskReport)
        {
            _TaskReport = TaskReport;
        }

        public bool Excute(TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());
            string Message = "";
            Node Target = null;
            Node Position = null;
            string tmp = "";
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
                    }
                }
            }
            try
            {

                switch (TaskJob.TaskName)
                {
                    case TaskFlowManagement.Command.ROBOT_GET_ERROR:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                //REQ_REMOTE (DI_00) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 0, PLC_Bit = 1 }));
                                break;
                            case 1:

                                //REQ_INPUT(DI_01) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 1, PLC_Bit = 0 }));
                                break;
                            case 2:

                                //REQ_EXEC(DI_02) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 2, PLC_Bit = 0 }));
                                break;
                            case 3:

                                //REQ_EXEC(DI_02) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 42, PLC_Bit = 1 }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return false;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_MODE:
                        string modeStr = TaskJob.Params["@Value"];
                         binaryStr = Convert.ToString(Convert.ToUInt16(modeStr), 2).PadLeft(2, '0');
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                //REQ_MODE_1 (DI_09) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 9, PLC_Bit = Convert.ToByte(binaryStr[1].ToString()) }));
                                break;
                            case 1:

                                //REQ_MODE_2 (DI_10) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 10, PLC_Bit = Convert.ToByte(binaryStr[0].ToString()) }));
                                break;
                            
                            default:
                                FinishTask(TaskJob);
                                return false;
                        }

                        break;
                    case TaskFlowManagement.Command.ROBOT_SPEED:
                        string SpeedStr = TaskJob.Params["@Value"];
                        double Speed = (int)Math.Round(Convert.ToDouble(SpeedStr));
                        int digit10 = (int)(Speed / Math.Pow(10, 1) % 10);
                         binaryStr = Convert.ToString(digit10, 2).PadLeft(4, '0');
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                //REQ_SPEED0 (DI_32) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 32, PLC_Bit = Convert.ToByte(binaryStr[3].ToString()) }));
                                break;
                            case 1:

                                //REQ_SPEED1 (DI_33) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 33, PLC_Bit = Convert.ToByte(binaryStr[2].ToString()) }));
                                break;
                            case 2:

                                //REQ_SPEED2 (DI_34) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 34, PLC_Bit = Convert.ToByte(binaryStr[1].ToString()) }));
                                break;
                            case 3:

                                //REQ_SPEED3 (DI_35) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 35, PLC_Bit = Convert.ToByte(binaryStr[0].ToString()) }));
                                break;
                            default:
                                FinishTask(TaskJob);
                                return false;
                        }

                        break;
                    case TaskFlowManagement.Command.ROBOT_HOME:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.ReadBit, PLC_Station = 1, PLC_Area = "X", PLC_StartAddress = 0x500, PLC_Len = 128 }));

                                break;
                            case 1:
                                Target = NodeManagement.Get("ROBOT");
                                Target.IO.TryGetValue("X*000500", out tmp);
                                if (tmp[0] == '0' || tmp[1] == '0' || tmp[2] == '0' || tmp[9] == '0')
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.CCLINK_POWER_ON).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), "TASK_ABORT");

                                        break;
                                    }
                                }
                                break;
                            case 2:
                                
                                break;
                            case 3:

                                 break;
                            case 4:

                               break;
                            default:
                                FinishTask( TaskJob);
                                return false;
                        }
                        break;
                    case TaskFlowManagement.Command.CCLINK_POWER_ON:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:

                                //REQ_ENTER(DI_07)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 7, PLC_Bit = 1 }));
                                break;
                            case 1:

                                //REQ_REMOTE(DI_00) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 0, PLC_Bit = 0 }));
                                break;
                            case 2:

                                //REQ_SERVO(DI_08) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 8, PLC_Bit = 0 }));
                                break;
                            case 3:

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.ReadBit, PLC_Station = 1, PLC_Area = "X", PLC_StartAddress = 0x500, PLC_Len = 128 }));

                                break;
                            case 4:
                                Target = NodeManagement.Get("ROBOT");
                                Target.IO.TryGetValue("X*000500", out tmp);
                                if (tmp[0] != '1')//STS_REDAY 
                                {
                                    AbortTask(TaskJob, Target, "STS_NOT_REDAY");
                                }
                                break;
                            case 5:
                                //REQ_REMOTE(DI_00) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 0, PLC_Bit = 1 }));

                                break;
                            case 6:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.ReadBit, PLC_Station = 1, PLC_Area = "X", PLC_StartAddress = 0x500, PLC_Len = 128 }));

                                break;
                            case 7:
                                Target = NodeManagement.Get("ROBOT");
                                Target.IO.TryGetValue("X*000500", out tmp);
                                if (tmp[1] == '0' || tmp[2] == '0')//Wait for  STS_REMOTE(DO_01)  STS_WAIT(DO_02) 
                                {
                                    TaskJob.CurrentIndex = TaskJob.CurrentIndex-2;
                                }
                                break;
                            case 8:
                                //REQ_SERVO(DI_08) 
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.WriteBit, PLC_Station = 1, PLC_Area = "Y", PLC_StartAddress = DEF_IN_DATA_ADDRESS + 8, PLC_Bit = 1 }));

                                break;
                            case 9:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT", "EXCUTED", new Transaction { Method = Transaction.Command.Mitsubishi_PLC.ReadBit, PLC_Station = 1, PLC_Area = "X", PLC_StartAddress = 0x500, PLC_Len = 128 }));

                                break;
                            case 10:
                                Target = NodeManagement.Get("ROBOT");
                                Target.IO.TryGetValue("X*000500", out tmp);
                                if (tmp[9] == '0')//Wait for STS_SERVO(DO_09)
                                {
                                    TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;
                                }
                                break;
                            default:
                                FinishTask( TaskJob);
                                return false;
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
                        NodeManagement.Get(eachCmd.NodeName).SendCommand(eachCmd.Txn, out Message);
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
                        bool re = this.Excute(TaskJob);
                        if (!re)
                        {
                            return false;
                        }
                    }
                }


            }
            catch (Exception e)
            {
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " exception: " + e.StackTrace);
                AbortTask( TaskJob, NodeManagement.Get("SYSTEM"), e.StackTrace);
                return false;
            }
            return true;
        }
        private void AbortTask( TaskFlowManagement.CurrentProcessTask TaskJob, Node Node, string Message)
        {
            _TaskReport.On_Alarm_Happen(new AlarmManagement.Alarm(Node, Message));

            _TaskReport.On_TaskJob_Aborted(TaskJob);
        }
        private void FinishTask( TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            _TaskReport.On_TaskJob_Finished(TaskJob);
        }
    }
}
