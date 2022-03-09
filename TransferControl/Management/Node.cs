using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TransferControl.Comm;
using TransferControl.CommandConvert;
using TransferControl.Config;
using TransferControl.Controller;
using TransferControl.Engine;

namespace TransferControl.Management
{
    public enum E84_Mode { UNKNOW = -1, MANUAL, AUTO }
    public enum Robot_ArmType { UNKNOW = -1,USE_RARM, USE_LARM, USE_RLARM}
    public class Node
    {
        ILog logger = LogManager.GetLogger(typeof(Node));

        public class ProcedureStatus
        {
            public const string Idle = "Idle";
            public const string Stop = "Stop";
            public const string ErrorStop = "ErrorStop";
            public const string CheckPresence = "CheckPresence";
            public const string GetPresence = "GetPresence";
            public const string LoadAndMapping = "LoadAndMapping";
            public const string TransferWafer = "TransferWafer";
            public const string CheckType = "CheckType";
            public const string AssignWafer = "AssignWafer";
            public const string IsLoaderEmpty = "IsLoaderEmpty";
            public const string IsUnloaderFull = "IsUnloaderFull";
            public const string Unload = "Unload";
            public const string WaitUnloadCompleted = "WaitUnloadCompleted";
            public const string OnlyOneWaferOnRArm = "OnlyOneWaferOnRArm";
            public const string OnlyOneWaferOnLArm = "OnlyOneWaferOnLArm";
            public const string NoWaferOnRLArm = "NoWaferOnRLArm";
            public const string FullWaferOnRLArm = "FullWaferOnRLArm";
            public const string Get = "Get";
            public const string Put = "Put";
        }

        public bool AssignWaferFinished { get; set; } 

        public int AckTimeOut { get; set; }
        public int MotionTimeOut { get; set; }
        public string ConnectionStatus { get; set; }
        /// <summary>
        /// 名稱
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 目前使用的Controoler名稱
        /// </summary>
        public string Controller { get; set; }
        /// <summary>
        /// Address Number
        /// </summary>
        public string AdrNo { get; set; }
        /// <summary>
        /// Node 類型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 廠牌
        /// </summary>
        public string Vendor { get; set; }


        /// <summary>
        /// Robot專用，目前手臂的位置
        /// </summary>
        public string CurrentPosition { get; set; }
        /// <summary>
        /// 啟用或停用此Node
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// LoadPort用於標記True為目前不能取放片，其他裝置用於標記True為正在執行命令中
        /// </summary>
        //public bool InterLock { get; set; }
        /// <summary>
        /// 目前機況
        /// </summary>
        public string State { get; set; }
        /// <summary>
        /// 上一次的機況
        /// </summary>
        public bool HasAlarm { get; set; }
        public string LastError { get; set; }
        /// <summary>
        /// LoadPort專用，標記LD/UD/LU
        /// </summary>
        public string Mode { get; set; }
        /// <summary>
        /// 是否需要Initial
        /// </summary>
        public bool InitialComplete { get; set; }

        public bool OrgSearchComplete { get; set; }
        /// <summary>
        /// LoadPort專用，Mapping完成
        /// </summary>
        public bool IsMapping { get; set; }
        //public bool IsCheckReMappingResult = true;

        public bool MappingHasError = false;

        /// <summary>
        /// 標記為虛擬裝置
        /// </summary>
        public bool ByPass { get; set; }
        /// <summary>
        /// LoadPort專用，Foup Load的時間
        /// </summary>
        public DateTime LoadTime { get; set; }
        /// <summary>
        /// LoadPort專用，Foup 存放 Wafer 容量
        /// </summary>
        public int Foup_Capacity { get; set; }

        public string CarrierType { get; set; }

        public bool Busy { get; set; }

        public bool OPACCESS { get; set; }

        public string LastFinMethod { get; set; }

        public string WaferSize { get; set; }

        public bool DoubleArmActive { get; set; }

        public bool RArmActive { get; set; }

        public bool LArmActive { get; set; }
        public bool IsWaferHold { get; set; }

        public string ErrorMsg { get; set; }

        public string MappingResult { get; set; }

        public bool R_Presence { get; set; }

        public bool L_Presence { get; set; }

        public string R_Hold_Status { get; set; }

        public string L_Hold_Status { get; set; }

        public string Y_Axis_Position { get; set; }
        public string Z_Axis_Position { get; set; }

        public string Door_Position { get; set; }

        public bool Foup_Placement { get; set; }

        public bool Foup_Presence { get; set; }
        public bool Latch_Open { get; set; }

        public bool Access_SW { get; set; }

        public bool Foup_Lock { get; set; }

        public string Load_LED { get; set; }

        public string UnLoad_LED { get; set; }

        public string AccessSW_LED { get; set; }

        public bool RArmClamp { get; set; }

        public bool RArmUnClamp { get; set; }

        public bool LArmClamp { get; set; }

        public bool LArmUnClamp { get; set; }

        public bool IsDock { get; set; }
        public bool IsLoad { get; set; }

        public bool IsExcuting { get; set; }
        public bool IsMoving { get; set; }
        public bool IsPause { get; set; }
        public string R_Flip_Degree { get; set; }

        public string L_Flip_Degree { get; set; }

        public string CurrentSlotPosition { get; set; }

        public string CurrentPoint { get; set; }

        public string X_Position { get; set; }

        public string R_Position { get; set; }

        public string L_Position { get; set; }

        public string Servo { get; set; }

        public string Associated_Node { get; set; }

        public string LockOn { get; set; }

        public bool Connected { get; set; }

        public string Speed { get; set; }

        public bool ByPassCheck { get; set; }

        public string FoupID { get; set; }

        private bool isPool { get; set; }

