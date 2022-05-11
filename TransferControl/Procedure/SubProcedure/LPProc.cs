using log4net;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TransferControl.Config;
using TransferControl.Management;

namespace TransferControl.Procedure.SubProcedure
{
    public class LPProc : SubProc
    {

        public LPProc(Node node, IProcReport procreport) : base(node, procreport)
        {
            logger = LogManager.GetLogger(typeof(LPProc));

        }
        ~LPProc()
        {
            IsProcExit = true;
        }
        public override void Run(Node procNode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            bool bRet = true;

            switch (procNode.ProcStatus)
            {
                case Node.ProcedureStatus.Idle:
                    StartToStop = false;
                    procNode.ProcStatus = Node.ProcedureStatus.CheckMappingResult;
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.CheckMappingResult:

                    SpinWait.SpinUntil(() => procNode.IsMapping, 3000);

                    if(procNode.IsMapping)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.CheckType;
                    }
                    else if(StartToStop)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;
                case Node.ProcedureStatus.LoadPortChangeForDemo:

                    procNode.ProcStatus = Node.ProcedureStatus.CheckMappingResult;

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;
                case Node.ProcedureStatus.CheckType:
                    procNode.AssignWaferFinished = false;
                    param.Add("@Target", procNode.Name);
                    if (Recipe.CurrentRecipe.RunMode.ToUpper().Equals("AUTO"))
                    {
                        if (procNode.Mode.Equals("LD"))
                        {
                            procNode.ProcStatus = bRet ? Node.ProcedureStatus.AssignWafer : Node.ProcedureStatus.AlarmStop;
                        }
                        else if (procNode.Mode.Equals("ULD"))
                        {
                            procNode.ProcStatus = bRet ? Node.ProcedureStatus.IsUnloaderFull : Node.ProcedureStatus.AlarmStop;
                        }
                        else
                        {
                            procNode.ProcStatus = Node.ProcedureStatus.Stop;
                        }
                    }
                    else
                    {
                        procNode.AssignWaferFinished = true;
                        procNode.ProcStatus = Node.ProcedureStatus.IsSemiLoaderFinished;
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;
                case Node.ProcedureStatus.IsSemiLoaderFinished:
                    if (procNode.AssignWaferFinished)
                    {
                        //未移動的 Wafer
                        var UntransWafers = from Wafer in JobManagement.GetJobList()
                                             where !Wafer.Position.ToUpper().Equals(Wafer.Destination.ToUpper()) &&
                                                    Wafer.IsAssigned
                                             select Wafer;

                        var WafersInLoadport = from Wafer in JobManagement.GetJobList()
                                               where Wafer.Destination.ToUpper().Equals(procNode.Name.ToUpper()) &&
                                                      !Wafer.Slot.ToUpper().Equals(Wafer.DestinationSlot.ToUpper()) &&
                                                      Wafer.IsAssigned
                                               select Wafer;

                        if(StartToStop && !procNode.RobotError)
                        {
                            //強制關門
                            procNode.ProcStatus = Node.ProcedureStatus.CloseDoor;
                        }
                        else if (UntransWafers.Count() == 0 && WafersInLoadport.Count() == 0 && !procNode.RobotError)
                        {
                            procNode.AssignWaferFinished = false;
                            procNode.ProcStatus = Node.ProcedureStatus.Unload;
                        }
                        else
                        {
                            SpinWait.SpinUntil(() => false, 1000);
                        }

                    }
                    else
                    {
                        if (StartToStop)
                            procNode.ProcStatus = Node.ProcedureStatus.CloseDoor;
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;
                case Node.ProcedureStatus.Unload:

                    param.Add("@Target", procNode.Name);
                    bRet = TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_UNCLAMP, param).Promise();

                    if(bRet && Recipe.CurrentRecipe.is_use_burnin)
                    {
                        procNode.Mode = procNode.Mode.Equals("LD") ? "ULD" : "LD";
                        bRet = TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_OPEN, param).Promise();

                        procNode.ProcStatus = bRet ? Node.ProcedureStatus.LoadPortChangeForDemo : Node.ProcedureStatus.AlarmStop;
                    }
                    else
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }


                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.AssignWafer:
                    List<Node> unloader = new List<Node>();
                    foreach (Node nd in NodeManagement.GetLoadPortList())
                    {
                        if (nd.Mode != null)
                            if (nd.Mode.Equals("ULD") && nd.IsMapping && !nd.AssignWaferFinished)
                                unloader.Add(nd);
                    }

