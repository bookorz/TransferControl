﻿using TransferControl.Management;
using TransferControl.CommandConvert;
using System.Collections.Generic;

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
        //void On_Controller_State_Changed(string Device_ID, string Status);
        void On_Node_Connection_Changed(string NodeName,string Status);
        void On_Job_Location_Changed(Job Job);
        void On_CST_Mode_Changed(Node node);
        void On_DIO_Data_Chnaged(string Parameter, string Value , string Type);
        void On_Connection_Error(string DIOName, string ErrorMsg);
        void On_Connection_Status_Report(string DIOName, string Status);
        void On_Alarm_Happen(AlarmManagement.Alarm Alarm);
        void On_TaskJob_Ack(TaskFlowManagement.CurrentProcessTask Task);
        void On_TaskJob_Aborted(TaskFlowManagement.CurrentProcessTask Task);
        void On_TaskJob_Finished(TaskFlowManagement.CurrentProcessTask Task);
         void On_Message_Log(string Type,string Message);
        void On_Status_Changed(string Type, string Message);


    }
}
