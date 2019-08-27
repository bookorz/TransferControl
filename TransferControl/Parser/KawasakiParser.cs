using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Parser
{
    class KawasakiParser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.RobotType.GetStatus:
                    return ParseStatus(Message);
                
                default:
                    throw new Exception(Command + " Not support");
            }
        }
        private Dictionary<string, string> ParseStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            for (int i = 0; i < Message.Length; i++)
            {

                switch (i + 1)
                {
                    case 10:
                        result.Add("Servo", Message[i].ToString());
                        break;
                }
            }
            return result;
        }
    }
}
