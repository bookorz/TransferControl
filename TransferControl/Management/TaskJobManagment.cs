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
            public string GotoIndex = "";
        }
        public static void LoadConfig()
        {
            TaskJobList = new ConcurrentDictionary<string, List<TaskJob>>();
            CurrentProceedTasks = new ConcurrentDictionary<string, CurrentProceedTask>();
            
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string Sql = @"SELECT t.task_name as TaskName,t.excute_obj as ExcuteObj, t.check_condition as CheckCondition, t.task_index as TaskIndex
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
        public static bool CheckTask(string Id, string NodeName, string ExcuteType, string ExcuteName, string ReturnType, out string Message)
        {
            bool result = false;
            Message = "";
            CurrentProceedTask tk;

            if (CurrentProceedTasks.TryGetValue(Id, out tk))
            {
                var findExcuted = from each in tk.ProceedTask.CheckList
                                  where each.NodeName.ToUpper().Equals(NodeName.ToUpper()) && each.ExcuteName.ToUpper().Equals(ExcuteName.ToUpper()) && each.ExcuteType.ToUpper().Equals(ExcuteType.ToUpper())
                                  select each;
                if (findExcuted.Count() != 0)
                {
                    if (findExcuted.First().FinishTrigger.ToUpper().Equals(ReturnType.ToUpper()))
                    {
                        findExcuted.First().Finished = true;//把做完的標記完成

                    }
                    findExcuted = from each in tk.ProceedTask.CheckList
                                  where !each.Finished
                                  select each;
                    if (findExcuted.Count() == 0)//當全部完成後，檢查設定的通過條件
                    {
                        result = CheckCondition(Id, out Message, NodeName);
                        tk.ProceedTask.CheckList.Clear();
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

        public static bool CheckCondition(string Id, out string Message,string TriggerNodeName)
        {
            bool result = false;
            string taskName = "";
            Message = "";

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

                        string[] Conditions = eachExcuteObj.Split(new char[] { ':', '=' });
                        if (Conditions.Length >= 2)
                        {
                            string NodeName = Conditions[0];
                            string Attr = Conditions[1];
                            string Value = "";
                            if (Conditions.Length >= 3)
                            {
                                Value = Conditions[2];
                            }
                            string SelfName = "";
                            string PositionName = "";
                            ExcutedTask.Params.TryGetValue("@Self", out SelfName);
                            ExcutedTask.Params.TryGetValue("@Position", out PositionName);
                            Node Self = NodeManagement.Get(SelfName);
                            Node Position = NodeManagement.Get(PositionName);
                            if (NodeName.ToUpper().Equals("FUNCTION"))
                            {
                                
                                switch (Attr.ToUpper())
                                {
                                    case "CHECK_PRESENCE_AND_STATUS_FOR_GET":
                                        if (Self == null)
                                        {
                                            logger.Error("CheckCondition失敗，找不到Node:" + SelfName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                            throw new Exception("CheckCondition失敗，找不到Node:" + SelfName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                        }
                                        else if (Position == null)
                                        {
                                            logger.Error("CheckCondition失敗，找不到Node:" + Position + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                            throw new Exception("CheckCondition失敗，找不到Node:" + Position + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                        }
                                        else
                                        {
                                            //檢查Robot手臂是否為空
                                            
                                            string RPresent = "";
                                            string LPresent = "";

                                            
                                            if (!Self.IO.TryGetValue("R-Present", out RPresent))
                                            {
                                                logger.Error("CheckCondition失敗，找不到R-Present:" + SelfName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                throw new Exception("CheckCondition失敗，找不到R-Present:" + SelfName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                            }
                                            if (!Self.IO.TryGetValue("L-Present", out LPresent))
                                            {
                                                logger.Error("CheckCondition失敗，找不到L-Present:" + SelfName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                throw new Exception("CheckCondition失敗，找不到L-Present:" + SelfName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                            }
                                            if ( RPresent.Equals("0") && LPresent.Equals("0"))
                                            {
                                                //ROBOT沒有在席
                                                if (Position.Type.ToUpper().Equals("LOADPORT"))
                                                {
                                                    //如果目的端是LoadPort，檢查門的狀態
                                                    string Y_Axis_Position = "";
                                                    string Door_Position = "";

                                                    if (!Position.Status.TryGetValue("Y Axis Position", out Y_Axis_Position))
                                                    {
                                                        logger.Error("CheckCondition失敗，找不到Y Axis Position:" + Position + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                        throw new Exception("CheckCondition失敗，找不到Y Axis Position:" + Position + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                    }
                                                    if (!Position.Status.TryGetValue("Door Position", out Door_Position))
                                                    {
                                                        logger.Error("CheckCondition失敗，找不到Door Position:" + Position + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                        throw new Exception("CheckCondition失敗，找不到Door Position:" + Position + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                    }
                                                    if (Y_Axis_Position.Equals("Dock position") && Door_Position.Equals("Open position"))
                                                    {
                                                        result = true;
                                                    }
                                                    else
                                                    {
                                                        
                                                        Message = "00300005";//LOAD_LOCK_NOT_READY
                                                        return false;
                                                    }

                                                }
                                                else
                                                {
                                                    //檢查目的端在席是否存在
                                                    
                                                    if (!Position.IO.TryGetValue("R-Present", out RPresent))
                                                    {
                                                        logger.Error("CheckCondition失敗，找不到R-Present:" + SelfName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                        throw new Exception("CheckCondition失敗，找不到R-Present:" + SelfName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                                    }
                                                    if (RPresent.Equals("1"))
                                                    {
                                                        result = true;
                                                    }
                                                    else
                                                    {
                                                        
                                                        if (Position.Type.ToUpper().Equals("ALIGNER"))
                                                        {
                                                            Message = "00300003";//ALIGNER_NO_WAFER
                                                        }
                                                        else if (Position.Type.ToUpper().Equals("STAGE"))
                                                        {
                                                            Message = "00300012";//LOAD_LOCK_NO_WAFER
                                                        }
                                                        return false;
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                
                                                if (!RPresent.Equals("0"))
                                                {
                                                    Message = "00300007";//ROBOT_NOT_EMPTY_ARM1
                                                }
                                                else if (!LPresent.Equals("0"))
                                                {
                                                    Message = "00300009";//ROBOT_NOT_EMPTY_ARM2
                                                }
                                                return false;
                                            }



                                        }
                                        break;
                                }
                            }
                            else if (NodeName.ToUpper().Equals("GOTO"))
                            {
                                string NextIdx = Conditions[3];

                                switch (Attr.ToUpper())
                                {
                                    case "TYPE":
                                        if (Position.Type.ToUpper().Equals(Value.ToUpper()))
                                        {
                                            ExcutedTask.GotoIndex = NextIdx;
                                        }
                                        
                                        break;
                                    case "TRUE":
                                        ExcutedTask.GotoIndex = NextIdx;
                                        break;
                                }
                                result = true;
                            }
                            else
                            {
                                Node Node = NodeManagement.Get(NodeName);
                                if (Node != null)
                                {
                                    string AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();
                                            if (AttrVal.Equals(Value.ToUpper()))
                                            {
                                                result = true;
                                            }
                                            else
                                            {
                                                
                                                Message += "比對不吻合";
                                                return false;
                                            }
                                           
                                    
                                }
                                else
                                {
                                    logger.Error("CheckCondition失敗，找不到Node:" + NodeName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                    throw new Exception("CheckCondition失敗，找不到Node:" + NodeName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                }
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
                    {//只有帶ID進來，此Task已經開始執行，找到下一個須執行的項目
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
                                           where each.TaskIndex == Convert.ToInt32(ExcutedTask.GotoIndex)
                                           select each;
                            tk = findTask.ToList();
                            ExcutedTask.GotoIndex = "";
                        }
                    }
                    tk.Sort((x, y) => { return x.TaskIndex.CompareTo(y.TaskIndex); });

                    if (tk.Count != 0)
                    {
                        CurrentProceedTask CurrTask = new CurrentProceedTask();
                        CurrTask.ProceedTask = tk.First();
                        CurrTask.ProceedTask.CheckList.Clear();
                        if (CurrTask.ProceedTask.TaskIndex != 1)
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
                                        CurrTask.ProceedTask.CheckList.Add(ex);

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
                                        Txn.RecipeID = "300MM";

                                        TaskJob.Excuted ex = new TaskJob.Excuted();
                                        ex.NodeName = NodeName;
                                        ex.ExcuteName = Method;
                                        ex.ExcuteType = Type;
                                        ex.FinishTrigger = FinishTrigger;
                                        ex.Txn = Txn;

                                        CurrTask.ProceedTask.CheckList.Add(ex);

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
                            foreach (TaskJob.Excuted ex in CurrTask.ProceedTask.CheckList)
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
                                            result = Node.ExcuteScript(ex.ExcuteName, Id, ex.param);
                                            if (!result)
                                            {
                                                ErrorMessage = "Send Command Fail";
                                            }
                                        }
                                        else if (ex.ExcuteType.ToUpper().Equals("CMD"))
                                        {
                                            result = Node.SendCommand(ex.Txn);
                                            if (!result)
                                            {
                                                ErrorMessage = "Send Command Fail";
                                            }
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

                return false;
            }
            return result;
        }
    }
}
