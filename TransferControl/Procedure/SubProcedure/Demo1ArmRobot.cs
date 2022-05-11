using System;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Procedure.SubProcedure
{
    class Demo1ArmRobot : SubProc
    {
        public Demo1ArmRobot(Node node, IProcReport procreport) : base(node, procreport)
        {
            logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
        ~Demo1ArmRobot()
        {
            IsProcExit = true;
        }

        public override void Run(Node procNode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();

            switch (procNode.ProcStatus)
            {
                case Node.ProcedureStatus.Idle:
                    procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.GetPresence:
                    param.Add("@Target", procNode.Name);
                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.GET_CLAMP, param).Promise())
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    else
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.CheckPresence;
                    }
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.CheckPresence:
                    procNode.ProcStatus = procNode.R_Presence ? Node.ProcedureStatus.Stop : Node.ProcedureStatus.TransferWafer;
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.TransferWafer:
                    List<Node> LD = new List<Node>();

                    foreach (Node loader in NodeManagement.GetLoadPortList())
                    {
                        if(loader.Mode != null)
                        if (loader.Mode.Equals("LD") && loader.IsMapping && loader.AssignWaferFinished)
                        {
                            LD.Add(loader);
                        }
                    }

                    if (LD.Count() != 0)
                    {
                        Node LDnode = LD.First();

                        var WaferList = from wafer in JobManagement.GetJobList()
                                        where wafer.IsAssigned &&
                                            !wafer.Destination.Equals("") &&
                                            !wafer.Position.ToUpper().Equals(wafer.Destination.ToUpper())
                                        select wafer;

                        if (WaferList.Count() != 0)
                        {
                            WaferList = WaferList.OrderBy(x => Convert.ToInt16(x.Slot));

                            Job wafer = WaferList.First();

                            param.Add("@Target", procNode.Name);
                            param.Add("@FromPosition", LDnode.Name);
                            param.Add("@ToPosition", wafer.Destination);
                            param.Add("@FromArm", "1");
                            param.Add("@ToArm", "1");
                            param.Add("@FromSlot", wafer.Slot);
                            param.Add("@ToSlot", wafer.DestinationSlot);
                            param.Add("@From_BYPASS_CHECK", LDnode.ByPassCheck ? "TRUE" : "FALSE");
                            param.Add("@To_BYPASS_CHECK", NodeManagement.Get(wafer.Destination).ByPassCheck ? "TRUE" : "FALSE");

                            if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_CARRY, param).Promise())
                            {
                                procNode.ProcStatus = Node.ProcedureStatus.Stop;
                            }
                            else
                            {
                                procNode.ProcStatus = Node.ProcedureStatus.CheckPresence;
                            }
                        }
                        else
                        {
                            procNode.ProcStatus = Node.ProcedureStatus.Idle;
                        }
                    }
                    else
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Idle;
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.Stop:
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    IsProcStop = true;
                    break;
                default:
                    throw new NotSupportedException();

            }

            SpinWait.SpinUntil(() => false, 50);
        }
    }
}
