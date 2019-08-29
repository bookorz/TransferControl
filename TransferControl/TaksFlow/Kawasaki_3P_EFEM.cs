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
    class Kawasaki_3P_EFEM : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(Kawasaki_3P_EFEM));
        public bool Excute(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport)
        {
            string Message = "";
            Node Target = null;
            Node Position = null;
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
                if (CheckEMO(TaskJob, TaskReport))
                {
                    switch (TaskJob.TaskName)
                    {
                        case TaskFlowManagement.Command.ROBOT_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:  
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetStatus, "1")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSpeed, "")));
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetMode, "")));
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetError, "00")));
                                    break;
                                default:
                                    Target.Busy = false;
                                    Target.InitialComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_ORGSH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300015");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300010");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Home, "")));
                                    }
                                    break;
                                default:
                                    Target.OrgSearchComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ROBOT_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (Target.Busy)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300010");
                                        return false;
                                    }
                                    else
                                    {
                                        Target.Busy = true;
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Reset, "")));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetError, "00")));
                                    break;
                                default:
                                    Target.Busy = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (Target.Busy)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        Target.Busy = true;
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Reset, "")));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetError, "00")));
                                    break;
                                default:
                                    Target.Busy = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_SERVO:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (Target.Busy)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Servo, TaskJob.Params["@Value"])));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_SPEED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Speed, TaskJob.Params["@Value"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSpeed, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_WAFER_HOLD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.WaferHold, "","", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSV, "1")));
                                    break;
                                case 2:
                                    if (!Target.R_Vacuum_Solenoid.Equals("1"))
                                    {
                                        if (TaskJob.RepeatCount < 10)
                                        {
                                            TaskJob.RepeatCount++;
                                            TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                            SpinWait.SpinUntil(() => false, 200);
                                        }
                                    }
                                    else
                                    {
                                        TaskJob.RepeatCount = 0;
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_WAFER_RELEASE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.WaferRelease, "", "", TaskJob.Params["@Arm"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSV, "1")));
                                    break;
                                case 2:
                                    if (!Target.R_Vacuum_Solenoid.Equals("1"))
                                    {
                                        if (TaskJob.RepeatCount < 10)
                                        {
                                            TaskJob.RepeatCount++;
                                            TaskJob.CurrentIndex = TaskJob.CurrentIndex - 2;//repeat
                                            SpinWait.SpinUntil(() => false, 200);
                                        }
                                    }
                                    else
                                    {
                                        TaskJob.RepeatCount = 0;
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_MODE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Mode, TaskJob.Params["@Value"])));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetMode, "")));
                                    break;                               
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_ALIGN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300017");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300043");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Align, TaskJob.Params["@Value"])));
                                    }
                                    break;
                                default:
                                    Target.Home_Position = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_HOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300017");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300043");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Home, "")));
                                    }
                                    break;
                                default:
                                    Target.Home_Position = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (Target.Busy)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetRIO, "8")));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSpeed, "")));
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetMode, "")));
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetError, "00")));
                                    break;
                                case 4:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetSV, "1")));
                                    break;
                                case 5:
                                    if (Target.WaferSize.Equals("200"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.SetAlign, "100000")));
                                    }
                                    else if (Target.WaferSize.Equals("300"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.SetAlign, "150000")));
                                    }
                                    break;
                                case 6:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.GetStatus, "1")));
                                    break;
                                default:
                                    Target.Busy = false;
                                    Target.InitialComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.ALIGNER_ORGSH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300017");
                                        return false;
                                    }
                                    else if (Target.Busy)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300002");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.OrginSearch, "")));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.AlignerType.Home, "")));
                                    break;
                                default:
                                    Target.Home_Position = true;
                                    Target.OrgSearchComplete = true;
                                    Target.Busy = false;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mode, "1")));
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("FOUP"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.EQASP, "0")));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.EQASP, "1")));
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mode, "0")));
                                    break;
                                case 4:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetLED, "0")));
                                    break;
                                default:
                                    Target.InitialComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Reset, "")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_ORGSH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else
                                    {
                                        if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                        {
                                            TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                            return false;
                                        }
                                        else
                                        {
                                            TaskReport.On_Task_Ack(TaskJob);
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ForceInitialPos, "")));
                                        }
                                    }
                                    break;
                                case 1:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Clamp, "")));
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Dock, "")));
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    Target.OrgSearchComplete = true;
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_CLAMP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (Target.Foup_Lock && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300142");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Clamp, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_UNCLAMP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Lock && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300167");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.UnClamp, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_OPEN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            if (NodeManagement.Get(Target.Associated_Node).Busy && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300010");
                                                return false;
                                            }
                                            else
                                            {//Robot mapping
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorOpen, "")));
                                            }
                                        }
                                        else
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MappingLoad, "")));
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorDown, "")));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                case 3:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mapping, "", Target.Name)));
                                    }
                                    break;
                                case 4:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                case 5:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_OPEN_NOMAP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorOpen, "")));
                                        }
                                        else
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Load, "")));
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorDown, "")));
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_CLOSE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            if (!Target.Door_Position.Equals("Open position") && !Target.Z_Axis_Position.Equals("Down position") && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300174");
                                                return false;
                                            }
                                            else
                                            {//Robot mapping
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Mapping, "", Target.Name)));
                                            }
                                        }
                                        else
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MappingUnload, "")));
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                case 3:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorClose, "")));
                                    }
                                    break;
                                case 4:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                case 5:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_CLOSE_NOMAP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            if (!Target.Door_Position.Equals("Open position") && !Target.Z_Axis_Position.Equals("Down position") && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300174");
                                                return false;
                                            }
                                            else
                                            {
                                                TaskReport.On_Task_Ack(TaskJob);
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                            }
                                        }
                                        else
                                        {
                                            TaskReport.On_Task_Ack(TaskJob);
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Unload, "")));
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorClose, "")));
                                    }
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOCK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Y_Axis_Position.Equals("Undock position") && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300173");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.Dock, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_UNDOCK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Y_Axis_Position.Equals("Dock position") && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300145");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.UnDock, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOOR_CLOSE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorClose, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOOR_DOWN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorDown, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOOR_OPEN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorOpen, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_DOOR_UP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_FORCE_ORGSH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ForceInitialPos, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_GET_MAPDT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (!Target.IsMapping && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300156");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                        }
                                        else
                                        {
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                        }
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_LATCH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (!Target.Latch_Open && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300144");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.LatchDoor, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_UNLATCH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (Target.Latch_Open && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300143");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskReport.On_Task_Ack(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.UnLatchDoor, "")));
                                    }
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_RE_MAPPING:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300168");
                                        return false;
                                    }
                                    else if (!Target.OrgSearchComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300169");
                                        return false;
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    }
                                    break;
                                case 1:
                                    if (!Target.Foup_Presence && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300166");
                                        return false;
                                    }
                                    else if (!Target.Door_Position.Equals("Open position") && !Target.Z_Axis_Position.Equals("Down position") && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300174");
                                        return false;
                                    }
                                    else
                                    {
                                        if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                        {
                                            if (NodeManagement.Get(Target.Associated_Node).Busy && !SystemConfig.Get().SaftyCheckByPass)
                                            {
                                                TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300010");
                                                return false;
                                            }
                                            else
                                            {//Robot mapping
                                                TaskReport.On_Task_Ack(TaskJob);
                                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.RobotType.Mapping, "", Target.Name)));
                                            }
                                        }
                                        else
                                        {
                                            TaskReport.On_Task_Ack(TaskJob);
                                            TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                        }
                                    }
                                    break;
                                case 2:
                                    if (Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Associated_Node, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorUp, "")));
                                    }
                                    break;
                                case 3:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorOpen, "")));
                                    }
                                    break;
                                case 4:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MapperStartPosition, "")));
                                    }
                                    break;
                                case 5:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MapperArmStretch, "")));
                                    }
                                    break;
                                case 6:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MappingDown, "")));
                                    }
                                    break;
                                case 7:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.MapperArmRetracted, "")));
                                    }
                                    break;
                                case 8:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.DoorDown, "")));
                                    }
                                    break;
                                case 9:
                                    if (!Target.CarrierType.ToUpper().Equals("ADAPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetMapping, "")));
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_READ_LED:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetLED, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_READ_STATUS:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadStatus, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_READ_VERSION:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.ReadVersion, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_READYTOLOAD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetUnLoad, "0")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetLoad, "1")));
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetOpAccess, "2")));
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetLED, "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_UNLOADCOMPLETE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetOpAccess, "0")));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetLoad, "0")));
                                    break;
                                case 2:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.SetUnLoad, "0")));
                                    break;
                                case 3:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.GetLED, "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_VAC_ON:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.VacuumON, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_VAC_OFF:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.LoadPortType.VacuumOFF, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
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
                            Target.SendCommand(eachCmd.Txn, out Message);
                        }
                    }
                    else
                    {//recursive
                        TaskJob.CurrentIndex++;
                        this.Excute(TaskJob, TaskReport);
                    }

                }
            }
            catch (Exception e)
            {
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " exception: " + e.StackTrace);
                TaskFlowManagement.Remove(TaskJob.Id);
                TaskReport.On_Task_Abort(TaskJob, "SYSTEM", "ABS", e.StackTrace);

                return false;
            }
            return true;
        }
        private bool CheckEMO(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport)
        {
            bool result = true;
            if (!SystemConfig.Get().SaftyCheckByPass)
            {
                if (RouteControl.Instance.DIO.GetIO("DIN", "SAFETYRELAY").ToUpper().Equals("TRUE"))
                {
                    TaskFlowManagement.Remove(TaskJob.Id);
                    TaskReport.On_Task_Abort(TaskJob, "SYSTEM", "CAN", "S0300170");
                    result = false;
                }
            }
            return result;
        }

    }
}
