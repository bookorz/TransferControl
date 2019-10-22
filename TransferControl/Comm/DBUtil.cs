using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Collections.Concurrent;
using System.Threading;
using TransferControl.Config;

namespace TransferControl.Comm
{
    public class DBUtil
    {
        static ConcurrentQueue<MySqlCommand> SQLBuffer = new ConcurrentQueue<MySqlCommand>();

        public enum QueryContainer
        {
            DBController,
            DBEquipmentModel
        }
        static ILog logger = LogManager.GetLogger(typeof(DBUtil));



        private static MySqlConnection open_Conn()
        {
            MySqlConnection Connection_;
            string connectionStr = SystemConfig.Get().DBConnectionString;
            Connection_ = new MySqlConnection(connectionStr);

            Connection_.Open();
            return Connection_;

            //MessageBox.Show("Connect OK!");
        }

        private static void close_Conn(MySqlConnection Connection_)
        {
            if (Connection_ != null)
            {
                Connection_.Close();
                //MessageBox.Show("Connect Close!");
            }
        }

        /// <summary>
        /// 取得 MySqlDataReader 
        /// while (data.Read())
        ///    {
        ///     //以欄位名稱取得資料並列出
        ///      Console.WriteLine("id={0} , name={1}", data["list_id"], data["list_name"]);
        ///     //以欄位順序取得資料並列出
        ///        Console.WriteLine("id={0} , name={1}", data[0], data[1]);
        ///    }
        ///data.Close();
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">參數</param>
        /// <returns></returns>
        public DataTableReader GetDataReader(string sql, Dictionary<string, object> parameters)
        {
            DataTableReader reader = null;
            try
            {
                //sql = "SELECT * FROM list_item";

                MySqlCommand command = new MySqlCommand(sql, open_Conn());
                // set parameters
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                }
                //get query result
                MySqlDataReader rs = command.ExecuteReader();
                var dt = new DataTable();
                dt.Load(rs);
                reader = dt.CreateDataReader();
                close_Conn(command.Connection);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString(),e);
            }
            return reader;
        }

        /// <summary>
        /// 取得 DataAdapter , 可做為dataGridView 的 source
        /// DataTable dataTable = new DataTable();
        /// adapter.Fill(dataTable);
        /// dataGridViewMariaDB.DataSource = dataTable;
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public MySqlDataAdapter GetDataAdapter(string sql)
        {
            //sql = "SELECT * FROM list_item";
            MySqlConnection conn = open_Conn();
            MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
            close_Conn(conn);
            return adapter;
        }

        /// <summary>
        /// 執行非 Query 類 SQL
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">參數</param>
        /// <returns>影響筆數</returns>
        public int ExecuteNonQuery(string sql, Dictionary<string, object> parameters)
        {

            MySqlConnection conn = open_Conn();
            //sql = string.Format("UPDATE list_item SET modify_timestamp = NOW()");
            string sqlInfo = sql + " : ";

            MySqlCommand command = new MySqlCommand(sql, conn);
            // set parameters
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                    sqlInfo += param.Key + " - " + param.Value;
                }
            }
            //logger.Debug("ExecuteNonQuery  "+ sqlInfo);
            int affectLines = command.ExecuteNonQuery();
            close_Conn(conn);
            return affectLines;
        }

        /// <summary>
        /// 執行非 Query 類 SQL
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">參數</param>
        /// <returns>影響筆數</returns>
        public void ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters)
        {
            //sql = string.Format("UPDATE list_item SET modify_timestamp = NOW()");
            string sqlInfo = sql + " : ";
            try
            {


                MySqlCommand command = new MySqlCommand(sql);
                // set parameters
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in parameters)
                    {

                        command.Parameters.AddWithValue(param.Key, param.Value);


                        sqlInfo += param.Key + " - " + param.Value;
                    }
                }
                //logger.Debug("ExecuteNonQuery  "+ sqlInfo);
                SQLBuffer.Enqueue(command);
                //int affectLines = command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                //logger.Error("ExecuteNonQueryAsync error:" + e.StackTrace);
                logger.Error("ExecuteNonQueryAsync error:" + e.StackTrace, e);
            }

        }

        public static void consumeSqlCmd(object obj)
        {
            while (true)
            {
                MySqlConnection conn = open_Conn();
                while (SQLBuffer.Count() != 0)
                {
                    MySqlCommand SqlCmd;
                    if (SQLBuffer.TryDequeue(out SqlCmd))
                    {

                        SqlCmd.Connection = conn;
                        try
                        {
                            int affectLines = SqlCmd.ExecuteNonQuery();
                        }catch(Exception e)
                        {
                            logger.Error(e.StackTrace,e);
                        }
                    }
                }
                close_Conn(conn);
                SpinWait.SpinUntil(() => false, 2000);
            }
        }

        /// <summary>
        /// 取得 MySqlDataTable
        /// </summary>
        /// <param name="sql">SQL</param>
        /// <param name="parameters">參數</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sql, Dictionary<string, object> parameters)
        {
            DataTable dt = new DataTable();
            try
            {
                //sql = "SELECT * FROM list_item";
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                MySqlConnection conn = open_Conn();
                MySqlCommand command = new MySqlCommand(sql, conn);

                // set parameters
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                //get query result
                MySqlDataReader rs = command.ExecuteReader();
                dt.Load(rs);
                close_Conn(conn);
            }
            catch (Exception e)
            {
                logger.Error(e.ToString(),e);
                throw new Exception(e.ToString());
            }
            return dt;
        }

       
    }
}
