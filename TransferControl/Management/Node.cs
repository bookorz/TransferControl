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
    public class Node
    {

        ILog logger = LogManager.GetLogger(typeof(Node));
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
        public string Brand { get; set; }
        /// <summary>
        /// Control Job ID
        /// </summary>
        public string CjID { get; set; }
        /// <summary>
        /// Process Request ID
        /// </summary>
        public string PrID { get; set; }
        /// <summary>
        /// Robot專用，取片階段用於標記Foup
        /// </summary>
        public string CurrentLoadPort { get; set; }
        /// <summary>
        /// Robot專用，目前手臂的位置
        /// </summary>
        public string CurrentPosition { get; set; }
        /// <summary>
        /// 啟用或停用此Node
        /// </summary>
        public bool Enable { get; set; }
        public bool ForcePutToUnload { get; set; }
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
        public bool MappingHasError = false;
        /// <summary>
        /// LoadPort專用，目前可以取片
        /// </summary>
        public bool Fetchable { get; set; }
        /// <summary>
        /// 標記為虛擬裝置
        /// </summary>
        public bool ByPass { get; set; }
        /// <summary>
        /// LoadPort專用，Foup Load的時間
        /// </summary>
        public DateTime LoadTime { get; set; }
        /// <summary>
        /// LoadPort專用，其他Port Assign用
        /// </summary>
        public ConcurrentDictionary<string, Job> ReserveList { get; set; }
        /// <summary>
        /// 在席列表
        /// </summary>
        public ConcurrentDictionary<string, Job> JobList { get; set; }

        public string CarrierType { get; set; }

        public bool Busy { get; set; }

        public bool OPACCESS { get; set; }

        public string LastFinMethod { get; set; }

        public bool WaitForFinish { get; set; }

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

        public string Door_Position { get; set; }

        public bool Foup_Placement { get; set; }

        public bool Foup_Presence { get; set; }

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

        public bool ReadyForGet { get; set; }

        public bool ReadyForPut { get; set; }

        public bool AccessAutoMode { get; set; }

        public int E87_TransferState { get; set; }

        public int E87_ReservationState { get; set; }

        public int E87_AssociationState { get; set; }

        public int PTN { get; set; }

        public string Speed { get; set; }

        public bool ByPassCheck { get; set; }

        public string FoupID { get; set; }

        private bool isPool { get; set; }

        private bool PoolThread { get; set; }

        public int PoolInterval { get; set; }

        public bool OCRSuccess { get; set; }
        public string CurrentStatus { get; set; }
        public string R_Vacuum_Solenoid { get; set; }
        public string L_Vacuum_Solenoid { get; set; }
        public string ConfigList { get; set; }
        public bool OCR_Read_TTL { get; set; }
        public bool OCR_Read_M12 { get; set; }
        public bool OCR_Read_T7 { get; set; }
        public bool Home_Position { get; set; }
        public bool ManaulControl { get; set; }
        public Dictionary<string, string> Status { get; set; }
        public Dictionary<string, string> IO { get; set; }
        public Dictionary<string, ActionRequest> RequestQueue = new Dictionary<string, ActionRequest>();
        private static DBUtil dBUtil = new DBUtil();
        public Carrier Carrier { get; set; }
        public int RobotGetState { get; set; }
        public int RobotPutState { get; set; }
        public class ActionRequest
        {
            public string TaskName { get; set; }
            public string Position { get; set; }
            public string Slot { get; set; }
            public string Slot2 { get; set; }
            public string Arm { get; set; }
            public string Value { get; set; }
            public string V2 { get; set; }
            public string V3 { get; set; }
            public string V4 { get; set; }
            public string V5 { get; set; }
            public string V6 { get; set; }
            public string V7 { get; set; }
            public string V8 { get; set; }
            public string V9 { get; set; }
            public long TimeStamp { get; set; }

            public ActionRequest()
            {
                TimeStamp = DateTime.Now.Ticks;
                Position = "";
                Slot = "";
                Slot2 = "";
                Arm = "";
                Value = "";
                V2 = "";
                V3 = "";
                V4 = "";
                V5 = "";
                V6 = "";
                V7 = "";
                V8 = "";
                V9 = "";
            }
        }

        public IController GetController()
        {
            return ControllerManagement.Get(Controller);
        }

        public void InitialObject()
        {
            JobList = new ConcurrentDictionary<string, Job>();
            ReserveList = new ConcurrentDictionary<string, Job>();

            RobotGetState = 0;
            RobotPutState = 0;
            Speed = "";
            ConfigList = "";
            ByPassCheck = false;
            Connected = false;
            OPACCESS = false;
            AccessAutoMode = false;
            MappingResult = "";
            CurrentLoadPort = "";
            CurrentStatus = "";
            PrID = "";
            CjID = "";
            R_Flip_Degree = "0";
            L_Flip_Degree = "0";
            CurrentPosition = "";

            FoupID = "";
            Status = new Dictionary<string, string>();
            IO = new Dictionary<string, string>();
            State = "UNORG";
            ReadyForPut = true;
            ReadyForGet = true;
            E87_TransferState = 0;
            E87_ReservationState = 0;
            E87_AssociationState = 0;
            Home_Position = false;
            ForcePutToUnload = false;
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
            WaitForFinish = false;
            InitialComplete = false;
            OrgSearchComplete = false;
            IsWaferHold = false;
            IsLoad = false;
            IsExcuting = false;
            IsPause = false;
            CurrentSlotPosition = "??";
            ErrorMsg = "";
            //Enable = true;
            Fetchable = false;
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
        }
        public void PoolStart(string TaskName)
        {
            this.isPool = false;
            SpinWait.SpinUntil(() => !PoolThread, 999999999);
            this.isPool = true;
            ThreadPool.QueueUserWorkItem(new WaitCallback(PoolState), TaskName);

        }
        public void PoolStop()
        {
            this.isPool = false;
        }
        private void PoolState(object TaskJob)
        {
            PoolThread = true;
            Dictionary<string, string> param = new Dictionary<string, string>();
            string Message = "";
            string TaskName = "";
            TaskJobManagment.CurrentProceedTask CurrTask;
            TaskName = TaskJob.ToString();
            param.Add("@Target", this.Name);
            while (this.isPool)
            {
                try
                {

                    RouteControl.Instance.TaskJob.Excute(this.Name + "_PoolState", out Message, out CurrTask, TaskName, param);
                    SpinWait.SpinUntil(() => CurrTask.Finished, 99999999);
                    SpinWait.SpinUntil(() => false, PoolInterval);
                }
                catch (Exception e)
                {
                    logger.Error(e.StackTrace);
                }

            }
            PoolThread = false;
        }
        public bool CheckForward(string Slot)
        {
            bool result = false;

            var wafers = from wafer in this.JobList.Values
                         where wafer.MapFlag && !wafer.ErrPosition && !wafer.NeedProcess && Convert.ToInt16(wafer.Slot) < Convert.ToInt16(Slot)
                         select wafer;
            if (wafers.Count() != 0)
            {
                result = true;
            }
            return result;
        }
        public bool CheckPrevious(string Slot)
        {
            bool result = false;

            var wafers = from wafer in this.JobList.Values
                         where wafer.MapFlag && !wafer.ErrPosition && !wafer.NeedProcess && (Convert.ToInt16(Slot) - Convert.ToInt16(wafer.Slot)) == 1
                         select wafer;
            if (wafers.Count() != 0)
            {
                result = true;
            }
            return result;
        }
        public bool CheckForwardPresence(string Slot)
        {
            bool result = false;

            var wafers = from wafer in this.JobList.Values
                         where wafer.MapFlag && Convert.ToInt16(wafer.Slot) < Convert.ToInt16(Slot)
                         select wafer;
            if (wafers.Count() != 0)
            {
                result = true;
            }
            return result;
        }

        public bool CheckPreviousPresence(string Slot)
        {
            bool result = false;

            var wafers = from wafer in this.JobList.Values
                         where wafer.MapFlag && (Convert.ToInt16(Slot) - Convert.ToInt16(wafer.Slot)) == 1
                         select wafer;
            if (wafers.Count() != 0)
            {
                result = true;
            }
            return result;
        }

        public void SetEnable(bool enable)
        {
            this.isPool = enable;
            this.Enable = enable;
            string SQL = @"update config_node set enable_flg = " + Convert.ToByte(enable).ToString() + " where equipment_model_id = '" + SystemConfig.Get().SystemMode + "' and node_id = '" + this.Name + "'";
            dBUtil.ExecuteNonQuery(SQL, null);
        }

        /// <summary>
        /// 傳送命令
        /// </summary>
        /// <param name="txn"></param>
        /// <param name="Force"></param>
        /// <returns></returns>
        public bool SendCommand(Transaction txn, out string Message, bool Force = false)
        {
            txn.RecipeID = this.WaferSize;
            Message = "";
            //if (this.Type.ToUpper().Equals("LOADPORT"))
            //{
            //    while (true)
            //    {
            //        SpinWait.SpinUntil(() => !this.IsExcuting, 99999999);
            //        lock (this)
            //        {
            //            if (!this.IsExcuting)
            //            {
            //                break;
            //            }
            //        }
            //    }
            //}
            this.IsExcuting = true;
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            // System.Threading.Thread.Sleep(500);
            try
            {
                txn.Slot = int.Parse(txn.Slot).ToString();
                txn.Slot2 = int.Parse(txn.Slot2).ToString();
            }
            catch
            {

            }
            foreach (Job j in txn.TargetJobs)
            {
                try
                {
                    j.Slot = int.Parse(j.Slot).ToString();
                }
                catch
                {

                }
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
                if (this.Brand.ToUpper().Equals("KAWASAKI"))
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
                //if (txn.Value != null)
                //{
                //    if (!txn.Value.Equals(""))
                //    {
                //        CmdParamManagement.ParamMapping Mapping = CmdParamManagement.FindMapping(this.Brand.ToUpper(), txn.Method, "Value", txn.Value);
                //        if (Mapping != null)
                //        {
                //            txn.Value = Mapping.MappingCode;
                //        }
                //    }
                //}
                //if (txn.Arm != null)
                //{
                //    if (!txn.Arm.Equals(""))
                //    {
                //        CmdParamManagement.ParamMapping Mapping = CmdParamManagement.FindMapping(this.Brand.ToUpper(), txn.Method, "Arm", txn.Arm);
                //        if (Mapping != null)
                //        {
                //            txn.Arm = Mapping.MappingCode;
                //        }
                //    }
                //}

                if (!txn.Position.Equals(""))
                {
                    if (txn.RecipeID.Equals(""))
                    {
                        if (txn.TargetJobs.Count != 0)
                        {
                            txn.RecipeID = txn.TargetJobs[0].RecipeID;
                        }
                        else if (!NodeManagement.Get(txn.Position).WaferSize.Equals(""))
                        {
                            txn.RecipeID = NodeManagement.Get(txn.Position).WaferSize;
                        }
                    }
                    RobotPoint point;
                    //if (txn.Method.Equals(Transaction.Command.RobotType.Mapping))
                    //{
                    //    point = PointManagement.GetMapPoint(this.Name,txn.Position, txn.RecipeID);
                    //}
                    //else
                    //{
                    point = PointManagement.GetPoint(Name, txn.Position);
                    //}
                    if (point == null)
                    {
                        logger.Error("point " + txn.Position + " not found!");
                        this.IsExcuting = false;
                        throw new Exception("point " + txn.Position + " not found!");
                    }

                    txn.Point = point.Point;
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

                switch (this.Type)
                {
                    case "FFU":
                        switch (txn.Method)
                        {
                            case Transaction.Command.FFUType.Start:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FFU.Start(AdrNo, ref txn);
                                break;
                            case Transaction.Command.FFUType.SetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FFU.SetSpeed(AdrNo, txn.Value, ref txn);
                                break;
                            case Transaction.Command.FFUType.GetStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().FFU.GetStatus(AdrNo);
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
                    case "LOADPORT":
                        switch (txn.Method)
                        {
                            case Transaction.Command.LoadPortType.GetSlotOffset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetSlotOffset();
                                break;
                            case Transaction.Command.LoadPortType.GetWaferOffset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetWaferOffset();
                                break;
                            case Transaction.Command.LoadPortType.GetSlotPitch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetSlotPitch();
                                break;
                            case Transaction.Command.LoadPortType.GetTweekDistance:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetTweekDistance();
                                break;
                            case Transaction.Command.LoadPortType.GetCassetteSize:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.GetCassetteSizeOption();
                                break;
                            case Transaction.Command.LoadPortType.SetSlotOffset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetSlotOffset(txn.Value);
                                break;
                            case Transaction.Command.LoadPortType.SetWaferOffset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetWaferOffset(txn.Value);
                                break;
                            case Transaction.Command.LoadPortType.SetSlotPitch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetSlotPitch(txn.Value);
                                break;
                            case Transaction.Command.LoadPortType.SetTweekDistance:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetTweekDistance(txn.Value);
                                break;
                            case Transaction.Command.LoadPortType.SetCassetteSize:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetCassetteSizeOption(txn.Value);
                                break;
                            case Transaction.Command.LoadPortType.Stop:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Stop(EncoderLoadPort.CommandType.Normal);
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
                                break;
                            case Transaction.Command.LoadPortType.SetOpAccessBlink:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.OpAccess, EncoderLoadPort.IndicatorStatus.Flashes);
                                break;
                            case Transaction.Command.LoadPortType.SetOpAccess:
                                if (txn.Value.Equals("1"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.OpAccess, EncoderLoadPort.IndicatorStatus.ON);
                                }
                                else if (txn.Value.Equals("0"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.OpAccess, EncoderLoadPort.IndicatorStatus.OFF);
                                }
                                else if (txn.Value.Equals("2"))
                                {
                                    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Indicator(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.IndicatorType.OpAccess, EncoderLoadPort.IndicatorStatus.Flashes);
                                }
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
                                break;
                            case Transaction.Command.LoadPortType.GetLED:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.LEDIndicatorStatus();
                                break;
                            case Transaction.Command.LoadPortType.GetMapping:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.WaferSorting(EncoderLoadPort.MappingSortingType.Asc);
                                break;
                            case Transaction.Command.LoadPortType.ReadStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Status();
                                break;
                            case Transaction.Command.LoadPortType.InitialPos:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.InitialPosition(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.ForceInitialPos:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.ForcedInitialPosition(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.Load:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Load(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.MappingLoad:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MappingLoad(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.MappingUnload:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapAndUnload(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Reset(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.Unload:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Unload(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.Mapping:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MappingInLoad(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.GetCount:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.WaferQuantity();
                                break;
                            case Transaction.Command.LoadPortType.UnClamp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.FOUPClampRelease(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.Clamp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.FOUPClampFix(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.Dock:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Dock(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.UnDock:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Undock(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.VacuumOFF:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.VacuumOFF(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.VacuumON:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.VacuumON(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.UnLatchDoor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.LatchkeyRelease(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.LatchDoor:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.LatchkeyFix(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.DoorClose:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DoorClose(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.DoorOpen:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DoorOpen(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.DoorDown:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DoorDown(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.DoorUp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DoorUp(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.ReadVersion:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Version();
                                break;
                            case Transaction.Command.LoadPortType.MapperWaitPosition:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperWaitPosition(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.MapperStartPosition:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperStartPosition(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.MapperArmRetracted:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperArmClose(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.MapperArmStretch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MapperArmOpen(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.MappingDown:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.MappingDown(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.UntilUnDock:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.UntilUndock(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.DockingPositionNoVac:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.DockingPositionNoVac(EncoderLoadPort.CommandType.Normal);
                                break;
                            case Transaction.Command.LoadPortType.MoveToSlot:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Slot(EncoderLoadPort.CommandType.Normal, txn.Value);
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
                                break;
                            case Transaction.Command.LoadPortType.SetCompleteEvent:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetEvent(EncoderLoadPort.CommandType.Normal, EncoderLoadPort.EventType.Complete, EncoderLoadPort.ParamState.Enable);
                                break;
                            //case Transaction.Command.LoadPortType:
                            //    txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.SetCassetteSizeOption(EncoderLoadPort.CassrtteSize.Disable_SlotSensor_INX2200);
                            //    break;
                            case Transaction.Command.LoadPortType.TweekDn:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Tweek(EncoderLoadPort.TweekType.TweekDown);
                                break;
                            case Transaction.Command.LoadPortType.TweekUp:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().LoadPort.Tweek(EncoderLoadPort.TweekType.TweekUp);
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
                            case Transaction.Command.RobotType.GetPosition:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ArmLocation(AdrNo, txn.Seq, txn.Value, "1");
                                break;
                            case Transaction.Command.RobotType.ArmReturn:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Retract(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.Exchange:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Exchange(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot, txn.Arm2, txn.Point2, txn.Slot2);
                                break;
                            case Transaction.Command.RobotType.GetMapping:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.MapList(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.RobotType.Mapping:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Mapping(AdrNo, txn.Seq, txn.Point, "1", txn.Slot);
                                break;
                            case Transaction.Command.RobotType.GetStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Status(AdrNo, txn.Seq);
                                break;
                            //case Transaction.Command.RobotType.GetCombineStatus:
                            //    txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.CombinedStatus(AdrNo, txn.Seq);
                            //    break;
                            case Transaction.Command.RobotType.GetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Speed(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.GetRIO:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.StatusIO(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.RobotType.GetSV:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.SolenoidValve(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.RobotType.Stop:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.DeviceStop(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.RobotType.Pause:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.DevicePause(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.Continue:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.DeviceContinue(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.GetMode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetMode(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.GetError:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ErrorMessage(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.RobotType.Get:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWafer(AdrNo, txn.Seq, txn.Arm, txn.Point, "0", txn.Slot);
                                if (txn.Arm.Equals("3"))
                                {
                                    txn.Method = Transaction.Command.RobotType.DoubleGet;
                                }
                                this.RobotGetState = 0;
                                break;
                            case Transaction.Command.RobotType.Put:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWafer(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                if (txn.Arm.Equals("3"))
                                {
                                    txn.Method = Transaction.Command.RobotType.DoublePut;
                                }
                                this.RobotPutState = 0;
                                break;
                            case Transaction.Command.RobotType.DoubleGet:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWafer(AdrNo, txn.Seq, "3", txn.Point, "0", txn.Slot);
                                this.RobotGetState = 0;
                                break;
                            case Transaction.Command.RobotType.DoublePut:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWafer(AdrNo, txn.Seq, "3", txn.Point, txn.Slot);
                                this.RobotPutState = 0;
                                break;
                            case Transaction.Command.RobotType.WaitBeforeGet:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWaferToStandBy(AdrNo, txn.Seq, txn.Arm, txn.Point, "0", txn.Slot);
                                this.RobotGetState = 1;
                                break;
                            case Transaction.Command.RobotType.WaitBeforePut:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWaferToStandBy(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                this.RobotPutState = 1;
                                break;
                            case Transaction.Command.RobotType.GetAfterWait:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWaferToContinue(AdrNo, txn.Seq, txn.Arm, txn.Point, "0", txn.Slot);
                                this.RobotGetState = 3;
                                break;
                            case Transaction.Command.RobotType.PutWithoutBack:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWaferToDown(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                this.RobotPutState = 2;
                                break;
                            case Transaction.Command.RobotType.GetWithoutBack:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWaferToUp(AdrNo, txn.Seq, txn.Arm, txn.Point, "0", txn.Slot);
                                this.RobotGetState = 2;
                                break;
                            case Transaction.Command.RobotType.PutBack:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWaferToContinue(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                this.RobotPutState = 3;
                                break;
                            case Transaction.Command.RobotType.GetWait:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.GetWaferToReady(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                break;
                            case Transaction.Command.RobotType.PutWait:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.PutWaferToReady(AdrNo, txn.Seq, txn.Arm, txn.Point, txn.Slot);
                                break;
                            case Transaction.Command.RobotType.Home:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Home(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.HomeSafety:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.HomeSafety(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.HomeA:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.HomeOrgin(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.OrginSearch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.OrginSearch(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.WaferRelease:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.WaferReleaseHold(AdrNo, txn.Seq, txn.Arm);
                                break;
                            case Transaction.Command.RobotType.WaferHold:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.WaferHold(AdrNo, txn.Seq, txn.Arm);
                                break;
                            case Transaction.Command.RobotType.Servo:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ServoOn(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.RobotType.Mode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.Mode(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.RobotType.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ErrorReset(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.RobotType.Speed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.setSpeed(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.RobotType.SetSV:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.setSolenoidValve(AdrNo, txn.Seq, txn.Arm, txn.Value);
                                break;
                        }
                        break;
                    case "ALIGNER":
                        switch (txn.Method)
                        {
                            case Transaction.Command.AlignerType.GetPosition:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Robot.ArmLocation(AdrNo, txn.Seq, txn.Value, "1");
                                break;
                            case Transaction.Command.AlignerType.SetAlign:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.SetSize(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.AlignerType.GetStatus:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Status(AdrNo, txn.Seq);
                                break;
                            //case Transaction.Command.AlignerType.GetCombineStatus:
                            //    txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.CombinedStatus(AdrNo, txn.Seq);
                            //    break;
                            case Transaction.Command.AlignerType.GetSpeed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Speed(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.GetRIO:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.StatusIO(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.AlignerType.Stop:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.DeviceStop(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.AlignerType.Pause:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.DevicePause(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.Continue:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.DeviceContinue(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.GetSV:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.SolenoidValve(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.AlignerType.GetError:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.ErrorMessage(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.AlignerType.GetMode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.GetMode(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.Home:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Home(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.Align:

                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Align(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.AlignerType.AlignOption:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Align(AdrNo, txn.Seq, txn.Value, "1", "0", "0");
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
                                break;
                            case Transaction.Command.AlignerType.Retract:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Retract(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.WaferRelease:
                                //txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.WaferReleaseHold(AdrNo, txn.Seq);
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.WaferReleaseHold(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.WaferHold:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.WaferHold(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.Servo:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.ServoOn(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.AlignerType.Mode:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.Mode(AdrNo, txn.Seq, txn.Value);
                                break;
                            case Transaction.Command.AlignerType.OrginSearch:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.OrginSearch(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.Reset:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.ErrorReset(AdrNo, txn.Seq);
                                break;
                            case Transaction.Command.AlignerType.Speed:
                                txn.CommandEncodeStr = Ctrl.GetEncoder().Aligner.setSpeed(AdrNo, txn.Seq, txn.Value);
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
                }

                if (this.Type.Equals("ROBOT"))
                {
                    if (txn.TargetJobs.Count == 0)
                    {
                        Job tmp;
                        switch (txn.Arm)
                        {
                            case "1":
                                if (this.JobList.TryGetValue("1", out tmp))
                                {
                                    txn.TargetJobs.Add(tmp);
                                }
                                break;
                            case "2":
                                if (this.JobList.TryGetValue("2", out tmp))
                                {
                                    txn.TargetJobs.Add(tmp);
                                }
                                break;
                            case "3":
                                if (this.JobList.TryGetValue("1", out tmp))
                                {
                                    txn.TargetJobs.Add(tmp);
                                }
                                if (this.JobList.TryGetValue("2", out tmp))
                                {
                                    txn.TargetJobs.Add(tmp);
                                }
                                break;
                        }
                    }
                }
                Job TargetJob;
                if (txn.TargetJobs != null)
                {
                    if (txn.TargetJobs.Count != 0)
                    {
                        TargetJob = txn.TargetJobs[0];
                    }
                    else
                    {
                        TargetJob = RouteControl.CreateJob();
                    }
                }
                else
                {
                    TargetJob = RouteControl.CreateJob();
                }
                //if (txn.CommandType.Equals("CMD") || txn.CommandType.Equals("MOV"))
                //{
                //    this.InitialComplete = false;
                //    if ((this.InterLock || !(this.UnLockByJob.Equals(TargetJob.Job_Id) || this.UnLockByJob.Equals(""))) && !Force)
                //    {
                //        ReturnMessage tmp = new ReturnMessage();
                //        tmp.Value = "Interlock!";
                //        logger.Error(this.Name + " Interlock! Txn:" + JsonConvert.SerializeObject(txn));
                //        this.GetController()._ReportTarget.On_Command_Error(this, txn, tmp);
                //        this.IsExcuting = false;
                //        return false;
                //    }
                //    if (this.Type.Equals("LOADPORT"))
                //    {
                //        this.InterLock = true;
                //    }
                //}
                if (txn.TargetJobs.Count == 0)
                {

                    Job dummy = RouteControl.CreateJob();
                    dummy.Job_Id = "dummy";
                    txn.TargetJobs.Add(dummy);

                }
                bool IsWaitData = false;
                if (txn.Method.Equals(Transaction.Command.SmartTagType.GetLCDData))
                {
                    IsWaitData = true;
                }

                txn.AckTimeOut = this.AckTimeOut + 2000;
                logger.Debug("Ack TimeOut:" + txn.AckTimeOut.ToString());
                int rate = 101 - Convert.ToInt32(this.Speed);
                txn.MotionTimeOut = this.MotionTimeOut * rate + 25000;
                logger.Debug("Motion TimeOut:" + txn.MotionTimeOut.ToString());
                if (Ctrl.DoWork(txn, IsWaitData))
                {
                    result = true;
                }
                else
                {
                    logger.Debug("SendCommand fail.");
                    Message = "COMM_ERR";
                    result = false;
                    //if (this.Type.Equals("LOADPORT"))
                    //{
                    //    this.InterLock = false;
                    //}
                }


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

        public Job GetJob(string Slot)
        {
            Job result = null;

            //lock (JobList)
            //{
            JobList.TryGetValue(Slot, out result);
            //}

            return result;
        }
        public bool AddJob(string Slot, Job Job)
        {
            bool result = false;
            //lock (JobList)
            //{
            if (!JobList.ContainsKey(Slot))
            {
                JobList.TryAdd(Slot, Job);
                result = true;
            }
            //}
            return result;
        }

        public bool RemoveJob(string Slot)
        {
            bool result = false;
            //lock (JobList)
            //{
            if (JobList.ContainsKey(Slot))
            {
                Job tmp;
                JobList.TryRemove(Slot, out tmp);
                result = true;
            }
            //}
            return result;
        }

        public Job GetReserve(string Slot)
        {
            Job result = null;

            //lock (JobList)
            //{
            ReserveList.TryGetValue(Slot, out result);
            //}

            return result;
        }
        public bool AddReserve(string Slot, Job Job)
        {
            bool result = false;
            //lock (JobList)
            //{
            if (!ReserveList.ContainsKey(Slot))
            {
                ReserveList.TryAdd(Slot, Job);
                result = true;
            }
            //}
            return result;
        }

        public bool RemoveReserve(string Slot)
        {
            bool result = false;
            //lock (JobList)
            //{
            if (ReserveList.ContainsKey(Slot))
            {
                Job tmp;
                ReserveList.TryRemove(Slot, out tmp);
                result = true;
            }
            //}
            return result;
        }

    }
}
