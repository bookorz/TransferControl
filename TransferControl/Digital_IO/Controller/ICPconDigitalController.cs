using log4net;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using TransferControl.Digital_IO.Config;

namespace TransferControl.Digital_IO.Controller
{
    class ICPconDigitalController : IDIOController
    {
        ILog logger = LogManager.GetLogger(typeof(ICPconDigitalController));
        IDIOReport _Report;
        CtrlConfig _Cfg;
        TcpClient tt;
        Modbus.Device.ModbusIpMaster Master;
        ConcurrentDictionary<int, ushort> AIN = new ConcurrentDictionary<int, ushort>();
        ConcurrentDictionary<int, ushort> AOUT = new ConcurrentDictionary<int, ushort>();
        ConcurrentDictionary<int, bool> DIN = new ConcurrentDictionary<int, bool>();
        ConcurrentDictionary<int, bool> DOUT = new ConcurrentDictionary<int, bool>();

        Object OutputLock = new Object();
        public ICPconDigitalController(CtrlConfig Config, IDIOReport TriggerReport)
        {
            _Cfg = Config;
            _Report = TriggerReport;

            Connect();

        }

        public void Close()
        {
            try
            {

                //20210830 Pingchung Lock start ++
                lock (OutputLock)
                {
                    tt.Close();
                    Master.Dispose();
                }
                //20210830 Pingchung Lock end ++
            }
            catch
            {

            }
            _Report.On_Connection_Status_Report(_Cfg.DeviceName, "Disconnect");
        }

