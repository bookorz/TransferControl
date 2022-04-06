using System;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransferControl.Config;
using TransferControl.Management;

namespace TransferControl.Procedure.SubProcedure
{
    public class DemoLPProc : SubProc
    {

        //ILog logger = LogManager.GetLogger(typeof(DemoLPProc));

        public DemoLPProc(Node node):base(node)
        {
            logger = LogManager.GetLogger(typeof(DemoLPProc));
        }
        ~DemoLPProc()
        {
            IsProcExit = true;
        }

        public override void Run(Node procNode)
        {            
            Dictionary<string, string> param = new Dictionary<string, string>();

            switch (procNode.ProcStatus)
            {
                case Node.ProcedureStatus.Idle:
                    procNode.ProcStatus = Node.ProcedureStatus.CheckPresence;
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.CheckPresence:

                    if ((procNode.Foup_Presence && procNode.Foup_Placement) || SystemConfig.Get().OfflineMode)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.ReadCarrierID;
                        logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    }

                    break;

                case Node.ProcedureStatus.ReadCarrierID:

                    if(procNode.FoupIDReader.Equals(""))
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    else
                    {
                        param.Add("@Target", procNode.FoupIDReader);
                        if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.GET_CSTID, param).Promise())
                        {
                            procNode.ProcStatus = Node.ProcedureStatus.Stop;
                        }
                        else
                        {
                            procNode.ProcStatus = Node.ProcedureStatus.LoadAndMapping;
                        }
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.LoadAndMapping:

                    param.Add("@Target", procNode.Name);
                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_OPEN, param).Promise())
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    else
                    {
                        //procNode.ProcStatus = Node.ProcedureStatus.WaitLoadCompleted;
                        procNode.ProcStatus = Node.ProcedureStatus.CheckType;
                    }
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.CheckType:
                    procNode.AssignWaferFinished = false;
                    if (procNode.Mode.Equals("LD"))
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.AssignWafer;
                    }
                    else if(procNode.Mode.Equals("ULD"))
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.IsUnloaderFull;
                    }
                    else
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.AssignWafer:
                    List<Node> unloader = new List<Node>();
                    foreach(Node nd in NodeManagement.GetLoadPortList())
                    {
                        if (nd.Mode!= null)
                        {
                            if (nd.Mode.Equals("ULD") && nd.ProcStatus.Equals(Node.ProcedureStatus.IsUnloaderFull))
                            {
                                unloader.Add(nd);
                            }
                        }
                    }

                    if (unloader.Count() != 0)
                    {
                        Node UND = unloader.First();

                        var AssignWaferInLoader = from Wafer in JobManagement.GetJobList()
                                            where Wafer.Position.ToUpper().Equals(procNode.Name.ToUpper()) && 
                                                    Wafer.MapFlag && !Wafer.ErrPosition 
                                            select Wafer;

                        if(AssignWaferInLoader.Count() != 0)
                        {
                            //由上放到下
                            //假設Slot no 為25
                            Job EmptySlot = null;
                            for (int i = UND.Foup_Capacity; i>0; i--)
                            {
                                EmptySlot = JobManagement.Get(UND.Name, i.ToString());

                                if (EmptySlot != null) continue;

                                var wafers = from Wafer in AssignWaferInLoader.ToList()
                                             where !Wafer.IsAssigned
                                             select Wafer;

                                wafers = wafers.OrderBy(x => Convert.ToInt16(x.Slot));

                                if (wafers.Count() != 0)
                                {
                                    Job wafer = wafers.First();
                                    wafer.Destination = UND.Name;
                                    wafer.DestinationSlot = i.ToString();
                                    wafer.IsAssigned = true;

                                    logger.Debug("Assign Wafer : from " + wafer.Position + " Slot " + wafer.Slot  + 
                                                ", to " + wafer.Destination + " Slot " + wafer.DestinationSlot);
                                }
                            }
                        }
                        UND.AssignWaferFinished = true;
                        procNode.AssignWaferFinished = true;
                        procNode.ProcStatus = Node.ProcedureStatus.IsLoaderEmpty;
                        logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    }
                    break;

                case Node.ProcedureStatus.IsLoaderEmpty:

                    var WaferInLoader = from Wafer in JobManagement.GetJobList()
                                        where Wafer.Position.ToUpper().Equals(procNode.Name.ToUpper()) &&
                                                Wafer.IsAssigned
                                        select Wafer;

                    if (WaferInLoader.Count() == 0)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Unload;
                        logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    }

                    break;

                case Node.ProcedureStatus.IsUnloaderFull:
                    //被Assign的Wafer Loadport 最終位置和 Loadport 內的 Wafer 數量吻合
                    if (procNode.AssignWaferFinished)
                    {
                        var WaferInUnloader = from Wafer in JobManagement.GetJobList()
                                              where Wafer.IsAssigned &&
                                                      Wafer.Destination.ToUpper().Equals(procNode.Name.ToUpper())
                                              select Wafer;

                        bool IsUnloaderFull = true;
                        foreach (Job wafer in WaferInUnloader.ToList())
                        {
                            if (!wafer.Destination.ToUpper().Equals(wafer.Position.ToUpper()))
                            {
                                IsUnloaderFull = false;
                                break;
                            }

                        }

                        if(IsUnloaderFull)
                        {
                            procNode.ProcStatus = Node.ProcedureStatus.Unload;
                            logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                        }
                    }

                    break;

                case Node.ProcedureStatus.Unload:

                    param.Add("@Target", procNode.Name);
                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_CLOSE, param).Promise())
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                        logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    }
                    else
                    {
                        if (procNode.Mode.Equals("LD"))
                        {
                            procNode.Mode = "ULD";
                        }
                        else if (procNode.Mode.Equals("ULD"))
                        {
                            procNode.Mode = "LD";
                        }

                        procNode.ProcStatus = Node.ProcedureStatus.Idle;
                        logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    }
                    break;

                case Node.ProcedureStatus.Stop:
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    IsProcStop = true;
                    break;
                default:
                    throw new NotSupportedException();

            }

            SpinWait.SpinUntil(() => false, 100);
        }
    }
}
