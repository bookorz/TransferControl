using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.CommandConvert
{
    public class CommandReturnMessage
    {
        public enum ReturnType
        {
            Excuted,
            Finished,
            Error,
            Event,
            Information,
            ReInformation,
            Abnormal,
            UserName,
            Password,
            Sending
        }
        public string OrgMsg = "";
        public ReturnType Type;
        public string Command;
        public bool IsInterrupt = false;
        public string NodeAdr = "";
        public string Seq = "";
        public string Value = "";
        public string FinCommand = "";
        public string CommandType = "";
    }
}
