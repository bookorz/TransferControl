using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Parser
{
    class ASYSTParser : IParser
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
                case "004":
                    result.Add("R-Hold Status", MsgAry[1]);
                    break;
                case "005":
                    result.Add("L-Hold Status", MsgAry[1]);
                    break;
                case "008":
                    result.Add("R-Present", MsgAry[1]);
                    break;
                case "009":
                    result.Add("L-Present", MsgAry[1]);
                    break;
            }
            return result;
        }
    }
}
