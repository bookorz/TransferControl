using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Parser
{
    class Atel_newParser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.RobotType.GetRIO:
                    return ParseRIO(Message);
                default:
                    throw new Exception(Command + " Not support");
            }

        }

        private Dictionary<string, string> ParseRIO(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] MsgAry = Message.Split(',');
            switch (MsgAry[0])
            {
                case "31":
                    result.Add("R-Clamp Sensor", MsgAry[1]);                   
                    break;
                case "32":
                    result.Add("R-UnClamp Sensor", MsgAry[1]);
                    break;              
            }
            return result;
        }
    }
}
