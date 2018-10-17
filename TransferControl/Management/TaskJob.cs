using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class TaskJob
    {
        public string TaskName { get; set; }
        public string ExcuteObj { get; set; }
        public string CheckCondition { get; set; }
        public string SkipCondition { get; set; }
        public int TaskIndex { get; set; }
        public class Excuted
        {
            public string NodeName { get; set; }
            public string ExcuteType { get; set; }
            public string ExcuteName { get; set; }
            public Dictionary<string, string> param = new Dictionary<string, string>();
            public Transaction Txn { get; set; }
            public string FinishTrigger { get; set; }
            public bool Finished = false;
        }
        
    }
}
