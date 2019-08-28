using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                        case TaskFlowManagement.Command.LOADPORT_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@Target"], Transaction.Command.LoadPortType.ReadStatus, "EXCUTED"));
                                    break;
                                case 1:
                                    Target.InitialComplete = true;
                                    break;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@Target"], Transaction.Command.LoadPortType.Reset, "EXCUTED"));
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@Target"], Transaction.Command.LoadPortType.ReadStatus, "EXCUTED"));
                                    break;
                            }
                            break;
                        case TaskFlowManagement.Command.LOADPORT_ORGSH:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    if (!Target.InitialComplete && !SystemConfig.Get().SaftyCheckByPass)
                                    {
                                        TaskFlowManagement.Remove(TaskJob.Id);
                                        TaskReport.On_Task_Abort(TaskJob, "SYSTEM", "CAN", "S0300168");
                                    }
                                    else
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@Target"], Transaction.Command.LoadPortType.ReadStatus, "EXCUTED"));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@Target"], Transaction.Command.LoadPortType.ForceInitialPos, "FINISHED"));
                                    break;
                            }
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    if (TaskJob.CheckList.Count != 0)
                    {
                        foreach (TaskFlowManagement.ExcutedCmd eachCmd in TaskJob.CheckList)
                        {
                            
                            Transaction Txn = new Transaction();
                            Txn.Method = eachCmd.ExcuteName;
                            Txn.TaskId = TaskJob.Id;
                            if (TaskJob.Params != null)
                            {
                                foreach (KeyValuePair<string, string> item in TaskJob.Params)
                                {
                                    switch (item.Key)
                                    {
                                        
                                        case "@Position":
                                            Txn.Position = item.Value;
                                            break;
                                        case "@Arm":
                                            Txn.Arm = item.Value;
                                            break;
                                        case "@Slot":
                                            Txn.Slot = item.Value;
                                            break;
                                        case "@Value":
                                            Txn.Value = item.Value;
                                            break;
                                    }
                                    //Txn.Position2 = Position2;
                                    //Txn.Arm2 = Arm2;
                                    //Txn.Slot2 = Slot2;
                                }
                            }

                            Target.SendCommand(Txn, out Message);
                        }
                    }
                    else
                    {
                        TaskFlowManagement.Remove(TaskJob.Id);
                        TaskReport.On_Task_Finished(TaskJob);
                        
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " exception: " + e.StackTrace);
                TaskFlowManagement.Remove(TaskJob.Id);
                TaskReport.On_Task_Abort(TaskJob,"SYSTEM","ABS", e.StackTrace);

                return false;
            }
            return true;
        }
        private bool CheckEMO(TaskFlowManagement.CurrentProcessTask TaskJob,ITaskFlowReport TaskReport)
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
        private bool CheckReady(TaskFlowManagement.CurrentProcessTask TaskJob,Node Target, ITaskFlowReport TaskReport)
        {
            bool result = true;
            if (!SystemConfig.Get().SaftyCheckByPass)
            {
                if (!Target.InitialComplete)
                {
                    TaskFlowManagement.Remove(TaskJob.Id);
                    TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300184");
                    result = false;
                }
                else if(!Target.OrgSearchComplete)
                {
                    TaskFlowManagement.Remove(TaskJob.Id);
                    TaskReport.On_Task_Abort(TaskJob, Target.Name, "CAN", "S0300185");
                    result = false;
                }
            }
            return result;
        }
    }
}
