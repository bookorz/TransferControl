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
    class BaseTaskFlow : ITaskFlow
    {
        public IUserInterfaceReport _TaskReport;

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
    }
}