                    if (unloader.Count() != 0)
                    {
                        //設定 Unloader
                        Node UND = unloader.First();

                        var AssignWaferInLoader = from Wafer in JobManagement.GetJobList()
                                                  where Wafer.Position.ToUpper().Equals(procNode.Name.ToUpper()) &&
                                                          Wafer.MapFlag && !Wafer.ErrPosition
                                                  select Wafer;

                        if (AssignWaferInLoader.Count() != 0)
                        {
                            string UnloaderPutRule = Recipe.CurrentRecipe.auto_put_constrict;

                            //由上放到下
                            //假設Slot no 為25
                            bool HaveWaferInRule2 = false;

                            for (int i = Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN") ? UND.Foup_Capacity : 1;
                                        Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN") ? i > 0 : i < UND.Foup_Capacity + 1;
                                        i = Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN") ? i - 1 : i + 1)
                                {
                                //該 Slot 有片便跳過
                                if (JobManagement.Get(UND.Name, i.ToString()) != null) continue;

                                if(UnloaderPutRule.Equals("1"))         //相鄰下方有片不放片
                                {
                                    if(i - 1 > 0 )
                                        if (JobManagement.Get(UND.Name, (i - 1).ToString()) != null)
                                            continue;
                                }
                                else if(UnloaderPutRule.Equals("2"))    //下方有片不放片
                                {
                                    for(int j = i-1; j > 0; j--)
                                    {
                                        if (JobManagement.Get(UND.Name, j.ToString()) != null)
                                        {
                                            HaveWaferInRule2 = true;
                                            break;
                                        }
                                    }
                                }

                                if (HaveWaferInRule2) break;


                                var wafers = from Wafer in AssignWaferInLoader.ToList()
                                             where !Wafer.IsAssigned
                                             select Wafer;

                                //Direction of get wafer
                                if (Recipe.CurrentRecipe.get_slot_order.Equals("BOTTOM_UP"))
                                {
                                    wafers = wafers.OrderBy(x => Convert.ToInt16(x.Slot));
                                }
                                else 
                                {
                                    wafers = wafers.OrderByDescending(x => Convert.ToInt16(x.Slot));
                                }

                                if (wafers.Count() != 0)
                                {
                                    Job wafer = wafers.First();
                                    Job wafer2 = null;
                                    bool wafer2IsAbnormal = false;

                                    if (Recipe.CurrentRecipe.is_use_r_arm && Recipe.CurrentRecipe.is_use_l_arm)
                                    {
                                        wafer2 = JobManagement.Get(wafer.Position,
                                                    (Recipe.CurrentRecipe.get_slot_order.Equals("BOTTOM_UP") ? Convert.ToInt16(wafer.Slot) + 1 : Convert.ToInt16(wafer.Slot) - 1).ToString());

                                        if (wafer2 != null)
                                        {
                                            if (!wafer2.IsAssigned && Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN") ? i > 1 : i < UND.Foup_Capacity)
                                            {
                                                if (JobManagement.Get(UND.Name, Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN") ? (i - 1).ToString() : (i + 1).ToString()) == null)
                                                {
                                                    if (Recipe.CurrentRecipe.put_slot_order.Equals(Recipe.CurrentRecipe.get_slot_order))
                                                    {
                                                        AssignWaferDestination(UND, wafer, i);
                                                        AssignWaferDestination(UND, wafer2, Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN") ? i - 1 : i + 1);
                                                    }
                                                    else
                                                    {
                                                        AssignWaferDestination(UND, wafer, Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN") ? i - 1 : i + 1);
                                                        AssignWaferDestination(UND, wafer2, i);
                                                    }

                                                    i = Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN") ? i - 1 : i + 1;
                                                }
                                                else
                                                {
                                                    wafer2IsAbnormal = true;
                                                    AssignWaferDestination(UND, wafer, i);
                                                }
                                            }
                                            else
                                            {
                                                wafer2IsAbnormal = true;
                                                AssignWaferDestination(UND, wafer, i);
                                            }
                                        }
                                        else
                                        {
                                            AssignWaferDestination(UND, wafer, i);
                                        }
                                    }
                                    else
                                    {

                                        AssignWaferDestination(UND, wafer, i);
                                    }

                                    if(wafer2 != null && !wafer2IsAbnormal)
                                    {
                                        Report.MessageReport(string.Format("<<{0} S{1}S{2} => {3} S{4}S{5}>>",
                                                                            wafer.Position.Replace("LOADPORT0", "L"), wafer.Slot, wafer2.Slot,
                                                                            wafer.Destination.Replace("LOADPORT0", "L"), wafer.DestinationSlot, wafer2.DestinationSlot));
                                    }
                                    else
                                    {
                                        Report.MessageReport(string.Format("<<{0} S{1} => {2} S{3}>>",
                                                                            wafer.Position.Replace("LOADPORT0", "L"), wafer.Slot,
                                                                            wafer.Destination.Replace("LOADPORT0", "L"), wafer.DestinationSlot));
                                    }
                                }
                            }
                        }


                        UND.AssignWaferFinished = true;

                        procNode.AssignWaferFinished = true;
                        procNode.ProcStatus = Node.ProcedureStatus.IsLoaderEmpty;
                        logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    }
                    else
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    break;

                case Node.ProcedureStatus.IsLoaderEmpty:

                    var WaferInLoader = from Wafer in JobManagement.GetJobList()
                                        where Wafer.Position.ToUpper().Equals(procNode.Name.ToUpper()) &&
                                                Wafer.IsAssigned
                                        select Wafer;

                    if (WaferInLoader.Count() == 0 && !procNode.RobotError)
                    {
                        WaferInLoader = from Wafer in JobManagement.GetJobList()
                                            where Wafer.Position.ToUpper().Equals(procNode.Name.ToUpper())
                                            select Wafer;


                        //完全沒有 Wafer or 有 Wafer, 但未 Assign
                        //(或者開始停機)
                        procNode.ProcStatus = WaferInLoader.Count() == 0 || StartToStop || Recipe.CurrentRecipe.is_use_burnin ? Node.ProcedureStatus.CloseDoor : Node.ProcedureStatus.AssignWafer;

                        logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    }
                    else
                    {
                        SpinWait.SpinUntil(() => false, 3000);

                        if (StartToStop && !procNode.RobotError)
                            procNode.ProcStatus = Node.ProcedureStatus.CloseDoor;
                    }

                    break;
                case Node.ProcedureStatus.CloseDoor:
                    procNode.IsMapping = false;
                    procNode.AssignWaferFinished = false;
                    param.Add("@Target", procNode.Name);
                    bRet = TaskFlowManagement.Excute(TaskFlowManagement.Command.LOADPORT_DOOR_CLOSE, param).Promise();
                    TimerManagement.Record();
                    procNode.ProcStatus = bRet ? StartToStop ? Node.ProcedureStatus.Stop  : Node.ProcedureStatus.WaitCarrierOut: Node.ProcedureStatus.AlarmStop;
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.WaitCarrierOut:
                    param.Add("@Target", procNode.Name);

                    if (!Recipe.CurrentRecipe.is_use_burnin)
                    {
                        bRet = TaskFlowManagement.Excute(TaskFlowManagement.Command.NOTIFY_CARRIER_OUT, param).Promise();
                        procNode.ProcStatus = bRet ? Node.ProcedureStatus.CheckMappingResult : Node.ProcedureStatus.AlarmStop;
                    }
                    else
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Unload;
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
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

                        if (IsUnloaderFull)
                        {
                            string UnloaderPutRule = Recipe.CurrentRecipe.auto_put_constrict;
                            bool HaveWaferInRule2 = false;
                            for (int i = procNode.Foup_Capacity; i > 0; i--)
                            {
                                procNode.ProcStatus = Node.ProcedureStatus.CloseDoor;
                                if (UnloaderPutRule.Equals("0"))
                                {
                                    if (JobManagement.Get(procNode.Name, i.ToString()) == null)
                                    {
                                        //還有空Slot提供放片
                                        //等待下次Assign
                                        procNode.AssignWaferFinished = false;
                                        procNode.ProcStatus = Node.ProcedureStatus.IsUnloaderFull;
                                        break;
                                    }
                                }
                                else if(UnloaderPutRule.Equals("1"))
                                {
                                    if (JobManagement.Get(procNode.Name, i.ToString()) == null)
                                    {
                                        if(i != 1 && JobManagement.Get(procNode.Name, (i-1).ToString()) == null)
                                        {
                                            //還有空Slot提供放片
                                            //等待下次Assign
                                            procNode.AssignWaferFinished = false;
                                            procNode.ProcStatus =  Node.ProcedureStatus.IsUnloaderFull;
                                            break;
                                        }
                                    }
                                }
                                else if(UnloaderPutRule.Equals("2"))
                                {
                                    if (JobManagement.Get(procNode.Name, i.ToString()) == null)
                                    {
                                        for(int j = i-1; j > 0; j--)
                                        {
                                            if (JobManagement.Get(procNode.Name, j.ToString()) != null)
                                            {
                                                HaveWaferInRule2 = true;
                                                break;
                                            }
                                        }

                                        if(!HaveWaferInRule2)
                                        {
                                            //還有空Slot提供放片
                                            procNode.AssignWaferFinished = false;
                                            procNode.ProcStatus = Node.ProcedureStatus.IsUnloaderFull;
                                        }

                                        break;
                                    }
                                }
                            }

                            if(procNode.ProcStatus.Equals(Node.ProcedureStatus.CloseDoor) && procNode.RobotError)
                                procNode.ProcStatus = Node.ProcedureStatus.IsUnloaderFull;

                            logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                        }
                        else if(StartToStop && !procNode.RobotError)
                        {
                            procNode.ProcStatus = Node.ProcedureStatus.CloseDoor;
                        }
                    }
                    else
                    {
                        SpinWait.SpinUntil(() => false, 500);

                        if (StartToStop && !procNode.RobotError)
                            procNode.ProcStatus = Node.ProcedureStatus.CloseDoor;
                    }

                    break;

                case Node.ProcedureStatus.AlarmStop:
                case Node.ProcedureStatus.Stop:
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    IsProcStop = true;

                    //procNode.InitialComplete = false;
                    //procNode.OrgSearchComplete = false;

                    StartToStop = false;
                    break;
                default:
                    throw new NotSupportedException();

            }
        }
        public override void Stop()
        {
            StartToStop = true;
        }
        private void AssignWaferDestination(Node ToPort, Job wafer, int slot)
        {
            wafer.Destination = ToPort.Name;
            wafer.DestinationSlot = slot.ToString();
            wafer.IsAssigned = true;
        }
    }
}
