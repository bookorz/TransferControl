﻿using System;
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
    class Demo2ArmRobot : SubProc
    {
        private string Arm;
        private string Slot;
        private string Position;
        private bool StartToStop = false;

        public Demo2ArmRobot(Node node) : base(node)
        {
            logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            node.RobotArmType = Robot_ArmType.USE_RLARM;

            Arm = "";
            Slot = "";
            Position = "";

        }
        ~Demo2ArmRobot()
        {
            IsProcExit = true;
        }

        public override void Run(Node procNode)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            Job WaferOnAligner = null;
            Job WaferOnRArm = null;
            Job WaferOnLArm = null;
            switch (procNode.ProcStatus)
            {
                case Node.ProcedureStatus.Idle:
                    procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.GetPresence:
                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.GET_CLAMP, param).Promise())
                    {
                        //Error Stop 
                        procNode.ProcStatus = Node.ProcedureStatus.ErrorStop;
                    }
                    else
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.CheckPresence;
                    }
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);

                    break;

                case Node.ProcedureStatus.CheckPresence:

                    if(procNode.R_Presence && procNode.L_Presence)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.FullWaferOnRLArm;
                    }
                    else if(!procNode.R_Presence && !procNode.L_Presence)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.NoWaferOnRLArm;
                    }
                    else if(procNode.R_Presence && !procNode.L_Presence)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.OnlyOneWaferOnRArm;
                    }
                    else if (!procNode.R_Presence && procNode.L_Presence)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.OnlyOneWaferOnLArm;
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.NoWaferOnRLArm:
                    WaferOnAligner = JobManagement.Get("ALIGNER01", "1");
                    logger.Debug("1");
                    if (WaferOnAligner != null)
                    {
                        logger.Debug("2");
                        if (WaferOnAligner.OCRFlag)  //OCR Finish, get wafer from aligner
                        {
                            logger.Debug("3");
                            switch (procNode.RobotArmType)
                            {
                                case Robot_ArmType.USE_RARM:
                                case Robot_ArmType.USE_RLARM:
                                    Arm = "1";
                                    break;

                                case Robot_ArmType.USE_LARM:
                                    Arm = "2";
                                    break;
                            }

                            Position = "ALIGNER01";
                            Slot = "1";

                            procNode.ProcStatus = Node.ProcedureStatus.Get;
                        }
                        else
                        {
                            logger.Debug("4");
                            if (procNode.RobotArmType.Equals(Robot_ArmType.USE_RLARM) && !StartToStop)
                            {
                                logger.Debug("5");
                                List<Node> LD = new List<Node>();
                                foreach (Node loader in NodeManagement.GetLoadPortList())
                                {
                                    if (loader.Mode != null)
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
                                                        wafer.Position.ToUpper().Equals(LDnode.Name.ToUpper())
                                                    select wafer;

                                    if (WaferList.Count() != 0)
                                    {
                                        WaferList = WaferList.OrderBy(x => Convert.ToInt16(x.Slot));
                                        Job wafer = WaferList.First();

                                        Arm = "1";
                                        Position = LDnode.Name;
                                        Slot = wafer.Slot;

                                        procNode.ProcStatus = Node.ProcedureStatus.Get;
                                    }
                                    else
                                    {
                                        procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                                    }
                                }
                                else
                                {
                                    procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                                }
                            }
                            else
                            {
                                procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                            }
                        }
                    }
                    else
                    {
                        //預備停止流逞
                        if (StartToStop)
                        {
                            //強制停機
                            procNode.ProcStatus = Node.ProcedureStatus.Stop;
                        }
                        else
                        {
                            List<Node> LD = new List<Node>();
                            foreach (Node loader in NodeManagement.GetLoadPortList())
                            {
                                if (loader.Mode != null)
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

                                    bool UseDoubleArm = false;
                                    Job wafer2 = JobManagement.Get(wafer.Position, (Convert.ToInt16(wafer.Slot) + 1).ToString());

                                    if (procNode.RobotArmType.Equals(Robot_ArmType.USE_RLARM) && wafer2 != null)
                                    {
                                        if (wafer2.IsAssigned && !wafer2.Destination.Equals("") &&
                                            !wafer2.Position.ToUpper().Equals(wafer2.Destination.ToUpper()))
                                            UseDoubleArm = true;
                                    }

                                    Position = LDnode.Name;
                                    if (UseDoubleArm)
                                    {
                                        Arm = "3";
                                        Slot = wafer2.Slot;
                                    }
                                    else
                                    {
                                        switch (procNode.RobotArmType)
                                        {
                                            case Robot_ArmType.USE_RARM:
                                            case Robot_ArmType.USE_RLARM:
                                                Arm = "1";
                                                break;

                                            case Robot_ArmType.USE_LARM:
                                                Arm = "2";
                                                break;
                                        }
                                        Slot = wafer.Slot;
                                    }

                                    procNode.ProcStatus = Node.ProcedureStatus.Get;
                                }
                                else
                                {
                                    procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                                }
                            }
                            else
                            {
                                procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                            }
                        }

                    }
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.OnlyOneWaferOnRArm:
                case Node.ProcedureStatus.OnlyOneWaferOnLArm:
                    WaferOnAligner = JobManagement.Get("ALIGNER01", "1");
                    if(WaferOnAligner != null)
                    {
                        if(WaferOnAligner.OCRFlag) //get wafer by empty arm from aligner
                        {
                            Arm = procNode.R_Presence ? "2" : "1";
                            Slot = "1";
                            Position = "ALIGNER01";

                            procNode.ProcStatus = Node.ProcedureStatus.Get;
                        }
                        else   
                        {
                            Arm = procNode.R_Presence ? "1" : "2";

                            Job WaferOnArm = JobManagement.Get(Arm.Equals("1") ? "ROBOT01_R" : "ROBOT01_L", "1");
                            if (WaferOnArm != null)
                            {
                                if(WaferOnArm.OCRFlag)  //put wafer to unloader 
                                {
                                    Position = WaferOnArm.Destination;
                                    Slot = WaferOnArm.DestinationSlot;

                                    procNode.ProcStatus = Node.ProcedureStatus.Put;
                                }
                                else
                                {
                                    procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                                }
                            }
                            else
                            {
                                //Error
                                procNode.ProcStatus = Node.ProcedureStatus.ErrorStop;
                            }
                        }
                    }
                    else
                    {
                        Arm = procNode.R_Presence ? "1" : "2";
                        Job WaferOnArm = JobManagement.Get(Arm.Equals("1") ? "ROBOT01_R" : "ROBOT01_L", "1");
                        if (WaferOnArm != null)
                        {
                            if(!IsNodeEnabled("ALIGNER01") || WaferOnArm.OCRFlag)   //put to unloader 
                            {
                                Position = WaferOnArm.Destination;
                                Slot = WaferOnArm.DestinationSlot;
                            }
                            else    //put wafer to aligner   
                            {
                                Position = "ALIGNER01";
                                Slot = "1";
                            }

                            procNode.ProcStatus = Node.ProcedureStatus.Put;
                        }
                        else
                        {
                            //Error
                            procNode.ProcStatus = Node.ProcedureStatus.Stop;
                        }
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.FullWaferOnRLArm:
                    WaferOnAligner = JobManagement.Get("ALIGNER01", "1");
                    if (WaferOnAligner != null)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    else
                    {
                        logger.Debug("a1");
                        WaferOnRArm = JobManagement.Get(procNode.Name + "_R", "1");
                        WaferOnLArm = JobManagement.Get(procNode.Name + "_L", "1");

                        if(!IsNodeEnabled("ALIGNER01") || !(WaferOnRArm.OCRFlag && WaferOnLArm.OCRFlag))
                        {
                            logger.Debug("a2");
                            Position = "ALIGNER01";
                            Slot = "1";
                            if (!WaferOnRArm.OCRFlag && !WaferOnLArm.OCRFlag)
                            {
                                logger.Debug("a3");
                                Arm = Convert.ToInt16(WaferOnRArm.DestinationSlot) > Convert.ToInt16(WaferOnLArm.DestinationSlot) ? "1" : "2";
                            }
                            else
                            {
                                logger.Debug("a4");
                                Arm = !WaferOnRArm.OCRFlag ? "1" : "2";
                            }
                            logger.Debug(Arm);
                        }
                        else
                        {
                            if (WaferOnRArm.Destination.Equals(WaferOnLArm.Destination) &&
                                Convert.ToInt16(WaferOnRArm.DestinationSlot) == Convert.ToInt16(WaferOnLArm.DestinationSlot) + 1)
                            {
                                Arm = "3";
                                Slot = WaferOnRArm.DestinationSlot;
                                Position = WaferOnRArm.Destination;
                            }
                            else
                            {
                                if (Convert.ToInt16(WaferOnRArm.DestinationSlot) > Convert.ToInt16(WaferOnLArm.DestinationSlot))
                                {
                                    Arm = "1";
                                    Slot = WaferOnRArm.DestinationSlot;
                                    Position = WaferOnRArm.Destination;
                                }
                                else
                                {
                                    Arm = "2";
                                    Slot = WaferOnLArm.DestinationSlot;
                                    Position = WaferOnLArm.Destination;
                                }
                            }
                        }
                        procNode.ProcStatus = Node.ProcedureStatus.Put;
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.Get:

                    param.Add("@Target", procNode.Name);
                    param.Add("@Arm", Arm);
                    param.Add("@Position", Position);
                    param.Add("@Slot", Slot);
                    param.Add("@BYPASS_CHECK", NodeManagement.Get(Position).ByPassCheck ? "TRUE" : "FALSE");
                    param.Add("@IsTransCommand", "FALSE");

                    logger.Debug("Get wafer form : " + Position + " Slot " + Slot + " by Arm " + Arm);

                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_GET, param).Promise())
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    else
                    {
                        if(SystemConfig.Get().OfflineMode)
                        {
                            if(Position.ToUpper().Equals("ALIGNER01"))
                            {
                                Node al = NodeManagement.Get("ALIGNER01");
                                al.R_Presence = false;
                                al.R_Vacuum_Solenoid = "0";
                            }

                            switch (Arm)
                            {
                                case "1":
                                    procNode.R_Presence = true;
                                    procNode.R_Vacuum_Solenoid = "1";
                                    break;
                                case "2":
                                    procNode.L_Presence = true;
                                    procNode.L_Vacuum_Solenoid = "1";
                                    break;
                                case "3":
                                    procNode.R_Presence = true;
                                    procNode.L_Presence = true;
                                    procNode.R_Vacuum_Solenoid = "1";
                                    procNode.L_Vacuum_Solenoid = "1";
                                    break;
                            }
                        }

                        procNode.ProcStatus = Node.ProcedureStatus.CheckPresence;
                    }


                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.Put:

                    param.Add("@Target", procNode.Name);
                    param.Add("@Arm", Arm);
                    param.Add("@Position", Position);
                    param.Add("@Slot", Slot);
                    param.Add("@BYPASS_CHECK", NodeManagement.Get(Position).ByPassCheck ? "TRUE" : "FALSE");
                    param.Add("@IsTransCommand", "FALSE");

                    logger.Debug("Put wafer to : " + Position + " Slot " + Slot + " by Arm " + Arm);

                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_PUT, param).Promise())
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    else
                    {
                        if (SystemConfig.Get().OfflineMode)
                        {
                            if (Position.ToUpper().Equals("ALIGNER01"))
                            {
                                Node al = NodeManagement.Get("ALIGNER01");
                                al.R_Presence = true;
                                al.R_Vacuum_Solenoid = "1";
                            }

                            switch (Arm)
                            {
                                case "1":
                                    procNode.R_Presence = false;
                                    procNode.R_Vacuum_Solenoid = "0";
                                    break;
                                case "2":
                                    procNode.L_Presence = false;
                                    procNode.L_Vacuum_Solenoid = "0";
                                    break;
                                case "3":
                                    procNode.R_Presence = false;
                                    procNode.L_Presence = false;
                                    procNode.R_Vacuum_Solenoid = "0";
                                    procNode.L_Vacuum_Solenoid = "0";
                                    break;
                            }
                        }
                        procNode.ProcStatus = Node.ProcedureStatus.CheckPresence;
                    }

                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    break;

                case Node.ProcedureStatus.ErrorStop:
                case Node.ProcedureStatus.Stop:
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    IsProcStop = true;
                    StartToStop = false;
                    break;
                default:
                    throw new NotSupportedException();

            }

            SpinWait.SpinUntil(() => false, 50);
        }
        public override void Stop()
        {
            //IsProcStop = true;
            IsProcPause = false;

            StartToStop = true;
        }
    }
}
