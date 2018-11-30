using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class ProcessJob
    {
        //Create Data
        public string PJ_ObjID;
        public int CarrierQuantity;
        public string CarrierID;
        public int SlopNumber;
        public int[] SlotID;
        public int PRRecipeMethod;
        public string RcpSpec;
        public int ProcessStart;

        //run time data
        public int PJState;


        public ProcessJob()
        {
            CarrierQuantity = 25;
            PJ_ObjID = "";
            CarrierID = "";
            SlopNumber = 0;
            PRRecipeMethod = 0;
            RcpSpec = "";
            ProcessStart = 0;
            SlotID = new int[0];

            PJState = 0;

        }
    }
}
