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
    class Sanwa_Sorter : ITaskFlow
    {
        ILog logger = LogManager.GetLogger(typeof(Sanwa_Sorter));
        IUserInterfaceReport _TaskReport;
        public Sanwa_Sorter(IUserInterfaceReport TaskReport)
        {
            _TaskReport = TaskReport;
        }
        public void Excute(object input)
        {
            TaskFlowManagement.CurrentProcessTask TaskJob = (TaskFlowManagement.CurrentProcessTask)input;
            if (TaskJob.TaskName != TaskFlowManagement.Command.CCLINK_GET_IO && TaskJob.TaskName != TaskFlowManagement.Command.CCLINK_SET_IO)
            {
                logger.Debug("ITaskFlow:" + TaskJob.TaskName.ToString() + " Index:" + TaskJob.CurrentIndex.ToString());

            }
            if (TaskJob.CurrentIndex == 0 && TaskJob.TaskName.ToString().IndexOf("CCLINK") == -1)
            {

                _TaskReport.On_Message_Log("CMD", TaskJob.TaskName.ToString() + " " + (TaskJob.Params.ContainsKey("@Target") ? TaskJob.Params["@Target"] : "") + " Executing");
            }
            string Message = "";
            Node Target = null;
            Node Position = null;
            string tmp = "";
            string Value = "";


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
                    }
                }
            }
            try
            {

                switch (TaskJob.TaskName)
                {

                }
            }
            catch (Exception e)
            {
                logger.Error("Excute fail Task Name:" + TaskJob.TaskName.ToString() + " " + (TaskJob.Params.ContainsKey("@Target") ? TaskJob.Params["@Target"] : "") + " exception: " + e.StackTrace);
                AbortTask(TaskJob, NodeManagement.Get("SYSTEM"), e.StackTrace);
                return;
            }
            return;
        }
        private void AbortTask(TaskFlowManagement.CurrentProcessTask TaskJob, Node Node, string Message)
        {
            _TaskReport.On_Alarm_Happen(AlarmManagement.NewAlarm(Node, Message));

            _TaskReport.On_TaskJob_Aborted(TaskJob);
            _TaskReport.On_Message_Log("CMD", TaskJob.TaskName.ToString() + " " + (TaskJob.Params.ContainsKey("@Target") ? TaskJob.Params["@Target"] : "") + " Aborted");

        }
        private void FinishTask(TaskFlowManagement.CurrentProcessTask TaskJob)
        {
            if (TaskJob.TaskName.ToString().IndexOf("CCLINK") == -1)
            {
                _TaskReport.On_Message_Log("CMD", TaskJob.TaskName.ToString() + " " + (TaskJob.Params.ContainsKey("@Target") ? TaskJob.Params["@Target"] : "") + " Finished");
            }
            _TaskReport.On_TaskJob_Finished(TaskJob);

        }
    }
}
