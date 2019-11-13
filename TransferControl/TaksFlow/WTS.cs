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
                        case TaskFlowManagement.Command.CLAMP_ELPT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.Clamp, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.UNCLAMP_ELPT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.Unclamp, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.MOVE_FOUP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    if (TaskJob.Params["@FromPosition"].Contains("ELPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@FromPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveIn, "")));
                                    }
                                    break;
                                case 1:
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.FoupRobot.Transfer, "", TaskJob.Params["@FromPosition"], "", "", TaskJob.Params["@ToPosition"])));
                                    break;
                                case 2:
                                    if (TaskJob.Params["@ToPosition"].Contains("ELPT"))
                                    {
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(TaskJob.Params["@ToPosition"], "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.MoveOut, "")));
                                    }
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.FOUP_ID:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ELPT.ReadCID, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.OPEN_FOUP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Load, "1")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CLOSE_FOUP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd(Target.Name, "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.ILPT.Unload, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_WTS:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR get ILPT
                                        //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Pick, TaskJob.Params["@Mode"], TaskJob.Params["@FromPosition"])));
                                        //CTU getwait
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, TaskJob.Params["@Mode"], "WHR", "", "", "", "", "", "0")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //CTU Putwait
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Place, TaskJob.Params["@Mode"], "WHR", "", "", "", "", "", "0")));
                                        //WHR Getwait
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.PreparePick, TaskJob.Params["@Mode"], "CTU")));

                                    }
                                    break;
                                case 1:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR putwait for CTU
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.PreparePlace, TaskJob.Params["@Mode"], "CTU")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //WHR Extend Get
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Extend, TaskJob.Params["@Mode"], "", "3", "", "", "", "", "0")));
                                    }
                                    break;
                                case 2:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR extend Put
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Extend, TaskJob.Params["@Mode"], "", "3", "", "", "", "","1")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //WHR Up
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Up, "")));

                                    }
                                    break;
                                case 3:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //CTU hold
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Hold, TaskJob.Params["@Mode"])));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //CTU Release
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Release, TaskJob.Params["@Mode"])));
                                    }
                                    break;
                                case 4:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR Down
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Down, "")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //WHR Retract
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Retract, "")));
                                    }
                                    break;
                                case 5:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //WHR Retract
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Retract, "")));

                                    }
                                    else if (TaskJob.Params["@FromPosition"].Contains("CTU"))
                                    {
                                        //CTU HOME
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Home, "")));
                                        //WHR Put ILPT
                                        //TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Place, TaskJob.Params["@Mode"], TaskJob.Params["@ToPosition"])));
                                    }
                                    break;
                                case 6:
                                    if (TaskJob.Params["@FromPosition"].Contains("ILPT"))
                                    {
                                        //CTU HOME
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Home, "")));
                                        //WHR Home
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.SHome, "")));

                                    }
                                    
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.TRANSFER_PTZ:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Transfer, TaskJob.Params["@Mode"], "", TaskJob.Params["@Station"], "", "", "", "", TaskJob.Params["@Direction"])));

                                    break;
                                case 1:
                                   
                                    //IN: put to ptz  OUT: get from ptz
                                    if (TaskJob.Params["@Way"].Equals("IN"))
                                    {
                                        //CTU Put to PTZ
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Place, TaskJob.Params["@Mode"], "PTZ", "", "", "", "", "", "1")));
                                    }
                                    else if (TaskJob.Params["@Way"].Equals("OUT"))
                                    {
                                        //CTU Get from PTZ
                                        TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, TaskJob.Params["@Mode"], "PTZ", "", "", "", "", "", "1")));
                                    }
                                    break;
                                case 2:
                                    //PTZ Home
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Home, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.BLOCK_PTZ:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Transfer, TaskJob.Params["@Path"], "3", "", "", "", "", "", TaskJob.Params["@Orientation"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.RELEASE_PTZ:
                            switch (TaskJob.CurrentIndex)
                            {
                                default:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.BLOCK_ALIGNER:
                            switch (TaskJob.CurrentIndex)
                            {
                                default:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.RELEASE_ALIGNER:
                            switch (TaskJob.CurrentIndex)
                            {
                                default:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.NOTCH_ALIGN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("ALIGNER", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WTSAligner.Align, TaskJob.Params["@Value"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_RETRACT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Retract,"")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Reset, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_SHOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.SHome, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_EXTEND:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Extend, TaskJob.Params["@Mode"],"", TaskJob.Params["@Station"],"","","","", TaskJob.Params["@Value"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_PREPAREPICK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.PreparePick, TaskJob.Params["@Mode"], TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_PREPAREPLACE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.PreparePlace, TaskJob.Params["@Mode"], TaskJob.Params["@Position"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_DOWN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Down, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WHR_UP:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WHR", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WHR.Up, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_RESET:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "EXCUTED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Reset, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_HOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Home, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_INIT:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.OrgSearch, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_PREPAREPICK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, TaskJob.Params["@Mode"], TaskJob.Params["@Position"], "", "", "", "", "", "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_PREPAREPLACE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Place, TaskJob.Params["@Mode"], TaskJob.Params["@Position"], "", "", "", "", "", "0")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_PLACE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Place, TaskJob.Params["@Mode"], TaskJob.Params["@Position"], "", "", "", "", "", "1")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_PICK:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Pick, TaskJob.Params["@Mode"], TaskJob.Params["@Position"], "", "", "", "", "", "1")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_HOLD:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Hold, TaskJob.Params["@Mode"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.CTU_RELEASE:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("CTU", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.CTU.Release, TaskJob.Params["@Mode"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.PTZ_TRANSFER:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Transfer, TaskJob.Params["@Mode"],"", TaskJob.Params["@Station"],"","","","", TaskJob.Params["@Direction"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.PTZ_HOME:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("PTZ", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.PTZ.Home, "")));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.WTSALIGNER_ALIGN:
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    TaskJob.CheckList.Add(new TaskFlowManagement.ExcutedCmd("WTSALIGNER", "FINISHED", new Transaction().SetAttr(TaskJob.Id, Transaction.Command.WTSAligner.Align, TaskJob.Params["@Value"])));
                                    break;
                                default:
                                    TaskReport.On_Task_Finished(TaskJob);
                                    return false;
                            }
                            break;
                        case TaskFlowManagement.Command.STOP_STOCKER:
                        case TaskFlowManagement.Command.RESUME_STOCKER:
                        case TaskFlowManagement.Command.ABORT_STOCKER:
                        case TaskFlowManagement.Command.RESET_STOCKER:



                        case TaskFlowManagement.Command.STOP_WTS:
                        case TaskFlowManagement.Command.RESUME_WTS:
                        case TaskFlowManagement.Command.ABORT_WTS:
                        case TaskFlowManagement.Command.RESET_WTS:




                        case TaskFlowManagement.Command.RESET_E84:
                        case TaskFlowManagement.Command.PORT_ACCESS_MODE:
                        
                            switch (TaskJob.CurrentIndex)
                            {
                                case 0:
                                    TaskReport.On_Task_Ack(TaskJob);
                                    break;
                                default:
                                    SpinWait.SpinUntil(() => false, 2000);
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
