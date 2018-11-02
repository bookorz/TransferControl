using SANWA.Utility;
using TransferControl.Management;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Engine
{
    public interface IUserInterfaceReport
    {
        void On_Command_Excuted(Node Node, Transaction Txn, ReturnMessage Msg);
        void On_Command_Error(Node Node, Transaction Txn, ReturnMessage Msg);
        void On_Command_Finished(Node Node, Transaction Txn, ReturnMessage Msg);
        void On_Command_TimeOut(Node Node, Transaction Txn);
        void On_Event_Trigger(Node Node, ReturnMessage Msg);
        void On_Node_State_Changed(Node Node, string Status);
        void On_Eqp_State_Changed(string OldStatus,string NewStatus);
        void On_Controller_State_Changed(string Device_ID, string Status);
        
        void On_Job_Location_Changed(Job Job);
     
        void On_Mode_Changed(string Mode);

        void On_Data_Chnaged(string Parameter, string Value);
        void On_Connection_Error(string DIOName, string ErrorMsg);
        void On_Connection_Status_Report(string DIOName, string Status);
        void On_Alarm_Happen(string DIOName, string ErrorCode);

        void On_TaskJob_Aborted(string TaskID, string NodeName, string ReportType, string Message);
        void On_TaskJob_Finished(string TaskID);
    }
}
