using log4net;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TransferControl.Comm;
using TransferControl.Config;

namespace TransferControl.Management
{
    public class AlarmManagement
    {
        static ILog logger = LogManager.GetLogger(typeof(AlarmManagement));
        private static List<Alarm> AlarmHis = new List<Alarm>();
        private static List<Alarm> CurrentAlarm = new List<Alarm>();
        //private static List<AlarmInfo> AlarmHistory = new List<AlarmInfo>();
        private static List<AlarmInfo> AlarmData = new List<AlarmInfo>();
        public class AlarmInfo
        {
            public string vendor { get; set; }
            public string errorCode { get; set; }
            public string errDesc { get; set; }
            public bool isAlert { get; set; }
            
            
        }

        public class Alarm
        {
            public string nodeName { get; set; }
            public string vendor { get; set; }
            public string errorCode { get; set; }
            public string errDesc { get; set; }
            public bool isAlert { get; set; }
            public DateTime TimeStamp { get; set; }

            public Alarm(Node Node, string ErrorCode)
            {
                if (AlarmData != null)
                {
                    var find = from alm in AlarmData
                               where (alm.vendor.ToUpper().Equals(Node.Vendor.ToUpper()) && alm.errorCode.ToUpper().Equals(ErrorCode.ToUpper()))
                               select alm;
                    if (find.Count() != 0)
                    {
                        vendor = find.First().vendor;
                        errorCode = find.First().errorCode;
                        errDesc = find.First().errDesc;
                        isAlert = find.First().isAlert;
                    }
                    else
                    {
                        vendor = "";
                        errDesc = "Error code 不存在";
                        errorCode = ErrorCode;
                        isAlert = false;
                    }
                    TimeStamp = DateTime.Now;
                }
            }
        }

        public static void InitialAlarm()
        {
            AlarmData = new ConfigTool<List<AlarmInfo>>().ReadFile("config/error_code.json");
            AlarmHis = new ConfigTool<List<Alarm>>().ReadFile("config/AlarmHistory.json");

            if (AlarmHis == null)
            {
                AlarmHis = new List<Alarm>();
            }
        }

        public static Alarm AddToHistory(Node node, string ErrorCode)
        {
            Alarm alarm = new Alarm(node, ErrorCode);
            try
            {
                
               
                lock (AlarmHis)
                {
                    AlarmHis.Insert(0, alarm);
                    CurrentAlarm.Insert(0, alarm);
                    if (AlarmHis.Count > 2000)
                    {
                        AlarmHis.RemoveAt(AlarmHis.Count-1);
                    }
                }
                new ConfigTool<List<Alarm>>().WriteFile("config/AlarmHistory.json", AlarmHis);

            }
            catch (Exception e)
            {
                logger.Error("AddToHistory error:" + e.StackTrace);
            }
            return alarm;
        }

        public static List<Alarm> GetHistory()
        {
            List<Alarm> result = null;
            try
            {
                result = AlarmHis;
            }
            catch (Exception e)
            {
                logger.Error("GetHistory error:" + e.StackTrace);
            }

            return result;
        }
        public static List<Alarm> GetCurrent()
        {
            List<Alarm> result = null;
            try
            {
                result = CurrentAlarm;
            }
            catch (Exception e)
            {
                logger.Error("GetCurrent error:" + e.StackTrace);
            }

            return result;
        }



    }
}
