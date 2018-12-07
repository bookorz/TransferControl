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
        public List<string> PrCtrlSpec_PrObjID;
        public List<string> CarrierInputSpec;
        public List<OutSpec> MtrlOutSpec;
        public int ProcessOrderMgnt;
        public int StartMethod;

        //runtime data
        public int CJState;

        public ControlJob()
        {
            CJ_ObjID = "";
            PrCtrlSpec_PrObjID = new List<string>();
            CarrierInputSpec = new List<string>();
            MtrlOutSpec = new List<OutSpec>();
            ProcessOrderMgnt = 0;
            StartMethod = 0;

            CJState = 0;
        }

        public class OutSpec
        {
            public string SrcCarrierID = "";
            public string DstCarrierID = "";
            public List<int> SrcMap = new List<int>();
            public List<int> DstMap = new List<int>();
        }
    }
}
