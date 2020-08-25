using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Config
{
    public static class Global
    {
        public static string currentUser = "";
        //private static readonly string CONFIG_DEVICE_SQL =
        //    " SELECT device_name as device_name_r, conn_address, conn_type, conn_port, com_parity_bit, com_data_bits, com_stop_bit, enable_flg" +
        //    "   FROM config_controller_setting " +
        //    "  WHERE equipment_model_id = '" + SystemConfig.Get().SystemMode + "'; ";
        //private static readonly string CONFIG_NODE_SQL =
        //    " SELECT node_id as node_id_r, notch_angle, bypass, enable_flg, associated_node " +
        //    "   FROM config_node " +
        //    "  WHERE equipment_model_id = '" + SystemConfig.Get().SystemMode + "'; ";
        //private static readonly string CONFIG_POINT_SQL =
        //    " SELECT node_name as node_name_r, recipe_id as recipe_id_r, position, position_type, point, offset " +
        //    "   FROM config_point " +
        //    "  WHERE equipment_model_id = '" + SystemConfig.Get().SystemMode + "'; ";
        //private static readonly string CONFIG_RECIPE_SQL =
        //    " SELECT device_name as device_name_r, conn_address, conn_type, conn_port, com_parity_bit, com_data_bits, com_stop_bit, enable_flg " +
        //    "   FROM config_controller_setting " +
        //    "  WHERE equipment_model_id = '" + SystemConfig.Get().SystemMode + "'; ";
        //private static readonly string CONFIG_DIO_SQL =
        //    " SELECT dioname as dioname_r,type as type_r, address as address_r,  parameter as  parameter_r, error_code as error_code_r, abnormal " +
        //    "   FROM config_dio_point " +
        //    "  WHERE equipment_model_id = '" + SystemConfig.Get().SystemMode + "' " +
        //    "  ORDER BY dioname,type, address; ";
        //public static string Config_sql(Config_Type config_Type)
        //{
        //    string sql = "";
        //    switch (config_Type)
        //    {
        //        case Config_Type.DEVICE:
        //            sql = CONFIG_DEVICE_SQL;
        //            break;
        //        case Config_Type.NODE:
        //            sql = CONFIG_NODE_SQL;
        //            break;
        //        case Config_Type.POINT:
        //            sql = CONFIG_POINT_SQL;
        //            break;
        //        case Config_Type.RECIPE:
        //            sql = CONFIG_RECIPE_SQL;
        //            break;
        //        case Config_Type.DIO:
        //            sql = CONFIG_DIO_SQL;
        //            break;
        //    }
        //    return sql;
        //}
    }
    public enum Config_Type
    {
        DEVICE, NODE, POINT, RECIPE, DIO
    }
    public static class ListExtentions
    {
        public static DataTable ToDataTable<T>(this List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
    public static class MD5Extensions
    {
        public static string ToMD5(this string str)
        {
            using (var cryptoMD5 = System.Security.Cryptography.MD5.Create())
            {
                //將字串編碼成 UTF8 位元組陣列
                var bytes = Encoding.UTF8.GetBytes(str);

                //取得雜湊值位元組陣列
                var hash = cryptoMD5.ComputeHash(bytes);

                //取得 MD5
                var md5 = BitConverter.ToString(hash)
                  .Replace("-", String.Empty)
                  .ToUpper();

                return md5;
            }
        }
    }
}
