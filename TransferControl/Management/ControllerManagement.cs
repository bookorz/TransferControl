using TransferControl.Controller;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Data;
using Newtonsoft.Json;
using System.Threading;
using TransferControl.Config;
using TransferControl.Comm;
using LiteDB;

namespace TransferControl.Management
{
    public static class ControllerManagement
    {
        static ILog logger = LogManager.GetLogger(typeof(ControllerManagement));

        //private static DBUtil dBUtil = new DBUtil();
        private static ConcurrentDictionary<string, IController> Controllers;

        public class config_controller_setting
        {
            public string equipment_model_id { get; set; }
            public string device_name { get; set; }
            public string vendor { get; set; }
            public string device_type { get; set; }
            public string conn_address { get; set; }
            public string conn_type { get; set; }
            public string conn_port { get; set; }
            public bool enable_flg { get; set; }
            public string controller_type { get; set; }

        }
        public static void LoadConfig(ICommandReport Report)
        {
            //if (Controllers != null)
            //{
            //    foreach(DeviceController each in Controllers.Values)
            //    {
            //        each.Close();
            //    }
            //}
            //  Dictionary<string, object> keyValues = new Dictionary<string, object>();
              Controllers = new ConcurrentDictionary<string, IController>();
            //  string Sql = @"SELECT UPPER(t.device_name) as DeviceName,t.device_type as DeviceType,
            //UPPER(t.vendor) as vendor,
            //                  case when t.conn_type = 'Socket' then  t.conn_address else '' end as IPAdress ,
            //                  case when t.conn_type = 'Socket' then  CONVERT(t.conn_port,SIGNED) else 0 end as Port ,
            //                  case when t.conn_type = 'Comport' then   CONVERT(t.conn_port,SIGNED) else 0 end as BaudRate ,
            //                  case when t.conn_type = 'Comport' then  t.conn_address else '' end as PortName ,                            
            //                  t.conn_type as ConnectionType,
            //                  t.enable_flg as Enable,
            //                  t.controller_type as ControllerType
            //                  FROM config_controller_setting t
            //                  WHERE t.equipment_model_id = @equipment_model_id
            //                  AND t.device_type <> 'DIO'";
            //  keyValues.Add("@equipment_model_id", SystemConfig.Get().SystemMode);
            //  DataTable dt = dBUtil.GetDataTable(Sql, keyValues);
            using (var db = new LiteDatabase(@"Filename=config\MyData.db;Connection=shared;"))
            {
                // Get customer collection
                var col = db.GetCollection<config_controller_setting>("config_controller_setting");
                var result = col.Query().Where(x => x.equipment_model_id.Equals(SystemConfig.Get().SystemMode) && !x.device_type.Equals("DIO"));
                List<config_controller_setting> cfgList = result.ToList();


                foreach (config_controller_setting row in cfgList)
                {
                    IController ctrl = null;
                    if (row.controller_type.Equals("ASCII"))
                    {
                        DeviceController d = new DeviceController();
                        d.DeviceName = row.device_name;
                        d.DeviceType = row.device_type;
                        d.Vendor = row.vendor;
                        d.ConnectionType = row.conn_type;
                        if (row.conn_type.Equals("Socket")) {
                            d.IPAdress = row.conn_address;
                            d.Port = Convert.ToInt32(row.conn_port);
                        }
                        else if (row.conn_type.ToUpper().Equals("COMPORT"))
                        {
                            d.BaudRate = Convert.ToInt32(row.conn_port);
                            d.PortName = row.conn_address;
                        }
                        d.Enable = row.enable_flg;
                        d.ControllerType = row.controller_type;

                        ctrl = d;
                    }
                    else if (row.controller_type.Equals("MODBUS"))
                    {
                        ModbusController m = new ModbusController();
                        m.DeviceName = row.device_name;
                        m.DeviceType = row.device_type;
                        m.Vendor = row.vendor;
                        m.ConnectionType = row.conn_type;
                        if (row.conn_type.Equals("Socket"))
                        {
                            m.IPAdress = row.conn_address;
                            m.Port = Convert.ToInt32(row.conn_port);
                        }
                        else if (row.conn_type.Equals("Comport"))
                        {
                            m.BaudRate = Convert.ToInt32(row.conn_port);
                            m.PortName = row.conn_address;
                        }
                       
                        m.Enable = row.enable_flg;
                        m.ControllerType = row.controller_type;
                        ctrl = m;
                    }


                    if (ctrl.GetEnable())
                    {
                        //each.ConnectionType = "Socket";
                        //each.IPAdress = "127.0.0.1";
                        //each.Port = 9527;
                        ctrl.SetReport(Report);
                        Controllers.TryAdd(ctrl.GetDeviceName().ToUpper(), ctrl);
                    }

                }
            }
        }

        public static IController Get(string Name)
        {
            IController result = null;

            Controllers.TryGetValue(Name.ToUpper(), out result);

            return result;
        }
        public static bool Add(string Name, IController Controller)
        {
            bool result = false;


            if (!Controllers.ContainsKey(Name.ToUpper()))
            {
                Controllers.TryAdd(Name.ToUpper(), Controller);
                result = true;
            }

            return result;
        }

        public static void ConnectAll()
        {
            foreach (IController each in Controllers.Values.ToList())
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(each.Start));
            }
        }

        //public static void ConnectAll()
        //{
        //    foreach (DeviceController each in Controllers.Values.ToList())
        //    {
        //        if (!each._Config.Vendor.Equals("HST")&& !each._Config.Vendor.Equals("COGNEX"))
        //        {
        //            each.Connect();
        //        }
        //    }
        //}

        //public static void DisonnectAll()
        //{
        //    foreach (DeviceController each in Controllers.Values.ToList())
        //    {
        //        each.Close();
        //    }
        //}




    }
}
