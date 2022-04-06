using log4net;
using System;
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
    class AlignerProc : SubProc
    {
        public AlignerProc(Node node) : base(node)
        {
            logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
        ~AlignerProc()
        {
            IsProcExit = true;
        }

        public override void Run(Node procNode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            Job WaferOnAligner = null;

            switch (procNode.ProcStatus)
            {
                case Node.ProcedureStatus.Idle:
                    procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.GetPresence:
                    param.Add("@Target", procNode.Name);
                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ALIGNER_GET_CLAMP, param).Promise())
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

                    if (procNode.R_Presence)
                    {
                        WaferOnAligner = JobManagement.Get("ALIGNER01", "1");

                        if(WaferOnAligner != null)
                        {
                            if (WaferOnAligner.AlignerFlag)
                            {
                                if (WaferOnAligner.OCRFlag)
                                {
                                    procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                                }
                                else
                                {
                                    procNode.ProcStatus = Node.ProcedureStatus.OCR;
                                }
                            }
                            else
                            {
                                procNode.ProcStatus = Node.ProcedureStatus.Align;
                            }
                        }
                        else
                        {
                            SpinWait.SpinUntil(() => false, 300);

                            procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                        }
                    }
                    else
                    {
                        SpinWait.SpinUntil(() => false, 300);

                        procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.Align:
                    param.Add("@Target", procNode.Name);
                    param.Add("@Value", procNode.AlignDegree);

                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ALIGNER_ALIGN, param).Promise())
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    else
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.CheckPresence;
                    }
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.OCR:
                    param.Add("@Target", procNode.Associated_Node);

                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.OCR_READ, param).Promise())
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    else
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.CheckPresence;
                    }
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.Stop:

                    IsProcStop = true;
                    break;
                


                default:
                    throw new NotSupportedException();
            }

            SpinWait.SpinUntil(() => false, 50);
        }
    }
}
