using System.Collections.Generic;


namespace TransferControl.Management
{
    public class Transaction
    {
        public string uuid { get; set; }
        public string AdrNo { get; set; }
        public string Seq { get; set; }
        public string NodeName { get; set; }
        public string NodeType { get; set; }
        public string Position { get; set; }
        public string Point { get; set; }
        public string Position2 { get; set; }
        public string Point2 { get; set; }
        public string Slot { get; set; }
        public string Slot2 { get; set; }
        public string Method { get; set; }
        public string Arm { get; set; }
        public string Arm2 { get; set; }
        public string Value { get; set; }
        public string Val2 { get; set; }
        public string CommandType { get; set; }
        public string CommandEncodeStr { get; set; }
        public byte ModbusSlaveID { get; set; }
        public string ModbusMethod { get; set; }
        public ushort ModbusRegisterAddress { get; set; }
        public ushort ModbusStartAddress { get; set; }
        public ushort ModbusNumOfPoints { get; set; }
        public ushort ModbusValue { get; set; }
        public string Type { get; set; }
        public int Piority { get; set; }
        public string ScriptName { get; set; }
        public string ScriptIndex { get; set; }
        public bool LastOneScript { get; set; }
        public TaskFlowManagement.CurrentProcessTask TaskObj { get; set; }
        public bool ByPassTimeout { get; set; }
        public int AckTimeOut { get; set; }
        public int MotionTimeOut { get; set; }

        public int PLC_Station { get; set; }
        public string PLC_Area { get; set; }
        public int PLC_StartAddress { get; set; }
        public int PLC_Len { get; set; }
        public byte PLC_Bit { get; set; }
        public ushort[] PLC_Word { get; set; }
        public bool WaitEvent { get; set; }

        //逾時
        private System.Timers.Timer timeOutTimer = new System.Timers.Timer();
        ITransactionReport TimeOutReport;

        public int RetryTime = 0;

