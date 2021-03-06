﻿using log4net;
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
    class Sanwa_EFEM : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(VerticalChamberOven_200));
        private const int DEF_OUT_DATA_ADDRESS = 0x500;
        private const int DEF_IN_DATA_ADDRESS = 0x500;
        private const int DEF_WOUT_DATA_ADDRESS = 0x6500;
        private const int DEF_WIN_DATA_ADDRESS = 0x6000;
        IUserInterfaceReport _TaskReport;
        public Sanwa_EFEM(IUserInterfaceReport TaskReport)
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

                                //if (NodeManagement.Get("ROBOT01").Enable && NodeManagement.Get("ROBOT01").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "ROBOT01" }, "S0300010");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT01").Enable && NodeManagement.Get("LOADPORT01").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT01" }, "S0300001");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT02").Enable && NodeManagement.Get("LOADPORT02").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT02" }, "S0300001");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT03").Enable && NodeManagement.Get("LOADPORT03").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT03" }, "S0300001");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT04").Enable && NodeManagement.Get("LOADPORT04").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT04" }, "S0300001");
                                //    return;
                                //}
                                //AckTask(TaskJob);
                                if (NodeManagement.Get("ROBOT01").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ROBOT01", "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));
                                }
                                if (NodeManagement.Get("LOADPORT01").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT01", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
                                }
                                if (NodeManagement.Get("LOADPORT02").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT02", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
                                }
                                if (NodeManagement.Get("LOADPORT03").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT03", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
                                }
                                if (NodeManagement.Get("LOADPORT04").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT04", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Reset }));
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

                                //if (NodeManagement.Get("ROBOT01").Enable && NodeManagement.Get("ROBOT01").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "ROBOT01" }, "S0300010");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT01").Enable && NodeManagement.Get("LOADPORT01").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT01" }, "S0300001");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT02").Enable && NodeManagement.Get("LOADPORT02").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT02" }, "S0300001");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT03").Enable && NodeManagement.Get("LOADPORT03").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT03" }, "S0300001");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT04").Enable && NodeManagement.Get("LOADPORT04").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT04" }, "S0300001");
                                //    return;
                                //}
                                AckTask(TaskJob);
                                if (NodeManagement.Get("ROBOT01").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_INIT, new Dictionary<string, string>() { { "@Target", "ROBOT01" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }
                                if (NodeManagement.Get("LOADPORT01").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_INIT, new Dictionary<string, string>() { { "@Target", "LOADPORT01" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }
                                if (NodeManagement.Get("LOADPORT02").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_INIT, new Dictionary<string, string>() { { "@Target", "LOADPORT02" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }
                                if (NodeManagement.Get("LOADPORT03").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_INIT, new Dictionary<string, string>() { { "@Target", "LOADPORT03" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }
                                if (NodeManagement.Get("LOADPORT04").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_INIT, new Dictionary<string, string>() { { "@Target", "LOADPORT04" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
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
                                if (NodeManagement.Get("ROBOT01").Enable && !NodeManagement.Get("ROBOT01").InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "ROBOT01" }, "S0300015");
                                    return;
                                }
                                if (NodeManagement.Get("LOADPORT01").Enable && !NodeManagement.Get("LOADPORT01").InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT01" }, "S0300019");
                                    return;
                                }
                                if (NodeManagement.Get("LOADPORT02").Enable && !NodeManagement.Get("LOADPORT02").InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT02" }, "S0300020");
                                    return;
                                }
                                if (NodeManagement.Get("LOADPORT03").Enable && !NodeManagement.Get("LOADPORT03").InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT03" }, "S0300021");
                                    return;
                                }
                                if (NodeManagement.Get("LOADPORT04").Enable && !NodeManagement.Get("LOADPORT04").InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT04" }, "S0300022");
                                    return;
                                }

                                //if (NodeManagement.Get("ROBOT01").Enable && NodeManagement.Get("ROBOT01").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "ROBOT01" }, "S0300010");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT01").Enable && NodeManagement.Get("LOADPORT01").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT01" }, "S0300001");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT02").Enable && NodeManagement.Get("LOADPORT02").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT02" }, "S0300001");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT03").Enable && NodeManagement.Get("LOADPORT03").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT03" }, "S0300001");
                                //    return;
                                //}
                                //if (NodeManagement.Get("LOADPORT04").Enable && NodeManagement.Get("LOADPORT04").IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = "LOADPORT04" }, "S0300001");
                                //    return;
                                //}
                                AckTask(TaskJob);
                                if (NodeManagement.Get("ROBOT01").Enable)
                                {
                                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_ORGSH, new Dictionary<string, string>() { { "@Target", "ROBOT01" } }, "", TaskJob.MainTaskId).Promise())
                                    {
                                        //中止Task
                                        AbortTask(TaskJob, null, "TASK_ABORT");

                                        break;
                                    }
                                }
                                if (NodeManagement.Get("LOADPORT01").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT01", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                }
                                if (NodeManagement.Get("LOADPORT02").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT02", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                }
                                if (NodeManagement.Get("LOADPORT03").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT03", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                }
                                if (NodeManagement.Get("LOADPORT04").Enable)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("LOADPORT04", "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));
                                }
                                break;

                            default:
                                if (NodeManagement.Get("ROBOT01").Enable)
                                {
                                    NodeManagement.Get("ROBOT01").OrgSearchComplete = true;
                                }
                                if (NodeManagement.Get("LOADPORT01").Enable)
                                {
                                    NodeManagement.Get("LOADPORT01").OrgSearchComplete = true;
                                }
                                if (NodeManagement.Get("LOADPORT02").Enable)
                                {
                                    NodeManagement.Get("LOADPORT02").OrgSearchComplete = true;
                                }
                                if (NodeManagement.Get("LOADPORT03").Enable)
                                {
                                    NodeManagement.Get("LOADPORT03").OrgSearchComplete = true;
                                }
                                if (NodeManagement.Get("LOADPORT04").Enable)
                                {
                                    NodeManagement.Get("LOADPORT04").OrgSearchComplete = true;
                                }
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
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
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "01" }));

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
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "01" }));

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
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "01" }));

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
                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM" }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);
                                if (!Target.IsPause)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSV, Value = "01" }));
                                }
                                break;
                            case 1:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "01" }));

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

                                //if (Position.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");
                                //    return;
                                //}
                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}

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
                                    if (MainControl.Instance.DIO.GetIO("DIN", Position.Name + "_ARM_EXTEND_ENABLE").ToUpper().Equals("FALSE") && !TaskJob.Params["@BYPASS_CHECK"].Equals("TRUE"))
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300164");
                                        return;
                                    }
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "false");


                                }
                                else
                                {
                                    Wafer = JobManagement.Get(Position.Name, Slot);


                                    if (Wafer == null)
                                    {

                                        AckTask(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.PutWait, Position = Position.Name, Arm = "1", Slot = "1" }));
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MoveToSlot, Value = Slot, Val2 = "0" }));
                                    }
                                    else
                                    {

                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300171");
                                        return;
                                    }
                                }
                                break;
                            case 2:

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Put, Position = Position.Name, Arm = "1", Slot = "1" }));

                                break;
                            case 3:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.TweekUp }));

                                Wafer = JobManagement.Get(Target.Name, "1");
                                if (Wafer == null)
                                {
                                    Wafer = JobManagement.Add();
                                    Wafer.MapFlag = true;
                                }
                                Wafer.LastNode = Wafer.Position;
                                Wafer.LastSlot = Convert.ToInt16(Wafer.Slot).ToString();
                                Wafer.Position = Position.Name;
                                Wafer.Slot = Convert.ToInt16(Slot).ToString();
                                if (Position.Type.Equals("LoadLock"))
                                {
                                    JobManagement.Remove(Wafer);
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "true");
                                }
                                _TaskReport.On_Job_Location_Changed(Wafer);
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
                                //if (Position.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");
                                //    return;
                                //}
                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}

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
                                    if (MainControl.Instance.DIO.GetIO("DIN", Position.Name + "_ARM_EXTEND_ENABLE").ToUpper().Equals("FALSE") && !TaskJob.Params["@BYPASS_CHECK"].Equals("TRUE"))
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300164");
                                        return;
                                    }
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "false");


                                }
                                else
                                {
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
                                    else
                                    {
                                        AckTask(TaskJob);
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.GetWait, Position = Position.Name, Arm = "1", Slot = "1" }));
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MoveToSlot, Value = Slot, Val2 = "0" }));
                                    }
                                }
                                break;
                            case 2:

                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.Get, Position = Position.Name, Arm = "1", Slot = "1" }));

                                break;
                            case 3:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Position.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.TweekDn }));
                                if (Position.Type.Equals("LoadLock"))
                                {
                                    MainControl.Instance.DIO.SetIO("ARM_NOT_EXTEND_" + Position.Name, "true");
                                }
                                Wafer = JobManagement.Get(Position.Name, Slot);
                                if (Wafer == null)
                                {
                                    Wafer = JobManagement.Add();
                                    Wafer.MapFlag = true;
                                }
                                Wafer.LastNode = Wafer.Position;
                                Wafer.LastSlot = Convert.ToInt16(Wafer.Slot).ToString();
                                Wafer.Position = Target.Name;
                                Wafer.Slot = "1";

                                _TaskReport.On_Job_Location_Changed(Wafer);
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
                                if (!Position.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300168");
                                    return;
                                }
                                if (!Target.InitialComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300015");
                                    return;
                                }
                                if (!Position.OrgSearchComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300169");
                                    return;
                                }
                                if (!Target.OrgSearchComplete)
                                {
                                    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300041");
                                    return;
                                }
                                //if (Position.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300001");
                                //    return;
                                //}
                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                //Get safety check
                                Wafer = JobManagement.Get(TaskJob.Params["@FromPosition"], TaskJob.Params["@FromSlot"]);

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
                                if (TaskJob.Params["@FromPosition"].Equals(TaskJob.Params["@ToPosition"]) && TaskJob.Params["@FromSlot"].Equals(TaskJob.Params["@ToSlot"]))
                                {

                                }
                                else
                                {
                                    Wafer = JobManagement.Get(TaskJob.Params["@ToPosition"], TaskJob.Params["@ToSlot"]);
                                    //Put safety check
                                    if (Wafer != null)
                                    {
                                        AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Position.Name }, "S0300171");
                                        return;
                                    }
                                }
                                AckTask(TaskJob);

                                break;
                            case 1:

                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_GET, new Dictionary<string, string>() { { "@Target", Target.Name }, { "@Position", TaskJob.Params["@FromPosition"] }, { "@Slot", TaskJob.Params["@FromSlot"] } }, "", TaskJob.MainTaskId).Promise())
                                {
                                    //中止Task
                                    AbortTask(TaskJob, null, "TASK_ABORT");

                                    break;
                                }
                                break;
                            case 2:
                                if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_PUT, new Dictionary<string, string>() { { "@Target", Target.Name }, { "@Position", TaskJob.Params["@ToPosition"] }, { "@Slot", TaskJob.Params["@ToSlot"] } }, "", TaskJob.MainTaskId).Promise())
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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.PutWait, Position = Position.Name, Arm = "1", Slot = "1" }));

                                break;
                            case 1:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "01" }));

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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.GetWait, Position = Position.Name, Arm = "1", Slot = "1" }));

                                break;
                            case 1:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "01" }));

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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.WaferRelease, Arm = Arm }));

                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "01" }));

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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.WaferHold, Arm = Arm }));

                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "01" }));

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
                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);
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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);
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


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);

                                //AckTask(TaskJob);
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


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);

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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
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



                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Reset }));

                                break;
                            case 1:
                                //if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_INIT, TaskJob.Params).Promise())
                                //{
                                //    //中止Task
                                //    AbortTask(TaskJob, null, "TASK_ABORT");

                                //    break;
                                //}
                                break;
                            case 2:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.RobotType.OrginSearch }));

                                break;


                            default:
                                Target.OrgSearchComplete = true;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_RESET:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                //AckTask(TaskJob);
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
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.ROBOT_INIT:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300010");
                                //    return;
                                //}
                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));

                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "01" }));

                                break;
                            case 2:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetRIO, Value = "32" }));

                                break;
                            case 3:
                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = "1" }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Mode, Value = "0" }));
                                }
                                break;
                            case 4:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetSpeed }));

                                break;
                            case 5:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetMode }));

                                break;
                            case 6:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetError, Value = "00" }));

                                break;
                            case 7:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.Servo, Value = "1" }));

                                break;
                            case 8:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.RobotType.GetStatus }));

                                break;
                            default:
                                Target.InitialComplete = true;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_INIT:
                        switch (TaskJob.CurrentIndex)
                        {

                            case 0:


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Mode, Value = "1" }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Mode, Value = "0" }));
                                }
                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetSpeed, Value = "0" }));
                                break;
                            case 2:
                                //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                break;
                            case 3:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.ReadStatus }));
                                break;
                            default:
                                Target.InitialComplete = true;
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.LOADPORT_GET_MAPDT:
                        switch (TaskJob.CurrentIndex)
                        {

                            case 0:


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                }
                                break;

                            default:

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



                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.InitialPos }));

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


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                //AckTask(TaskJob);
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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.UnClamp }));

                                break;
                            default:
                                Target.Foup_Lock = false;
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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Clamp }));

                                break;
                            default:
                                Target.Foup_Lock = true;
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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.RetryMapping }));

                                break;
                            case 1:
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));

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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.MoveToSlot, Value = Value, Val2 = "0" }));

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


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.SetSpeed, Value = Value }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;


                    case TaskFlowManagement.Command.LOADPORT_CLOSE:
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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Unload }));

                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;

                    case TaskFlowManagement.Command.LOADPORT_OPEN:
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

                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

                                AckTask(TaskJob);
                                TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.Load }));

                                break;
                            case 1:
                                if (SystemConfig.Get().DummyMappingData)
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "EXCUTED", new Transaction { Method = Transaction.Command.LoadPortType.GetMappingDummy }));
                                }
                                else
                                {
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction { Method = Transaction.Command.LoadPortType.GetMapping }));
                                }
                                break;
                            default:
                                FinishTask(TaskJob);
                                return;
                        }
                        break;
                    case TaskFlowManagement.Command.GET_CSTID:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

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
                        break;
                    case TaskFlowManagement.Command.SET_CSTID:
                        switch (TaskJob.CurrentIndex)
                        {
                            case 0:


                                //if (Target.IsExcuting)
                                //{
                                //    AbortTask(TaskJob, new Node() { Vendor = "SYSTEM", Name = Target.Name }, "S0300001");
                                //    return;
                                //}

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
                        break;
                    default:
                        throw new NotSupportedException();
                }
                if (TaskJob.CheckList.Count != 0)
                {
                    foreach (TaskFlowManagement.ExcutedCmd eachCmd in TaskJob.CheckList)
                    {
                        eachCmd.Txn.TaskObj = TaskJob;
                        if (NodeManagement.Get(eachCmd.NodeName).Connected)
                        {
                            NodeManagement.Get(eachCmd.NodeName).SendCommand(eachCmd.Txn);
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
    }
}
