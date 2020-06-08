using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TransferControl.CommandConvert;
using TransferControl.Config;
using TransferControl.Engine;
using TransferControl.TaksFlow;

namespace TransferControl.Management
{
    public class TaskFlowManagement : IUserInterfaceReport
    {
        static ILog logger = LogManager.GetLogger(typeof(TaskFlowManagement));
        static IUserInterfaceReport _TaskReport;
        static ITaskFlow TaskFlow;
        static Dictionary<string, CurrentProcessTask> CurrentProcessTasks = new Dictionary<string, CurrentProcessTask>();
        public class CurrentProcessTask
        {
            public string Id { get; set; }
            public Command TaskName { get; set; }
            public Dictionary<string, string> Params { get; set; }
            public List<ExcutedCmd> CheckList = new List<ExcutedCmd>();
            public int RepeatCount = 0;
            public int CurrentIndex = 0;
            public bool Finished = false;
            public bool HasError = false;
           
            public bool Promise()
            {
                while (!Finished)
                {
                    SpinWait.SpinUntil(() => Finished, 999999);
                }
                return !HasError;
            }
        }
        public class ExcutedCmd
        {
            public ExcutedCmd(string NodeName, string ExcuteType, Transaction Txn)
            {
                this.NodeName = NodeName;
                this.ExcuteName = Txn.Method;
                this.ExcuteType = ExcuteType;
                this.Txn = Txn;
            }
            public string NodeName { get; set; }
            public string ExcuteName { get; set; }
            public string ExcuteType { get; set; }
            public Transaction Txn { get; set; }
            public bool Finished = false;
        }
        public TaskFlowManagement(IUserInterfaceReport TaskReport)
        {

            _TaskReport = TaskReport;
            MainControl ctrl = new MainControl(this);
            switch (SystemConfig.Get().TaskFlow.ToUpper())
            {
                case "KAWASAKI_3P_EFEM":
                    //TaskFlow = new Kawasaki_3P_EFEM();
                    break;
                case "SANWA_SORTER":
                    //TaskFlow = new Sanwa_Sorter();
                    break;
                case "WTS":
                    //TaskFlow = new WTS();
                    break;
                case "VERTICALCHAMBEROVEN_200":
                    TaskFlow = new VerticalChamberOven_200(this);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        public static void Clear()
        {
            lock (CurrentProcessTasks)
            {
                CurrentProcessTasks.Clear();
            }
        }
        public static void TaskRemove(CurrentProcessTask Task)
        {
           
            lock (CurrentProcessTasks)
            {

                logger.Debug("Delete Task "+ Task.TaskName.ToString()+ ":" + Task.Id);

                CurrentProcessTasks.Remove(Task.Id);
            }
           
        }
        private static void Next(Node Node, Transaction Txn, string ReturnType)
        {
            CurrentProcessTask CurrentTask = Txn.TaskObj;
            int count = 0;
           
                lock (CurrentProcessTasks)
                {
                    var findExcuted = from each in CurrentTask.CheckList
                                      where each.ExcuteName.Equals(Txn.Method) && each.ExcuteType.Equals(ReturnType.ToUpper()) && each.NodeName.Equals(Txn.NodeName) && !each.Finished
                                      select each;
                    if (findExcuted.Count() != 0)//標記完成的命令
                    {
                        findExcuted.First().Finished = true;
                    }

                    findExcuted = from each in CurrentTask.CheckList
                                  where !each.Finished
                                  select each;
                    count = findExcuted.Count();
                }
                if (count == 0)//當全部完成後，繼續下一步
                {
                    CurrentTask.CurrentIndex++;
                    CurrentTask.CheckList.Clear();
                    if (!TaskFlow.Excute(CurrentTask))
                    {
                        //TaskRemove(CurrentTask.Id);//執行發生異常時，移除此Task
                    }
                }
           

        }
        public static CurrentProcessTask Excute( Command TaskName, Dictionary<string, string> param = null)
        {
            CurrentProcessTask result = null;
            string Id = Guid.NewGuid().ToString();
            lock (CurrentProcessTasks)
            {
                if (!CurrentProcessTasks.ContainsKey(Id))
                {
                    result = new CurrentProcessTask();
                    CurrentProcessTasks.Add(Id, result);
                    result.Id = Id;
                    result.Params = param;
                    result.TaskName = TaskName;
                    logger.Debug("TaskName:" + TaskName.ToString());
                    if (!TaskFlow.Excute(result))
                    {
                        //TaskRemove(result.Id);//執行發生異常時，移除此Task
                    }
                }
                else
                {
                    logger.Error("ID is exsit:" + Id);
                }
            }
            return result;
        }

        public void On_Command_Excuted(Node Node, Transaction Txn, CommandReturnMessage Msg)
        {
            Next(Node, Txn, "Excuted");
            _TaskReport.On_Command_Excuted(Node,  Txn,  Msg);
        }

        public void On_Command_Error(Node Node, Transaction Txn, CommandReturnMessage Msg)
        {
            _TaskReport.On_Command_Error(Node, Txn, Msg);
            On_TaskJob_Aborted(Txn.TaskObj);
        }

        public void On_Command_Finished(Node Node, Transaction Txn, CommandReturnMessage Msg)
        {
            Next(Node, Txn, "Finished");
            _TaskReport.On_Command_Finished(Node, Txn, Msg);
        }

        public void On_Command_TimeOut(Node Node, Transaction Txn)
        {
            _TaskReport.On_Command_TimeOut(Node, Txn);           
            
            On_TaskJob_Aborted(Txn.TaskObj);
        }

        public void On_Event_Trigger(Node Node, CommandReturnMessage Msg)
        {
            _TaskReport.On_Event_Trigger(Node, Msg);
        }

        public void On_Node_State_Changed(Node Node, string Status)
        {
            _TaskReport.On_Node_State_Changed(Node, Status);
        }

        public void On_Node_Connection_Changed(string NodeName, string Status)
        {
            _TaskReport.On_Node_Connection_Changed(NodeName, Status);
        }

        public void On_Job_Location_Changed(Job Job)
        {
            _TaskReport.On_Job_Location_Changed(Job);
        }

        public void On_DIO_Data_Chnaged(string Parameter, string Value, string Type)
        {
            _TaskReport.On_DIO_Data_Chnaged( Parameter,  Value,  Type);
        }

        public void On_Connection_Error(string DIOName, string ErrorMsg)
        {
            _TaskReport.On_Connection_Error(DIOName, ErrorMsg);
        }

        public void On_Connection_Status_Report(string DIOName, string Status)
        {
            _TaskReport.On_Connection_Status_Report( DIOName,  Status);
        }

        public void On_Alarm_Happen(AlarmManagement.Alarm Alarm)
        {
            _TaskReport.On_Alarm_Happen( Alarm);
        }

        public void On_TaskJob_Ack(CurrentProcessTask Task)
        {

            _TaskReport.On_TaskJob_Ack( Task);
        }

        public void On_TaskJob_Aborted(CurrentProcessTask Task)
        {
            Task.HasError = true;
            Task.Finished = true;
            _TaskReport.On_TaskJob_Aborted(Task);
            
            TaskRemove(Task);
        }

        public void On_TaskJob_Finished(CurrentProcessTask Task)
        {
            Task.Finished = true;
            _TaskReport.On_TaskJob_Finished(Task);
            TaskRemove(Task);
        }

        public void On_Message_Log(string Type, string Message)
        {
            _TaskReport.On_Message_Log( Type,  Message);
        }

        public void On_Status_Changed(string Type, string Message)
        {
            _TaskReport.On_Status_Changed( Type,  Message);
        }

        public enum Command
        {
            CCLINK_,
            FFU_SET_SPEED,
            FFU_START,
            FFU_STOP,
            FFU_ALARM_BYPASS,
            ROBOT_RESET,
            CCLINK_POWER_ON,
            ROBOT_ORGSH,
            ROBOT_HOME,
            ROBOT_RETRACT,
            ROBOT_SERVO,
            ROBOT_WAFER_HOLD,
            ROBOT_WAFER_RELEASE,
            ROBOT_MODE,
            ROBOT_SPEED,
            ROBOT_GETWAIT,
            ROBOT_GET,
            ROBOT_PUTWAIT,
            ROBOT_PUT,
            ROBOT_GET_ERROR,
            ALIGNER_ALIGN,
            ALIGNER_HOME,
            ALIGNER_INIT,
            ALIGNER_ORGSH,
            ALIGNER_RESET,
            ALIGNER_SERVO,
            ALIGNER_SPEED,
            ALIGNER_MODE,
            ALIGNER_WAFER_HOLD,
            ALIGNER_WAFER_RELEASE,
            LOADPORT_INIT,
            LOADPORT_OPEN,
            LOADPORT_OPEN_NOMAP,
            LOADPORT_CLOSE,
            LOADPORT_CLOSE_NOMAP,
            LOADPORT_ORGSH,
            LOADPORT_RESET,
            LOADPORT_UNLOADCOMPLETE,
            LOADPORT_READYTOLOAD,
            LOADPORT_CLAMP,
            LOADPORT_UNCLAMP,
            LOADPORT_DOCK,
            LOADPORT_UNDOCK,
            LOADPORT_DOOR_CLOSE,
            LOADPORT_DOOR_DOWN,
            LOADPORT_DOOR_OPEN,
            LOADPORT_DOOR_UP,
            LOADPORT_FORCE_ORGSH,
            LOADPORT_GET_MAPDT,
            LOADPORT_LATCH,
            LOADPORT_UNLATCH,
            LOADPORT_VAC_ON,
            LOADPORT_VAC_OFF,
            LOADPORT_RE_MAPPING,
            LOADPORT_READ_LED,
            LOADPORT_READ_STATUS,
            LOADPORT_READ_VERSION,
            ALL_INIT,
            DISABLE_OPACCESS,
            SET_ALL_SPEED,
            STOP,
            TRANSFER_GET_LOADPORT,
            TRANSFER_GET_LOADPORT_2ARM,
            TRANSFER_PUT_UNLOADPORT,
            TRANSFER_PUT_UNLOADPORT_2ARM,
            TRANSFER_GET_ALIGNER01,
            TRANSFER_GET_ALIGNER02,
            TRANSFER_GET_ALIGNER01_1,
            TRANSFER_GET_ALIGNER02_1,
            TRANSFER_GET_ALIGNER01_2,
            TRANSFER_GET_ALIGNER02_2,
            TRANSFER_GETW_ALIGNER01,
            TRANSFER_GETW_ALIGNER02,
            TRANSFER_PUT_ALIGNER01,
            TRANSFER_PUT_ALIGNER02,
            TRANSFER_PUT_ALIGNER01_1,
            TRANSFER_PUT_ALIGNER02_1,
            TRANSFER_PUT_ALIGNER01_2,
            TRANSFER_PUT_ALIGNER02_2,
            TRANSFER_PUTW_ALIGNER01,
            TRANSFER_PUTW_ALIGNER02,
            TRANSFER_ALIGNER_WHLD,
            TRANSFER_ALIGNER_WRLS,
            TRANSFER_ALIGNER_ALIGN,
            TRANSFER_ALIGNER_HOME,
            //WTS
            CLAMP_ELPT,
            UNCLAMP_ELPT,
            FOUP_ID,
            MOVE_FOUP,
            STOP_STOCKER,
            RESUME_STOCKER,
            ABORT_STOCKER,
            RESET_STOCKER,
            INIT_STOCKER,
            OPEN_FOUP,
            CLOSE_FOUP,
            TRANSFER_WTS,
            STOP_WTS,
            RESUME_WTS,
            ABORT_WTS,
            RESET_WTS,
            TRANSFER_PTZ,
            BLOCK_PTZ,
            RELEASE_PTZ,
            PORT_ACCESS_MODE,
            RESET_E84,
            E84_MODE,
            BLOCK_ALIGNER,
            RELEASE_ALIGNER,
            //WTS Manual
            WHR_RESET,
            WHR_EXTEND,
            WHR_RETRACT,
            WHR_UP,
            WHR_DOWN,
            WHR_SHOME,
            WHR_PREPAREPICK,
            WHR_PICK,
            WHR_PREPAREPLACE,
            WHR_PLACE,
            WHR_SET_SPEED,
            CTU_HOME,
            CTU_RESET,
            CTU_INIT,
            CTU_PREPAREPICK,
            CTU_PREPAREPLACE,
            CTU_HOLD,
            CTU_RELEASE,
            CTU_PLACE,
            CTU_PICK,
            CTU_SET_SPEED,
            PTZ_TRANSFER,
            PTZ_HOME,
            PTZ_SET_SPEED,
            WTSALIGNER_ALIGN,
            WTSALIGNER_SET_SPEED,
            //Stocker Manual
            ELPT_READ_RFID,
            ELPT_CLAMP,
            ELPT_UNCLAMP,
            ELPT_OPEN_SHUTTER,
            ELPT_CLOSE_SHUTTER,
            ELPT_MOVE_IN,
            ELPT_MOVE_OUT,
            ELPT_SET_SPEED,
            ELPT_INIT,
            ELPT_RESET,
            LIGHT_CURTAIN_RESET,
            LIGHT_CURTAIN_ENABLED,
            ILPT_LOAD,
            ILPT_UNLOAD,
            ILPT_INIT,
            ILPT_RESET,
            FOUPROBOT_PREPARE_PICK,
            FOUPROBOT_PREPARE_PLACE,
            FOUPROBOT_PICK,
            FOUPROBOT_PLACE,
            FOUPROBOT_INIT,
            FOUPROBOT_RESET,
            FOUPROBOT_SET_SPEED,
            GET_IO,
            SET_IO,
            CONTROL_MODE
        }
    }
}
