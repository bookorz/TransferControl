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
    class WTS : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(WTS));
        public bool Excute(TaskFlowManagement.CurrentProcessTask TaskJob, ITaskFlowReport TaskReport)
        {
            logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());
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
                        case TaskFlowManagement.Command.ALIGNER_ALIGN:

                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    if (TaskJob.CheckList.Count != 0)
                    {
                        foreach (TaskFlowManagement.ExcutedCmd eachCmd in TaskJob.CheckList)
                        {
                            if (NodeManagement.Get(eachCmd.NodeName).Enable)
                            {
                                NodeManagement.Get(eachCmd.NodeName).SendCommand(eachCmd.Txn, out Message);
                                if (eachCmd.Txn.Method == Transaction.Command.LoadPortType.GetMappingDummy)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                eachCmd.Finished = true;
                            }
                        }
                    }
                    else
                    {//recursive
                        TaskJob.CurrentIndex++;
                        if (!this.Excute(TaskJob, TaskReport))
                        {
                            return false;
                        }
                    }
                }


            }
            catch (Exception e)
            {
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " exception: " + e.StackTrace);
                AbortTask(TaskReport, TaskJob, "SYSTEM", "ABS", e.StackTrace);
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
                    TaskFlowManagement.TaskRemove(TaskJob.Id);
                    AbortTask(TaskReport, TaskJob, "SYSTEM", "CAN", "S0300170");
                    result = false;
                }
            }
            return result;
        }
        private void MoveWip(string FromPosition, string FromSlot, string ToPosition, string ToSlot)
        {
            try
            {
                Node FNode = NodeManagement.Get(FromPosition);
                if (!FNode.Enable)
                {
                    return;
                }
                Node TNode = NodeManagement.Get(ToPosition);
                if (!TNode.Enable)
                {
                    return;
                }
                Job J;
                Job tmp;
                if (!FNode.JobList.TryRemove(FromSlot, out J))
                {
                    J = RouteControl.CreateJob();//當沒有帳時強制建帳
                    J.Job_Id = JobManagement.GetNewID();
                    J.Host_Job_Id = J.Job_Id;
                    J.Position = FNode.Name;
                    J.Slot = FromSlot;
                    J.MapFlag = true;
                    J.MappingValue = "0";
                    JobManagement.Add(J.Job_Id, J);
                }
                if (FNode.Type.ToUpper().Equals("LOADPORT"))
                {
                    //LOADPORT空的Slot要塞空資料                                       
                    tmp = RouteControl.CreateJob();
                    tmp.Job_Id = "No wafer";
                    tmp.Host_Job_Id = "No wafer";
                    tmp.Slot = FromSlot;
                    tmp.Position = FNode.Name;
                    tmp.MappingValue = "0";
                    FNode.JobList.TryAdd(tmp.Slot, tmp);
                    //從LOADPORT取出，處理開始
                    J.InProcess = true;
                    J.StartTime = DateTime.Now;
                    J.FromPort = FNode.Name;
                    J.FromPortSlot = FromSlot;
                    J.FromFoupID = FNode.FoupID;
                }
                if (TNode.Type.ToUpper().Equals("LOADPORT"))
                {
                    //放回UNLOADPORT，處理結束
                    J.InProcess = false;
                    J.NeedProcess = false;
                    J.EndTime = DateTime.Now;
                    J.ToFoupID = TNode.FoupID;
                    J.ToPort = TNode.Name;
                    J.ToPortSlot = ToSlot;
                }
                if (TNode.Type.ToUpper().Equals("ALIGNER"))
                {
                    J.AlignerFlag = true;
                }
                J.LastNode = J.Position;
                J.LastSlot = J.Slot;

                TNode.JobList.TryRemove(ToSlot, out tmp);
                if (TNode.JobList.TryAdd(ToSlot, J))
                {
                    //更新WAFER位置
                    J.Position = TNode.Name;
                    J.Slot = ToSlot;
                    J.PositionChangeReport();
                }
                else
                {
                    logger.Error("Move wip error(Add): From=" + FromPosition + " Slot=" + FromSlot + " To=" + ToPosition + " Slot=" + ToSlot);
                }
                FNode.RefreshMap();
                TNode.RefreshMap();
            }
            catch (Exception e)
            {
                logger.Error("Move wip fail:" + e.Message + " exception: " + e.StackTrace);
            }
        }
        private void AbortTask(ITaskFlowReport TaskReport, TaskFlowManagement.CurrentProcessTask TaskJob, string Location, string ReportType, string Message)
        {

            TaskReport.On_Task_Abort(TaskJob, Location, ReportType, Message);
            TaskJob.HasError = true;
        }
    }
}
