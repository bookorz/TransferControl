using log4net;
using Newtonsoft.Json;
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
        //private static List<AlarmInfo> AlarmHistory = new List<AlarmInfo>();
        private static List<AlarmInfo> AlarmData = new List<AlarmInfo>();
        public class AlarmInfo
        {
            public string NodeName { get; set; }
            public string AddressNo { get; set; }
            public string Controller { get; set; }
            public string ErrorCode { get; set; }
            public string ErrorDesc { get; set; }
            public string ALID { get; set; }
            public string ALTX { get; set; }
            public DateTime TimeStamp { get; set; }

        }

        public static void InitialAlarm()
        {
            AlarmData = new ConfigTool<List<AlarmInfo>>().ReadFile("config/error_code_en.json");
            AlarmHis = new ConfigTool<List<AlarmInfo>>().ReadFile("config/AlarmHistory.json");

            if (AlarmHis == null)
            {
                AlarmHis = new List<AlarmInfo>();
            }
        }

        public static AlarmInfo AddToHistory(string Controller,string AddressNo, string ErrorCode)
        {
            AlarmInfo almInfo = null;
            try
            {

                lock (AlarmData)
                {
                    var find = from alm in AlarmData
                               where (alm.Controller.ToUpper().Equals(Controller.ToUpper()) && alm.AddressNo.Equals(AddressNo) && alm.ErrorCode.Equals(ErrorCode))
                               select alm;
                    if (find.Count() != 0)
                    {
                        almInfo = find.First();
                        if (almInfo.NodeName == null)
                        {
                            almInfo.NodeName = "";
                        }
                    }
                    else
                    {
                        almInfo = new AlarmInfo();
                        almInfo.Controller = Controller;
                        almInfo.ErrorCode = ErrorCode;
                        almInfo.ErrorDesc = "Error code not exist";
                        almInfo.ALID = "";
                        almInfo.ALTX = "";
                        almInfo.NodeName = "";
                    }
                    if (almInfo.NodeName.Equals(""))
                    {
                        almInfo.NodeName = NodeManagement.GetByController(Controller, AddressNo) != null ? NodeManagement.GetByController(Controller, AddressNo).Name : "";
                    }
                    almInfo.TimeStamp = DateTime.Now;
                }


                lock (AlarmHis)
                {
                    AlarmHis.Insert(0,almInfo);
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




    }
}