        private bool PoolThread { get; set; }

        public int PoolInterval { get; set; }

        public string PoolTask { get; set; }

        public bool OCRSuccess { get; set; }
        public string CurrentStatus { get; set; }
        public string R_Vacuum_Solenoid { get; set; }
        public string L_Vacuum_Solenoid { get; set; }
        public string ConfigList { get; set; }
        public string OCR_ID { get; set; }
        public string OCR_Score { get; set; }
        public bool OCR_Pass { get; set; }
        public bool Home_Position { get; set; }
        public bool ManaulControl { get; set; }
        public Dictionary<string, string> Status { get; set; }
        public Dictionary<string, byte[]> IO { get; set; }

        public bool DataReady { get; set; }
        public Carrier Carrier { get; set; }
        public int RobotGetState { get; set; }
        public int RobotPutState { get; set; }
        public string ArmExtend { get; set; }
        public string MappingDataSnapshot { get; set; }
        public bool WaferProtrusionSensor { get; set; }
        public object ExcuteLock = new object();

        /// <summary>
        /// SMIF 硬體總類
        /// -1 = Unknow
        /// 0 = Standard
        /// 1 = Extend
        /// </summary>
        public int HardwareType { get; set; }
        /// <summary>
        /// -1 = Unknow 
        /// 0 = Not Support
        /// 1 = Support Pusher Function
        /// </summary>
        public int PusherSupport { get; set; }
        /// <summary>
        /// 0 = None
        /// 1 = Pod
        /// 2 = Open Cassette
        /// 9 = Unknow
        /// </summary>
        public int Workpiece { get; set; }
        /// <summary>
        /// for Sanwa-SMIF
        /// 0 = Auto, 1 = Manual
        /// </summary>
        public int ManualMode { get; set; }
        /// <summary>
        /// for Sanwa-SMIF
        ///0 = Indexer is not ready(No ORG Serch 、 Alarm)
        ///1 = Indexer ready for new command
        /// </summary>
        public int Ready { get; set; }
        /// <summary>
        ///for Sanwa-SMIF
        ///0 = Power up (Boot /Reset
        ///1 = Homing Calibration(ORG__)
        ///2 = Close/Auto Home(HOME_)
        ///3 = Open/Reach Stage(STAGE)
        ///4 = Index(GOTO_/TWEEK)
        ///5 = Map(MAP__)
        ///6 = W afer search(MSLOT)
        ///7 = Slot search(SLOT_)
        ///8 = Unknown(STEST
        /// </summary>
        public string LFUNC { get; set; }
        /// <summary>
        /// Sanwa SMIF POS(Position)
        ///0 = Unknown
        ///1 = Home
        ///2 = Lift
        ///3 = Stage
        ///4 = Z Axis Li mit+
        ///5 = Other
        /// </summary>
        public string Position { get; set; }
        /// <summary>
        /// Sanwa SMIF
        /// 0~25 (0 : Ignore)
        /// </summary>
        public int SlotNumber { get; set; }
        /// <summary>
        /// Sanwa SMIF 
        /// ELUD (Elevator Up/Down)
        /// 0 = Down
        /// 1 = Up
        /// </summary>
        public bool ElevatorUp { get; set; }
        /// <summary>
        /// LPS(Lift Present Status)
        /// 0 = Absence
        /// 1 = Present
        /// </summary>
        public bool LiftPresent { get; set; }
        public string StatusRawData { get; set; }
        public string NickName { get; set; }
        public bool NeedReadFoupID { get; set; }
        public IController GetController()
        {
            return ControllerManagement.Get(Controller);
        }
        public Dictionary<string, bool> E84IOStatus { get; set; }
        public E84_Mode E84Mode;
        public Robot_ArmType RobotArmType;
        public string TransReqMode;

        public string AlignDegree{ get; set; }

        public string ProcStatus { get; set; }

        public void InitialObject()
        {
            R_Vacuum_Solenoid = "0";
            L_Vacuum_Solenoid = "0";
            DataReady = false;
            ArmExtend = "";
            RobotGetState = 0;
            RobotPutState = 0;
            Speed = "";
            ConfigList = "";

            //ByPassCheck = false;
            Connected = false;
            OPACCESS = false;

            MappingResult = "";

            CurrentStatus = "";
            MappingDataSnapshot = "";

            NeedReadFoupID = false;

            R_Flip_Degree = "0";
            L_Flip_Degree = "0";
            CurrentPosition = "";
            IsMoving = false;
            FoupID = "";
            Status = new Dictionary<string, string>();
            IO = new Dictionary<string, byte[]>();
            E84IOStatus = new Dictionary<string, bool>
            {
                { "GO", false },
                { "SELECT", false },
                { "MODE", false },
                { "AUTO", false },
                { "MANUAL", false },
                { "ERROR", false },
                { "VALID", false },
                { "CS_0", false },
                { "CS_1", false },
                { "AM_AVBL", false },
                { "TR_REQ", false },
                { "BUSY", false },
                { "COMPT", false },
                { "CONT", false },
                { "L_REQ", false },
                { "U_REQ", false },
                { "VA", false },
                { "READY", false },
                { "VS_0", false },
                { "VS_1", false },
                { "HO_AVBL", false },
                { "ES", false },
                { "CLAMP", false },
                { "EMO", false },
                { "ALARM", false },
                { "LC", false },
            };

            E84Mode = E84_Mode.MANUAL;
            TransReqMode = "Stop";

            State = "UNORG";

            Home_Position = false;

            //if (Type.Equals("LOADPORT"))
            //{
            //    State = "Ready To Load";
            //}
            LockOn = "";
            HasAlarm = false;
            LastFinMethod = "";
            CurrentPoint = "";
            X_Position = "";
            R_Position = "";
            L_Position = "";
            Busy = false;
            //InterLock = false;
            OCRSuccess = false;
            InitialComplete = false;
            OrgSearchComplete = false;
            IsWaferHold = false;
            IsLoad = false;
            IsExcuting = false;
            IsPause = false;
            CurrentSlotPosition = "??";
            ErrorMsg = "";
            //Enable = true;
            LoadTime = new DateTime();

            MappingResult = "";

            R_Presence = false;

            L_Presence = false;

            R_Hold_Status = "";

            L_Hold_Status = "";

            Y_Axis_Position = "";

            Door_Position = "";

            Foup_Placement = false;

            Foup_Presence = false;

            Access_SW = false;

            Foup_Lock = false;
            Latch_Open = false;

            Load_LED = "";


            UnLoad_LED = "";

            AccessSW_LED = "";

            RArmClamp = false;
            RArmUnClamp = false;
            LArmClamp = false;
            LArmUnClamp = false;

            IsDock = false;
            isPool = false;
            PoolThread = false;
            PoolInterval = 50;
            Speed = "100";

            HardwareType = -1;
            PusherSupport = -1;
            Workpiece = -9;

            ManualMode = 0;
            Ready = 1;
            LFUNC = "0";
            Position = "0";
            SlotNumber = 25;
            ElevatorUp = true;
            LiftPresent = true;

            //20210628 Pingchung TDK USE
            StatusRawData = "";

            ProcStatus = ProcedureStatus.Idle;

            Foup_Capacity = 25;

            RobotArmType = Robot_ArmType.USE_RARM;
        }
       

