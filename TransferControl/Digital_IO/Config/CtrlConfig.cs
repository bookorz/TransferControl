using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Digital_IO.Config
{
    public class CtrlConfig
    {
        public string equipment_model_id { get; set; }
        [BsonField("device_name")]
        public string DeviceName { get; set; }
        [BsonField("device_type")]
        public string DeviceType { get; set; }
        public string Vendor { get; set; }
        [BsonField("conn_address")]
        public string IPAdress { get; set; }
        [BsonField("conn_port")]
        public int Port { get; set; }
        [BsonField("conn_type")]
        public string ConnectionType { get; set; }
        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public string ParityBit { get; set; }
        public int DataBits { get; set; }
        public string StopBit { get; set; }
        public int Retries { get; set; }
        public int ReadTimeout { get; set; }
        public int DigitalInputQuantity { get; set; }
        public int Delay { get; set; }
        public byte slaveID { get; set; }
        public bool Enable { get; set; }
        public bool Digital { get; set; }
        public bool Analog { get; set; }
    }
}
