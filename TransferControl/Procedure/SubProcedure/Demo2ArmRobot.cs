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
    class Demo2ArmRobot : SubProc
    {
        private string Arm;
        private string Slot;
        private string Position;

        public Demo2ArmRobot(Node node, IProcReport procreport) : base(node, procreport)
        {
            logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
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
                        procNode.ProcStatus = Node.ProcedureStatus.AlarmStop;
                    }
                    else
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.CheckPresence;
                    }
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);

                    break;

                case Node.ProcedureStatus.CheckPresence:

                    if(Recipe.CurrentRecipe.RunMode.ToUpper().Equals("SEMIAUTO"))
                    {
                        var Loaders = from Loader in NodeManagement.GetLoadPortList()
                                      where Loader.IsMapping && !Loader.Foup_Lock
                                      select Loader;

                        if(Loaders.Count() == 0)
                        {
                            TaskFlowManagement.Excute(TaskFlowManagement.Command.NOTIFY_SEMIAUTO_FINISHED, param);
                            procNode.ProcStatus = Node.ProcedureStatus.Stop;
                            logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                            break;
                        }
                    }

                    if (procNode.R_Presence && procNode.L_Presence)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.FullWaferOnRLArm;
                    }
                    else if (!procNode.R_Presence && !procNode.L_Presence)
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.NoWaferOnRLArm;
                    }
                    else if (procNode.R_Presence && !procNode.L_Presence)
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
                    if (WaferOnAligner != null)
                    {
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
                            if (procNode.RobotArmType.Equals(Robot_ArmType.USE_RLARM) && !StartToStop)
                            {
                                List<Node> LD = new List<Node>();
                                foreach (Node loader in NodeManagement.GetLoadPortList())
                                {
                                    if (loader.Mode != null)
                                    {
                                        if (Recipe.CurrentRecipe.RunMode.ToUpper().Equals("AUTO") ? loader.Mode.Equals("LD") : true && 
                                            loader.IsMapping && 
                                            loader.AssignWaferFinished)
                                        {
                                            LD.Add(loader);
                                        }
                                    }

                                }

                                if (LD.Count() != 0)
                                {
                                    foreach(Node LDnode in LD.ToList())
                                    {
                                        var WaferList = from wafer in JobManagement.GetJobList()
                                                        where wafer.IsAssigned &&
                                                            !wafer.Destination.Equals("") &&
                                                            (!wafer.Position.Equals(wafer.Destination.ToUpper()) ||
                                                            !wafer.Slot.Equals(wafer.DestinationSlot.ToUpper())) &&
                                                            wafer.Position.ToUpper().Equals(LDnode.Name.ToUpper())
                                                        select wafer;

                                        if (WaferList.Count() != 0)
                                        {
                                            if (Recipe.CurrentRecipe.get_slot_order.Equals("BOTTOM_UP"))
                                            {
                                                WaferList = WaferList.OrderBy(x => Convert.ToInt16(x.Slot));
                                            }
                                            else
                                            {
                                                WaferList = WaferList.OrderByDescending(x => Convert.ToInt16(x.Slot));
                                            }

                                            Job wafer = WaferList.First();

                                            Arm = "1";
                                            Position = LDnode.Name;
                                            Slot = wafer.Slot;

                                            procNode.ProcStatus = Node.ProcedureStatus.Get;
                                            break;
                                        }
                                    }

                                    if (!procNode.ProcStatus.Equals(Node.ProcedureStatus.Get))
                                        procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
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
                                {
                                    if (Recipe.CurrentRecipe.RunMode.ToUpper().Equals("AUTO") ? loader.Mode.Equals("LD") : true
                                        && loader.IsMapping
                                        && loader.AssignWaferFinished)
                                    {
                                        LD.Add(loader);
                                    }
                                }
                            }

                            if (LD.Count() != 0)
                            {
                                foreach (Node LDnode in LD.ToList().OrderBy(x => x.Name))
                                {
                                    var WaferList = from wafer in JobManagement.GetJobList()
                                                    where wafer.IsAssigned &&
                                                        !wafer.Destination.Equals("") &&
                                                        (!wafer.Position.Equals(wafer.Destination.ToUpper()) ||
                                                        !wafer.Slot.Equals(wafer.DestinationSlot.ToUpper())) &&
                                                        wafer.Position.ToUpper().Equals(LDnode.Name.ToUpper())
                                                    select wafer;

                                    if (WaferList.Count() != 0)
                                    {
                                        if (Recipe.CurrentRecipe.get_slot_order.Equals("BOTTOM_UP"))
                                        {
                                            WaferList = WaferList.OrderBy(x => Convert.ToInt16(x.Slot));
                                        }
                                        else
                                        {
                                            WaferList = WaferList.OrderByDescending(x => Convert.ToInt16(x.Slot));
                                        }

                                        Job wafer = WaferList.First();

                                        bool UseDoubleArm = false;
                                        Job wafer2 = JobManagement.Get(wafer.Position, 
                                            (Recipe.CurrentRecipe.get_slot_order.Equals("BOTTOM_UP") ? Convert.ToInt16(wafer.Slot) + 1 : Convert.ToInt16(wafer.Slot) - 1).ToString());

                                        if (procNode.RobotArmType.Equals(Robot_ArmType.USE_RLARM) && wafer2 != null)
                                        {
                                            if (wafer2.IsAssigned && !wafer2.Destination.Equals("") &&
                                                (!wafer2.Position.ToUpper().Equals(wafer2.Destination.ToUpper()) ||
                                                !wafer2.Slot.ToUpper().Equals(wafer2.DestinationSlot.ToUpper()) )&&
                                                wafer.Destination.ToUpper().Equals(wafer2.Destination.ToUpper()) &&
                                                 wafer2.Position.ToUpper().Equals(LDnode.Name.ToUpper()) 
                                                 )                                             
                                            {
                                                if (Recipe.CurrentRecipe.put_slot_order.Equals(Recipe.CurrentRecipe.get_slot_order))
                                                {
                                                    if(Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN"))
                                                    {
                                                        if (Convert.ToInt16(wafer.DestinationSlot) == Convert.ToInt16(wafer2.DestinationSlot) + 1)
                                                            UseDoubleArm = true;
                                                    }
                                                    else
                                                    {
                                                        if (Convert.ToInt16(wafer.DestinationSlot) == Convert.ToInt16(wafer2.DestinationSlot) - 1)
                                                            UseDoubleArm = true;
                                                    }
                                                }
                                                else
                                                {
                                                    if (Recipe.CurrentRecipe.put_slot_order.Equals("TOP_DOWN"))
                                                    {
                                                        if (Convert.ToInt16(wafer.DestinationSlot) == Convert.ToInt16(wafer2.DestinationSlot) - 1)
                                                            UseDoubleArm = true;
                                                    }
                                                    else
                                                    {
                                                        if (Convert.ToInt16(wafer.DestinationSlot) == Convert.ToInt16(wafer2.DestinationSlot) + 1)
                                                            UseDoubleArm = true;
                                                    }
                                                }
                                            }

                                        }

                                        Position = LDnode.Name;
                                        if (UseDoubleArm)
                                        {
                                            Arm = "3";
                                            Slot = Recipe.CurrentRecipe.get_slot_order.Equals("BOTTOM_UP") ? wafer2.Slot : wafer.Slot;
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
                                        break;
                                    }
                                }

                                if(!procNode.ProcStatus.Equals(Node.ProcedureStatus.Get))
                                    procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                            }
                            else
                            {
                                procNode.ProcStatus = Node.ProcedureStatus.GetPresence;
                            }
                        }

                    }

                    //減少資源使用
                    if (procNode.ProcStatus.Equals(Node.ProcedureStatus.GetPresence))
                        SpinWait.SpinUntil(() => false, 1000);

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
                                procNode.ProcStatus = Node.ProcedureStatus.AlarmStop;
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
                        WaferOnRArm = JobManagement.Get(procNode.Name + "_R", "1");
                        WaferOnLArm = JobManagement.Get(procNode.Name + "_L", "1");

                        if(IsNodeEnabled("ALIGNER01") && !(WaferOnRArm.OCRFlag && WaferOnLArm.OCRFlag))
                        {
                            Position = "ALIGNER01";
                            Slot = "1";
                            if (!WaferOnRArm.OCRFlag && !WaferOnLArm.OCRFlag)
                            {
                                Arm = Convert.ToInt16(WaferOnRArm.DestinationSlot) > Convert.ToInt16(WaferOnLArm.DestinationSlot) ? "1" : "2";
                            }
                            else
                            {
                                Arm = !WaferOnRArm.OCRFlag ? "1" : "2";
                            }
                        }
                        else
                        {

                            //if (procNode.RobotArmType.Equals(Robot_ArmType.USE_RLARM) && !Recipe.CurrentRecipe.RunMode.ToUpper().Equals("SEMIAUTO"))
                            //{
                            //    if (Convert.ToInt16(WaferOnLArm.DestinationSlot) == Convert.ToInt16(WaferOnRArm.DestinationSlot) + 1)
                            //    {
                            //        WaferOnRArm.DestinationSlot = WaferOnLArm.DestinationSlot;
                            //        WaferOnLArm.DestinationSlot = (Convert.ToInt16(WaferOnRArm.DestinationSlot) - 1).ToString();

                            //        logger.Debug("Re-Assign Wafer : to " + WaferOnRArm.Destination + " Slot " + WaferOnRArm.DestinationSlot);

                            //        logger.Debug("Re-Assign Wafer : to " + WaferOnLArm.Destination + " Slot " + WaferOnLArm.DestinationSlot);

                            //    }
                            //}

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

                    Report.MessageReport("Get wafer form : " + Position + " Slot " + Slot + " by Arm " + Arm);
                    //TimerManagement.Record();

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

                    Report.MessageReport("Put wafer to : " + Position + " Slot " + Slot + " by Arm " + Arm);
                    //TimerManagement.Record();

                    if (!TaskFlowManagement.Excute(TaskFlowManagement.Command.ROBOT_PUT, param).Promise())
                    {
                        procNode.ProcStatus = Node.ProcedureStatus.Stop;
                    }
                    else
                    {
                        if(Position.ToUpper().Contains("LOADPORT"))
                            TimerManagement.Add(Arm.Equals("3") ? 2 : 1);

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

                case Node.ProcedureStatus.AlarmStop:
                case Node.ProcedureStatus.Stop:
                    logger.Debug("Node : " + procNode.Name + ", go to " + procNode.ProcStatus);
                    IsProcStop = true;
                    StartToStop = false;

                    procNode.InitialComplete = false;
                    procNode.OrgSearchComplete = false;
                    break;
                default:
                    throw new NotSupportedException();

            }

            SpinWait.SpinUntil(() => false, 50);
        }
        public override void Stop()
        {
            IsProcPause = false;
            StartToStop = true;
        }
    }
}