        public byte[] GetIO(string Area)
        {

            return this.IO[Area];

        }
        public void SetIO(string Area, byte[] Val)
        {
            byte[] ResultCopy = new byte[512];
            Val.CopyTo(ResultCopy, 0);

            if (this.IO.ContainsKey(Area))
            {
                this.IO[Area] = ResultCopy;
            }
            else
            {
                this.IO.Add(Area, ResultCopy);
            }

        }
        public void SetIO(string Area, int Pos, byte Val)
        {

            this.GetIO(Area)[Pos] = Val;


        }
        /// <summary>
        /// 傳送命令
        /// </summary>
        /// <param name="txn"></param>
        /// <param name="Force"></param>
        /// <returns></returns>
        public bool SendCommand(Transaction txn)
        {
            lock (ExcuteLock)
            {
                if (IsExcuting)
                {
                    logger.Debug("SendCommand(Transaction txn)" + "if (IsExcuting)");
                    return false;
                }
                else
                {
                    IsExcuting = true;
                }
            }


            try
            {
                txn.Slot = int.Parse(txn.Slot).ToString();
                txn.Slot2 = int.Parse(txn.Slot2).ToString();
            }
            catch
            {

            }

            bool result = false;
            try
            {
                //if (this.ByPass)
                //{

                //    logger.Debug("Command cancel,Cause " + this.Name + " in by pass mode.");
                //    this.IsExcuting = false;
                //    return true;

                //}

                IController Ctrl = this.GetController();
                if (this.Vendor.ToUpper().Equals("KAWASAKI"))
                {

                    txn.Seq = Ctrl.GetNextSeq();

                }
                else
                {
                    txn.Seq = "";
                }
                txn.AdrNo = AdrNo;
                txn.NodeName = this.Name;
                txn.NodeType = Type;

                if (!txn.Position.Equals(""))
                {

                    RobotPoint point = PointManagement.GetPoint(Name, txn.Position);
                    
                    if (point == null)
                    {
                        logger.Error("point " + txn.Position + " not found!");
                        //this.IsExcuting = false;
                        throw new Exception("point " + txn.Position + " not found!");
                    }

                    if (txn.Method.Equals(Transaction.Command.RobotType.Mapping))
                    {
                        txn.Point = point.MappingPoint;
                    }
                    else if (txn.Method.Equals(Transaction.Command.RobotType.PreMapping))
                    {
                        txn.Point = point.PreMappingPoint;
                    }
                    else if(txn.Method.Equals(Transaction.Command.RobotType.PutByLArm) ||
                        txn.Method.Equals(Transaction.Command.RobotType.GetByLArm) ||
                        txn.Method.Equals(Transaction.Command.RobotType.PutWaitByLArm) ||
                        txn.Method.Equals(Transaction.Command.RobotType.GetWaitByLArm))
                    {
                        //設定POINT2的情況下，L Arm 使用Point2
                        txn.Point = point.Point2.Equals("") ? point.Point:point.Point2;
                    }
                    else
                    {
                        txn.Point = point.Point;
                    }
                    //檢查Loadport門是否為可以取放片狀態
                    //if (point.PositionType.Equals("LOADPORT"))
                    //{
                    //    Node port = NodeManagement.Get(point.Position);
                    //    if (port != null)
                    //    {
                    //        if (!port.ByPass)
                    //        {
                    //            Transaction InterLockTxn = new Transaction();
                    //            InterLockTxn.Method = Transaction.Command.LoadPortType.ReadStatus;
                    //            InterLockTxn.FormName = "InterLockChk";
                    //            port.SendCommand(InterLockTxn);
                    //        }
                    //    }
                    //}
                    if (!txn.Position2.Equals(""))
                    {
                        //if (txn.Method.Equals(Transaction.Command.RobotType.Mapping))
                        //{
                        //    point = PointManagement.GetMapPoint(txn.Position2, txn.RecipeID);
                        //}
                        //else
                        //{
                        point = PointManagement.GetPoint(Name, txn.Position2);
                        //}
                        if (point == null)
                        {
                            logger.Error("point " + txn.Position2 + " not found!");
                            this.IsExcuting = false;
                            throw new Exception("point " + txn.Position + " not found!");
                        }

                        txn.Point2 = point.Point;
                        //if (point.PositionType.Equals("LOADPORT"))
                        //{
                        //    Node port = NodeManagement.Get(point.Position);
                        //    if (port != null)
                        //    {
                        //        if (!port.ByPass)
                        //        {
                        //            Transaction InterLockTxn = new Transaction();
                        //            InterLockTxn.Method = Transaction.Command.LoadPortType.ReadStatus;
                        //            InterLockTxn.FormName = "InterLockChk";
                        //            port.SendCommand(InterLockTxn);
                        //        }
                        //    }
                        //}
                    }
                }

                txn.AckTimeOut = this.AckTimeOut;
                txn.MotionTimeOut = this.MotionTimeOut;

                switch (this.Type)
                {
                    case "MITSUBISHI_PLC":
                        switch (txn.Method)
                        {
                            case Transaction.Command.Mitsubishi_PLC.ReadBit:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().PLC.ReadBit(txn.PLC_Station, txn.PLC_Area, txn.PLC_StartAddress, txn.PLC_Len);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.Mitsubishi_PLC.ReadWord:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().PLC.ReadWord(txn.PLC_Station, txn.PLC_Area, txn.PLC_StartAddress, txn.PLC_Len);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.Mitsubishi_PLC.WriteBit:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().PLC.WriteBit(txn.PLC_Station, txn.PLC_Area, txn.PLC_StartAddress, txn.PLC_Bit);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.Mitsubishi_PLC.WriteWord:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().PLC.WriteWord(txn.PLC_Station, txn.PLC_Area, txn.PLC_StartAddress, txn.PLC_Word);
                                txn.CommandType = "SET";
                                break;
                        }
                        break;
                    case "WTS_ALIGNER":
                        switch (txn.Method)
                        {
                            case Transaction.Command.WTSAligner.Align:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WTSAligner.Align(txn.AdrNo, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WTSAligner.SetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WTSAligner.SetSpeed(txn.AdrNo, txn.Value);
                                txn.CommandType = "SET";
                                break;
                        }
                        break;
                    case "PTZ":
                        switch (txn.Method)
                        {
                            case Transaction.Command.PTZ.Rotate:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().PTZ.Rotate(txn.AdrNo, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.PTZ.Home:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().PTZ.Home(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.PTZ.Transfer:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().PTZ.Transfer(txn.AdrNo, txn.Slot, txn.Value, txn.Val2);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.PTZ.SetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().PTZ.SetSpeed(txn.AdrNo, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.PTZ.SetPath:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().PTZ.SetPath(txn.AdrNo, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                        }
                        break;
                    case "CTU":
                        switch (txn.Method)
                        {
                            case Transaction.Command.CTU.Pick:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Pick(txn.AdrNo, txn.Point, txn.Value, txn.Val2);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.CTU.Place:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Place(txn.AdrNo, txn.Point, txn.Value, txn.Val2);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.CTU.Hold:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Hold(txn.AdrNo, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.CTU.Release:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Release(txn.AdrNo, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.CTU.Home:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Home(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.CTU.OrgSearch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.OrgSearch(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.CTU.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Reset(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.CTU.SetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.SetSpeed(txn.AdrNo, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.CTU.Pause:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Pause();
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.CTU.Continue:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Continue();
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.CTU.Stop:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Stop();
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.CTU.Initial_IO:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Initial_IO();
                                txn.CommandType = "SET";
                                break;
                                //case Transaction.Command.CTU.Get_IO:
                                //    txn.CommandEncodeStr = Ctrl.GetEncoder().CTU.Get_IO(txn.Value);
                                //    txn.CommandType = "GET";
                                //    break;
                        }
                        break;
                    case "WHR":
                        switch (txn.Method)
                        {
                            case Transaction.Command.WHR.PreparePick:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.PreparePick(txn.AdrNo, txn.Point, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.Pick:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Pick(txn.AdrNo, txn.Point, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.PreparePlace:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.PreparePlace(txn.AdrNo, txn.Point, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.Place:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Place(txn.AdrNo, txn.Point, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.ToPick:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Extend(txn.AdrNo, txn.Point, txn.Value, "0");
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.CompletePick:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Retract(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.ToPlace:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Extend(txn.AdrNo, txn.Point, txn.Value, "1");
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.CompletePlace:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Retract(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.Extend:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Extend(txn.AdrNo, txn.Slot, txn.Value, txn.Val2);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.Retract:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Retract(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.Up:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Up(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.Down:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Down(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.SHome:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.SHome(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Reset(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.WHR.SetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.SetSpeed(txn.AdrNo, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.WHR.Pause:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Pause();
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.WHR.Continue:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Continue();
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.WHR.Stop:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Stop();
                                txn.CommandType = "SET";
                                break;
                                //case Transaction.Command.WHR.Get_IO:
                                //    txn.CommandEncodeStr = Ctrl.GetEncoder().WHR.Get_IO(txn.Value);
                                //    txn.CommandType = "GET";
                                //    break;
                        }
                        break;
                    case "SHELF":
                        switch (txn.Method)
                        {
                            case Transaction.Command.Shelf.GetFOUPPresence:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Shelf.GetFOUPPresence(txn.AdrNo, "0");
                                txn.CommandType = "CMD";
                                break;
                        }
                        break;
                    case "FOUP_ROBOT":
                        switch (txn.Method)
                        {
                            case Transaction.Command.FoupRobot.PreparePick:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.PreparePick(txn.AdrNo, txn.Point);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Pick:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Pick(txn.AdrNo, txn.Point);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.PreparePlace:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.PreparePlace(txn.AdrNo, txn.Point);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Place:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Place(txn.AdrNo, txn.Point);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Extend:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Extend(txn.AdrNo, txn.Point, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Retract:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Retract(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Grab:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Grab(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Release:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Release(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Up:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Up(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Down:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Down(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.SHome:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.SHome(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Transfer:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Transfer(txn.AdrNo, txn.Point, txn.Point2);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.SetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.SetSpeed(txn.AdrNo, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.FoupRobot.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Reset(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.FoupRobot.Pause:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Pause();
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.FoupRobot.Continue:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Continue();
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.FoupRobot.Stop:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Stop();
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.FoupRobot.Initial_IO:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Initial_IO();
                                txn.CommandType = "SET";
                                break;
                                //case Transaction.Command.FoupRobot.Get_IO:
                                //    txn.CommandEncodeStr = Ctrl.GetEncoder().FoupRobot.Get_IO(txn.Value);
                                //    txn.CommandType = "GET";
                                //    break;
                        }
                        break;
                    case "ILPT":
                        switch (txn.Method)
                        {
                            case Transaction.Command.ILPT.Clamp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.Clamp(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.Unclamp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.Unclamp(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.Dock:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.Dock(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.Undock:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.Undock(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.OpenLatch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.OpenLatch(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.CloseLatch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.CloseLatch(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.VacuumOn:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.VacuumOn(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.VacuumOff:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.VacuumOff(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.OpenDoor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.OpenDoor(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.CloseDoor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.CloseDoor(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.UpDoor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.UpDoor(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.DownDoor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.DownDoor(txn.AdrNo, txn.Value, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.OpenLower:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.OpenLower(txn.AdrNo, txn.Value, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.RaiseClose:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.RaiseClose(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.Map:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.Map(txn.AdrNo, txn.Value, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.Load:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.Load(txn.AdrNo, txn.Value, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.Unload:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.Unload(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.OrgSearch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.OrgSearch(txn.AdrNo, (Convert.ToInt32(txn.AdrNo) - 2).ToString());
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ILPT.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ILPT.Reset(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                        }
                        break;
                    case "ELPT":
                        switch (txn.Method)
                        {
                            case Transaction.Command.ELPT.Clamp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.Clamp(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.UnClamp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.Unclamp(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.OpenShutter:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.OpenShutter(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.CloseShutter:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.CloseShutter(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.ReadCID:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.ReadCID(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.MoveIn:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.MoveIn(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.MoveOut:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.MoveOut(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.getOutSensor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.GetOutSensor(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.getInSensor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.Clamp(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.getFOUPPresence:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.GetFOUPPresence(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.OrgSearch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.OrgSearch(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.Reset(txn.AdrNo);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.ELPT.SetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.SetSpeed(txn.AdrNo, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.ELPT.LightCurtainEnabled:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.LightCurtainEnabled(txn.AdrNo, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.ELPT.LightCurtainReset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().ELPT.LightCurtainReset(txn.AdrNo);
                                txn.CommandType = "SET";
                                break;
                        }
                        break;
                    case "FFU":
                        switch (txn.Method)
                        {
                            case Transaction.Command.FFUType.Start:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FFU.Start(AdrNo, ref txn);
                                break;
                            case Transaction.Command.FFUType.End:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FFU.End(AdrNo, ref txn);
                                break;
                            case Transaction.Command.FFUType.AlarmBypass:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FFU.AlarmBypass(AdrNo, ref txn);
                                break;
                            case Transaction.Command.FFUType.SetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FFU.SetSpeed(AdrNo, txn.Value, ref txn);
                                break;
                            case Transaction.Command.FFUType.GetStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FFU.GetStatus(AdrNo, ref txn);
                                break;
                        }
                        break;
                    case "SMARTTAG":
                        switch (txn.Method)
                        {
                            case Transaction.Command.SmartTagType.Hello:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().SmartTag.Hello();

                                break;
                            case Transaction.Command.SmartTagType.GetLCDData:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().SmartTag.GetLCDData();

                                break;
                            case Transaction.Command.SmartTagType.SelectLCDData:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().SmartTag.SelectLCDData();

                                break;
                            case Transaction.Command.SmartTagType.SetLCDData:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().SmartTag.SetLCDData(txn.Value);

                                break;
                        }
                        break;
                    case "RFID":
                        switch (txn.Method)
                        {
                            case Transaction.Command.RFIDType.Hello:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().RFID.Hello();
                                break;

                            case Transaction.Command.RFIDType.Mode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().RFID.Mode(txn.Value);
                                break;

                            case Transaction.Command.RFIDType.GetCarrierID:
                                if (txn.Value.Equals(""))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().RFID.ReadCarrierID();
                                }
                                else
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().RFID.ReadCarrierID(txn.Value);
                                }


                                break;
                            case Transaction.Command.RFIDType.SetCarrierID:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().RFID.WriteCarrierID(txn.Value);
                                break;
                        }
                        break;
                    case "LOADPORT":
                        switch (txn.Method)
                        {
                            case Transaction.Command.LoadPortType.SetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetSpeed(txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.LoadPortType.GetSlotOffset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetSlotOffset();
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.GetWaferOffset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetWaferOffset();
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.GetSlotPitch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetSlotPitch();
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.GetTweekDistance:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetTweekDistance();
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.GetCassetteSize:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetCassetteSizeOption();
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.SetSlotOffset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetSlotOffset(txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.SetWaferOffset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetWaferOffset(txn.Value);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.SetSlotPitch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetSlotPitch(txn.Value);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.SetTweekDistance:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetTweekDistance(txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.LoadPortType.SetCassetteSize:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetCassetteSizeOption(txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.LoadPortType.Stop:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Stop(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.EQASP:
                                if (txn.Value.Equals("1"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.EQASP(EncoderLoadPort.ParamState.Enable);
                                }
                                else if (txn.Value.Equals("0"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.EQASP(EncoderLoadPort.ParamState.Disable);
                                }
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.LoadPortType.Mode:
                                if (txn.Value.Equals("1"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Mode(EncoderLoadPort.ModeType.Maintenance);
                                }
                                else if (txn.Value.Equals("0"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Mode(EncoderLoadPort.ModeType.Online);
                                }
                                else if (txn.Value.Equals("2"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Mode(EncoderLoadPort.ModeType.Auto);
                                }

                                txn.CommandType = "SET";

                                if (Vendor.ToUpper().Equals("ASYST"))
                                    txn.CommandType = "CMD";


                                break;
                            case Transaction.Command.LoadPortType.SetOpAccessBlink:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.OpAccess, EncoderLoadPort.IndicatorStatus.Flashes);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.SetOpAccess:
                                if (txn.Value.Equals("1"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.OpAccess, EncoderLoadPort.IndicatorStatus.ON);
                                    this.OPACCESS = true;
                                }
                                else if (txn.Value.Equals("0"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.OpAccess, EncoderLoadPort.IndicatorStatus.OFF);
                                    this.OPACCESS = false;
                                }
                                else if (txn.Value.Equals("2"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.OpAccess, EncoderLoadPort.IndicatorStatus.Flashes);
                                    this.OPACCESS = true;
                                }
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.SetLoad:
                                if (txn.Value.Equals("1"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.Load, EncoderLoadPort.IndicatorStatus.ON);
                                }
                                else if (txn.Value.Equals("0"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.Load, EncoderLoadPort.IndicatorStatus.OFF);
                                }
                                else if (txn.Value.Equals("2"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.Load, EncoderLoadPort.IndicatorStatus.Flashes);
                                }
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.SetUnLoad:
                                if (txn.Value.Equals("1"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.Unload, EncoderLoadPort.IndicatorStatus.ON);
                                }
                                else if (txn.Value.Equals("0"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.Unload, EncoderLoadPort.IndicatorStatus.OFF);
                                }
                                else if (txn.Value.Equals("2"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.Unload, EncoderLoadPort.IndicatorStatus.Flashes);
                                }
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.GetLED:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.LEDIndicatorStatus();
                                break;
                            case Transaction.Command.LoadPortType.GetMappingDummy:
                                txn.CommandEncodeStr = "GetMappingDummy";
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.GetMapping:
                            case Transaction.Command.LoadPortType.GetMappingData:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.WaferSorting(EncoderLoadPort.MappingSortingType.Asc);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.ReadStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Status();
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.InitialPos:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.InitialPosition(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.ForceInitialPos:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.ForcedInitialPosition(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.Load:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Load(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.LoadWithLift:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.LoadWithLift(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;

                            case Transaction.Command.LoadPortType.MappingLoad:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MappingLoad(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.MappingUnload:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapAndUnload(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Reset(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";

                                if (Vendor.ToUpper().Equals("ASYST"))
                                    txn.CommandType = "SET";
                                break;
                            case Transaction.Command.LoadPortType.Unload:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Unload(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.Mapping:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MappingInLoad(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.RetryMapping:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.RetryMapping(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.GetCount:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.WaferQuantity();
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.UnClamp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.FOUPClampRelease(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.Clamp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.FOUPClampFix(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.Dock:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Dock(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.UnDock:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Undock(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.VacuumOFF:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.VacuumOFF(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.VacuumON:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.VacuumON(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.UnLatchDoor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.LatchkeyRelease(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.LatchDoor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.LatchkeyFix(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.DoorClose:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DoorClose(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.DoorOpen:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DoorOpen(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.DoorDown:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DoorDown(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.DoorUp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DoorUp(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.ReadVersion:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Version();
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.MapperWaitPosition:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperWaitPosition(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.MapperStartPosition:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperStartPosition(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.MapperArmRetracted:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperArmClose(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.MapperArmStretch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperArmOpen(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.MapperStopperOn:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperStopperON(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.MapperStopperOff:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperStopperOFF(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;

                            case Transaction.Command.LoadPortType.MappingDown:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MappingDown(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.UntilUnDock:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.UntilUndock(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.DockingPositionNoVac:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DockingPositionNoVac(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.UntilDoorCloseVacOFF:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.UntilDoorCloseVacOFF(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.MoveToSlot:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Slot(EncoderLoadPort.CommandType.Normal, txn.Value, txn.Val2);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.SetAllEvent:
                                if (txn.Value.Equals("1"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetEvent(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.EventType.All, EncoderLoadPort.ParamState.Enable);
                                }
                                else if (txn.Value.Equals("0"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetEvent(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.EventType.All, EncoderLoadPort.ParamState.Disable);
                                }
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.LoadPortType.SetCompleteEvent:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetEvent(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.EventType.Complete, EncoderLoadPort.ParamState.Enable);
                                txn.CommandType = "SET";
                                break;
                            //case Transaction.Command.LoadPortType:
                            //    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetCassetteSizeOption(EncoderLoadPort.CassrtteSize.Disable_SlotSensor_INX2200);
                            //    break;
                            case Transaction.Command.LoadPortType.TweekDn:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Tweek(EncoderLoadPort.TweekType.TweekDown);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.LoadPortType.TweekUp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Tweek(EncoderLoadPort.TweekType.TweekUp);
                                txn.CommandType = "CMD";
                                break;
                            //取得CST模式
                            case Transaction.Command.LoadPortType.GetOPMode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetOPMode(EncoderLoadPort.CommandType.Normal);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.LoadPortType.SaveLog:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SaveLog(AdrNo, txn.Seq);
                                txn.CommandType = "SET";
                                txn.AckTimeOut = 180000;

                                if (Vendor.ToUpper().Equals("ASYST"))
                                    txn.CommandType = "CMD";
                                break;

                                //case Transaction.Command.LoadPortType.SetSlotOffset:
                                //    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetSlotOffset(txn.Value);
                                //    break;
                                //case Transaction.Command.LoadPortType.SetWaferOffset:
                                //    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetWaferOffset(txn.Value);
                                //    break;
                                //case Transaction.Command.LoadPortType.SetSlotPitch:
                                //    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetSlotPitch(txn.Value);
                                //    break;
                                //case Transaction.Command.LoadPortType.SetTweekDistance:
                                //    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetTweekDistance(txn.Value);
                                //    break;
                        }
                        break;
                    case "ROBOT":
                        switch (txn.Method)
                        {
                            case Transaction.Command.RobotType.GetPresence:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetPresence(AdrNo, txn.Seq, txn.Arm);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.RobotType.GetPosition:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ArmLocation(AdrNo, txn.Seq, txn.Value, "1");
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.RobotType.Initialize:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Initialize(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.ArmReturn:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Retract(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.Exchange:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Exchange(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot, txn.Arm2, txn.Point2, txn.Slot2);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.GetMapping:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.MapList(AdrNo, txn.Seq, "1");
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.RobotType.Mapping:
                            case Transaction.Command.RobotType.PreMapping:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Mapping(AdrNo, txn.Seq, txn.Point, "1", "0");
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.GetStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Status(AdrNo, txn.Seq);
                                txn.CommandType = "GET";
                                break;
                            //case Transaction.Command.RobotType.GetCombineStatus:
                            //    txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.CombinedStatus(AdrNo, txn.Seq);
                            //    break;
                            case Transaction.Command.RobotType.GetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Speed(AdrNo, txn.Seq);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.RobotType.GetRIO:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.StatusIO(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.RobotType.GetSV:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.SolenoidValve(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.RobotType.Stop:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.DeviceStop(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.RobotType.Pause:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.DevicePause(AdrNo, txn.Seq);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.RobotType.Continue:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.DeviceContinue(AdrNo, txn.Seq);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.RobotType.GetMode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetMode(AdrNo, txn.Seq);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.RobotType.GetError:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ErrorMessage(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.RobotType.Get:
                            case Transaction.Command.RobotType.GetByRArm:
                            case Transaction.Command.RobotType.GetByLArm:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWafer(AdrNo, txn.Seq, txn.Arm, txn.Point, "0", txn.Slot);
                                if (txn.Arm.Equals("3"))
                                {
                                    txn.Method = Transaction.Command.RobotType.DoubleGet;
                                }
                                this.RobotGetState = 0;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.Put:
                            case Transaction.Command.RobotType.PutByRArm:
                            case Transaction.Command.RobotType.PutByLArm:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWafer(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                if (txn.Arm.Equals("3"))
                                {
                                    txn.Method = Transaction.Command.RobotType.DoublePut;
                                }
                                this.RobotPutState = 0;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.DoubleGet:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWafer(AdrNo, txn.Seq, "3", txn.Point, "0", txn.Slot);
                                this.RobotGetState = 0;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.DoublePut:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWafer(AdrNo, txn.Seq, "3", txn.Point, txn.Slot);
                                this.RobotPutState = 0;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.WaitBeforeGet:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWaferToStandBy(AdrNo, txn.Seq, txn.Arm, txn.Point, "0", txn.Slot);
                                this.RobotGetState = 1;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.WaitBeforePut:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWaferToStandBy(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                this.RobotPutState = 1;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.GetAfterWait:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWaferToContinue(AdrNo, txn.Seq, txn.Arm, txn.Point, "0", txn.Slot);
                                this.RobotGetState = 3;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.PutWithoutBack:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWaferToDown(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                this.RobotPutState = 2;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.GetWithoutBack:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWaferToUp(AdrNo, txn.Seq, txn.Arm, txn.Point, "0", txn.Slot);
                                this.RobotGetState = 2;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.PutBack:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWaferToContinue(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                this.RobotPutState = 3;
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.GetWait:
                            case Transaction.Command.RobotType.GetWaitByRArm:
                            case Transaction.Command.RobotType.GetWaitByLArm:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWaferToReady(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.PutWait:
                            case Transaction.Command.RobotType.PutWaitByRArm:
                            case Transaction.Command.RobotType.PutWaitByLArm:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWaferToReady(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.Home:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Home(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.HomeSafety:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.HomeSafety(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.HomeA:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.HomeOrgin(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.OrginSearch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.OrginSearch(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.WaferRelease:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.WaferReleaseHold(AdrNo, txn.Seq, txn.Arm);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.WaferHold:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.WaferHold(AdrNo, txn.Seq, txn.Arm);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.RobotType.Servo:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ServoOn(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.RobotType.Mode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Mode(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.RobotType.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ErrorReset(AdrNo, txn.Seq);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.RobotType.Speed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.setSpeed(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.RobotType.SetSV:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.setSolenoidValve(AdrNo, txn.Seq, txn.Arm, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.RobotType.SaveLog:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.SaveLog(AdrNo, txn.Seq);
                                txn.CommandType = "SET";
                                txn.AckTimeOut = 180000;
                                break;
                        }
                        break;
                    case "ALIGNER":
                        switch (txn.Method)
                        {
                            case Transaction.Command.AlignerType.GetPosition:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ArmLocation(AdrNo, txn.Seq, txn.Value, "1");
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.AlignerType.SetAlign:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.SetSize(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.AlignerType.GetStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Status(AdrNo, txn.Seq);
                                txn.CommandType = "GET";
                                break;
                            //case Transaction.Command.AlignerType.GetCombineStatus:
                            //    txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.CombinedStatus(AdrNo, txn.Seq);
                            //    break;
                            case Transaction.Command.AlignerType.GetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Speed(AdrNo, txn.Seq);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.AlignerType.GetRIO:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.StatusIO(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.AlignerType.Stop:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.DeviceStop(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.AlignerType.Pause:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.DevicePause(AdrNo, txn.Seq);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.AlignerType.Continue:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.DeviceContinue(AdrNo, txn.Seq);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.AlignerType.GetSV:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.SolenoidValve(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.AlignerType.SetSV:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.setSolenoidValve(AdrNo, txn.Seq, txn.Value, txn.Val2);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.AlignerType.GetError:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.ErrorMessage(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.AlignerType.GetMode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.GetMode(AdrNo, txn.Seq);
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.AlignerType.Home:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Home(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.AlignerType.Align:

                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Align(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.AlignerType.AlignOption:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Align(AdrNo, txn.Seq, txn.Value, "1", "0", "0");
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.AlignerType.AlignOffset://使用上次Align結果，不用先回Home
                                if (txn.Value.Equals(""))
                                {

                                    //if (txn.Method.Equals(Transaction.Command.RobotType.Mapping))
                                    //{
                                    //    RobotPoint point = PointManagement.GetMapPoint(txn.Position, txn.RecipeID);
                                    //}
                                }
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Align(AdrNo, txn.Seq, txn.Value, "0", "0", "0");
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.AlignerType.Retract:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Retract(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.AlignerType.WaferRelease:
                                //txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.WaferReleaseHold(AdrNo, txn.Seq);
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.WaferReleaseHold(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.AlignerType.WaferHold:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.WaferHold(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.AlignerType.Servo:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.ServoOn(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.AlignerType.Mode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Mode(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.AlignerType.OrginSearch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.OrginSearch(AdrNo, txn.Seq);
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.AlignerType.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.ErrorReset(AdrNo, txn.Seq);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.AlignerType.Speed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.setSpeed(AdrNo, txn.Seq, txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.AlignerType.SaveLog:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.SaveLog(AdrNo, txn.Seq);
                                txn.CommandType = "SET";
                                txn.AckTimeOut = 180000;
                                break;
                        }
                        break;
                    case "OCR":
                        switch (txn.Method)
                        {
                            case Transaction.Command.OCRType.GetOnline:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().OCR.GetOnline();
                                txn.CommandType = "GET";
                                break;
                            case Transaction.Command.OCRType.Online:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().OCR.SetOnline(EncoderOCR.OnlineStatus.Online);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.OCRType.Offline:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().OCR.SetOnline(EncoderOCR.OnlineStatus.Offline);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.OCRType.Read:
                            case Transaction.Command.OCRType.ReadM12:
                            case Transaction.Command.OCRType.ReadT7:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().OCR.Read();
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.OCRType.ReadConfig:
                                txn.CommandEncodeStr = "Ev ReadConfig(A4," + txn.Value + ",0)";
                                txn.CommandType = "CMD";
                                break;
                            case Transaction.Command.OCRType.SetConfigEnable:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().OCR.SetConfigEnable(txn.Value);
                                txn.CommandType = "CMD";
                                break;
                        }
                        break;
                    case "E84":
                        switch (txn.Method)
                        {
                            case Transaction.Command.E84.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.Reset();
                                txn.CommandType = "SET";
                                break;

                            case Transaction.Command.E84.SetAutoMode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.AutoMode();
                                txn.CommandType = "SET";
                                break;

                            case Transaction.Command.E84.SetManualMode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.ManualMode();
                                txn.CommandType = "SET";
                                break;

                            case Transaction.Command.E84.GetE84IOStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.GetE84IOStatus();
                                txn.CommandType = "GET";
                                break;

                            case Transaction.Command.E84.GetDIOStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.GetDIOStatus();
                                txn.CommandType = "GET";
                                break;

                            case Transaction.Command.E84.SetTP1:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.SetTP1(txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.E84.SetTP2:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.SetTP2(txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.E84.SetTP3:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.SetTP3(txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.E84.SetTP4:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.SetTP4(txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.E84.SetTP5:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.SetTP5(txn.Value);
                                txn.CommandType = "SET";
                                break;
                            case Transaction.Command.E84.SetTP6:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().E84.SetTP6(txn.Value);
                                txn.CommandType = "SET";
                                break;
                        }
                        break;
                }

                bool IsWaitData = false;
                if (txn.Method.Equals(Transaction.Command.SmartTagType.GetLCDData) ||
                    txn.Method.Equals(Transaction.Command.RFIDType.GetCarrierID))
                {
                    IsWaitData = true;
                }

                //txn.AckTimeOut = this.AckTimeOut;
                logger.Debug("Ack TimeOut:" + txn.AckTimeOut.ToString());

                //txn.MotionTimeOut = this.MotionTimeOut;
                logger.Debug("Motion TimeOut:" + txn.MotionTimeOut.ToString());
                Ctrl.DoWork(txn, IsWaitData);
                result = true;


            }
            catch (Exception e)
            {
                logger.Error("SendCommand " + e.Message + "\n" + e.StackTrace);
                this.IsExcuting = false;
                throw new Exception("SendCommand " + e.Message + "\n" + e.StackTrace);
            }
            //watch.Stop();
            //var elapsedMs = watch.ElapsedMilliseconds;
            //logger.Info("SendCommand ProcessTime:"+ elapsedMs.ToString());

            return result;

        }


    }
}
