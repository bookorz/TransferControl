using log4net;
using TransferControl.Config;
using TransferControl.Controller;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransferControl.Comm
{
    class ComPortClient : IConnection
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(ComPortClient));
        private SerialPort port;
        IConnectionReport ConnReport;
        IController cfg;
        public bool _WaitForData = false;

        public ComPortClient(IController _Config, IConnectionReport _ConnReport)
        {
            cfg = _Config;
            ConnReport = _ConnReport;
            Parity p;
            if (_Config.GetVendor().Equals("SMARTTAG8200"))
            {
                p = Parity.Mark;
            }
            else if (_Config.GetVendor().Equals("SMARTTAG8400"))
            {
                p = Parity.None;
            }
            else
            {
                p = Parity.None;
            }

            StopBits s = StopBits.One;

            port = new SerialPort(_Config.GetPortName(), _Config.GetBaudRate(), p, 8, s);

            if (_Config.GetVendor().Equals("SMARTTAG8200"))
            {
                port.Handshake = Handshake.None;
            }
            else if (_Config.GetVendor().Equals("SMARTTAG8400"))
            {
                port.Handshake = Handshake.None;
            }
            else if (_Config.GetVendor().Equals("OMRON_V640"))
            {
                port.Handshake = Handshake.None;
            }
            else if(_Config.GetVendor().Equals("RFID_HR4136"))
            {
                port.Handshake = Handshake.None;
            }
            else if(_Config.GetVendor().Equals("MITSUBISHI_PLC"))
            {
                port.DtrEnable = true;
                port.RtsEnable = true;
                //port.ReadTimeout = System.IO.Ports.SerialPort.InfiniteTimeout;
                port.ReadTimeout = 5000;
                port.WriteTimeout = 5000;
            }
        }

        public void WaitForData(bool Enable)
        {
            _WaitForData = Enable;
        }


        public void Reconnect()
        {
            //port.Close();
            //ConnReport.On_Connection_Disconnected("Close");

            //port = new SerialPort(cfg.GetPortName(), cfg.GetBaudRate(), Parity.None, 8, StopBits.One);
            //if (cfg.GetVendor().Equals("SMARTTAG"))
            //{
            //    port.Handshake = Handshake.None;
            //    port.RtsEnable = true;
            //    port.ReadTimeout = 5000;
            //    port.WriteTimeout = 5000;
            //}
        }

        public void Start()
        {
            if (!port.IsOpen)
            {
                Thread ComTd = new Thread(ConnectServer);
                ComTd.IsBackground = true;
                ComTd.Start();
            }
        }

        public void Send(object Message)
        {
            logger.Debug( this.cfg.GetDeviceName()+" Send:"+Message);
            try
            {
                if (cfg.GetVendor().ToUpper().Equals("ACDT"))
                {
                    string hexString = Message.ToString().Replace("-", "");
                    byte[] byteOUT = new byte[hexString.Length / 2];
                    for (int i = 0; i < hexString.Length; i = i + 2)
                    {
                        //每2位16進位數字轉換為一個10進位整數
                        byteOUT[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                    }
                    port.Write(byteOUT, 0, byteOUT.Length);
                }
                else
                {
                    port.Write((string)Message);
                    //int ttt = port.ReadByte();
                    //byte[] buf = new byte[250];
                    //port.Read(buf, 0, buf.Length);
                    //string data = ByteArrayToString(buf);
                }
               
            }
            catch (Exception e)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ConnectServer )" + e.Message + "\n" + e.StackTrace);
               
            }
        }

        public void SendHexData(object Message)
        {
            try
            {
                if (cfg.GetVendor().Equals("SMARTTAG8200"))
                {
                    if(port.Parity != Parity.Mark)
                        port.Parity = Parity.Mark;
                }
                else if (cfg.GetVendor().Equals("SMARTTAG8400"))
                {
                    if(port.Parity != Parity.None)
                        port.Parity = Parity.None;

                    tmp = tmp8400Data = "";
                }


                logger.Debug(this.cfg.GetDeviceName() + " Send:" + Message.ToString());
                byte[] buf = HexStringToByteArray(Message.ToString());

                port.Write(buf, 0, buf.Length);
                
            }
            catch (Exception e)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ConnectServer )" + e.Message + "\n" + e.StackTrace);
               
            }
        }

        private void ConnectServer()
        {
            try
            {
                ConnReport.On_Connection_Connecting("Connecting to ");
                port.Open();
                ConnReport.On_Connection_Connected("Connected! ");
                switch (cfg.GetVendor().ToUpper())
                {
                    case "ACDT":
                        port.DataReceived += new SerialDataReceivedEventHandler(ACDT_DataReceived);
                        break;
                    case "TDK":

                        port.DataReceived += new SerialDataReceivedEventHandler(TDK_DataReceived);
                        break;
                    case "ATEL_NEW":
                    case "SANWA":
                    case "SANWA_MC":
                    case "SANWA_HWATSING_MC":
                        //case "OMRON_V640":
                        port.DataReceived += new SerialDataReceivedEventHandler(Sanwa_DataReceived);
                        break;
                    case "ASYST":
                        port.DataReceived += new SerialDataReceivedEventHandler(ASYST_DataReceived);
                        break;
                    //case "SMARTTAG8200":
                    //    port.DataReceived += new SerialDataReceivedEventHandler(SMARTTAG8200_DataReceived);
                    //    break;
                    //case "SMARTTAG8400":
                    //    port.DataReceived += new SerialDataReceivedEventHandler(SMARTTAG8400_DataReceived);
                    //    break;
                    case "SMARTTAG8200":
                    case "SMARTTAG8400":
                        port.DataReceived += new SerialDataReceivedEventHandler(SMARTTAG_DataReceived);
                        break;

                    case "MITSUBISHI_PLC":
                        port.DataReceived += new SerialDataReceivedEventHandler(MITSUBISHI_PLC_DataReceived); 
                        break;
                    case "FRANCES":
                        port.DataReceived += new SerialDataReceivedEventHandler(FRANCES_DataReceived);
                        break;
                    case "RFID_HR4136":
                    case "OMRON_V640":
                        port.DataReceived += new SerialDataReceivedEventHandler(RFID_DataReceived);
                        //port.DataReceived += new SerialDataReceivedEventHandler(RFIDHR4136_DataReceived);
                        break;
                }
            }
            catch (Exception e)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ConnectServer )" + e.Message + "\n" + e.StackTrace);
            }
        }
        string S = "";
        private void ASYST_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {

                //string data = "";

                //data = port.ReadTo("\r\n");
                //logger.Debug(this.cfg.GetDeviceName() + "Received:" + data);

                //ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                string data = "";
                if ((sender as SerialPort).BytesToRead > 0)
                {
                    byte[] buf = new byte[(sender as SerialPort).BytesToRead];
                    port.Read(buf, 0, buf.Length);

                    logger.Debug(this.cfg.GetDeviceName() + " Rawdata Received:" + ByteArrayToString(buf));

                    S += Encoding.ASCII.GetString(buf);
                    if (S.LastIndexOf("\r\n") != -1)
                    {
                        //logger.Debug("s:" + S);
                        data = S.Substring(0, S.LastIndexOf("\r\n"));
                        //logger.Debug("data:" + data);

                        S = S.Substring(S.LastIndexOf("\r\n") + 1);
                        //logger.Debug("s:" + S);
                        logger.Debug(this.cfg.GetDeviceName() + " Received:" + data);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                    }

                }
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ASYST_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }

        //private void MITSUBISHI_PLC_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    if ((sender as SerialPort).BytesToRead > 0)
        //    {
        //        try
        //        {
        //            string data = "";

        //            data = port.ReadTo(Convert.ToString((char)3));


        //            ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
        //        }
        //        catch (Exception e1)
        //        {
        //            //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
        //            ConnReport.On_Connection_Error("(ASYST_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
        //        }
        //    }
        //}
        string tmp = "";
        private void MITSUBISHI_PLC_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //SpinWait.SpinUntil(() => !_WaitForData, 1000);
                if ((sender as SerialPort).BytesToRead > 0)
                {
                    byte[] buf = new byte[(sender as SerialPort).BytesToRead];
                    port.Read(buf, 0, buf.Length);
                    tmp += Encoding.ASCII.GetString(buf);
                    if (tmp.IndexOf((char)2) != -1 && tmp.IndexOf((char)3) != -1)
                    {
                        if (tmp.IndexOf((char)3) < tmp.IndexOf((char)2))
                        {
                            tmp = tmp.Substring(tmp.IndexOf((char)2));
                        }
                        else
                        {
                            string msg = tmp.Substring(tmp.IndexOf((char)2), (tmp.IndexOf((char)3) + 1) - tmp.IndexOf((char)2));
                            //logger.Debug("Received:" + msg);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), msg);
                            tmp = tmp.Substring(tmp.IndexOf((char)3) + 1);
                        }
                    }
                    else if(tmp.IndexOf((char)6) != -1 && tmp.Substring(tmp.IndexOf((char)6)).Length>6)
                    {
                        string msg = tmp.Substring(tmp.IndexOf((char)6));
                        //logger.Debug("Received:" + msg);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), msg);
                        tmp = "";
                    }
                    
                }
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(MITSUBISHI_PLC_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }
        private void RFID_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            switch (cfg.GetVendor().ToUpper())
            {
                case "RFID_HR4136":
                    RFIDHR4136_DataReceived(sender, e);
                    break;
                case "OMRON_V640":
                    Sanwa_DataReceived(sender, e);
                    break;
            }
        }
        private void SMARTTAG_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            switch (cfg.GetVendor().ToUpper())
            {
                case "SMARTTAG8200":
                    SMARTTAG8200_DataReceived(sender, e);
                    break;
                case "SMARTTAG8400":
                    SMARTTAG8400_DataReceived(sender, e);
                    break;
            }
        }
        private void SMARTTAG8200_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SpinWait.SpinUntil(() => !_WaitForData, 1000);
                _WaitForData = false;
                byte[] buff = new byte[port.BytesToRead];
                port.Read(buff, 0, buff.Length);
                string data = ByteArrayToString(buff);
                logger.Debug(this.cfg.GetDeviceName() +  " Received:" + data);
                ConnReport.On_Connection_Message(data.Trim());

            }
            catch (Exception e1)
            {
                ConnReport.On_Connection_Error("(ASYST_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }

        string tmp8400Data = "";
        private void SMARTTAG8400_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if ((sender as SerialPort).BytesToRead > 0)
                {
                    byte[] buf = new byte[(sender as SerialPort).BytesToRead];
                    port.Read(buf, 0, buf.Length);
                    tmp += Encoding.ASCII.GetString(buf);
                    logger.Debug(this.cfg.GetDeviceName() + " Received:" + ByteArrayToString(buf));
                    tmp8400Data += ByteArrayToString(buf);

                    if (tmp[0] == (char)4)
                    {
                        string msg = tmp[0].ToString();

                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), msg);

                        tmp = tmp8400Data = "";
                    }
                    else if (tmp[0] == (char)6)
                    {
                        if (tmp.Length == 2 && tmp[1] == (char)5)
                            ThreadPool.QueueUserWorkItem(new WaitCallback(SendHexData), "04");

                        tmp = tmp8400Data = "";
                    }
                    else if (tmp[0] == (char)5)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(SendHexData), "04");
                        tmp = tmp8400Data = "";
                    }
                    else
                    {
                        if (tmp.Length >= Convert.ToInt16(tmp[0]))
                        {
                            byte[] hexbuf = HexStringToByteArray(tmp8400Data.ToString());

                            string msg = "";

                            if (tmp.Length > 18)
                            {

                                logger.Debug(this.cfg.GetDeviceName() + " hexbuf.Length:" + hexbuf.Length.ToString());
                                logger.Debug(this.cfg.GetDeviceName() + " Convert.ToInt32(hexbuf[0]):" + (Convert.ToInt32(hexbuf[0]) + 3).ToString());
                                if (hexbuf.Length != Convert.ToInt32(hexbuf[0]) + 3) return;

                                //logger.Debug(this.cfg.GetDeviceName() + " Received:" + ByteArrayToString(buf));

                                msg = tmp.Substring(18, Convert.ToInt16(tmp[17]));
                            }
                            else
                            {
                                if (hexbuf[0] != 13) return;

                                msg = tmp.Substring(10);
                            }

                            ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), msg);
                            tmp = tmp8400Data = "";
                            ThreadPool.QueueUserWorkItem(new WaitCallback(SendHexData), "06");

                        }
                    }

                }
            }
            catch (Exception e1)
            {
                ConnReport.On_Connection_Error("(MITSUBISHI_PLC_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }
        string tmpHR4136Data = "";
        private void RFIDHR4136_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if ((sender as SerialPort).BytesToRead > 0)
                {
                    byte[] buf = new byte[(sender as SerialPort).BytesToRead];
                    port.Read(buf, 0, buf.Length);
                    
                    logger.Debug(this.cfg.GetDeviceName() + " Received:" + ByteArrayToString(buf));

                    tmp += Encoding.ASCII.GetString(buf);

                    tmpHR4136Data += ByteArrayToString(buf);

                    if (tmp[0] == (char)4)
                    {
                        string msg = tmp[0].ToString();

                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), tmp);

                        tmp = tmpHR4136Data = "";
                    }
                    else if (tmp[0] == (char)6)
                    {
                        if(tmp.Length == 2 && tmp[1] == (char)5)
                            ThreadPool.QueueUserWorkItem(new WaitCallback(SendHexData), "04");

                        tmp = tmpHR4136Data = "";
                    }
                    else if (tmp[0] == (char)5)
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(SendHexData), "04");
                        tmp = tmpHR4136Data =  "";
                    }
                    else
                    {
                        if(tmpHR4136Data != "")
                        {
                            byte[] hexbuf = HexStringToByteArray(tmpHR4136Data.ToString());

                            if (hexbuf.Length >= Convert.ToInt32(hexbuf[0]) + 3)
                            {
                                string msg = "";
                                if (hexbuf[19] != 0x4E || hexbuf[20] != 0x4F)
                                {
                                    msg = "SSACK_" + char.ConvertFromUtf32(hexbuf[19]) + char.ConvertFromUtf32(hexbuf[20]);
                                }
                                else
                                {
                                    for (int i = 0; i < Convert.ToInt32(hexbuf[22]); i++)
                                    {
                                        msg += char.ConvertFromUtf32(hexbuf[22 + i + 1]);
                                    }
                                }

                                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), msg);
                                tmp = tmpHR4136Data = "";
                                ThreadPool.QueueUserWorkItem(new WaitCallback(SendHexData), "06");
                            }
                        }


                    }

                }
            }
            catch (Exception e1)
            {
                ConnReport.On_Connection_Error("(RFIDHR4136_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }
        private void TDK_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = "";
                //switch (cfg.DeviceType)
                //{
                //    case "TDKController":
                //data = port.ReadTo("\r");

                //        break;
                //    case "SanwaController":
                data = port.ReadTo(((char)3).ToString());
                //        break;
                //}
                logger.Debug(this.cfg.GetDeviceName() + " Received:" + data);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(TDK_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }
        int currentIdx = 0;
        byte[] readB = new byte[100];
        private void ACDT_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = "";
                //switch (cfg.DeviceType)
                //{
                //    case "TDKController":
                //data = port.ReadTo("\r");

                //        break;
                //    case "SanwaController":
                //data = port.ReadTo(((char)3).ToString());
                while (((readB[currentIdx] = Convert.ToByte(port.ReadByte())) != 3) || (readB[1]==105 && currentIdx < 7))
                {

                    currentIdx++;
                }
                data = BitConverter.ToString(readB.Take(currentIdx + 1).ToArray());
                //        break;
                //}
                currentIdx = 0;
                readB = new byte[100];
                logger.Debug(this.cfg.GetDeviceName() + " Received:" + data);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(TDK_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }

        private void Sanwa_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = "";

                data = port.ReadTo("\r");

                logger.Debug(this.cfg.GetDeviceName() + " Received:" + data);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(Sanwa_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }
        private void FRANCES_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if ((sender as SerialPort).BytesToRead > 0)
            {
                byte[] buf = new byte[(sender as SerialPort).BytesToRead];
                port.Read(buf, 0, buf.Length);
                tmp += ByteArrayToString(buf);

                logger.Debug(this.cfg.GetDeviceName() + " Received:" + ByteArrayToString(buf));

                byte[] hexbuf = HexStringToByteArray(tmp.ToString());

                if (hexbuf[hexbuf.Length -1 ] != 0xbb)
                {
                    logger.Debug(this.cfg.GetDeviceName() + "buf incomplete");
                    return;
                }


                //byte[] hexbuf = HexStringToByteArray(tmp.ToString());
                string data = "";
                for (int i = 0; i < hexbuf.Length;)
                {
                    //ex. 0xaa 0x?? 0x?? 0x?? 0x?? 0xbb
                    if (hexbuf[i] == 0xaa)
                    {
                        if (i + 5 > hexbuf.Length) break;

                        buf = new byte[6];
                        for (int j = 0; j < 6; j++)
                            buf[j] = hexbuf[i + j];

                        data += ByteArrayToString(buf);
                        data = data.Remove(data.LastIndexOf(' '), 1);
                        data += "\r";
                        i = i + 6;

                    }
                    else if (hexbuf[i] == 0xff)     //ex. 0xff 0xff(UNKNOWN COMMAND)
                    {                               //    0xff 0xfd(EOC ERROR)
                        if (i + 2 > hexbuf.Length) break;

                        buf = new byte[2];
                        for (int j = 0; j < 2; j++)
                            buf[j] = hexbuf[i + j];

                        data += ByteArrayToString(buf);
                        data = data.Remove(data.LastIndexOf(' '), 1);
                        data += "\r";
                        i = i + 2;
                    }
                }

                if(!data.Equals(""))
                {
                    tmp = tmp.Replace(data.Replace("\r", " "), "");
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
                }
            }
        }

        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);


            foreach (byte b in ba)
            {
                if (b == 0)
                {
                    //continue;
                }
                hex.AppendFormat("{0:X2}", b);
                hex.Append(" ");
            }
            return hex.ToString();
        }


        private byte[] HexStringToByteArray(string s)
        {

            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        public void Dispose()
        {

        }
    }
}
