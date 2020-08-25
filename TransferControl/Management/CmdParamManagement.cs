using LiteDB;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using TransferControl.Comm;
using TransferControl.Config;

namespace TransferControl.Management
{
    public class CmdParamManagement
    {
        public class ParamMapping
        {
            [BsonField("func_type")]
            public string FuncType { get; set; }
            [BsonField("param_name")]
            public string ParamName { get; set; }
            [BsonField("code_id")]
            public string CodeId { get; set; }
            [BsonField("code_desc")]
            public string CodeDesc { get; set; }
            public string Vendor { get; set; }
            [BsonField("mapping_code")]
            public string MappingCode { get; set; }
        }

        static Dictionary<string, ParamMapping> MappingList;
        static ILog logger = LogManager.GetLogger(typeof(CmdParamManagement));

        public static void Initialize()
        {
            try
            {
                MappingList = new Dictionary<string, ParamMapping>();
                //DBUtil dBUtil = new DBUtil();

                //DataTable dtCommand = new DataTable();

                //string strSql = "SELECT  func_type,param_name, code_id, code_desc, vendor, mapping_code FROM param_mapping";



                //dtCommand = dBUtil.GetDataTable(strSql, null);
                List<ParamMapping> tJList = null;
                using (var db = new LiteDatabase(@"Filename=config\MyData.db;Connection=shared;"))
                {
                    // Get customer collection
                    var col = db.GetCollection<ParamMapping>("param_mapping");
                    var result = col.Query();
                    tJList = result.ToList();
                }

                if (tJList.Count > 0)
                {
                    foreach (ParamMapping row in tJList)
                    {
                        ParamMapping each = new ParamMapping();
                        each.CodeDesc = row.CodeDesc;
                        each.CodeId = row.CodeId;
                        each.FuncType = row.FuncType;
                        each.ParamName = row.ParamName;
                        each.MappingCode = row.MappingCode;
                        each.Vendor = row.Vendor;
                        string key = each.Vendor + each.FuncType + each.ParamName + each.CodeId;
                        MappingList.Add(key, each);
                    }
                }
                else
                {
                    throw new Exception("TransferControl.Management.CmdParamManagement\r\nException: Parameter List not exists.");
                }
            }
            catch (Exception ex)
            {
                //throw new Exception(ex.StackTrace);
                logger.Error(ex.StackTrace);
            }

        }
        public static ParamMapping FindMapping(string Vendor,string FuncType, string ParamName,string CodeId)
        {
            ParamMapping result = null;
            string key = Vendor + FuncType + ParamName+ CodeId;

            MappingList.TryGetValue(key, out result);

            return result;
        }
    }
}