        public class Command
        {
            public class Mitsubishi_PLC
            {
                public const string ReadBit = "ReadBit";
                public const string ReadWord = "ReadWord";
                public const string WriteBit = "WriteBit";
                public const string WriteWord = "WriteWord";
            }
            public class WTSAligner
            {
                public const string Align = "Align";
                public const string Reset = "Reset";
                public const string SetSpeed = "SetSpeed";
                public const string Pause = "Pause";
                public const string Continue = "Continue";
                public const string Stop = "Stop";
            }
            public class ELPT
            {
                public const string Clamp = "Clamp";
                public const string UnClamp = "UnClamp";
                public const string OpenShutter = "OpenShutter";
                public const string CloseShutter = "CloseShutter";
                public const string ReadCID = "ReadCID";
                public const string MoveIn = "MoveIn";
                public const string MoveOut = "MoveOut";
                public const string getOutSensor = "getOutSensor";
                public const string getInSensor = "getInSensor";
                public const string getFOUPPresence = "getFOUPPresence";
                public const string OrgSearch = "OrgSearch";
                public const string Reset = "Reset";
                public const string SetSpeed = "SetSpeed";
                public const string LightCurtainEnabled = "LightCurtainEnabled";
                public const string LightCurtainReset = "LightCurtainReset";
            }
            public class ILPT
            {
                public const string Clamp = "Clamp";
                public const string Unclamp = "Unclamp";
                public const string Dock = "Dock";
                public const string Undock = "Undock";
                public const string OpenLatch = "OpenLatch";
                public const string CloseLatch = "CloseLatch";
                public const string VacuumOn = "VacuumOn";
                public const string VacuumOff = "VacuumOff";
                public const string OpenDoor = "OpenDoor";
                public const string CloseDoor = "CloseDoor";
                public const string UpDoor = "UpDoor";
                public const string DownDoor = "DownDoor";
                public const string OpenLower = "OpenLower";
                public const string RaiseClose = "RaiseClose";
                public const string Map = "Map";
                public const string Load = "Load";
                public const string Unload = "Unload";
                public const string OrgSearch = "OrgSearch";
                public const string Reset = "Reset";
            }
            public class FoupRobot
            {
                public const string PreparePick = "PreparePick";
                public const string Pick = "Pick";
                public const string PreparePlace = "PreparePlace";
                public const string Place = "Place";
                public const string Extend = "Extend";
                public const string Retract = "Retract";
                public const string Grab = "Grab";
                public const string Release = "Release";
                public const string Up = "Up";
                public const string Down = "Down ";
                public const string SHome = "SHome";
                public const string Transfer = "Transfer";
                public const string Reset = "Reset";
                public const string SetSpeed = "SetSpeed";
                public const string Pause = "Pause";
                public const string Continue = "Continue";
                public const string Stop = "Stop";
                public const string Initial_IO = "Initial_IO";
                public const string Get_IO = "Get_IO";
            }
            public class WHR
            {
                public const string PreparePick = "PreparePick";
                public const string Pick = "Pick";
                public const string PreparePlace = "PreparePlace";
                public const string Place = "Place";
                public const string ToPick = "ToPick";
                public const string CompletePick = "CompletePick";
                public const string ToPlace = "ToPlace";
                public const string CompletePlace = "CompletePlace";
                public const string Extend = "Extend";
                public const string Retract = "Retract ";
                public const string Up = "Up";
                public const string Down = "Down ";
                public const string SHome = "SHome";
                public const string Reset = "Reset";
                public const string SetSpeed = "SetSpeed";
                public const string Pause = "Pause";
                public const string Continue = "Continue";
                public const string Stop = "Stop";
                public const string Get_IO = "Get_IO";
            }
            public class CTU
            {
                public const string Pick = "Pick";
                public const string Place = "Place";
                public const string Hold = "Hold";
                public const string Release = "Release";
                //public const string PrepareForRobot = "PrepareForRobot";
                //public const string PrepareForPTZ = "PrepareForPTZ";
                public const string Home = "Home";
                public const string OrgSearch = "OrgSearch";
                public const string Reset = "Reset";
                public const string SetSpeed = "SetSpeed";
                public const string Pause = "Pause";
                public const string Continue = "Continue";
                public const string Stop = "Stop";
                public const string Initial_IO = "Initial_IO";
                public const string Get_IO = "Get_IO";
            }
            public class PTZ
            {
                public const string Rotate = "Rotate";
                public const string Home = "Home";
                public const string Transfer = "Transfer";
                public const string Reset = "Reset";
                public const string SetSpeed = "SetSpeed";
                public const string SetPath = "SetPath";

            }
            public class Shelf
            {
                public const string GetFOUPPresence = "GetFOUPPresence";
            }
            public class ModbusMethod
            {
                public const string ReadHoldingRegisters = "ReadHoldingRegisters";
                public const string WriteSingleRegister = "WriteSingleRegister";
            }
            public class SmartTagType
            {
                public const string Hello = "Hello";
                public const string GetLCDData = "GetLCDData";
                public const string SelectLCDData = "SelectLCDData";
                public const string SetLCDData = "SetLCDData";
            }

            public class RFIDType
            {
                public const string Hello = "Hello";
                public const string GetCarrierID = "GetCarrierID";
                public const string SetCarrierID = "SetCarrierID";
                public const string Mode = "Mode";
            }

