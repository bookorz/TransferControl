using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using log4net;
using TransferControl.Comm;
using TransferControl.Config;
using LiteDB;

namespace TransferControl.Management
{
    /// <summary>
    /// Alarm Message Mapping
    /// </summary>
    public class AlarmMapping
    {
        //private DataTable dtCode;
        private static readonly ILog logger = LogManager.GetLogger(typeof(AlarmMapping));

        public AlarmMapping()
        {
           
            //string strSql = string.Empty;
            //DBUtil dBUtil = new DBUtil();
            //Dictionary<string, object> keyValues = new Dictionary<string, object>();
            //try
            //{
               

            //    dtCode = new DataTable();

            //    strSql = "SELECT * FROM view_alarm_code WHERE equipment_model_id = @equipment_model_id";

            //    keyValues.Add("@equipment_model_id", Config.SystemConfig.Get().SystemMode);

            //    dtCode = dBUtil.GetDataTable(strSql, keyValues);

            //    if (dtCode.Rows.Count < 0)
            //    {
            //        throw new Exception("SANWA.Utility.AlarmMapping\r\nException: Code List not exists.");
            //    }

            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.StackTrace, ex);
            //    throw new Exception(ex.ToString());
            //}
            //finally
            //{
               
            //}

        }
        public class view_alarm_code
        {
            public string equipment_model_id { get; set; }
            public string node_type { get; set; }
            public string vendor { get; set; }
            public string node_id { get; set; }
            public string sys_code_id { get; set; }
            public string device_code_id { get; set; }
            public string device_address_id { get; set; }
            public string error_code_id { get; set; }
            public string model_no { get; set; }
            public string category { get; set; }
            public string return_code_type { get; set; }
            public string return_code { get; set; }
            public string code_name { get; set; }
            public string code_desc { get; set; }
            public string code_desc_en { get; set; }
            public string is_stop { get; set; }
            public string active { get; set; }
            public string location { get; set; }

        }
        /// <summary>
        /// Get alarm message
        /// </summary>
        /// <param name="node_id"> 設備系統代碼 </param>
        /// <param name="error_message"> 錯誤訊息 </param>
        /// <returns></returns>
        public AlarmMessage Get(string node_id, string error_message)
        {
            AlarmMessage alarm;
            DataTable dtTemp;

            string strAlarmAxis = string.Empty;
            string strAlarmAxisEnglish = string.Empty;
            string strCode28 = string.Empty;
            //int itFirst = 0;
            //int itCode28 = 0;
       
            

           

            try
            {
                Node Target = NodeManagement.Get(node_id);
                
               

                alarm = new AlarmMessage();
                alarm.Return_Code_ID = error_message;

                using (var db = new LiteDatabase(@"Filename=config\MyData.db;Connection=shared;"))
                {
                    // Get customer collection

                    var col = db.GetCollection<view_alarm_code>("view_alarm_code");
                    var result = Target is null?
                        col.Query().Where(x => x.equipment_model_id.Equals(SystemConfig.Get().SystemMode) &&
                                                   x.node_type.Equals("SYSTEM") &&
                                                   x.return_code.Equals(error_message))
                        :
                        col.Query().Where(x => x.equipment_model_id.Equals(SystemConfig.Get().SystemMode) && 
                                                   x.node_type.Equals(Target.Type) &&
                                                   x.vendor.Equals( Target.Brand) &&
                                                   x.return_code.Equals(error_message) &&
                                                   x.device_address_id.Equals( Target.AdrNo));



                    //var query = (from a in dtCode.AsEnumerable()
                    //             where a.Field<string>("node_type") == eqp_type.ToUpper()
                    //                && a.Field<string>("vendor") == supplier.ToUpper()
                    //                && a.Field<string>("return_code").ToUpper() == strErrorCode.ToUpper()
                    //                && a.Field<string>("device_address_id") == address
                    //             select a).ToList();

                    if (result.Count() > 0)
                    {
                        view_alarm_code each = result.First();
                        alarm.CodeID = strAlarmAxis == string.Empty ? each.sys_code_id  : each.device_code_id + alarm.Return_Code_ID;
                        alarm.IsStop = each.is_stop == "Y" ? true : false;
                        alarm.Code_Type = each.return_code_type;
                        alarm.Code_Name = each.code_name;
                        alarm.Code_Cause = strAlarmAxis == string.Empty ? each.code_desc : strAlarmAxis + " " + each.code_desc;
                        alarm.Code_Cause_English = strAlarmAxisEnglish == string.Empty ? each.code_desc_en : strAlarmAxisEnglish + " " + each.code_desc_en;
                        alarm.Position = each.location;
                        alarm.Code_Group = each.category;
                    }
                    else
                    {
                        alarm.CodeID = error_message;
                        alarm.IsStop = false;
                        alarm.Code_Type = error_message;
                        alarm.Code_Name = error_message;
                        alarm.Code_Cause = "未知";
                        alarm.Code_Cause_English = "unknown";
                        alarm.Position = string.Empty;
                        alarm.Code_Group = "UNDEFINITION";
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return alarm;
        }

        ///// <summary>
        ///// Get alarm message only ATEL
        ///// </summary>
        ///// <param name="eqp_type"> 設備種類 </param>
        ///// <param name="Command"> 輸入命令 </param>
        ///// <param name="error_message"> 錯誤訊息 </param>
        ///// <returns></returns>
        //public AlarmMessage GetATEL(string eqp_type, string Command, string error_message)
        //{
        //    AlarmMessage alarm;
        //    DataTable dtTemp;
        //    string supplier = "ATEL";
        //    string strAlarmType = string.Empty;
        //    string strAlarmCode = string.Empty;
        //    string strAlarmAxis = string.Empty;
        //    string strAlarmAxisEnglish = string.Empty;
        //    string strSql = string.Empty;
        //    DBUtil dBUtil = new DBUtil();

        //    try
        //    {
        //        alarm = new AlarmMessage();

        //        var query = (from a in dtCode.AsEnumerable()
        //                     where a.Field<string>("node_type") == eqp_type.ToUpper()
        //                        && a.Field<string>("vendor") == supplier.ToUpper()
        //                        && Command.Contains(a.Field<string>("code_type"))
        //                        && a.Field<string>("code_id") == error_message.ToUpper()
        //                     select a).ToList();

        //        if (query.Count > 0)
        //        {
        //            dtTemp = query.CopyToDataTable();
        //            strAlarmType = dtTemp.Rows[0]["alarm_code_id"].ToString();
        //            strAlarmCode = dtTemp.Rows[0]["Code_ID"].ToString();
        //            alarm.CodeID = string.Format("{0}{1}", strAlarmType, strAlarmCode);
        //            alarm.IsStop = dtTemp.Rows[0]["Is_stop"].ToString() == "Y" ? true : false;

        //            alarm.Code_Type = dtTemp.Rows[0]["Code_Type"].ToString();
        //            alarm.Code_Name = dtTemp.Rows[0]["Code_Name"].ToString();
        //            alarm.Code_Cause = strAlarmAxis == string.Empty ? dtTemp.Rows[0]["Code_Desc"].ToString() : strAlarmAxis + " " + dtTemp.Rows[0]["Code_Desc"].ToString();
        //            alarm.Code_Cause_English = strAlarmAxisEnglish == string.Empty ? dtTemp.Rows[0]["Code_Desc_EN"].ToString() : strAlarmAxisEnglish + " " + dtTemp.Rows[0]["Code_Desc_EN"].ToString();
        //        }
        //        else
        //        {
        //            throw new Exception(string.Format("SANWA.Utility.AlarmMapping\r\nException: {0} {1} Alarm type not exists.", supplier.ToUpper(), eqp_type.ToUpper()));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.ToString());
        //    }

        //    return alarm;
        //}
    }
}