        public void Connect()
        {
            try
            {
                Close();
                Thread CTd = new Thread(ConnectServer);
                CTd.IsBackground = true;
                CTd.Start();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        private void ConnectServer()
        {
            try
            {
                switch (_Cfg.ConnectionType)
                {
                    case "Socket":
                        try
                        {
                            _Report.On_Connection_Status_Report(_Cfg.DeviceName, "Connecting");
                            tt = new TcpClient(_Cfg.IPAdress, _Cfg.Port);

                            Master = Modbus.Device.ModbusIpMaster.CreateIp(tt);
                            _Report.On_Connection_Status_Report(_Cfg.DeviceName, "Connected");

                        }
                        catch (Exception e)
                        {
                            _Report.On_Connection_Error(_Cfg.DeviceName, e.StackTrace);
                            _Report.On_Connection_Status_Report(_Cfg.DeviceName, "Connection_Error");
                            return;
                        }
                        break;
                }
                Master.Transport.Retries = _Cfg.Retries;
                Master.Transport.ReadTimeout = _Cfg.ReadTimeout;

                Thread ReceiveTd = new Thread(Polling);
                ReceiveTd.IsBackground = true;
                ReceiveTd.Start();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        private void Polling()
        {
            DateTime AnalogUpdateTime = DateTime.Now;
            while (true)
            {
                try
                {
                    if (_Cfg.Digital)
                    {
                        bool[] Response = new bool[0];
                        try
                        {
                            lock (Master)
                            {
                                Response = Master.ReadInputs(_Cfg.slaveID, 0, Convert.ToUInt16(_Cfg.DigitalInputQuantity));
                            }
                        }
                        catch (Exception e)
                        {
                            _Report.On_Connection_Error(_Cfg.DeviceName, "Disconnect");
                            Master.Dispose();
                            break;
                        }
                        for (int i = 0; i < _Cfg.DigitalInputQuantity; i++)
                        {
                            if (DIN.ContainsKey(i))
                            {
                                bool org;
                                DIN.TryGetValue(i, out org);
                                if (!org.Equals(Response[i]))
                                {
                                    DIN.TryUpdate(i, Response[i], org);
                                    _Report.On_Data_Chnaged(_Cfg.DeviceName, "DIN", i.ToString(), org.ToString(), Response[i].ToString());
                                }
                            }
                            else
                            {
                                DIN.TryAdd(i, Response[i]);
                                _Report.On_Data_Chnaged(_Cfg.DeviceName, "DIN", i.ToString(), "False", Response[i].ToString());
                            }
                        }
                    }
                    if (_Cfg.Analog)
                    {
                        TimeSpan timeDiff = DateTime.Now - AnalogUpdateTime;
                        if (timeDiff.TotalMilliseconds > 400)
                        {

                            ushort[] Response2 = new ushort[0];
                            try
                            {
                                lock (Master)
                                {
                                    Response2 = Master.ReadInputRegisters(_Cfg.slaveID, 0, Convert.ToUInt16(_Cfg.DigitalInputQuantity));
                                    AnalogUpdateTime = DateTime.Now;
                                }
                            }
                            catch (Exception e)
                            {
                                _Report.On_Connection_Error(_Cfg.DeviceName, "Disconnect");
                                Master.Dispose();
                                break;
                            }
                            for (int i = 0; i < _Cfg.DigitalInputQuantity; i++)
                            {
                                if(AIN.ContainsKey(i))
                                {
                                    ushort org;
                                    AIN.TryGetValue(i, out org);
                                    if (!org.Equals(Response2[i].ToString()))
                                    {
                                        AIN.TryUpdate(i, Response2[i], org);
                                    }
                                    
                                    _Report.On_Data_Chnaged(_Cfg.DeviceName, "AIN", i.ToString(), ((Convert.ToDouble(org) * 10.0 / 32767.0 - 1.0) / 4.0 * 50.0).ToString(), ((Convert.ToDouble(Response2[i]) * 10.0 / 32767.0 - 1.0) / 4.0 * 50.0).ToString().Substring(0, ((Convert.ToDouble(Response2[i]) * 10.0 / 32767.0 - 1.0) / 4.0 * 50.0).ToString().IndexOf(".") + 2));
                                    //_Report.On_Data_Chnaged(_Cfg.DeviceName, "AIN", i.ToString(), ((Convert.ToDouble(org) * 10.0 / 32767.0 ) ).ToString(), ((Convert.ToDouble(Response2[i]) * 10.0 / 32767.0 ) ).ToString().Substring(0, ((Convert.ToDouble(Response2[i]) * 10.0 / 32767.0 )).ToString().IndexOf(".") + 2));

                                }
                                else
                                {
                                    AIN.TryAdd(i, Response2[i]);
                                    _Report.On_Data_Chnaged(_Cfg.DeviceName, "AIN", i.ToString(), "0", ((Convert.ToDouble(Response2[i]) * 10.0 / 32767.0 - 1.0) / 4.0 * 50.0).ToString().Substring(0, ((Convert.ToDouble(Response2[i]) * 10.0 / 32767.0 - 1.0) / 4.0 * 50.0).ToString().IndexOf(".") + 2));
                                }
                            }
                        }
                    }
                    SpinWait.SpinUntil(() => false, _Cfg.Delay);
                }
                catch (Exception e)
                {
                    logger.Error(e.StackTrace);
                }
            }
        }

        public void SetOut(string Address, string Value)
        {
            try
            {
                //logger.Debug(string.Format("{0}:SetOut({1},{2})", this._Cfg.DeviceName, Address, Value));
                //20210830 Pingchung Lock start ++
                lock (OutputLock)
                {
                    ushort adr = Convert.ToUInt16(Address);
                    if (bool.TryParse(Value, out bool boolVal))
                    {
                        //bool[] Response;
                        try
                        {
                            if (Master != null)
                            {
                                lock (Master)
                                {
                                    //logger.Debug(string.Format("{0}:WriteSingleCoil({1},{2},{3})", this._Cfg.DeviceName, _Cfg.slaveID, adr, boolVal));
                                    SpinWait.SpinUntil(() => false, 20);
                                    Master.WriteSingleCoil(_Cfg.slaveID, adr, boolVal);
                                }
                            }
                        }
                        catch
                        {
                            throw new Exception(this._Cfg.DeviceName + " connection error!");
                        }

                        // 沒有意義 Mark by Pingchung
                        // 當兩個Command太接近時，第二個Command的動作會被忽略
                        // 但是讀回來的值會和之前輸出的Command是一樣的
                        // 應該為硬體的異常(待釐清)
                        //lock (Master)
                        //{
                        //    Response = Master.ReadCoils(_Cfg.slaveID, adr, 1);
                        //    logger.Debug(string.Format("{0}:ReadCoils({1},{2},1), Return : {3}", this._Cfg.DeviceName, _Cfg.slaveID, adr, Response[0]));
                        //}


                        //bool org;
                        //if (DOUT.TryGetValue(adr, out org))
                        //{
                        //    if (!org.Equals(Response[0]))
                        //    {
                        //        DOUT.TryUpdate(adr, Response[0], org);
                        //        _Report.On_Data_Chnaged(_Cfg.DeviceName, "DOUT", adr.ToString(), org.ToString(), Response[0].ToString());
                        //    }
                        //}
                        //else
                        //{
                        //    DOUT.TryAdd(adr, Response[0]);
                        //    _Report.On_Data_Chnaged(_Cfg.DeviceName, "DOUT", adr.ToString(), "N/A", Response[0].ToString());
                        //}

                        if (DOUT.TryGetValue(adr, out bool org))
                        {
                            if (!org.Equals(boolVal))
                            {
                                DOUT.TryUpdate(adr, boolVal, org);
                                _Report.On_Data_Chnaged(_Cfg.DeviceName, "DOUT", adr.ToString(), org.ToString(), boolVal.ToString());
                            }
                        }
                        else
                        {
                            DOUT.TryAdd(adr, boolVal);
                            _Report.On_Data_Chnaged(_Cfg.DeviceName, "DOUT", adr.ToString(), "N/A", boolVal.ToString());
                        }
                    }
                    else
                    {
                        ushort[] Response2 = null;
                        try
                        {
                            if (Master != null)
                            {
                                lock (Master)
                                {
                                    SpinWait.SpinUntil(() => false, 10);
                                    Master.WriteSingleRegister(_Cfg.slaveID, adr, Convert.ToUInt16(Value));
                                }
                            }
                        }
                        catch
                        {
                            throw new Exception(this._Cfg.DeviceName + " connection error!");
                        }

                        if (Master != null)
                        {
                            lock (Master)
                            {
                                SpinWait.SpinUntil(() => false, 10);
                                Response2 = Master.ReadHoldingRegisters(_Cfg.slaveID, adr, 1);
                            }
                        }

                        if (AOUT.TryGetValue(adr, out ushort org))
                        {
                            if (!org.Equals(Response2[0]))
                            {
                                AOUT.TryUpdate(adr, Response2[0], org);
                                _Report.On_Data_Chnaged(_Cfg.DeviceName, "DOUT", adr.ToString(), org.ToString(), Response2[0].ToString());
                            }
                        }
                        else
                        {
                            AOUT.TryAdd(adr, Response2[0]);
                            _Report.On_Data_Chnaged(_Cfg.DeviceName, "DOUT", adr.ToString(), "N/A", Response2[0].ToString());
                        }
                    }
                }
                //20210830 Pingchung Lock end ++
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        public void SetOutWithoutUpdate(string Address, string Value)
        {
            try
            {
                //20210830 Pingchung Lock start ++
                //logger.Debug(string.Format("{0}:SetOutWithoutUpdate({1},{2})", this._Cfg.DeviceName, Address, Value));
                lock (OutputLock)
                {
                    ushort adr = Convert.ToUInt16(Address);
                    if (bool.TryParse(Value, out bool boolVal))
                    {
                        if (DOUT.TryGetValue(adr, out bool org))
                        {
                            if (org != bool.Parse(Value))
                            {
                                DOUT.TryUpdate(adr, bool.Parse(Value), org);
                                _Report.On_Data_Chnaged(_Cfg.DeviceName, "DOUT", adr.ToString(), org.ToString(), bool.Parse(Value).ToString());
                            }
                        }
                        else
                        {
                            DOUT.TryAdd(adr, bool.Parse(Value));
                            _Report.On_Data_Chnaged(_Cfg.DeviceName, "DOUT", adr.ToString(), "N/A", bool.Parse(Value).ToString());
                        }
                    }
                    else
                    {
                        if (AOUT.TryGetValue(adr, out ushort org))
                        {
                            if (org != Convert.ToUInt16(Value))
                            {
                                AOUT.TryUpdate(adr, Convert.ToUInt16(Value), org);
                                _Report.On_Data_Chnaged(_Cfg.DeviceName, "AOUT", adr.ToString(), org.ToString(), Value);
                            }
                        }
                        else
                        {
                            AOUT.TryAdd(adr, Convert.ToUInt16(Value));
                            _Report.On_Data_Chnaged(_Cfg.DeviceName, "AOUT", adr.ToString(), "N/A", Value);
                        }
                    }
                }
                //20210830 Pingchung Lock end ++

            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        public void UpdateOut()
        {
            try
            {
                //20210830 Pingchung Lock start ++
                //logger.Debug(string.Format("{0}:UpdateOut()", this._Cfg.DeviceName));
                lock (OutputLock)
                {
                    bool[] data = new bool[_Cfg.DigitalInputQuantity];
                    for (int i = 0; i < _Cfg.DigitalInputQuantity; i++)
                    {
                        if (DOUT.TryGetValue(i, out bool val))
                        {
                            data[i] = val;
                        }
                        else
                        {
                            data[i] = false;
                        }
                    }
                    if(Master != null)
                    {
                        lock (Master)
                        {
                            Master.WriteMultipleCoils(_Cfg.slaveID, 0, data);
                        }
                    }


                    ushort[] data2 = new ushort[_Cfg.DigitalInputQuantity];
                    for (int i = 0; i < _Cfg.DigitalInputQuantity; i++)
                    {
                        if (AOUT.TryGetValue(i, out ushort val))
                        {
                            data2[i] = Convert.ToUInt16(Convert.ToDouble(val) * 32767.0 / 10.0);
                        }
                        else
                        {
                            data2[i] = 0;
                        }
                    }
                    if (Master != null)
                    {
                        lock (Master)
                        {
                            Master.WriteMultipleRegisters(_Cfg.slaveID, 0, data2);
                        }
                    }
                }
                //20210830 Pingchung Lock end ++
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
        }

        public string GetIn(string Address)
        {
            bool result = false;
            try
            {
                int key = Convert.ToInt32(Address);
                if (DIN.ContainsKey(key))
                {
                    if (!DIN.TryGetValue(key, out result))
                    {
                        throw new Exception("DeviceName:" + _Cfg.DeviceName + " Address " + Address + " get fail!");
                    }
                }
                else
                {
                    throw new Exception("DeviceName:" + _Cfg.DeviceName + " Address " + Address + " not exist!");
                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
            return result.ToString();
        }

        public string GetOut(string Address)
        {

            bool result = false;
            try
            {
                if (Master == null)
                {
                    return "";
                }

                lock (Master)
                {
                    SpinWait.SpinUntil(() => false, 10);
                    result = Master.ReadCoils(_Cfg.slaveID, Convert.ToUInt16(Address), 1)[0];
                }
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
            }
            return result.ToString();
        }


    }
}
