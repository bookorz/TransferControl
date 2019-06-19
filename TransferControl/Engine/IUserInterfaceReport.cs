﻿using TransferControl.Management;
using TransferControl.CommandConvert;

namespace TransferControl.Engine
{
    public interface IUserInterfaceReport
    {
        void On_Command_Excuted(Node Node, Transaction Txn, CommandReturnMessage Msg);
        void On_Command_Error(Node Node, Transaction Txn, CommandReturnMessage Msg);
        void On_Command_Finished(Node Node, Transaction Txn, CommandReturnMessage Msg);
        void On_Command_TimeOut(Node Node, Transaction Txn);
        void On_Event_Trigger(Node Node, CommandReturnMessage Msg);
        void On_Node_State_Changed(Node Node, string Status);
        void On_Eqp_State_Changed(string OldStatus,string NewStatus);
        //void On_Controller_State_Changed(string Device_ID, string Status);
        void On_Node_Connection_Changed(string NodeName,string Status);
        void On_Job_Location_Changed(Job Job);
     
        void On_Mode_Changed(string Mode);

        void On_Data_Chnaged(string Parameter, string Value , string Type);
        void On_Connection_Error(string DIOName, string ErrorMsg);
        void On_Connection_Status_Report(string DIOName, string Status);
        void On_Alarm_Happen(string DIOName, string ErrorCode);

        void On_TaskJob_Aborted(TaskJobManagment.CurrentProceedTask Task, string NodeName, string ReportType, string Message);
        void On_TaskJob_Finished(TaskJobManagment.CurrentProceedTask Task);
    }
}
