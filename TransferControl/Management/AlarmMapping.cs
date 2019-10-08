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

namespace TransferControl.Management
{
    /// <summary>
    /// Alarm Message Mapping
    /// </summary>
    public class AlarmMapping
    {
        private static DataTable dtCode = null;
        private static object locker = new object();
        private static readonly ILog logger = LogManager.GetLogger(typeof(AlarmMapping));

        public static void Init()
        {
           
            string strSql = string.Empty;
            DBUtil dBUtil = new DBUtil();
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            try
            {
               

                dtCode = new DataTable();

                strSql = "SELECT * FROM view_alarm_code WHERE equipment_model_id = @equipment_model_id";

                keyValues.Add("@equipment_model_id", Config.SystemConfig.Get().SystemMode);

                dtCode = dBUtil.GetDataTable(strSql, keyValues);

                if (dtCode.Rows.Count < 0)
                {
                    throw new Exception("SANWA.Utility.AlarmMapping\r\nException: Code List not exists.");
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.StackTrace, ex);
                throw new Exception(ex.ToString());
            }
            finally
            {
               
            }

        }

        /// <summary>
        /// Get alarm message
        /// </summary>
        /// <param name="node_id"> 設備系統代碼 </param>
        /// <param name="error_message"> 錯誤訊息 </param>
        /// <returns></returns>
        public static AlarmMessage Get(string node_id, string error_message)
        {
            lock (locker)
            {
                if (dtCode == null)
                {
                    Init();
                }
            }
            AlarmMessage alarm;
            DataTable dtTemp;

            string strAlarmAxis = string.Empty;
            string strAlarmAxisEnglish = string.Empty;
            string strCode28 = string.Empty;
            //int itFirst = 0;
            //int itCode28 = 0;
            string strErrorCode = string.Empty;
            string strSql = string.Empty;
            DBUtil dBUtil = new DBUtil();

            string supplier = string.Empty;
            string eqp_type = string.Empty;
            string address = string.Empty;

            try
            {
                strSql = "select * from config_node where node_id = '" + node_id + "' and equipment_model_id = '"+SystemConfig.Get().SystemMode+"'";
                dtTemp = dBUtil.GetDataTable(strSql, null);

                if (dtTemp.Rows.Count > 0)
                {
                    supplier = dtTemp.Rows[0]["vendor"].ToString();
                    eqp_type = dtTemp.Rows[0]["node_type"].ToString();
                    address = dtTemp.Rows[0]["sn_no"].ToString();
                }
                else
                {
                    supplier = "SANWA";
                    eqp_type = "SYSTEM";
                    address = "0";
                }

                alarm = new AlarmMessage();
                alarm.Return_Code_ID = error_message;

                // * Special rule
                switch (supplier.ToUpper())
                {
                    //case "SANWA":

                    //    int s = 0;
                    //    if (int.TryParse(error_message.Substring(0, 1), out s))
                    //    {
                    //        itFirst = s;
                    //        strCode28 = Convert.ToString(itFirst, 2);
                    //        itCode28 = int.Parse(strCode28.Substring(strCode28.Length - 1, 1));

                    //        if (itCode28 == 1)
                    //        {
                    //            strSql = "SELECT * " +
                    //                        "FROM config_list_item " +
                    //                        "WHERE list_type = 'SANWA_CODE' " +
                    //                        "  AND list_value = '" + error_message.Substring(5, 1) + "'" +
                    //                        "ORDER BY sort_sequence ASC";

                    //            dtTemp = dBUtil.GetDataTable(strSql, null);

                    //            if (dtTemp.Rows.Count > 0)
                    //            {
                    //                strAlarmAxis = dtTemp.Rows[0]["list_name"].ToString();
                    //                strAlarmAxisEnglish = dtTemp.Rows[0]["list_name_en"].ToString();
                    //            }
                    //            else
                    //            {
                    //                throw new Exception("SANWA.Utility.AlarmMapping\r\nException: Alarm Axis Code not exists.");
                    //            }

                    //            strErrorCode = error_message.Substring(0, 5) + "0" + error_message.Substring(6, 2);

                    //        }
                    //        else
                    //        {
                    //            strErrorCode = error_message;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        strErrorCode = error_message;
                    //    }

                    //    break;

                    //case "TDK":
                    //case "KAWASAKI":
                    //case "ATEL":

                    //    strErrorCode = error_message;

                    //    break;

                    //case "ASYST":

                    //    for (int i = 0; i < error_message.Split(' ').Length; i++)
                    //    {
                    //        if (i >= 2)
                    //        {
                    //            if (strErrorCode.Length == 0)
                    //            {
                    //                strErrorCode = error_message.Split(' ')[i].ToString();
                    //            }
                    //            else
                    //            {
                    //                strErrorCode = strErrorCode + " " + error_message.Split(' ')[i].ToString();
                    //            }
                    //        }
                    //    }

                    //    break;

                    default:

                        strErrorCode = error_message;

                        break;

                }

                var query = (from a in dtCode.AsEnumerable()
                             where a.Field<string>("node_type") == eqp_type.ToUpper()
                                && a.Field<string>("vendor") == supplier.ToUpper()
                                && a.Field<string>("return_code").ToUpper() == strErrorCode.ToUpper()
                                && a.Field<string>("device_address_id") == address
                             select a).ToList();

                if (query.Count > 0)
                {
                    dtTemp = query.CopyToDataTable();
                    alarm.CodeID = strAlarmAxis == string.Empty ? dtTemp.Rows[0]["sys_code_id"].ToString() : dtTemp.Rows[0]["device_code_id"].ToString() + alarm.Return_Code_ID;
                    alarm.IsStop = dtTemp.Rows[0]["Is_stop"].ToString() == "Y" ? true : false;
                    alarm.Code_Type = dtTemp.Rows[0]["return_code_type"].ToString();
                    alarm.Code_Name = dtTemp.Rows[0]["Code_Name"].ToString();
                    alarm.Code_Cause = strAlarmAxis == string.Empty ? dtTemp.Rows[0]["Code_Desc"].ToString() : strAlarmAxis + " " + dtTemp.Rows[0]["Code_Desc"].ToString();
                    alarm.Code_Cause_English = strAlarmAxisEnglish == string.Empty ? dtTemp.Rows[0]["Code_Desc_EN"].ToString() : strAlarmAxisEnglish + " " + dtTemp.Rows[0]["Code_Desc_EN"].ToString();
                    alarm.Position = dtTemp.Rows[0]["location"].ToString();
                    alarm.Code_Group = dtTemp.Rows[0]["category"].ToString();
                }
                else
                {
                    alarm.CodeID = error_message;
                    alarm.IsStop = true;
                    alarm.Code_Type = error_message;
                    alarm.Code_Name = error_message;
                    alarm.Code_Cause = "未知";
                    alarm.Code_Cause_English = "unknown";
                    alarm.Position = string.Empty;
                    alarm.Code_Group = "UNDEFINITION";
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