            //LoadPort
            public class LoadPortType
            {
                public const string GetSpeed = "GetSpeed";
                public const string SetSpeed = "SetSpeed";
                public const string Stop = "Stop";
                public const string GetTweekDistance = "GetTweekDistance";
                public const string GetSlotPitch = "GetSlotPitch";
                public const string GetWaferOffset = "GetWaferOffset";
                public const string GetSlotOffset = "GetSlotOffset";
                public const string GetCassetteSize = "GetCassetteSize";
                public const string SetTweekDistance = "SetTweekDistance";
                public const string SetSlotPitch = "SetSlotPitch";
                public const string SetWaferOffset = "SetWaferOffset";
                public const string SetSlotOffset = "SetSlotOffset";
                public const string SetCassetteSize = "SetCassetteSize";
                public const string TweekDn = "TweekDn";
                public const string TweekUp = "TweekUp";
                public const string SetAllEvent = "SetAllEvent";
                public const string SetCompleteEvent = "SetCompleteEvent";
                public const string SetFoupEvent = "SetFoupEvent";
                public const string MoveToSlot = "MoveToSlot";
                public const string EQASP = "EQASP";
                public const string Mode = "Mode";
                public const string Load = "Load";
                public const string LoadWithLift = "LoadWithLift"; 
                public const string Mapping = "Mapping";
                public const string RetryMapping = "RetryMapping";
                public const string MappingLoad = "MappingLoad";
                public const string Unload = "Unload";
                public const string MappingUnload = "MappingUnload";
                public const string GetMapping = "GetMapping";
                public const string GetMappingDummy = "GetMappingDummy";
                public const string GetMappingData = "GetMappingData";
                public const string GetLED = "GetLED";
                //public const string GetStatus = "GetStatus";
                public const string Reset = "Reset";
                public const string InitialPos = "InitialPos";
                public const string ForceInitialPos = "ForceInitialPos";
                public const string GetCount = "GetCount";
                public const string UnClamp = "UnClamp";
                public const string Clamp = "Clamp";
                public const string UnDock = "UnDock";
                public const string Dock = "Dock";
                public const string VacuumOFF = "VacuumOFF";
                public const string VacuumON = "VacuumON";
                public const string UnLatchDoor = "UnLatchDoor";
                public const string LatchDoor = "LatchDoor";
                public const string DoorClose = "DoorClose";
                public const string DoorOpen = "DoorOpen";
                public const string DoorUp = "DoorUp";
                public const string DoorDown = "DoorDown";
                public const string ReadVersion = "ReadVersion";
                public const string ReadStatus = "ReadStatus";
                public const string MapperWaitPosition = "MapperWaitPosition";
                public const string MapperStartPosition = "MapperStartPosition";
                public const string MapperArmRetracted = "MapperArmRetracted";
                public const string MapperArmStretch = "MapperArmStretch";
                public const string MapperStopperOn = "MapperStopperOn";
                public const string MapperStopperOff = "MapperStopperOff";
                public const string MappingDown = "MappingDown";
                public const string SaveLog = "SaveLog";
                public const string SetOpAccess = "SetOpAccess";
                public const string SetOpAccessBlink = "SetOpAccessBlink";
                public const string SetLoad = "SetLoad";
                public const string SetUnLoad = "SetUnLoad";
                public const string UntilUnDock = "UntilUnDock";
                public const string DockingPositionNoVac = "DockingPositionNoVac";
                public const string UntilDoorCloseVacOFF = "UntilDoorCloseVacOFF";
                //取得CST模式
                public const string GetOPMode = "GetOPMode";                        
            }


            //Robot
            public class RobotType
            {
                public const string GetPresence = "GetPresence";
                public const string GetPosition = "GetPosition";
                public const string ArmReturn = "ArmReturn";
                public const string Exchange = "Exchange";
                public const string Get = "Get";
                public const string Initialize = "Initialize";
                public const string DoubleGet = "DoubleGet";
                public const string WaitBeforeGet = "WaitBeforeGet";
                public const string WaitBeforePut = "WaitBeforePut";
                public const string GetAfterWait = "GetAfterWait";
                public const string Put = "Put";
                public const string PutWithoutBack = "PutWithoutBack";
                public const string GetWithoutBack = "GetWithoutBack";
                public const string PutBack = "PutBack";
                public const string DoublePut = "DoublePut";
                public const string GetWait = "GetWait";
                public const string PutWait = "PutWait";
                public const string WaferHold = "WaferHold";
                public const string WaferRelease = "WaferRelease";
                public const string Home = "Home";
                public const string HomeA = "HomeA";
                public const string HomeSafety = "HomeSafety";
                public const string OrginSearch = "OrginSearch";
                public const string Mode = "Mode";
                public const string Speed = "Speed";
                public const string Reset = "Reset";
                public const string GetStatus = "GetStatus";
                public const string GetCombineStatus = "GetCombineStatus";
                public const string GetSpeed = "GetSpeed";
                public const string GetRIO = "GetRIO";
                public const string GetError = "GetError";
                public const string SaveLog = "SaveLog";
                public const string SetSV = "SetSV";
                public const string Servo = "Servo";
                public const string Stop = "Stop";
                public const string Pause = "Pause";
                public const string Continue = "Continue";
                public const string GetMode = "GetMode";
                public const string GetSV = "GetSV";
                public const string Mapping = "Mapping";
                public const string PreMapping = "PreMapping";
                public const string GetMapping = "GetMapping";
                public const string PutByRArm = "PutByRArm";
                public const string PutByLArm = "PutByLArm";
                public const string GetByRArm = "GetByRArm";
                public const string GetByLArm = "GetByLArm";
                public const string PutWaitByRArm = "PutWaitByRArm";
                public const string PutWaitByLArm = "PutWaitByLArm";
                public const string GetWaitByRArm = "GetWaitByRArm";
                public const string GetWaitByLArm = "GetWaitByLArm";
            }
            //Aligner
            public class AlignerType
            {
                public const string GetPosition = "GetPosition";
                public const string SetAlign = "SetAlign";
                public const string Align = "Align";
                public const string AlignOption = "AlignOption";
                public const string AlignOffset = "AlignOffset";
                public const string WaferHold = "WaferHold";
                public const string WaferRelease = "WaferRelease";
                public const string Retract = "Retract";
                public const string Mode = "Mode";
                public const string Speed = "Speed";
                public const string OrginSearch = "OrginSearch";
                public const string Servo = "Servo";
                public const string Home = "Home";
                public const string GetStatus = "GetStatus";
                public const string GetCombineStatus = "GetCombineStatus";
                public const string Reset = "Reset";
                public const string GetSpeed = "GetSpeed";
                public const string GetRIO = "GetRIO";
                public const string Stop = "Stop";
                public const string Pause = "Pause";
                public const string Continue = "Continue";
                public const string GetError = "GetError";
                public const string GetMode = "GetMode";
                public const string GetSV = "GetSV";
                public const string SetSV = "SetSV";
                public const string SaveLog = "SaveLog";
            }
            //OCR
            public class OCRType
            {
                public const string Read = "Read";
                public const string ReadM12 = "ReadM12";
                public const string ReadT7 = "ReadT7";
                public const string Online = "Online";
                public const string Offline = "Offline";
                public const string GetOnline = "GetOnline";
                public const string ReadConfig = "ReadConfig";
                public const string SetConfigEnable = "SetConfigEnable";
            }

