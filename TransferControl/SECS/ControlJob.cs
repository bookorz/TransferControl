using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.SECS
{
    /// <summary>
    /// 參考 SEMI E94.1
    /// </summary>
    class ControlJob
    {
        public string ObjType = "ControlJob";
        public string ObjID  { get; set; }
        public List<string> CarrierInputSpec;//CARRIERID1~CARRIERIDn
        public List<string> CurrentPRJob;//PRJOBID1~PRJOBIDn
        public string DataCollectionPlan = "";//20 (A) DataCollectionPlan
        public Object MtrlOutByStatus;//不使用
        public List<MtrlOut> MtrlOutSpec;
        public List<string> PauseEvent;//collection events
        public Object ProcessingCtrlSpec;//不使用
        public uint ProcessOrderMgmt = 1;//只支援1, 1 = ARRIVAL, 2 = OPTIMIZE, 3 = LIST
        public List<PRJobStatus> PRJobStatusList;
        public Boolean StartMethod = true;//只支援 TRUE, TRUE – Auto, FALSE – UserStart
        public CJStates State = CJStates.QUEUED;//default
    }
    public enum CJStates
    {
        QUEUED,
        SELECTED,
        WAITINGFORSTART,
        EXECUTING,
        PAUSED,
        COMPLETED
    }
    class MtrlOut
    {
        string srcCarrier;
        string[] srcSlots;
        string destCarrier;
        string[] destSlots;
    }
    class PRJobStatus
    {
        string PRJOBID1;
        string PRSTATE1;
    }
}
