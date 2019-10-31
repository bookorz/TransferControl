using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Parser
{
    class Sanwa_MCParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.ILPT.Load:
                    return ParseMap(Message);
                default:
                    throw new Exception(Command + " Not support");
            }

        }
        private Dictionary<string, string> ParseMap(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(); 
            result.Add("Mapping", Message.Substring(Message.IndexOf(",")+1));
            return result;
        }
    }
}
