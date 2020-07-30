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
        private static Dictionary<string, Alarm> CurrentAlarm = new Dictionary<string, Alarm>();
        //private static List<AlarmInfo> AlarmHistory = new List<AlarmInfo>();
        private static List<AlarmInfo> AlarmData = new List<AlarmInfo>();
        private class AlarmInfo
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
            public string taskID { get; set; }
            public DateTime TimeStamp { get; set; }

            
        }
        public static Alarm NewAlarm(Node Node, string ErrorCode, string TaskID = "")
        {
            Alarm result = new Alarm();
            result.nodeName = Node == null ? "SYSTEM" : (Node.Name==null? "SYSTEM" : Node.Name);
            result.errorCode = ErrorCode;
            result.TimeStamp = DateTime.Now;
            result.taskID = TaskID;

            if (AlarmData != null && Node != null)
            {
                try
                {
                    var find = from alm in AlarmData
                               where (alm.vendor.ToUpper().Equals(Node.Vendor.ToUpper()) && alm.errorCode.ToUpper().Equals(ErrorCode.ToUpper()))
                               select alm;
                    if (find.Count() != 0)
                    {
                        result.vendor = find.First().vendor;
                        result.errorCode = find.First().errorCode;
                        result.errDesc = find.First().errDesc;
                        result.isAlert = find.First().isAlert;
                    }
                    else
                    {
                        result.vendor = "";
                        result.errDesc = "Error code 不存在";
                        result.errorCode = ErrorCode;
                        result.isAlert = true;
                    }
                }catch(Exception e)
                {
                    result.vendor = "";
                    result.errDesc = "Error code 不存在";
                    result.errorCode = ErrorCode;
                    result.isAlert = true;
                }

            }
            else
            {
                result.vendor = "";
                result.errDesc = "Error code 不存在";
                result.errorCode = ErrorCode;
                result.isAlert = true;
            }
            lock (CurrentAlarm)
            {
                if (CurrentAlarm.ContainsKey(result.nodeName+result.errorCode))
                {
                    CurrentAlarm[result.nodeName + result.errorCode] = result;
                }
                else
                {
                    CurrentAlarm.Add(result.nodeName + result.errorCode, result);
                }
            }
            AlarmHis.Insert(0, result);
            if (AlarmHis.Count > 5000)
            {
                AlarmHis.RemoveAt(AlarmHis.Count - 1);
            }
            lock (AlarmHis)
            {
                new ConfigTool<List<Alarm>>().WriteFile("config/AlarmHistory.json", AlarmHis);
            }
            return result;
        }

        public static void InitialAlarm()
        {
            AlarmData = new ConfigTool<List<AlarmInfo>>().ReadFile("config/error_code_en.json");
            AlarmHis = new ConfigTool<List<Alarm>>().ReadFile("config/AlarmHistory.json");

            if (AlarmHis == null)
            {
                AlarmHis = new List<Alarm>();
            }
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
                result = CurrentAlarm.Values.ToList();
                result.Sort((x, y) => { return y.TimeStamp.CompareTo(x.TimeStamp); });
            }
            catch (Exception e)
            {
                logger.Error("GetCurrent error:" + e.StackTrace);
            }

            return result;
        }

        public static void ClearALL()
        {
            
            try
            {
                CurrentAlarm.Clear();
            }
            catch (Exception e)
            {
                logger.Error("GetCurrent error:" + e.StackTrace);
            }

            
        }
        public static void Clear(string NodeName)
        {

            try
            {
                var find = from alm in CurrentAlarm.Values.ToList()
                           where alm.nodeName.Equals(NodeName)
                           select alm;
                if (find.Count() != 0)
                {
                    foreach(Alarm each in find)
                    {
                        CurrentAlarm.Remove(each.errorCode);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("GetCurrent error:" + e.StackTrace);
            }


        }
    }
}