            //FFU
            public class FFUType
            {
                public const string Start = "Start";
                public const string End = "End";
                public const string AlarmBypass = "AlarmBypass";
                public const string SetSpeed = "SetSpeed";
                public const string GetStatus = "GetStatus";
            }

            public class E84
            {
                public const string Reset = "Reset";
                public const string SetAutoMode = "SetAutoMode";
                public const string SetManualMode = "SetManualMode";
                public const string GetE84IOStatus = "GetE84IOStatus";
                public const string GetDIOStatus = "GetDIOStatus";
                public const string SetTP1 = "TP1";
                public const string SetTP2 = "TP2";
                public const string SetTP3 = "TP3";
                public const string SetTP4 = "TP4";
                public const string SetTP5 = "TP5";
                public const string SetTP6 = "TP6";
            }
        }

        public Transaction()
        {
            AdrNo = "";
            NodeType = "";
            Position = "";
            Point = "";
            Position2 = "";
            Point2 = "";
            Slot = "";
            Slot2 = "";
            Method = "";
            Arm = "";
            Arm2 = "";
            Value = "";
            Val2 = "";
            CommandType = "";
            CommandEncodeStr = "";
            ScriptName = "";
            Type = "";
            WaitEvent = false;
            
            ByPassTimeout = false;
            timeOutTimer.Enabled = false;

            //timeOutTimer.Interval = 10000;
            timeOutTimer.Interval = 30000;

            timeOutTimer.Elapsed += new System.Timers.ElapsedEventHandler(TimeOutMonitor);

        }

        public void SetTimeOut(int Timeout)
        {
            timeOutTimer.Interval = Timeout;
        }

        public void SetTimeOutMonitor(bool Enabled)
        {
            //Enabled = false;
            if (Enabled)
            {
                timeOutTimer.Enabled = true;
                timeOutTimer.Start();
            }
            else
            {
                timeOutTimer.Stop();
                timeOutTimer.Enabled = false;
            }
        }

        public void SetTimeOutReport(ITransactionReport _TimeOutReport)
        {
            TimeOutReport = _TimeOutReport;
        }

        private void TimeOutMonitor(object sender, System.Timers.ElapsedEventArgs e)
        {
            SetTimeOutMonitor(false);
            if (TimeOutReport != null)
            {
                if (ByPassTimeout)
                {
                    TimeOutReport.On_Transaction_BypassTimeOut(this);
                }
                else
                {
                    TimeOutReport.On_Transaction_TimeOut(this);
                }
            }
        }
    }
}
