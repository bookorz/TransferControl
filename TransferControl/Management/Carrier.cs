using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class Carrier
    {
        public string CarrierID { get; set; }
        public int IDStatus { get; set; }
        public int Capcity { get; set; }
        public int SlopMapStatus { get; set; }
        public int ContentMapList { get; set; }
        public int SlopMapCapcity { get; set; }
        public int AccessingStatus { get; set; }
        public string LocationID { get; set; }
        public int SubstrateCount { get; set; }
        public string Usage { get; set; }


        public string[] LotID { get; set; }
        public string[] SubStID { get; set; }
        public int[] SlopMap { get; set; }

        public Carrier()
        {
            CarrierID = "";
            IDStatus = 0;
            Capcity = 25;
            ContentMapList = 0;
            SlopMapStatus = 0;
            SlopMapCapcity = 25;
            AccessingStatus = 0;
            LocationID = "";
            SubstrateCount = 0;
            Usage = "";
            LotID = new string[0];
            SubStID = new string[0];
            SlopMap = new int[0];
        }
    }
}
