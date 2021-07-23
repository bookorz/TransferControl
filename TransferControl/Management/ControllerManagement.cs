using TransferControl.Controller;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Data;
using System.Threading;
using TransferControl.Config;
using TransferControl.Comm;

namespace TransferControl.Management
{
    public static class ControllerManagement
    {
        static ILog logger = LogManager.GetLogger(typeof(ControllerManagement));

        private static ConcurrentDictionary<string, IController> Controllers;

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

            List<DeviceController> controllerList = new ConfigTool<List<DeviceController>>().ReadFile("config/Controller.json");
            foreach (DeviceController each in controllerList)
            {
                if (!each.DeviceType.Equals("DIO"))
                {
                    IController ctrl = null;
                    if (each.ControllerType.Equals("ASCII"))
                    {
                        DeviceController d = new DeviceController
                        {
                            DeviceName = each.DeviceName,
                            DeviceType = each.DeviceType,
                            Vendor = each.Vendor,
                            IPAdress = each.IPAdress,
                            Port = each.Port,
                            BaudRate = each.BaudRate,
                            PortName = each.PortName,
                            ConnectionType = each.ConnectionType,
                            Enable = each.Enable,
                            ControllerType = each.ControllerType
                        };

                        ctrl = d;
                    }
                    else if (each.ControllerType.Equals("MODBUS"))
                    {
                        ModbusController m = new ModbusController
                        {
                            DeviceName = each.DeviceName,
                            DeviceType = each.DeviceType,
                            Vendor = each.Vendor,
                            IPAdress = each.IPAdress,
                            Port = each.Port,
                            BaudRate = each.BaudRate,
                            PortName = each.PortName,
                            ConnectionType = each.ConnectionType,
                            Enable = each.Enable,
                            ControllerType = each.ControllerType
                        };
                        ctrl = m;
                    }
                    else if (each.ControllerType.Equals("PLC"))
                    {
                        PLCController m = new PLCController
                        {
                            DeviceName = each.DeviceName,
                            DeviceType = each.DeviceType,
                            Vendor = each.Vendor,
                            IPAdress = each.IPAdress,
                            Port = each.Port,
                            BaudRate = each.BaudRate,
                            PortName = each.PortName,
                            ConnectionType = each.ConnectionType,
                            Enable = each.Enable,
                            ControllerType = each.ControllerType
                        };
                        ctrl = m;
                    }


                    if (ctrl.GetEnable())
                    {
                        //each.ConnectionType = "Socket";
                        //each.IPAdress = "127.0.0.1";
                        //each.Port = 9527;
                        ctrl.SetReport(Report);
                        Controllers.TryAdd(ctrl.GetDeviceName(), ctrl);
                    }
                }
                
            }
        }
        public static void Save()
        {



            List<IController> result = Controllers.Values.ToList();

            result.Sort((x, y) => { return x.GetDeviceName().CompareTo(y.GetDeviceName()); });
            new ConfigTool<List<IController>>().WriteFile("config/Controller.json", result);
           
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


            if (!Controllers.ContainsKey(Name))
            {
                Controllers.TryAdd(Name, Controller);
                result = true;
            }

            return result;
        }

        //public static void ConnectAll()
        //{
        //    foreach (IController each in Controllers.Values.ToList())
        //    {
        //        ThreadPool.QueueUserWorkItem(new WaitCallback(each.Start));
        //    }
        //}

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
