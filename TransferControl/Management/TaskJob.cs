using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class TaskJob
    {
        public string equipment_model_id { get; set; }
        [BsonField("task_name")]
        public string TaskName { get; set; }
        [BsonField("excute_obj")]
        public string ExcuteObj { get; set; }
        [BsonField("check_condition")]
        public string CheckCondition { get; set; }
        [BsonField("skip_condition")]
        public string SkipCondition { get; set; }
        [BsonField("is_safety_check")]
        public bool IsSafetyCheck { get; set; }
        [BsonField("task_index")]
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
