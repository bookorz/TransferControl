using log4net;
using Newtonsoft.Json;
using SANWA.Utility;
using SANWA.Utility.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using TransferControl.Engine;


namespace TransferControl.Management
{
    public class TaskJobManagment
    {
        ILog logger = LogManager.GetLogger(typeof(TaskJobManagment));
        ConcurrentDictionary<string, List<TaskJob>> TaskJobList;
        ConcurrentDictionary<string, CurrentProceedTask> CurrentProceedTasks;
        private DBUtil dBUtil = new DBUtil();
        ITaskJobReport _TaskReport;
        bool SaftyCheckByPass = true;

        public class CurrentProceedTask
        {
            public string Id { get; set; }
            public TaskJob ProceedTask { get; set; }
            public Dictionary<string, string> Params { get; set; }
            public List<TaskJob.Excuted> CheckList = new List<TaskJob.Excuted>();
            public string GotoIndex = "";
            public int ExcutedCount = 0;
            public bool Finished = false;
            public bool HasError = false;
            public string MainTaskId = "";
        }
        public TaskJobManagment(ITaskJobReport TaskReport)
        {
            SaftyCheckByPass = SANWA.Utility.Config.SystemConfig.Get().SaftyCheckByPass;
            _TaskReport = TaskReport;
            TaskJobList = new ConcurrentDictionary<string, List<TaskJob>>();
            CurrentProceedTasks = new ConcurrentDictionary<string, CurrentProceedTask>();

            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            string Sql = @"SELECT t.task_name as TaskName,t.excute_obj as ExcuteObj, t.check_condition as CheckCondition, t.task_index as TaskIndex,t.skip_condition as SkipCondition,t.is_safety_check as IsSafetyCheck
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
        public bool IsTask(string Id, out CurrentProceedTask tk)
        {
            tk = null;
            if (CurrentProceedTasks.TryGetValue(Id, out tk))
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
        public bool CheckTask(string Id, string NodeName, string ExcuteType, string ExcuteName, string ReturnType, out string Message, out string Report, out string Location)
        {
            bool result = false;
            Message = "";
            Report = "";
            Location = "";
            CurrentProceedTask tk;

            if (ExcuteName.Equals("DoubleGet"))
            {
                ExcuteName = "Get";
            }
            if (ExcuteName.Equals("DoublePut"))
            {
                ExcuteName = "Put";
            }
            logger.Debug("CurrentProceedTasks Count = " + CurrentProceedTasks.Count());
            if (CurrentProceedTasks.TryGetValue(Id, out tk))
            {
                lock (tk)//避免兩個命令同時回報完成，造成同時命令完成
                {
                    var findExcuted = from each in tk.CheckList
                                      where each.NodeName.ToUpper().Equals(NodeName.ToUpper()) && each.ExcuteName.ToUpper().Equals(ExcuteName.ToUpper()) && each.ExcuteType.ToUpper().Equals(ExcuteType.ToUpper())
                                      select each;
                    if (findExcuted.Count() != 0)
                    {
                        TaskJob.Excuted tmp = findExcuted.First();
                        if (tmp.FinishTrigger.ToUpper().Equals(ReturnType.ToUpper()))
                        {
                            tmp.Finished = true;//把做完的標記完成
                            logger.Debug("命令完成:" + ExcuteName + " Node:" + NodeName + "ExcuteType:" + ExcuteType);
                        }

                        findExcuted = from each in tk.CheckList
                                      where !each.Finished
                                      select each;
                        if (findExcuted.Count() == 0)//當全部完成後，檢查設定的通過條件
                        {
                            //logger.Debug("全部命令完成，檢查通過條件");
                            result = CheckCondition(Id, out Message, out Report, out Location);
                            tk.CheckList.Clear();
                        }
                    }
                    else if (ExcuteName.Equals(""))
                    {
                        //Excute為空白

                        result = CheckCondition(Id, out Message, out Report, out Location);


                        //logger.Error("CheckTask失敗，找不到設定值.ExcuteName:" + ExcuteName+ " ExcuteType:"+ ExcuteType);
                        //throw new Exception("CheckTask失敗，找不到Id:" + Id);
                        //Message += " " + "CheckTask失敗，找不到設定值.ExcuteName:" + ExcuteName + " ExcuteType:" + ExcuteType;
                    }
                    else
                    {
                        logger.Error("CheckTask失敗，找不到該命令:" + ExcuteName + " Node:" + NodeName);
                    }
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

        public bool CheckCondition(string Id, out string Message, out string Report, out string Location)
        {
            bool result = false;
            string taskName = "";
            Message = "";
            Report = "";
            Location = "";
            //bool UnCheck = false;

            //Node TriggerNode = NodeManagement.Get(TriggerNodeName);
            //if (TriggerNode == null)
            //{
            //    logger.Error("CheckCondition失敗，找不到TriggerNode:" + TriggerNodeName);
            //    throw new Exception("CheckCondition失敗，找不到TriggerNode:" + TriggerNodeName);
            //}
            try
            {
                CurrentProceedTask ExcutedTask = null;
                if (CurrentProceedTasks.TryGetValue(Id, out ExcutedTask))
                {
                    if (ExcutedTask.ProceedTask.IsSafetyCheck && this.SaftyCheckByPass)
                    {//當安全檢查取消打開時，跳過所有標記為安全檢查的項目
                        return true;
                    }
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
                    if (ExcutedTask.ProceedTask.CheckCondition.Trim().Equals(""))
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
                            Location = "";
                            string[] tmpAry;
                            string TargetName = "";
                            string PositionName = "";
                            string FromPosition = "";
                            string FromSlot = "";
                            string ToPosition = "";
                            string ToSlot = "";
                            string Slot = "";
                            string Arm = "";
                            int Val = 0;
                            int diff = 0;

                            ExcutedTask.Params.TryGetValue("@Target", out TargetName);
                            ExcutedTask.Params.TryGetValue("@Position", out PositionName);
                            if (PositionName == null)
                            {
                                PositionName = "";
                            }
                            ExcutedTask.Params.TryGetValue("@Slot", out Slot);
                            //Node Target = NodeManagement.Get(TargetName);
                            //Node Position = NodeManagement.Get(PositionName);
                            switch (Type.ToUpper())
                            {                                          //times:interval  
                                case "REPEAT"://REPEAT:@Target:IsPause=true:10:200;
                                    if (Conditions.Length >= 5)
                                    {
                                        NodeName = Conditions[1];
                                        Attr = Conditions[2].Split('=')[0];
                                        Value = Conditions[2].Split('=')[1];
                                        int times = Convert.ToInt32(Conditions[3]);
                                        int interval = Convert.ToInt32(Conditions[4]);
                                        if(ExcutedTask.ExcutedCount>= times)
                                        {
                                            //次數達成就離開迴圈
                                            result = true;
                                            ExcutedTask.ExcutedCount = 0;
                                            break;
                                        }
                                        ExcutedTask.ExcutedCount++;
                                        //delay
                                        SpinWait.SpinUntil(() => false, interval);

                                        Node = NodeManagement.Get(NodeName);
                                        if (!Node.Enable)
                                        {
                                            result = true;
                                            break;
                                        }
                                        if (Node != null)
                                        {
                                            string AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();
                                            if (!AttrVal.Equals(Value.ToUpper()))
                                            {
                                                //如果條件尚未達成，再做一次
                                                ExcutedTask.GotoIndex = ExcutedTask.ProceedTask.TaskIndex.ToString();

                                            }
                                            result = true;
                                        }
                                        else
                                        {
                                            logger.Error("CheckCondition失敗，找不到Node:" + NodeName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                            throw new Exception("CheckCondition失敗，找不到Node:" + NodeName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                        }

                                    }
                                    else
                                    {
                                        logger.Error("REPEAT error: " + eachExcuteObj);
                                    }
                                    break;
                                case "DELAY":
                                    int DelayTime = Convert.ToInt32(Attr);
                                    SpinWait.SpinUntil(() => false, DelayTime);
                                    result = true;
                                    break;
                                case "REPORT":

                                    result = true;
                                    Report = Attr;
                                    break;
                                case "MOVE_WIP":
                                    FromPosition = Conditions[1];
                                    FromSlot = Convert.ToInt32(Conditions[2]).ToString();
                                    ToPosition = Conditions[3];
                                    ToSlot = Convert.ToInt32(Conditions[4]).ToString();
                                    Node FNode = NodeManagement.Get(FromPosition);
                                    if (!FNode.Enable)
                                    {
                                        result = true;
                                        break;
                                    }
                                    Node TNode = NodeManagement.Get(ToPosition);
                                    if (!TNode.Enable)
                                    {
                                        result = true;
                                        break;
                                    }
                                    Job J;
                                    Job tmp;
                                    if (!FNode.JobList.TryRemove(FromSlot, out J))
                                    {
                                        J = RouteControl.CreateJob();//當沒有帳時強制建帳
                                        J.Job_Id = JobManagement.GetNewID();
                                        J.Position = FNode.Name;
                                        J.Slot = FromSlot;
                                    }
                                    if (FNode.Type.ToUpper().Equals("LOADPORT"))
                                    {
                                        //LOADPORT空的Slot要塞空資料                                       
                                        tmp = RouteControl.CreateJob();
                                        tmp.Job_Id = "No wafer";
                                        tmp.Host_Job_Id = "No wafer";
                                        tmp.Slot = FromSlot;
                                        tmp.Position = FNode.Name;
                                        FNode.JobList.TryAdd(tmp.Slot, tmp);
                                        //從LOADPORT取出，處理開始
                                        J.InProcess = true;
                                    }
                                    if (TNode.Type.ToUpper().Equals("LOADPORT"))
                                    {
                                        //放回UNLOADPORT，處理結束
                                        J.InProcess = false;
                                    }

                                    J.LastNode = J.Position;
                                    J.LastSlot = J.Slot;

                                    TNode.JobList.TryRemove(ToSlot, out tmp);
                                    if (TNode.JobList.TryAdd(ToSlot, J))
                                    {
                                        //更新WAFER位置
                                        J.Position = TNode.Name;
                                        J.Slot = ToSlot;
                                        J.PositionChangeReport();
                                    }
                                    else
                                    {
                                        logger.Error("Move wip error(Add): From=" + FromPosition + " Slot=" + FromSlot + " To=" + ToPosition + " Slot=" + ToSlot);
                                    }
                                    //}
                                    //else
                                    //{
                                    //    logger.Error("Move wip error(Remove): From=" + FromPosition + " Slot=" + FromSlot + " To=" + ToPosition + " Slot=" + ToSlot);
                                    //}
                                    result = true;
                                    break;
                                case "CHECK_DIO":

                                    string Param = Conditions[1].Split('=')[0];
                                    Value = Conditions[1].Split('=')[1];
                                    ErrorType = Conditions[2].Split('=')[0];
                                    ErrorCode = Conditions[2].Split('=')[1];
                                    tmpAry = ErrorCode.Split(',');
                                    if (tmpAry.Length >= 2)
                                    {
                                        ErrorCode = tmpAry[0];
                                        Location = tmpAry[1];
                                    }


                                    string CurrVal = RouteControl.Instance.DIO.GetIO("IN", Param);
                                    if (CurrVal.ToUpper().Equals(Value.ToUpper()))
                                    {
                                        result = true;
                                    }
                                    else
                                    {
                                        Report = ErrorType;
                                        Message = ErrorCode;
                                        result = false;
                                        break;
                                    }
                                    break;

                                case "FUNCTION":

                                    string FunctionName = Conditions[1];
                                    ErrorType = Conditions[2].Split('=')[0];
                                    ErrorCode = Conditions[2].Split('=')[1];
                                    tmpAry = ErrorCode.Split(',');
                                    if (tmpAry.Length >= 2)
                                    {
                                        ErrorCode = tmpAry[0];
                                        Location = tmpAry[1];
                                    }
                                    Node TarNode = null;
                                    int slotNo = 0;
                                    switch (FunctionName)
                                    {

                                        case "Put_Safty_Check":
                                            if (PositionName.Equals(""))
                                            {
                                                ExcutedTask.Params.TryGetValue("@ToPosition", out PositionName);
                                                ExcutedTask.Params.TryGetValue("@ToSlot", out Slot);
                                                ExcutedTask.Params.TryGetValue("@ToArm", out Arm);
                                                ExcutedTask.Params.TryGetValue("@FromPosition", out FromPosition);
                                                ExcutedTask.Params.TryGetValue("@FromSlot", out FromSlot);
                                                if (!FromPosition.Equals("") && !FromSlot.Equals(""))
                                                {
                                                    if (FromPosition.Equals(PositionName) && FromSlot.Equals(Slot))
                                                    {
                                                        //Trans 如果來源目的相同，略過放片檢查
                                                        result = true;
                                                        break;
                                                    }
                                                }
                                                TarNode = NodeManagement.Get(PositionName);
                                                if (!TarNode.Enable)
                                                {
                                                    result = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                TarNode = NodeManagement.Get(PositionName);
                                                if (!TarNode.Enable)
                                                {
                                                    result = true;
                                                    break;
                                                }
                                            }

                                            slotNo = 0;
                                            if (int.TryParse(Slot, out slotNo))
                                            {
                                                Job SlotData = null;
                                                TarNode.JobList.TryGetValue(slotNo.ToString(), out SlotData);
                                                if (!SlotData.MapFlag && !SlotData.ErrPosition)
                                                {
                                                    result = true;
                                                    if (Arm.Equals("3"))
                                                    {//雙取時要多檢查一個Slot
                                                        TarNode.JobList.TryGetValue((slotNo - 1).ToString(), out SlotData);
                                                        if (!SlotData.MapFlag && !SlotData.ErrPosition)
                                                        {
                                                            result = true;
                                                        }
                                                        else
                                                        {
                                                            Report = ErrorType;
                                                            Message = ErrorCode;
                                                            result = false;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Report = ErrorType;
                                                    Message = ErrorCode;
                                                    result = false;
                                                    break;
                                                }

                                            }
                                            else
                                            {
                                                Report = ErrorType;
                                                Message = ErrorCode;
                                                result = false;
                                                break;
                                            }
                                            break;
                                        case "Get_Safty_Check":
                                            if (PositionName.Equals(""))
                                            {
                                                ExcutedTask.Params.TryGetValue("@FromPosition", out PositionName);
                                                ExcutedTask.Params.TryGetValue("@FromSlot", out Slot);
                                                ExcutedTask.Params.TryGetValue("@FromArm", out Arm);
                                                TarNode = NodeManagement.Get(PositionName);
                                                if (!TarNode.Enable)
                                                {
                                                    result = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                TarNode = NodeManagement.Get(PositionName);
                                                if (!TarNode.Enable)
                                                {
                                                    result = true;
                                                    break;
                                                }
                                            }
                                            slotNo = 0;
                                            if (int.TryParse(Slot, out slotNo))
                                            {
                                                Job SlotData = null;
                                                TarNode.JobList.TryGetValue(slotNo.ToString(), out SlotData);
                                                if (SlotData.MapFlag && !SlotData.ErrPosition)
                                                {
                                                    result = true;
                                                    if (Arm.Equals("3"))
                                                    {//雙取時要多檢查一個Slot
                                                        TarNode.JobList.TryGetValue((slotNo - 1).ToString(), out SlotData);

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
                                        case "Get_Access_Sequentially":
                                        case "Put_Access_Sequentially":

                                            if (PositionName.Equals(""))
                                            {
                                                if (FunctionName.Equals("Get_Access_Sequentially"))
                                                {
                                                    ExcutedTask.Params.TryGetValue("@FromPosition", out PositionName);
                                                    ExcutedTask.Params.TryGetValue("@FromSlot", out Slot);
                                                }
                                                else if (FunctionName.Equals("Put_Access_Sequentially"))
                                                {
                                                    ExcutedTask.Params.TryGetValue("@ToPosition", out PositionName);
                                                    ExcutedTask.Params.TryGetValue("@ToSlot", out Slot);
                                                }
                                                TarNode = NodeManagement.Get(PositionName);
                                                if (!TarNode.Enable)
                                                {
                                                    result = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                TarNode = NodeManagement.Get(PositionName);
                                                if (!TarNode.Enable)
                                                {
                                                    result = true;
                                                    break;
                                                }
                                            }
                                            slotNo = 0;
                                            if (int.TryParse(Slot, out slotNo))
                                            {
                                                Job SlotData = null;

                                                if (TarNode.R_Flip_Degree.Equals("0"))
                                                {
                                                    //取放片Slot 1 不檢查
                                                    if (slotNo != 1)
                                                    {
                                                        TarNode.JobList.TryGetValue((slotNo - 1).ToString(), out SlotData);
                                                        ExcutedTask.Params.TryGetValue("@FromPosition", out FromPosition);
                                                        ExcutedTask.Params.TryGetValue("@FromSlot", out FromSlot);
                                                        ExcutedTask.Params.TryGetValue("@ToPosition", out ToPosition);
                                                        ExcutedTask.Params.TryGetValue("@ToSlot", out ToSlot);

                                                        //其餘Slot的前一個Slot不能有片(Slot 是反序)
                                                        if (!SlotData.MapFlag && !SlotData.ErrPosition)
                                                        {
                                                            result = true;
                                                        }
                                                        else if (ToPosition != null && ToSlot != null && FromSlot != null)
                                                        {
                                                            if (FromPosition.Equals(ToPosition) && (Convert.ToInt32(ToSlot) - 1) == Convert.ToInt32(FromSlot))
                                                            {//因放片時，會造成干涉的Slot已被取走，所以不會有問題
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
                                                    }
                                                }
                                                else if (TarNode.R_Flip_Degree.Equals("180"))
                                                {
                                                    //取放片Slot 1 不檢查
                                                    if (slotNo != 25)
                                                    {
                                                        TarNode.JobList.TryGetValue((slotNo + 1).ToString(), out SlotData);
                                                        ExcutedTask.Params.TryGetValue("@FromPosition", out FromPosition);
                                                        ExcutedTask.Params.TryGetValue("@FromSlot", out FromSlot);
                                                        ExcutedTask.Params.TryGetValue("@ToPosition", out ToPosition);
                                                        ExcutedTask.Params.TryGetValue("@ToSlot", out ToSlot);

                                                        //其餘Slot的後一個Slot不能有片(Slot 是反序)
                                                        if (!SlotData.MapFlag && !SlotData.ErrPosition)
                                                        {
                                                            result = true;
                                                        }
                                                        else if (ToPosition != null && ToSlot != null && FromSlot != null)
                                                        {
                                                            if (FromPosition.Equals(ToPosition) && (Convert.ToInt32(ToSlot) + 1) == Convert.ToInt32(FromSlot))
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
                                                    }
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
                                        case "Check_LP_Safty":

                                            foreach (Node robot in NodeManagement.GetEnableRobotList())
                                            {
                                                TarNode = NodeManagement.Get(TargetName);//取得Port物件
                                                if (!TarNode.Enable)
                                                {
                                                    continue;
                                                }
                                                if (TarNode.Associated_Node.ToUpper().Equals(robot.Name.ToUpper()))//找到可以存取該Port的Robot
                                                {
                                                    Val = 0;
                                                    if (TarNode.CurrentPosition.ToUpper().Equals(TargetName))//Robot如果在這個Port前
                                                    {
                                                        if (!TarNode.R_Position.Equals(""))
                                                        {
                                                            Val = Convert.ToInt32(TarNode.R_Position);
                                                            diff = Val;

                                                            logger.Debug("Diff:" + diff.ToString());
                                                            if (diff > 500)//手臂伸出超過500就發警報
                                                            {
                                                                Report = ErrorType;
                                                                Message = ErrorCode;
                                                                result = false;
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                result = true;
                                                            }
                                                        }
                                                        if (!TarNode.L_Position.Equals(""))
                                                        {
                                                            Val = Convert.ToInt32(TarNode.L_Position);
                                                            diff = Val;

                                                            logger.Debug("Diff:" + diff.ToString());
                                                            if (diff > 500)//手臂伸出超過500就發警報
                                                            {
                                                                Report = ErrorType;
                                                                Message = ErrorCode;
                                                                result = false;
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                result = true;
                                                            }
                                                        }
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
                                            }

                                            break;
                                        case "Get_Check_X_AXIS_Position":
                                        case "Put_Check_X_AXIS_Position":

                                            if (PositionName.Equals(""))
                                            {
                                                if (FunctionName.Equals("Get_Check_X_AXIS_Position"))
                                                {
                                                    ExcutedTask.Params.TryGetValue("@FromPosition", out PositionName);
                                                    //ExcutedTask.Params.TryGetValue("@FromSlot", out Slot);
                                                }
                                                else if (FunctionName.Equals("Put_Check_X_AXIS_Position"))
                                                {
                                                    ExcutedTask.Params.TryGetValue("@ToPosition", out PositionName);
                                                    //ExcutedTask.Params.TryGetValue("@ToSlot", out Slot);
                                                }
                                                TarNode = NodeManagement.Get(PositionName);
                                                if (!TarNode.Enable)
                                                {
                                                    result = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                TarNode = NodeManagement.Get(PositionName);
                                                if (!TarNode.Enable)
                                                {
                                                    result = true;
                                                    break;
                                                }
                                            }
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
                                case "CHECK_NODE":

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
                                        if (!Node.Enable)
                                        {
                                            result = true;
                                            break;
                                        }
                                        if (Node != null)
                                        {
                                            string AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();
                                            if (AttrVal.Equals(Value.ToUpper()))
                                            {
                                                RouteControl.Instance.DIO.SetIO(ParamName, Set);

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
                                        tmpAry = ErrorCode.Split(',');
                                        if (tmpAry.Length >= 2)
                                        {
                                            ErrorCode = tmpAry[0];
                                            Location = tmpAry[1];
                                        }
                                        Node = NodeManagement.Get(NodeName);
                                        if (!Node.Enable)
                                        {
                                            result = true;
                                            break;
                                        }
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
                                case "SET_NODE":

                                    NodeName = Conditions[1];
                                    Attr = Conditions[2].Split('=')[0];
                                    Value = Conditions[2].Split('=')[1];
                                    string SetAttr = Conditions[3].Split('=')[0];
                                    string SetVal = Conditions[3].Split('=')[1];
                                    Node = NodeManagement.Get(NodeName);
                                    if (!Node.Enable)
                                    {
                                        result = true;
                                        break;
                                    }
                                    if (Node != null)
                                    {


                                        string AttrVal = "";
                                        if (Attr.Equals("WipCount"))
                                        {
                                            AttrVal = Node.JobList.Count.ToString();
                                        }
                                        else
                                        {
                                            AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();

                                            if (Attr.ToUpper().Equals("SERVO") && AttrVal.Equals("0"))
                                            {//當偵測到ROBOT被切過手動，把所有PORT改為未Mapping狀態   聲鑫要求
                                                foreach (Node p in NodeManagement.GetLoadPortList())
                                                {
                                                    p.IsMapping = false;
                                                }

                                            }
                                        }

                                        if (AttrVal.Equals(Value.ToUpper()))
                                        {
                                            //string AttrVal = Node.GetType().GetProperty(Attr).GetValue(Node, null).ToString().ToUpper();
                                            if (SetAttr.Equals("RequestQueue"))
                                            {
                                                try
                                                {
                                                    Node.ActionRequest req = new Node.ActionRequest();
                                                    if (ConditionsStr.IndexOf("TRANSFER_GETW_") != -1)
                                                    {

                                                    }
                                                    if (Conditions.Length >= 5)
                                                    {
                                                        string[] Attrs = Conditions[4].Split(',');
                                                        foreach (string atr in Attrs)
                                                        {
                                                            if (atr.Trim().Equals(""))
                                                            {
                                                                continue;
                                                            }
                                                            string k = atr.Split('=')[0];
                                                            string v = atr.Split('=')[1];
                                                            switch (k)
                                                            {
                                                                case "Position":
                                                                    req.Position = v;
                                                                    break;
                                                                case "Slot":
                                                                    req.Slot = v;
                                                                    break;
                                                                case "Arm":
                                                                    req.Arm = v;
                                                                    break;
                                                                case "Value":
                                                                    req.Value = v;
                                                                    break;
                                                            }
                                                        }
                                                    }

                                                    req.TaskName = SetVal;
                                                    lock (Node.RequestQueue)
                                                    {
                                                        if (!Node.RequestQueue.ContainsKey(SetVal))
                                                        {
                                                            Node.RequestQueue.Add(SetVal, req);
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    logger.Error("加入RequestQueue失敗，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex + " " + e.StackTrace);
                                                    throw new Exception("加入RequestQueue失敗，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex + " " + e.StackTrace);
                                                }
                                            }
                                            else
                                            {
                                                switch (Node.GetType().GetProperty(SetAttr).PropertyType.Name)
                                                {
                                                    case "String":
                                                        try
                                                        {
                                                            Node.GetType().GetProperty(SetAttr).SetValue(Node, SetVal);
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
                                                            Node.GetType().GetProperty(SetAttr).SetValue(Node, Convert.ToInt32(SetVal));
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
                                                            Node.GetType().GetProperty(SetAttr).SetValue(Node, bool.Parse(SetVal));
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
                                        }
                                    }
                                    else
                                    {
                                        logger.Error("CheckCondition失敗，找不到Node:" + NodeName + "，Task :" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                        throw new Exception("CheckCondition失敗，找不到Node:" + NodeName + "，Task Name:" + ExcutedTask.ProceedTask.TaskName + " TaskIndex:" + ExcutedTask.ProceedTask.TaskIndex);
                                    }
                                    result = true;
                                    break;
                            }
                        }
                        else
                        {
                            logger.Error("Task CheckCondition 解析失敗，Task Name:" + taskName);
                            throw new Exception("Task CheckCondition 解析失敗，Task Name:" + taskName);
                        }
                        if (result == false)
                        {
                            break;
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
            logger.Debug("result:" + result + " Report:" + Report + " Message:" + Message);
            return result;
        }

        public CurrentProceedTask Remove(string Id)
        {
            logger.Debug("Delete Task ID:" + Id);
            CurrentProceedTask tmp;
            CurrentProceedTasks.TryRemove(Id, out tmp);
           
            return tmp;
        }

        public void Clear()
        {
            foreach (CurrentProceedTask tk in CurrentProceedTasks.Values)
            {
                _TaskReport.On_Task_Abort(tk);
            }
            CurrentProceedTasks.Clear();
        }

        public bool Excute(string Id, out string ErrorMessage, out CurrentProceedTask Task, string taskName = "", Dictionary<string, string> param = null,string MainTaskId = "")
        {
            bool result = false;
            Task = null;
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
                        Task = ExcutedTask;
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
                            ExcutedTask.ExcutedCount = 0;
                        }
                        else
                        {//如果有GOTO的Index，優先執行
                            var findTask = from each in tk
                                           where each.TaskIndex >= Convert.ToInt32(ExcutedTask.GotoIndex)
                                           select each;
                            tk = findTask.ToList();
                            ExcutedTask.GotoIndex = "";
                            ExcutedTask.ExcutedCount++;
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
                            {//過濾Task
                                if (eachSkip.Trim().Equals(""))
                                {
                                    continue;
                                }
                                string[] EachSkipConditionAry = eachSkip.Split(':');
                                string NodeName = EachSkipConditionAry[0];
                                if (NodeName.Equals("VAR"))
                                {
                                    if (EachSkipConditionAry[1].IndexOf("=") != -1)
                                    {
                                        string[] ConditionAry = EachSkipConditionAry[1].Split('=');
                                        string Value = ConditionAry[1];
                                        string Attr = ConditionAry[0];
                                        string AttrVal = CurrParam["@" + Attr].ToString().ToUpper();
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
                                        string AttrVal = CurrParam["@" + Attr].ToString().ToUpper();
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
                                                tmp1.Remove(eachTask);
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
                                                tmp1.Remove(eachTask);
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
                                }
                                tmp1.Add(eachTask);
                            }

                        }


                    }
                    tk = tmp1;

                    tk.Sort((x, y) => { return x.TaskIndex.CompareTo(y.TaskIndex); });

                    if (tk.Count != 0)
                    {
                        CurrentProceedTask CurrTask;
                        if (ExcutedTask == null)
                        {
                             CurrTask = new CurrentProceedTask();
                        }
                        else
                        {
                            CurrTask = ExcutedTask;
                        }
                        if (!MainTaskId.Equals(""))
                        {//記下mainTask ID
                            CurrTask.MainTaskId = MainTaskId;
                        }
                        else
                        {
                            if (Id.Equals(CurrTask.MainTaskId))
                            {
                                CurrTask.MainTaskId = "";//sub task已做完，清除main task id
                            }
                        }
                        
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
                        CurrTask.Id = Id;
                        if (CurrentProceedTasks.TryAdd(Id, CurrTask))
                        {
                            Task = CurrTask;
                            string ExcuteObjStr = CurrTask.ProceedTask.ExcuteObj;

                            if (ExcuteObjStr.Trim().Equals(""))
                            {
                                _TaskReport.On_Task_NoExcuted(CurrTask);
                                return true;
                            }

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
                            bool NodeDisabled = false;
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
                                    if (NodeName.ToUpper().Equals("TASK"))
                                    {//do sub task
                                        string TaskName = ExcuteObj[1];
                                        string errStr = "";
                                        CurrentProceedTask tmpTask;
                                        Excute(Guid.NewGuid().ToString(), out errStr, out tmpTask, TaskName);
                                        return true;
                                    }


                                    Node target = NodeManagement.Get(NodeName);
                                    if (target != null)
                                    {
                                        if (!target.Enable)
                                        {
                                            logger.Debug("Node disabled:" + eachExcuteObj);
                                            NodeDisabled = true;
                                            continue;
                                        }
                                    }
                                    string Type = ExcuteObj[1];

                                    if (Type.ToUpper().Equals("CMD"))
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
                            if (CurrTask.CheckList.Count == 0)
                            {
                                if (NodeDisabled)
                                {
                                    _TaskReport.On_Task_NoExcuted(CurrTask);
                                    return true;
                                }
                                else
                                {
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

                                        result = Node.SendCommand(ex.Txn, out ErrorMessage);
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
                        logger.Info("已找不到Task，完成工作，TaskId:" + Id);
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
