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
            Parity p = Parity.None;
            //switch (_Config.ParityBit)
            //{
            //    case "Even":
            //        p = Parity.Even;
            //        break;
            //    case "Mark":
            //        p = Parity.Mark;
            //        break;
            //    case "None":
            //        p = Parity.None;
            //        break;
            //    case "Odd":
            //        p = Parity.Odd;
            //        break;
            //    case "Space":
            //        p = Parity.Space;
            //        break;
            //}
            StopBits s = StopBits.One;
            //switch (_Config.StopBit)
            //{
            //    case "None":
            //        s = StopBits.None;
            //        break;
            //    case "One":
            //        s = StopBits.One;
            //        break;
            //    case "OnePointFive":
            //        s = StopBits.OnePointFive;
            //        break;
            //    case "Two":
            //        s = StopBits.Two;
            //        break;
            //}


            port = new SerialPort(_Config.GetPortName(), _Config.GetBaudRate(), p, 8, s);

        
            
            
    

            if (_Config.GetVendor().Equals("SMARTTAG"))
            {
                port.Handshake = Handshake.None;
                port.RtsEnable = true;
                port.ReadTimeout = 5000;
                port.WriteTimeout = 5000;
            }else if (_Config.GetVendor().Equals("MITSUBISHI_PLC"))
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

        public bool SendHexData(object Message)
        {
            try
            {
                byte[] buf = HexStringToByteArray(Message.ToString());

                port.Write(buf, 0, buf.Length);
                return true;
            }
            catch (Exception e)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ConnectServer )" + e.Message + "\n" + e.StackTrace);
                return false;
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
                        port.DataReceived += new SerialDataReceivedEventHandler(Sanwa_DataReceived);
                        break;
                    case "ASYST":
                        port.DataReceived += new SerialDataReceivedEventHandler(ASYST_DataReceived);
                        break;
                    case "SMARTTAG8200":
                        port.DataReceived += new SerialDataReceivedEventHandler(SMARTTAG8200_DataReceived);
                        break;
                    case "SMARTTAG8400":
                        port.DataReceived += new SerialDataReceivedEventHandler(SMARTTAG8400_DataReceived);
                        break;
                    case "MITSUBISHI_PLC":
                        port.DataReceived += new SerialDataReceivedEventHandler(MITSUBISHI_PLC_DataReceived); 
                        break;
                }
            }
            catch (Exception e)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ConnectServer )" + e.Message + "\n" + e.StackTrace);
            }
        }

        private void ASYST_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {

                string data = "";

                data = port.ReadTo("\r\n");
                logger.Debug(this.cfg.GetDeviceName() + "Received:" + data);

                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), data);
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
        private void SMARTTAG8200_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SpinWait.SpinUntil(() => !_WaitForData, 1000);

                byte[] buf = new byte[250];
                port.Read(buf, 0, buf.Length);
                string data = ByteArrayToString(buf);
                logger.Debug(this.cfg.GetDeviceName() +  " Received:" + data);
                ConnReport.On_Connection_Message(data.Trim());

            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(ASYST_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
            }
        }
        private void SMARTTAG8400_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //SpinWait.SpinUntil(() => !_WaitForData, 1000);
                if ((sender as SerialPort).BytesToRead > 0)
                {
                    byte[] buf = new byte[(sender as SerialPort).BytesToRead];
                    port.Read(buf, 0, buf.Length);
                    tmp += Encoding.ASCII.GetString(buf);
                    if (tmp[0]==(char)4 || tmp[0]==(char)6)
                    {
                        string msg = tmp[0].ToString();
                        logger.Debug(this.cfg.GetDeviceName() + " Received:" + msg);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), msg);
                        
                        if (tmp.IndexOf((char)5) != -1)
                        {
                            Send((char)4);
                        }
                        tmp = "";
                    }
                    else if(tmp.IndexOf((char)5) != -1)
                    {
                        Send((char)4);
                        tmp = "";
                    }
                    else
                    {
                        if (Convert.ToInt16(tmp[0]) == tmp.Length - 1 + 2)
                        {
                            string msg = tmp;
                            logger.Debug(this.cfg.GetDeviceName() + " Received:" + msg);
                            ThreadPool.QueueUserWorkItem(new WaitCallback(ConnReport.On_Connection_Message), msg);
                            tmp = "";
                            Send((char)6);
                        }
                    }

                }
            }
            catch (Exception e1)
            {
                //logger.Error("(ConnectServer " + RmIp + ":" + SPort + ")" + e.Message + "\n" + e.StackTrace);
                ConnReport.On_Connection_Error("(MITSUBISHI_PLC_DataReceived )" + e1.Message + "\n" + e1.StackTrace);
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
