using log4net;
using Newtonsoft.Json;
using SANWA.Utility;
using SANWA.Utility.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Engine;
using static TransferControl.Management.TaskJob;

namespace TransferControl.Management
{
    public static class TaskJobManagment
    {
        static ILog logger = LogManager.GetLogger(typeof(TaskJobManagment));
        static ConcurrentDictionary<string, List<TaskJob>> TaskJobList;
        static ConcurrentDictionary<string, CurrentProceedTask> CurrentProceedTasks;
        private static DBUtil dBUtil = new DBUtil();

        public class CurrentProceedTask
        {
            public TaskJob ProceedTask { get; set; }
            public Dictionary<string, string> Params { get; set; }
            public List<Excuted> CheckList = new List<Excuted>();
            public string GotoIndex = "";
        }
        public static void LoadConfig()
        {
            TaskJobList = new ConcurrentDictionary<string, List<TaskJob>>();
            CurrentProceedTasks = new ConcurrentDictionary<string, CurrentProceedTask>();

            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string Sql = @"SELECT t.task_name as TaskName,t.excute_obj as ExcuteObj, t.check_condition as CheckCondition, t.task_index as TaskIndex,t.skip_condition as SkipCondition
                            FROM config_task_job t WHERE t.equipment_model_id = @equipment_model_id";
            keyValues.Add("@equipment_model_id", SystemConfig.Get().SystemMode);
            DataTable dt = dBUtil.GetDataTable(Sql, keyValues);
            string str_json = JsonConvert.SerializeObject(dt, Formatting.Indented);

            List<TaskJob> tJList = JsonConvert.DeserializeObject<List<TaskJob>>(str_json);
            List<TaskJob> tmp;

            foreach (TaskJob each in tJList)
            {
                if (TaskJobList.TryGetValue(each.TaskName, out tmp))
                {
                    tmp.Add(each);
                }
                else
                {
                    tmp = new List<TaskJob>();
                    tmp.Add(each);
                    TaskJobList.TryAdd(each.TaskName, tmp);
                }


            }

        }
        /// <summary>
        /// 檢查是否為有效TaskID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static bool IsTask(string Id)
        {
            if (CurrentProceedTasks.ContainsKey(Id))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 標記完成Task內的工作
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="ExcuteType"></param>
        /// <param name="ExcuteName"></param>
        /// <param name="ReturnType">Executed/Finished</param>
        /// <returns>true:當工作全部完成</returns>
        public static bool CheckTask(string Id, string NodeName, string ExcuteType, string ExcuteName, string ReturnType, out string Message, out string Report)
        {
            bool result = false;
            Message = "";
            Report = "";
            CurrentProceedTask tk;

            if (ExcuteName.Equals("DoubleGet"))
            {
                ExcuteName = "Get";
            }
            if (ExcuteName.Equals("DoublePut"))
            {
                ExcuteName = "Put";

            }

            if (CurrentProceedTasks.TryGetValue(Id, out tk))
            {
                var findExcuted = from each in tk.CheckList
                                  where each.NodeName.ToUpper().Equals(NodeName.ToUpper()) && each.ExcuteName.ToUpper().Equals(ExcuteName.ToUpper()) && each.ExcuteType.ToUpper().Equals(ExcuteType.ToUpper())
                                  select each;
                if (findExcuted.Count() != 0)
                {
                    if (findExcuted.First().FinishTrigger.ToUpper().Equals(ReturnType.ToUpper()))
                    {
                        findExcuted.First().Finished = true;//把做完的標記完成

                    }
                    findExcuted = from each in tk.CheckList
                                  where !each.Finished
                                  select each;
                    if (findExcuted.Count() == 0)//當全部完成後，檢查設定的通過條件
                    {
                        if (ExcuteName.ToUpper().Equals("STOP"))
                        {
                            System.Threading.Thread.Sleep(1000);
                        }
                        result = CheckCondition(Id, NodeName, out Message, out Report);
                        tk.CheckList.Clear();
                    }
                }
                else
                {
                    //logger.Error("CheckTask失敗，找不到設定值.ExcuteName:" + ExcuteName+ " ExcuteType:"+ ExcuteType);
                    //throw new Exception("CheckTask失敗，找不到Id:" + Id);
                    //Message += " " + "CheckTask失敗，找不到設定值.ExcuteName:" + ExcuteName + " ExcuteType:" + ExcuteType;
                }
            }
            else
            {
                logger.Error("CheckTask失敗，找不到Id:" + Id);
                //throw new Exception("CheckTask失敗，找不到Id:" + Id);
                Message += " " + "CheckTask失敗，找不到Id:" + Id;
            }

            return result;
        }

        public static bool CheckCondition(string Id, string TriggerNodeName, out string Message, out string Report)
        {
            bool result = false;
            string taskName = "";
            Message = "";
            Report = "";
            //bool UnCheck = false;

            Node TriggerNode = NodeManagement.Get(TriggerNodeName);
            if (TriggerNode == null)
            {
                logger.Error("CheckCondition失敗，找不到TriggerNode:" + TriggerNodeName);
                throw new Exception("CheckCondition失敗，找不到TriggerNode:" + TriggerNodeName);
            }
            try
            {
                CurrentProceedTask ExcutedTask = null;
                if (CurrentProceedTasks.TryGetValue(Id, out ExcutedTask))
                {
                    string ConditionsStr = ExcutedTask.ProceedTask.CheckCondition;
                    //指令替代
                    if (ExcutedTask.Params != null)
                    {
                        foreach (KeyValuePair<string, string> item in ExcutedTask.Params)
                        {
                            ConditionsStr = ConditionsStr.Replace(item.Key, item.Value);
                        }
                    }
                    else
                    {
                        ExcutedTask.Params = new Dictionary<string, string>();
                    }


                    string[] ExcuteObjs = ConditionsStr.Split(';');
                    if (ExcutedTask.ProceedTask.CheckCondition.Equals(""))
                    {
                        return true;
                    }
                    //e.g. ALIGNER01:InitialComplete=TRUE;LOADPORT01:InitialComplete=TRUE;LOADPORT01:InitialComplete=TRUE;
                    foreach (string eachExcuteObj in ExcuteObjs)
                    {
                        if (eachExcuteObj.Trim().Equals(""))
                        {
                            continue;
                        }

                        string[] Conditions = eachExcuteObj.Split(new char[] { ':' });
                        if (Conditions.Length >= 2)
                        {
                            string Type = Conditions[0];
                            string NodeName = "";
                            string Attr = Conditions[1];
                            string Value = "";
                            Node Node = null;
                            string ErrorType = "";
                            string ErrorCode = "";
                            string TargetName = "";
                            string PositionName = "";
                            string Slot = "";
                            int Val = 0;
                            int diff = 0;

                            ExcutedTask.Params.TryGetValue("@Target", out TargetName);
                            ExcutedTask.Params.TryGetValue("@Position", out PositionName);
                            ExcutedTask.Params.TryGetValue("@Slot", out Slot);
                            //Node Target = NodeManagement.Get(TargetName);
                            //Node Position = NodeManagement.Get(PositionName);
                            switch (Type.ToUpper())
                            {
                                case "REPORT":

                                    result = true;
                                    Report = Attr;
                                    break;

                                case "DIO":

                                    string Param = Conditions[1].Split('=')[0];
                                    Value = Conditions[1].Split('=')[1];
                                    ErrorType = Conditions[2].Split('=')[0];
                                    ErrorCode = Conditions[2].Split('=')[1];
                                    string CurrVal = RouteControl.DIO.GetIO("IN", Param);
                                    if (CurrVal.ToUpper().Equals(Value.ToUpper()))
                                    {
                                        result = true;
                                    }
                                    else
                                    {
                                        Report = ErrorType;
                                        Message = ErrorCode;
                                        result = false;
                                    }
                                    break;
                                case "SET":

                                    NodeName = Conditions[1];
                                    Attr = Conditions[2].Split('=')[0];
                                    Value = Conditions[2].Split('=')[1];
                                    Node = NodeManagement.Get(NodeName);
                                    if (Node != null)
                                    {
                                        //string AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();
                                        switch (Node.GetType().GetProperty(Attr).PropertyType.Name)
                                        {
                                            case "String":
                                                try
                                                {
                                                    Node.GetType().GetProperty(Attr).SetValue(Node, Value);
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Error("CheckCondition失敗，String型別不符，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                    throw new Exception("CheckCondition失敗，String型別不符，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                }
                                                break;
                                            case "Int64":
                                            case "Int32":
                                            case "Int16":
                                                try
                                                {
                                                    Node.GetType().GetProperty(Attr).SetValue(Node, Convert.ToInt32(Value));
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Error("CheckCondition失敗，Int型別不符，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                    throw new Exception("CheckCondition失敗，Int型別不符，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                }
                                                break;
                                            case "Boolean":
                                                try
                                                {
                                                    Node.GetType().GetProperty(Attr).SetValue(Node, bool.Parse(Value));
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Error("CheckCondition失敗，Bool型別不符，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                    throw new Exception("CheckCondition失敗，Bool型別不符，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                }
                                                break;
                                            default:
                                                logger.Error("CheckCondition失敗，型別不符，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                throw new Exception("CheckCondition失敗，型別不符，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                        }
                                    }
                                    else
                                    {
                                        logger.Error("CheckCondition失敗，找不到Node:" + NodeName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                        throw new Exception("CheckCondition失敗，找不到Node:" + NodeName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                    }
                                    break;
                                case "FUNCTION":

                                    string FunctionName = Conditions[1];
                                    ErrorType = Conditions[2].Split('=')[0];
                                    ErrorCode = Conditions[2].Split('=')[1];
                                    Node TarNode = null;
                                    switch (FunctionName)
                                    {
                                        case "GetSaftyCheck":
                                            TarNode = NodeManagement.Get(PositionName);

                                            int slotNo = 0;
                                            if (int.TryParse(Slot, out slotNo))
                                            {
                                                Job SlotData = null;
                                                TarNode.JobList.TryGetValue(slotNo.ToString(), out SlotData);
                                                if (SlotData.MapFlag && !SlotData.ErrPosition)
                                                {
                                                    result = true;
                                                }
                                                else
                                                {
                                                    Report = ErrorType;
                                                    Message = ErrorCode;
                                                    result = false;
                                                }

                                            }
                                            else
                                            {
                                                Report = ErrorType;
                                                Message = ErrorCode;
                                                result = false;
                                            }
                                            break;
                                        case "AccessSequentially":
                                            //string Slot = 
                                            //TarNode = NodeManagement.Get(TargetName);

                                            //if (TarNode.R_Flip_Degree.Equals("0"))
                                            //{
                                            //    //取放片Slot 1 不檢查，其餘Slot的前一個Slot不能有片

                                            //}
                                            //else if (TarNode.R_Flip_Degree.Equals("180"))
                                            //{
                                            //    //取放片Slot 25 不檢查，其餘Slot的後一個Slot不能有片

                                            //}
                                            //else
                                            //{
                                            //    result = false;
                                            //}
                                            result = true;
                                            break;
                                        case "Check_LP_Safty":

                                            TarNode = NodeManagement.Get("ROBOT01");
                                            Val = 0;
                                            if (TarNode.CurrentPosition.ToUpper().Equals(TargetName))
                                            {
                                                Val = Convert.ToInt32(TarNode.R_Position);
                                                diff = 0 - Val;
                                                diff = Math.Abs(diff);
                                                logger.Debug("Diff:" + diff.ToString());
                                                if (diff > 500)
                                                {
                                                    Report = ErrorType;
                                                    Message = ErrorCode;
                                                    result = false;
                                                }
                                                else
                                                {
                                                    result = true;
                                                }
                                            }
                                            else
                                            {
                                                result = true;
                                            }


                                            break;
                                        case "Check_X_AXIS_Position":

                                            TarNode = NodeManagement.Get(TargetName);
                                            int Spec = 0;
                                            Val = 0;
                                            switch (TarNode.CurrentPoint)
                                            {
                                                case "71":
                                                    Spec = Convert.ToInt32("00009062");

                                                    break;
                                                case "72":
                                                    //00358343
                                                    Spec = Convert.ToInt32("00358343");
                                                    break;
                                                case "73":
                                                    //01057385
                                                    Spec = Convert.ToInt32("01057385");
                                                    break;
                                                case "74":
                                                    //001407895
                                                    Spec = Convert.ToInt32("001407895");
                                                    break;
                                                case "1":
                                                    //00164791
                                                    Spec = Convert.ToInt32("00164791");//164810
                                                    break;
                                                case "2":
                                                    //00797166
                                                    Spec = Convert.ToInt32("00824895");//824906
                                                    break;
                                                case "81":
                                                    Spec = Convert.ToInt32("00021510");

                                                    break;
                                                case "82":
                                                    //00371187
                                                    Spec = Convert.ToInt32("00371187");
                                                    break;
                                                case "83":
                                                    //01070479
                                                    Spec = Convert.ToInt32("01070479");
                                                    break;
                                                case "84":
                                                    //01420896
                                                    Spec = Convert.ToInt32("01420896");
                                                    break;

                                            }
                                            Val = Convert.ToInt32(TarNode.X_Position);
                                            diff = Spec - Val;
                                            diff = Math.Abs(diff);
                                            logger.Debug("Diff:" + diff.ToString());
                                            if (diff > 500)
                                            {
                                                Report = ErrorType;
                                                Message = ErrorCode;
                                                result = false;
                                            }
                                            else
                                            {
                                                result = true;
                                            }
                                            break;
                                        default:
                                            result = true;
                                            break;
                                    }


                                    break;
                                case "CHECK":

                                    //ALIGNER02:InitialComplete=TRUE:ERR=8888888
                                    //     0            1                 2
                                    NodeName = Conditions[1];
                                    Attr = Conditions[2].Split('=')[0];
                                    Value = Conditions[2].Split('=')[1];
                                    ErrorType = Conditions[3].Split('=')[0];
                                    if (ErrorType.Equals("DIO"))
                                    {
                                        string ParamName = Conditions[3].Split('=')[1];
                                        string Set = Conditions[3].Split('=')[2];
                                        Node = NodeManagement.Get(NodeName);
                                        if (Node != null)
                                        {
                                            string AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();
                                            if (AttrVal.Equals(Value.ToUpper()))
                                            {
                                                RouteControl.DIO.SetIO(ParamName, Set);

                                            }

                                        }
                                        else
                                        {
                                            logger.Error("CheckCondition失敗，找不到Node:" + NodeName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                            throw new Exception("CheckCondition失敗，找不到Node:" + NodeName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                        }
                                        result = true;

                                    }
                                    else
                                    {
                                        ErrorCode = Conditions[3].Split('=')[1];
                                        Node = NodeManagement.Get(NodeName);
                                        if (Node != null)
                                        {
                                            string AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();
                                            if (AttrVal.Equals(Value.ToUpper()))
                                            {
                                                result = true;
                                            }
                                            else
                                            {
                                                Report = ErrorType;
                                                Message = ErrorCode;
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            logger.Error("CheckCondition失敗，找不到Node:" + NodeName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                            throw new Exception("CheckCondition失敗，找不到Node:" + NodeName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                        }
                                    }

                                    break;
                            }
                        }
                        else
                        {
                            logger.Error("Task CheckCondition 解析失敗，Task Name:" + taskName);
                            throw new Exception("Task CheckCondition 解析失敗，Task Name:" + taskName);
                        }
                    }
                }
                else
                {
                    logger.Error("CheckCondition失敗，找不到ID:" + Id);
                    throw new Exception("CheckCondition失敗，找不到ID:" + Id);
                }


            }
            catch (Exception e)
            {
                logger.Error("CheckCondition fail Task Id:" + Id + " exception: " + e.StackTrace);
                throw new Exception("CheckCondition fail Task Id:" + Id + " exception: " + e.Message);
            }

            return result;
        }

        public static void Remove(string Id)
        {
            CurrentProceedTask tmp;
            CurrentProceedTasks.TryRemove(Id, out tmp);
        }

        public static bool Excute(string Id, out string ErrorMessage, string taskName = "", Dictionary<string, string> param = null)
        {
            bool result = false;
            try
            {
                CurrentProceedTask tmpTk;
                List<TaskJob> tk;
                CurrentProceedTask ExcutedTask = null;
                Dictionary<string, string> LastParam = null;
                ErrorMessage = "";
                if (taskName.Equals(""))
                {
                    //只有帶ID進來，此Task已經開始執行，尋找上一次執行的Task
                    if (CurrentProceedTasks.TryGetValue(Id, out ExcutedTask))
                    {
                        taskName = ExcutedTask.ProceedTask.TaskName;
                        Remove(Id);
                        LastParam = ExcutedTask.Params;
                    }
                    else
                    {
                        logger.Error("ExcuteObj失敗，找不到ID:" + Id);
                        //throw new Exception("ExcuteObj失敗，找不到ID:" + Id);
                        ErrorMessage = "ExcuteObj失敗，找不到ID:" + Id;
                        return false;
                    }
                }

                if (TaskJobList.TryGetValue(taskName, out tk))
                {
                    if (ExcutedTask != null)
                    {


                        //只有帶ID進來，此Task已經開始執行，找到下一個須執行的項目
                        if (ExcutedTask.GotoIndex.Equals(""))
                        {
                            var findTask = from each in tk
                                           where each.TaskIndex > ExcutedTask.ProceedTask.TaskIndex
                                           select each;
                            tk = findTask.ToList();
                        }
                        else
                        {//如果有GOTO的Index，優先執行
                            var findTask = from each in tk
                                           where each.TaskIndex >= Convert.ToInt32(ExcutedTask.GotoIndex)
                                           select each;
                            tk = findTask.ToList();
                            ExcutedTask.GotoIndex = "";
                        }
                    }

                    //略過所有Skip condition符合項目
                    List<TaskJob> tmp1 = new List<TaskJob>();
                    Dictionary<string, string> CurrParam = null;
                    if (param == null)
                    {
                        CurrParam = LastParam;//拿之前的
                    }
                    else
                    {
                        CurrParam = param;//用傳入的
                    }
                    foreach (TaskJob eachTask in tk)
                    {
                        string SkipCondition = "";
                        if (eachTask.SkipCondition.Trim().Equals(""))
                        {
                            tmp1.Add(eachTask);
                        }
                        else
                        {
                            SkipCondition = eachTask.SkipCondition;
                            foreach (KeyValuePair<string, string> item in CurrParam)
                            {
                                SkipCondition = SkipCondition.Replace(item.Key, item.Value);
                            }
                            string[] SkipConditionAry = SkipCondition.Split(';');
                            foreach (string eachSkip in SkipConditionAry)
                            {
                                if (eachSkip.Trim().Equals(""))
                                {
                                    continue;
                                }
                                string[] EachSkipConditionAry = eachSkip.Split(':');
                                string NodeName = EachSkipConditionAry[0];
                                Node Node = NodeManagement.Get(NodeName);
                                if (Node != null)
                                {

                                    if (EachSkipConditionAry[1].IndexOf("=") != -1)
                                    {
                                        string[] ConditionAry = EachSkipConditionAry[1].Split('=');
                                        string Value = ConditionAry[1];
                                        string Attr = ConditionAry[0];
                                        string AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();
                                        if (AttrVal.Equals(Value.ToUpper()))
                                        {
                                            break;
                                        }
                                    }
                                    else if (EachSkipConditionAry[1].IndexOf("<>") != -1)
                                    {
                                        string[] ConditionAry = EachSkipConditionAry[1].Split(new string[] { "<>" }, StringSplitOptions.None);
                                        string Value = ConditionAry[1];
                                        string Attr = ConditionAry[0];
                                        string AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();
                                        if (!AttrVal.Equals(Value.ToUpper()))
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        logger.Error("SkipCondition失敗，格式錯誤，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                        throw new Exception("SkipCondition失敗，格式錯誤，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                    }
                                }
                                else
                                {
                                    logger.Error("SkipCondition失敗，找不到Node:" + NodeName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                    throw new Exception("SkipCondition失敗，找不到Node:" + NodeName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                }
                                tmp1.Add(eachTask);
                            }

                        }


                    }
                    tk = tmp1;

                    tk.Sort((x, y) => { return x.TaskIndex.CompareTo(y.TaskIndex); });

                    if (tk.Count != 0)
                    {
                        CurrentProceedTask CurrTask = new CurrentProceedTask();
                        CurrTask.ProceedTask = tk.First();
                        CurrTask.CheckList.Clear();
                        if (param == null)
                        {//拿之前的
                            CurrTask.Params = LastParam;
                        }
                        else
                        {//用傳入的
                            CurrTask.Params = param;
                        }
                        if (CurrentProceedTasks.TryAdd(Id, CurrTask))
                        {
                            string ExcuteObjStr = CurrTask.ProceedTask.ExcuteObj;


                            //指令替代
                            if (CurrTask.Params != null)
                            {
                                foreach (KeyValuePair<string, string> item in CurrTask.Params)
                                {
                                    ExcuteObjStr = ExcuteObjStr.Replace(item.Key, item.Value);
                                }
                            }
                            else
                            {
                                CurrTask.Params = new Dictionary<string, string>();
                            }
                            string[] ExcuteObjs = ExcuteObjStr.Split(';');
                            //e.g. ALIGNER01:SCRIPT:AlignerInit:NONE;LOADPORT01:SCRIPT:LoadPortInit:NONE;LOADPORT02:SCRIPT:LoadPortInit:NONE;
                            foreach (string eachExcuteObj in ExcuteObjs)
                            {
                                if (eachExcuteObj.Trim().Equals(""))
                                {
                                    continue;
                                }
                                string[] ExcuteObj = eachExcuteObj.Split(':');
                                if (ExcuteObj.Length == 4)
                                {
                                    string NodeName = ExcuteObj[0];
                                    string Type = ExcuteObj[1];

                                    if (Type.ToUpper().Equals("SCRIPT"))
                                    {
                                        string ScriptName = ExcuteObj[2];
                                        string[] Params = ExcuteObj[3].Split(',');

                                        foreach (string eachParam in Params)
                                        {
                                            if (eachParam.Equals("NONE"))
                                            {
                                                break;
                                            }
                                            else
                                            {
                                                string[] tmp = eachParam.Split('=');
                                                if (tmp.Length == 2)
                                                {
                                                    param.Add(tmp[0], tmp[1]);
                                                }
                                                else
                                                {
                                                    logger.Error("Task Parameter 解析失敗，Task Name:" + taskName);
                                                    CurrentProceedTasks.TryRemove(Id, out tmpTk);
                                                    //throw new Exception("Task Parameter 解析失敗，Task Name:" + taskName);
                                                    ErrorMessage = "Task Parameter 解析失敗，Task Name:" + taskName;
                                                    return false;
                                                }
                                            }
                                        }


                                        TaskJob.Excuted ex = new TaskJob.Excuted();
                                        ex.NodeName = NodeName;
                                        ex.ExcuteName = ScriptName;
                                        ex.ExcuteType = Type;
                                        ex.FinishTrigger = "Finished";
                                        ex.param = param;
                                        CurrTask.CheckList.Add(ex);

                                    }
                                    else if (Type.ToUpper().Equals("CMD"))
                                    {
                                        string Method = ExcuteObj[2];
                                        string[] Param = ExcuteObj[3].Split(',');

                                        string Position = "";
                                        string Arm = "";
                                        string Slot = "";
                                        string Position2 = "";
                                        string Arm2 = "";
                                        string Slot2 = "";
                                        string Value = "";
                                        string FinishTrigger = "";

                                        foreach (string each in Param)
                                        {
                                            string[] tmp = each.Split('=');
                                            if (tmp.Length == 2)
                                            {
                                                switch (tmp[0].ToUpper())
                                                {
                                                    case "POSITION":
                                                        Position = tmp[1];
                                                        break;
                                                    case "ARM":
                                                        Arm = tmp[1];
                                                        break;
                                                    case "SLOT":
                                                        Slot = tmp[1];
                                                        break;
                                                    case "POSITION2":
                                                        Position2 = tmp[1];
                                                        break;
                                                    case "ARM2":
                                                        Arm2 = tmp[1];
                                                        break;
                                                    case "SLOT2":
                                                        Slot2 = tmp[1];
                                                        break;
                                                    case "VALUE":
                                                        Value = tmp[1];
                                                        break;
                                                    case "FIN":
                                                        FinishTrigger = tmp[1].ToUpper();
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                logger.Error("Task Parameter 解析失敗，Task Name:" + taskName);
                                                CurrentProceedTasks.TryRemove(Id, out tmpTk);
                                                //throw new Exception("Task Parameter 解析失敗，Task Name:" + taskName);
                                                ErrorMessage = "Task Parameter 解析失敗，Task Name:" + taskName;
                                                return false;
                                            }

                                        }

                                        Transaction Txn = new Transaction();
                                        Txn.Method = Method;
                                        Txn.Position = Position;
                                        Txn.Arm = Arm;
                                        Txn.Slot = Slot;
                                        Txn.Position2 = Position2;
                                        Txn.Arm2 = Arm2;
                                        Txn.Slot2 = Slot2;
                                        Txn.Value = Value;
                                        Txn.FormName = Id;
                                        //Txn.RecipeID = "300MM";



                                        TaskJob.Excuted ex = new TaskJob.Excuted();
                                        ex.NodeName = NodeName;
                                        ex.ExcuteName = Method;
                                        ex.ExcuteType = Type;
                                        ex.FinishTrigger = FinishTrigger;
                                        ex.Txn = Txn;

                                        CurrTask.CheckList.Add(ex);

                                    }
                                }
                                else
                                {
                                    logger.Error("ExcuteObj 解析失敗，Task Name:" + taskName);
                                    CurrentProceedTasks.TryRemove(Id, out tmpTk);
                                    //throw new Exception("ExcuteObj 解析失敗，Task Name:" + taskName);
                                    ErrorMessage = "ExcuteObj 解析失敗，Task Name:" + taskName;
                                    return false;
                                }
                            }
                            foreach (TaskJob.Excuted ex in CurrTask.CheckList.ToList())
                            {

                                Node Node = NodeManagement.Get(ex.NodeName);
                                if (Node != null)
                                {
                                    if (Node.ByPass)
                                    {
                                        logger.Error("ExcuteObj失敗，Node:" + ex.NodeName + " 目前為Bypass模式!");
                                        CurrentProceedTasks.TryRemove(Id, out tmpTk);
                                        ErrorMessage = "Bypass Mode";
                                        return false;
                                    }
                                    else
                                    {
                                        if (ex.ExcuteType.ToUpper().Equals("SCRIPT"))
                                        {
                                            result = Node.ExcuteScript(ex.ExcuteName, Id, ex.param, out ErrorMessage);

                                        }
                                        else if (ex.ExcuteType.ToUpper().Equals("CMD"))
                                        {
                                            result = Node.SendCommand(ex.Txn, out ErrorMessage);

                                        }
                                    }
                                }
                                else
                                {
                                    logger.Error("ExcuteObj失敗，找不到Node:" + ex.NodeName + "，Task Name:" + taskName);

                                    CurrentProceedTasks.TryRemove(Id, out tmpTk);
                                    //throw new Exception("ExcuteObj失敗，找不到Node:" + NodeName + "，Task Name:" + taskName);
                                    ErrorMessage = "Node NotFound";
                                    return false;
                                }



                            }
                            result = true;
                        }
                        else
                        {
                            logger.Error("Task加入失敗，TaskId:" + Id);
                            //throw new Exception("Task加入失敗，TaskId:" + Id);
                            ErrorMessage = "Task加入失敗，TaskId:" + Id;
                            return false;
                        }
                    }
                    else
                    {
                        logger.Error("已找不到Task，完成工作，TaskId:" + Id);
                        Remove(Id);
                        //throw new Exception("已找不到Task，完成工作，TaskId:" + Id);
                        //ErrorMessage = "已找不到Task，完成工作，TaskId:" + Id;
                        return false;
                    }
                }
                else
                {
                    logger.Error("找不到此Task Name:" + taskName);
                    //throw new Exception("找不到此Task Name:" + taskName);
                    ErrorMessage = "找不到此Task Name:" + taskName;
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Error("Excute fail Task Name:" + taskName + " exception: " + e.StackTrace);
                //throw new Exception("Excute fail Task Name:" + taskName + " exception: " + e.Message);
                ErrorMessage = "Excute fail Task Name:" + taskName + " exception: " + e.Message;
                Remove(Id);
                return false;
            }
            return result;
        }
    }
}
