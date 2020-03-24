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
        private static List<AlarmInfo> AlarmHis = new List<AlarmInfo>();
        private static List<AlarmInfo> CurrentAlarm = new List<AlarmInfo>();
        //private static List<AlarmInfo> AlarmHistory = new List<AlarmInfo>();
        private static List<AlarmInfo> AlarmData = new List<AlarmInfo>();
        public class AlarmInfo
        {
            public string nodeType { get; set; }
            public string vendor { get; set; }
            public string errorCode { get; set; }
            public string errDesc { get; set; }
            public string errDescEn { get; set; }
            public bool isAlert { get; set; }
            

        }

        public class Alarm
        {
            public string nodeName { get; set; }
            public AlarmInfo Detail { get; set; }
            public DateTime TimeStamp { get; set; }
        }

        public static void InitialAlarm()
        {
            AlarmData = new ConfigTool<List<AlarmInfo>>().ReadFile("config/error_code.json");
            AlarmHis = new ConfigTool<List<AlarmInfo>>().ReadFile("config/AlarmHistory.json");

            if (AlarmHis == null)
            {
                AlarmHis = new List<AlarmInfo>();
            }
        }

        public static Alarm AddToHistory(Node node, string ErrorCode)
        {
            Alarm alarm = new Alarm();
            try
            {

                lock (AlarmData)
                {
                    if (ErrorCode.Equals("ConnectionError"))
                    {
                        alarm.Detail = new AlarmInfo();
                        alarm.Detail.nodeType = node.Type;
                        alarm.Detail.vendor = node.Vendor;
                        alarm.Detail.errDesc = "可能是纜線問題，請確認纜線連接狀況。";
                        alarm.Detail.errDescEn = "Caused by abnormal connection, please check the cable to troubleshoot the problem.";

                    }
                    else
                    {
                        var find = from alm in AlarmData
                                   where (alm.nodeType.Equals(node.Type) && alm.vendor.Equals(node.Vendor) && alm.errorCode.Equals(ErrorCode))
                                   select alm;
                        if (find.Count() != 0)
                        {
                            alarm.Detail = find.First();
                           
                        }
                        else
                        {
                            AlarmInfo almInfo = new AlarmInfo();
                            almInfo.nodeType = node.Type;
                            almInfo.vendor = node.Vendor;
                            almInfo.errDescEn = "Error code not exist";
                            almInfo.errDesc = "Error code 不存在";
                            almInfo.errorCode = ErrorCode;
                            alarm.Detail = almInfo;
                        }
                        
                    }
                    alarm.TimeStamp = DateTime.Now;
                    
                }


                lock (AlarmHis)
                {
                    AlarmHis.Insert(0,almInfo);
                    CurrentAlarm.Insert(0, almInfo);
                    if (AlarmHis.Count > 2000)
                    {
                        AlarmHis.RemoveAt(AlarmHis.Count-1);
                    }
                }
                new ConfigTool<List<AlarmInfo>>().WriteFile("config/AlarmHistory.json", AlarmHis);

            }
            catch (Exception e)
            {
                logger.Error("AddToHistory error:" + e.StackTrace);
            }
            return almInfo;
        }

        public static List<AlarmInfo> GetHistory()
        {
            List<AlarmInfo> result = null;
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
        public static List<AlarmInfo> GetCurrent()
        {
            List<AlarmInfo> result = null;
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
