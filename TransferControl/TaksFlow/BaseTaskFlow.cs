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
    class BaseTaskFlow : ITaskFlow
    {
        public IUserInterfaceReport _TaskReport;
        protected int LoadportCount = 4;
        public BaseTaskFlow(IUserInterfaceReport TaskReport)
        {
            _TaskReport = TaskReport;
        }
        /// <summary>
        /// If node is disable or Null, OrgSearchComplete is false
        /// </summary>
        /// <param name="Name">Node Name</param>
        protected void OrgSearchCompleted(string Name)
        {
            if (NodeManagement.Get(Name) != null)
                NodeManagement.Get(Name).OrgSearchComplete = IsNodeEnabledOrNull(Name);
        }
        /// <summary>
        /// If node is disable or Null, return false
        /// </summary>
        /// <param name="Name">Node Name</param>
        /// <returns></returns>
        protected bool IsNodeEnabledOrNull(string Name)
        {
            return NodeManagement.Get(Name) != null ? NodeManagement.Get(Name).Enable : false;
        }
        /// <summary>
        /// If node unitialCompleted, abort current task
        /// </summary>
        /// <param name="node">Currnet Node</param>
        /// <param name="TaskJob">Currnet Task</param>
        /// <returns></returns>
        protected bool IsNodeInitialComplete(Node node, TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            if (!node.InitialComplete)
            {
                switch (node.Type.ToUpper())
                {
                    case "LOADPORT":
                        switch (node.Name.ToUpper())
                        {
                            case "LOADPORT01":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300019");
                                break;
                            case "LOADPORT02":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300020");
                                break;
                            case "LOADPORT03":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300021");
                                break;
                            case "LOADPORT04":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300022");
                                break;
                            case "LOADPORT05":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300023");
                                break;
                            case "LOADPORT06":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300024");
                                break;
                            case "LOADPORT07":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300025");
                                break;
                            case "LOADPORT08":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300026");
                                break;
                            default:
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300168");
                                break;
                        }
                        return false;
                    case "ALIGNER":
                        switch (node.Name.ToUpper())
                        {
                            case "ALIGNER01": 
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300017");
                                break;
                            case "ALIGNER02":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300018");
                                break;
                            default:
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300017");
                                break;
                        }
                        
                        return false;
                    case "ROBOT":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300015");
                        return false;
                    case "LOADLOCK":
                    case "E84":
                    case "RFID":
                    case "SMARTTAG":
                        break;
                }
            }
            return true;
        }
        /// <summary>
        /// If node unOrgSearchCompleted, abort current task
        /// </summary>
        /// <param name="node">Currnet Node</param>
        /// <param name="TaskJob">Currnet Task</param>
        /// <returns></returns>
        protected bool IsNodeOrgSearchComplete(Node node, TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            if (!node.OrgSearchComplete)
            {
                switch (node.Type.ToUpper())
                {
                    case "LOADPORT":
                        switch (node.Name.ToUpper())
                        {
                            case "LOADPORT01":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300045");
                                break;
                            case "LOADPORT02":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300046");
                                break;
                            case "LOADPORT03":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300047");
                                break;
                            case "LOADPORT04":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300048");
                                break;
                            case "LOADPORT05":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300049");
                                break;
                            case "LOADPORT06":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300050");
                                break;
                            case "LOADPORT07":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300051");
                                break;
                            case "LOADPORT08":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300052");
                                break;
                            default:
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300169");
                                break;
                        }
                        return false;
                    case "ALIGNER":
                        switch (node.Name.ToUpper())
                        {
                            case "ALIGNER01":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300043");
                                break;
                            case "ALIGNER02":
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300044");
                                break;
                            default:
                                AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300043");
                                break;
                        }
                        return false;
                    case "ROBOT":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = node.Name }, "S0300041");
                        return false;
                    case "LOADLOCK":
                        break;
                }
            }

            return true;
        }
        /// <summary>
        /// Check node InitialCompleted and OrgSearchCompleted ?
        /// </summary>
        /// <param name="node">Currnet Node</param>
        /// <param name="TaskJob">Currnet Task</param>
        /// <returns></returns>
        protected bool CheckNodeStatusOnTaskJob(Node node, TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            if (!IsNodeInitialComplete(node, TaskJob)) return false;

            if (!IsNodeOrgSearchComplete(node, TaskJob)) return false;

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TaskJob"></param>
        public virtual void Excute(object TaskJob) { return; }
        /// <summary>
        /// Task Ready, Reply to Client
        /// </summary>
        /// <param name="TaskJob"></param>
        public virtual void AckTask(TaskFlowManagement.CurrentProcessTask TaskJob)
        {

            if (TaskJob.State == TaskFlowManagement.CurrentProcessTask.TaskState.None)
            {
                TaskJob.State = TaskFlowManagement.CurrentProcessTask.TaskState.ACK;
                _TaskReport.On_TaskJob_Ack(TaskJob);
            }
        }
        /// <summary>
        /// Abort Task, Reply to Client
        /// </summary>
        /// <param name="TaskJob"></param>
        /// <param name="Node"></param>
        /// <param name="Message"></param>
        public virtual void AbortTask(TaskFlowManagement.CurrentProcessTask TaskJob, Node Node, string Message)
        {
            if (Node != null)
            {
                _TaskReport.On_Alarm_Happen(AlarmManagement.NewAlarm(Node, Message, TaskJob.MainTaskId));
            }
            _TaskReport.On_TaskJob_Aborted(TaskJob);
        }
        /// <summary>
        /// Task Finished, Reply to Client
        /// </summary>
        /// <param name="TaskJob"></param>
        public virtual void FinishTask(TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            _TaskReport.On_TaskJob_Finished(TaskJob);
        }

        public virtual bool GetCSTID(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target , string Value = "")
        {
            if (Target.Type.ToUpper().Equals("SMARTTAG"))
            {
                switch (TaskJob.CurrentIndex)
                {
                    case 0:

                        AckTask(TaskJob);

                        if(IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.SmartTagType.Hello }));

                        break;
                    case 1:
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.SmartTagType.GetLCDData }));

                        break;
                    default:
                        FinishTask(TaskJob);
                        return false;
                }
            }
            else if (Target.Type.ToUpper().Equals("RFID"))
            {
                switch (TaskJob.CurrentIndex)
                {
                    case 0:
                        AckTask(TaskJob);

                        if (IsNodeEnabledOrNull(Target.Name))
                            if (Target.Vendor.Equals("RFID_HR4136"))
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RFIDType.Hello }));

                        break;

                    case 1:
                        if(Value == "")
                        {
                            if (IsNodeEnabledOrNull(Target.Name))
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RFIDType.GetCarrierID }));
                        }
                        else
                        {
                            if (IsNodeEnabledOrNull(Target.Name))
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RFIDType.GetCarrierID , Value = Value }));
                        }

                        break;

                    default:
                        FinishTask(TaskJob);
                        return false;
                }
            }

            return true;
        }
        public virtual bool SetCSTID(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            if (Target.Type.ToUpper().Equals("SMARTTAG"))
            {
                switch (TaskJob.CurrentIndex)
                {
                    case 0:
                        AckTask(TaskJob);
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.SmartTagType.Hello }));
                        break;
                    case 1:
                        if (IsNodeEnabledOrNull(Target.Name))
                            if (Target.Vendor.ToUpper().Equals("SMARTTAG8200"))
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.SmartTagType.SelectLCDData }));
                        
                        break;
                    case 2:
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.SmartTagType.SetLCDData, Value = Value }));

                        break;
                    default:
                        FinishTask(TaskJob);
                        return false;
                }
            }
            else if (Target.Type.ToUpper().Equals("RFID") && Target.Vendor.Equals("RFID_HR4136"))
            {
                switch (TaskJob.CurrentIndex)
                {
                    case 0:
                        AckTask(TaskJob);
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RFIDType.Hello }));
                        break;

                    case 1:
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RFIDType.Mode, Value = "MT" }));
                        break;

                    case 2:
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RFIDType.Hello }));
                        break;

                    case 3:
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RFIDType.SetCarrierID, Value = Value }));
                        break;

                    case 4:
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RFIDType.Hello }));

                        break;

                    case 5:
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RFIDType.Mode, Value = "OP" }));
                        break;
                    default:
                        FinishTask(TaskJob);
                        return false;
                }
            }
            else if (Target.Type.ToUpper().Equals("RFID"))
            {
                switch (TaskJob.CurrentIndex)
                {
                    case 0:
                        AckTask(TaskJob);

                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RFIDType.SetCarrierID, Value = Value }));

                        break;
                    default:
                        FinishTask(TaskJob);
                        return false;
                }
            }

            return true;
        }
        #region SANWA_ROBOT
                public virtual bool Sanwa_RobotAbort(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:
                            AckTask(TaskJob);
                            if (Target.IsPause)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Stop, Value = "1" }));


                            Target.IsPause = false;
                            break;

                        default:
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotReStart(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:
                            AckTask(TaskJob);
                            Target.IsPause = false; 
                            //if (Target.IsPause)
                            //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Continue }));
                            break;

                        default:
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotHold(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:
                            AckTask(TaskJob);
                            Target.IsPause = true;
                            //if (!Target.IsPause)
                            //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Pause }));
                            break;

                        default:
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                /// <summary>
                /// 不直接讀取Robot狀態，而是動作結束(PUT\GET)後，更新Robot狀態
                /// </summary>
                /// <param name="TaskJob"></param>
                /// <param name="Target"></param>
                /// <param name="Value"></param>
                /// <returns></returns>
                public virtual bool Sanwa_RobotGetClamp(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
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
                public virtual bool Sanwa_RobotWait(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, Node Position, string Arm, string Slot)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:
                            AckTask(TaskJob);

                            if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                            break;
                        case 1:
                            GetRobotPosition(TaskJob, Target);
                            break;
                        case 2:

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

                        case 3:
                            GetRobotPosition(TaskJob, Target);
                            break;

                        default:
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotWaferVAC(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target,  string Arm)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:
                            AckTask(TaskJob);

                            if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;


                            //if (TaskJob.TaskName == TaskFlowManagement.Command.ROBOT_WAFER_HOLD)
                            //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.WaferHold, Arm = Arm }));
                            //else
                            //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.WaferRelease, Arm = Arm }));
                            //break;

                            if (TaskJob.TaskName == TaskFlowManagement.Command.ROBOT_WAFER_HOLD)
                            {
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = Arm.Equals("1") ? "01" : "02", Value = "1" }));
                            }
                            else
                            {
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = Arm.Equals("1") ? "01" : "02", Value = "0" }));
                            }
                            break;
                    
                        case 1:
                            SpinWait.SpinUntil(() => false, 500);
                            break;
                        case 2:
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = Arm.Equals("1") ? "008" : "009" }));
                            break;

                        case 3:
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = Arm.Equals("1") ? "01" : "02" }));
                            break;

                        default:
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotMode(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
                {
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
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotRetract(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:
                            AckTask(TaskJob);

                            if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;


                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.ArmReturn }));

                            break;


                        default:
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotSpeed(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
                {
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
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotGetSpeed(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:

                            AckTask(TaskJob);
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));

                            break;

                        default:
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotServo(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
                {
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
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotHome(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:
                            AckTask(TaskJob);

                            if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Home }));

                            break;


                        default:
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotORG(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:
                            AckTask(TaskJob);

                            if (!IsNodeInitialComplete(Target, TaskJob)) return false;

                            break;
                        case 1:
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.OrginSearch }));
                            break;

                        default:
                            OrgSearchCompleted(Target.Name);
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotReset(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
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
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotINIT(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:
                            AckTask(TaskJob);
                            //Servo on
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Servo, Value = "1" }));

                            break;

                        case 1:
                            //開啟R軸電磁閥
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "1" }));

                            break;

                        case 2:
                            //開啟L軸電磁閥
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "1" }));

                            break;

                        case 3:
                            SpinWait.SpinUntil(() => false, 500);
                            break;

                        case 4:
                            //確認R軸 Presence                   
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "008" }));
                            break;

                        case 5:
                            //確認L軸 Presence
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "009" }));
                            break;

                        case 6:
                            //R軸 Presence 不存在,則關閉R軸電磁閥
                            if (!SystemConfig.Get().OfflineMode && !Target.R_Presence)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "01", Value = "0" }));

                            break;

                        case 7:
                            //L軸 Presence 不存在,則關閉L軸電磁閥
                            if (!SystemConfig.Get().OfflineMode && !Target.L_Presence)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SetSV, Arm = "02", Value = "0" }));

                            break;

                        case 8:
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

                        case 9:
                            //取得R軸電磁閥最後狀態
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "01" }));
                            break;

                        case 10:
                            //取得L軸電磁閥最後狀態
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "02" }));
                            break;

                        case 11:
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Speed, Value = "100" }));
                            break;

                        case 12:
                            //取得速度
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));

                            break;

                        case 13:
                            //取得模式
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetMode }));

                            break;

                        case 14:
                            //取得異常
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetError, Value = "00" }));

                            break;

                        case 15:
                            //更新Bobot狀態
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));

                            break;
                        case 16:
                            //確認R軸 Presence                   
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "008" }));
                            break;

                        case 17:
                            //確認L軸 Presence
                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "009" }));
                            break;

                        default:
                            Target.InitialComplete = true;
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }
                public virtual bool Sanwa_RobotSaveLog(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
                {
                    switch (TaskJob.CurrentIndex)
                    {
                        case 0:

                            if (!SystemConfig.Get().OfflineMode)
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.SaveLog }));

                            break;

                        default:
                            FinishTask(TaskJob);
                            return false;
                    }

                    return true;
                }

                #endregion
        #region TDK_LOADPORT
        public virtual bool TDK_LoadportINIT(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportORG(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!IsNodeInitialComplete(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));

                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                    break;

                default:
                    OrgSearchCompleted(Target.Name);
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportReset(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportForceORG(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!IsNodeInitialComplete(Target, TaskJob)) return false;

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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportReadStatus(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportClamp(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportUnclamp(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Unload }));
                    //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UnClamp }));

                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportDock(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportUndock(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UntilUnDock }));

                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportVACOn(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportVACOff(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }
            return true;
        }
        public virtual bool TDK_LoadportUnlatch(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportLatch(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }
            return true;
        }
        public virtual bool TDK_LoadportDoorOpen(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportDoorClose(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }
            return true;
        }
        public virtual bool TDK_LoadportDoorDown(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }
            return true;
        }
        public virtual bool TDK_LoadportDoorUp(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportAllOpen(TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    Node lpNode = null;
                    for (int i = 0; i < LoadportCount; i++)
                    {
                        lpNode = NodeManagement.Get(string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0')));
                        if (!CheckNodeStatusOnTaskJob(lpNode, TaskJob)) return false;
                    }

                    AckTask(TaskJob);

                    for (int i = 0; i < LoadportCount; i++)
                    {
                        lpNode = NodeManagement.Get(string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0')));

                        //移除所有Wafer
                        foreach (Job eachJob in JobManagement.GetByNode(lpNode.Name))
                            JobManagement.Remove(eachJob);

                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(lpNode.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MappingLoad }));
                    }
                    break;
                case 1:
                    for (int i = 0; i < LoadportCount; i++)
                    {
                        lpNode = NodeManagement.Get(string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0')));
                        if (SystemConfig.Get().DummyMappingData)
                        {
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(lpNode.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                        }
                        else
                        {
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(lpNode.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                        }
                    }
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        for (int i = 0; i < LoadportCount; i++)
                        {
                            lpNode = NodeManagement.Get(string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0')));
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(lpNode.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                        }
                    }
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportOpen(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    //移除所有Wafer
                    foreach (Job eachJob in JobManagement.GetByNode(Target.Name))
                        JobManagement.Remove(eachJob);

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MappingLoad }));

                    break;

                case 1:
                    if (TaskJob.TaskName.Equals(TaskFlowManagement.Command.LOADPORT_OPEN))
                    {
                        //if (SystemConfig.Get().DummyMappingData)
                        //{
                        //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                        //}
                        //else
                        //{
                        //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                        //}
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                    }
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportAllClose(TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    Node lpNode = null;
                    for (int i = 0; i < LoadportCount; i++)
                    {
                        lpNode = NodeManagement.Get(string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0')));
                        if (!CheckNodeStatusOnTaskJob(lpNode, TaskJob)) return false;
                    }

                    AckTask(TaskJob);

                    for (int i = 0; i < LoadportCount; i++)
                    {
                        lpNode = NodeManagement.Get(string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0')));
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(lpNode.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Unload }));
                    }
                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        for (int i = 0; i < LoadportCount; i++)
                        {
                            lpNode = NodeManagement.Get(string.Format("LOADPORT{0}", (i + 1).ToString().PadLeft(2, '0')));
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(lpNode.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                        }
                    }
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportClose(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    Target.IsMapping = false;

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UntilDoorCloseVacOFF }));

                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportGetMapData(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);

                    //if (SystemConfig.Get().DummyMappingData)
                    //{
                    //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                    //}
                    //else
                    //{
                    //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                    //}
                    break;

                default:

                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportReMapping(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!CheckLoadportAndRobotPos(TaskJob, Target)) return false;

            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    //移除所有Wafer
                    foreach (Job eachJob in JobManagement.GetByNode(Target.Name))
                        JobManagement.Remove(eachJob);

                    if (!SystemConfig.Get().OfflineMode)
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
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportSetOPAccessIndicator(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetOpAccess, Value = Value }));


                    break;

                case 1:
                    //if (!SystemConfig.Get().OfflineMode)
                    //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportSetLoadIndicator(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetLoad, Value = Value }));
                    break;

                case 1:
                    //if (!SystemConfig.Get().OfflineMode)
                    //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportSetUnloadIndicator(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetUnLoad, Value = Value }));
                    break;

                case 1:
                    //if (!SystemConfig.Get().OfflineMode)
                    //    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));

                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool TDK_LoadportReadLed(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            Node E84Node = NodeManagement.Get(Target.Name.Replace("LOADPORT", "E84"));
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetLED }));

                    //同步化 E84 Status
                    if (!SystemConfig.Get().OfflineMode)
                        if (E84Node != null)
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Node.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));
                    break;

                case 1:
                    //同步化 E84 Status
                    if (!SystemConfig.Get().OfflineMode)
                        if (E84Node != null)
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(E84Node.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        #endregion
        #region SANWA_LOADPORT
        #endregion
        #region SANWA_E84
        public virtual bool Sanwa_E84INIT(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));

                    break;
                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_E84Reset(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));
                    }
                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                    }
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (IsNodeEnabledOrNull(Target.Name))
                        {
                            if (NodeManagement.Get(Target.Name).E84Mode == E84_Mode.AUTO)
                            {
                                //確認天車完全移走
                                if (!NodeManagement.Get(Target.Name).E84IOStatus["VALID"] &&
                                    !NodeManagement.Get(Target.Name).E84IOStatus["CS_0"] &&
                                    !NodeManagement.Get(Target.Name).E84IOStatus["CS_1"] &&
                                    !NodeManagement.Get(Target.Name).E84IOStatus["AM_AVBL"] &&
                                    !NodeManagement.Get(Target.Name).E84IOStatus["TR_REQ"] &&
                                    !NodeManagement.Get(Target.Name).E84IOStatus["BUSY"] &&
                                    !NodeManagement.Get(Target.Name).E84IOStatus["COMPT"] &&
                                    !NodeManagement.Get(Target.Name).E84IOStatus["CONT"] &&
                                    !NodeManagement.Get(Target.Name).E84IOStatus["HO_AVBL"])
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.Reset }));
                                }
                            }
                        }
                    }
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        SpinWait.SpinUntil(() => false, 600);

                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));
                    }
                    break;

                case 4:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                    }
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_E84Mode(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (Target.E84IOStatus["VALID"])
                        {
                            AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, string.Format("S04000{0}", Target.Name.Substring(3, 2)));
                            return false;
                        }
                    }
                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (Value.Equals("1")) //set auto
                        {
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetAutoMode }));
                        }
                        else //set manual
                        {
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetManualMode }));
                        }
                    }
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetE84IOStatus }));
                    }
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                    {
                        if (IsNodeEnabledOrNull(Target.Name))
                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.GetDIOStatus }));
                    }
                    break;

                default:

                    if (TaskJob.TaskName == TaskFlowManagement.Command.E84_MODE)
                        NodeManagement.Get(Target.Associated_Node).E84Mode = Value.Equals("1") ? E84_Mode.AUTO : E84_Mode.MANUAL;

                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_E84SetTP1(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetTP1, Value = Value }));
                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }
            return true;
        }
        public virtual bool Sanwa_E84SetTP2(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetTP2, Value = Value }));
                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }
            return true;
        }
        public virtual bool Sanwa_E84SetTP3(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetTP3, Value = Value }));
                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }
            return true;
        }
        public virtual bool Sanwa_E84SetTP4(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetTP4, Value = Value }));
                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }
            return true;
        }
        public virtual bool Sanwa_E84SetTP5(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetTP5, Value = Value }));
                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }
            return true;
        }
        public virtual bool Sanwa_E84SetTP6(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.E84.SetTP6, Value = Value }));
                    break;
                default:
                    FinishTask(TaskJob);
                    return false;
            }
            return true;
        }
        #endregion
        #region SANWA_ALIGNER
        public virtual bool Sanwa_AlignerReset(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);
                    break;

                case 1:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Reset }));
                    break;

                case 2:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetError, Value = "00" }));
                    break;

                case 3:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerINIT(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);
                    break;

                case 1:
                    //開啟電磁閥
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SetSV, Value = "01", Val2 = "1" }));
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        SpinWait.SpinUntil(() => false, 500);
                    break;

                case 3:
                    //確認 Presence
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                    break;

                case 4:
                    //Presence 不存在,則關閉R軸電磁閥
                    if (!SystemConfig.Get().OfflineMode && !Target.R_Presence)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SetSV, Value = "01", Val2 = "0" }));
                    break;

                case 5:
                    //確認 Presence
                    if (!SystemConfig.Get().OfflineMode)
                        SpinWait.SpinUntil(() => false, 500);
                    break;

                case 6:
                    //取得電磁閥最後狀態
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                    break;

                case 7:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Speed, Value = "100" }));

                    break;

                case 8:
                    //取得速度
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSpeed }));
                    break;
                case 9:
                    //取得模式
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetMode }));
                    break;

                case 10:
                    //取得異常
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetError, Value = "00" }));
                    break;

                case 11:
                    //Servo on
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Servo, Value = "1" }));
                    break;

                case 12:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SetAlign, Value = "150000" }));
                    break;

                case 13:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));
                    break;

                default:
                    Target.InitialComplete = true;
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerORG(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);

                    if (!IsNodeInitialComplete(Target, TaskJob)) return false;

                    break;
                case 1:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.OrginSearch }));
                    break;

                case 2:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.Home }));
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
                    //取得速度
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSpeed }));
                    break;

                case 6:
                    //取得模式
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetMode }));
                    break;

                default:
                    OrgSearchCompleted(Target.Name);
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerHome(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);

                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;
                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.Home }));
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerServo(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Servo, Value = Value }));

                    break;
                case 1:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));

                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerSpeed(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);

                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.Speed, Value = Value }));

                    break;
                case 1:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSpeed }));

                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerGetSpeed(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSpeed }));

                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerMode(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
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
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerWaferHold(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    AckTask(TaskJob);

                    break;
                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferHold }));
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

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerWaferRelease(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);
                    break;
                case 1:
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferRelease }));
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

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerSaveLog(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:

                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.SaveLog }));

                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }

            return true;
        }
        public virtual bool Sanwa_AlignerAlign(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target, string Value)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    if (!CheckNodeStatusOnTaskJob(Target, TaskJob)) return false;

                    AckTask(TaskJob);

                    break;

                case 1:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferHold }));
                    break;

                case 2:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.Align, Value = Value }));
                    break;

                case 3:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.AlignerType.WaferRelease }));
                    break;

                case 4:
                    //確認 Presence
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetRIO, Value = "008" }));
                    break;

                case 5:
                    //取得電磁閥最後狀態
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetSV, Value = "01" }));
                    break;

                case 6:
                    if (!SystemConfig.Get().OfflineMode)
                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.AlignerType.GetStatus }));
                    break;
                case 7:
                    Job Wafer = JobManagement.Get(Target.Name, "1");
                    if(Wafer != null)
                        Wafer.AlignerFlag = true;
                    break;

                default:
                    FinishTask(TaskJob);
                    return false;
            }
            return true;
        }
        public virtual bool Sanwa_AlignerGetClamp(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            switch (TaskJob.CurrentIndex)
            {
                case 0:
                    AckTask(TaskJob);
                    break;

                //case 1:
                //    break;


                default:
                    FinishTask(TaskJob);
                    return false;
            }
            return true;
        }
        #endregion

        public virtual void GetRobotPresence(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {

        }
        public virtual void GetRobotPosition(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!SystemConfig.Get().OfflineMode)
            {
                if (Target.Enable)
                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.GetPosition, Value = "2" }));
            }
        }
        /// <summary>
        /// Sanwa 300mm Loadport use
        /// </summary>
        /// <param name="TaskJob"></param>
        /// <param name="Target"></param>
        public virtual void GetLoadportStatus(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (!SystemConfig.Get().OfflineMode)
                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetStatus }));
        }
        /// <summary>
        /// 避免 Roboot 位置異常時，Loadport 執行關門的動作
        /// </summary>
        /// <param name="TaskJob"></param>
        /// <param name="Target"></param>
        /// <returns></returns>
        public virtual bool CheckLoadportAndRobotPos(TaskFlowManagement.CurrentProcessTask TaskJob, Node Target)
        {
            if (Target.RobotError)
            {
                switch(Target.Name)
                {
                    case "LOADPORT01":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300180");
                        return false;
                    case "LOADPORT02":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300181");
                        return false;
                    case "LOADPORT03":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300182");
                        return false;
                    case "LOADPORT04":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300183");
                        return false;
                    case "LOADPORT05":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300184");
                        return false;
                    case "LOADPORT06":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300185");
                        return false;
                    case "LOADPORT07":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300186");
                        return false;
                    case "LOADPORT08":
                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300187");
                        return false;
                }

            }

            return true;
        }
    }
}
