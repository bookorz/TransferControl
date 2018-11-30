using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class ControlJob
    {
        //Create Data
        public string CJ_ObjID;
        public int PrCtrlSpecNumber;
        public string[] PrCtrlSpec_PrObjID;
        public int ProcessOrderMgnt;
        public int StartMethod;

        //runtime data
        public int CJState;

        public ControlJob()
        {
            PrCtrlSpecNumber = 25;
            CJ_ObjID = "";
            PrCtrlSpec_PrObjID = new string[0];

            ProcessOrderMgnt = 0;
            StartMethod = 0;

            CJState = 0;
        }
    }
}
