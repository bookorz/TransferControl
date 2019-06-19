using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Management
{
    public class AlarmMessage
    {
        /// <summary>
        /// Alarm 系統原因代碼
        /// </summary>
        public string CodeID = string.Empty;

        /// <summary>
        /// 設備錯誤代碼
        /// </summary>
        public string Return_Code_ID = string.Empty;

        /// <summary>
        /// Alarm 代碼類別
        /// </summary>
        public string Code_Type = string.Empty;

        /// <summary>
        /// Alarm 原因代碼名稱
        /// </summary>
        public string Code_Name = string.Empty;

        /// <summary>
        /// Alarm 原因錯誤原因-中
        /// </summary>
        public string Code_Cause = string.Empty;

        /// <summary>
        /// /// Alarm 原因錯誤原因-英
        /// </summary>
        public string Code_Cause_English = string.Empty;
        //public string LED_Red = string.Empty;
        //public string LED_Yellow = string.Empty;
        //public string LED_Green = string.Empty;
        //public string LED_Bule = string.Empty;
        //public string Buzzer01 = string.Empty;
        //public string Buzzer02 = string.Empty;

        /// <summary>
        /// Alarm 發生是否立即停止
        /// </summary>
        public bool IsStop = false;

        /// <summary>
        /// Alarm Device ID
        /// </summary>
        public string Position = string.Empty;

        /// <summary>
        /// Alarm 錯誤所屬群組
        /// </summary>
        public string Code_Group = string.Empty;
    }
}
