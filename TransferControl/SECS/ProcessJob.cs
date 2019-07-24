using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.SECS
{
    public class ProcessJob
    {
        public string ObjID { get; set; }
        public string ObjType = "ProcessJob";
        public List<string> PauseEvent;//collection events
        public PJStates State = PJStates.QUEUED;//default
        public List<Carrier> PRMtlNameList;
        public MF PRMtlType = MF.QtyCarrier;//只支援 carriers (e.g., FOUP, SMIF pod, cassette)
        //Indicates that the process resource start processing immediately when ready:
        public Boolean PRProcessStart = true;//TRUE = Automatic Start(只支援這個), FALSE = Manual Start
        public PJRecipeMethod PRRecipeMethod = PJRecipeMethod.RECIPE_ONLY;//只支援這個
        public string RecID { get; set; }
        public Dictionary<string, string> RecVariableList;//會支援這個, RCPPARNM1,RCPPARVAL1 ... RCPPARNMn,RCPPARVALn
    }
    public enum MF
    {
        QtyCarrier = 13,
        QtySubstrates = 14
    }
    public enum PJStates
    {
        QUEUED,
        SETTING_UP,
        WAITING_FOR_START,
        PROCESSING,
        PROCESS_COMPLETE,
        Reserved,
        PAUSING,
        PAUSED,
        STOPPING,
        ABORTING,
        STOPPED,
        ABORTED
    }
    public class Carrier
    {
        public string PORTID;//非 SEMI定義，　Sorter 程式自己要使用的
        public string CARRIERID;
        public List<string> Slots;//collection events

    }
    public enum PJRecipeMethod
    {
        RECIPE_ONLY = 1,
        RECIPE_WITH_VARIABLE_TUNING = 2
    }
}
