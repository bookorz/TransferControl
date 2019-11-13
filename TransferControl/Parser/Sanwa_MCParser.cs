using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Parser
{
    class Sanwa_MCParser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.ILPT.Load:
                case Transaction.Command.PTZ.Transfer:
                case Transaction.Command.PTZ.Home:
                    return ParseMap(Message);
                default:
                    throw new Exception(Command + " Not support");
            }

        }
        private Dictionary<string, string> ParseMap(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(); 
            result.Add("Mapping", Message.Replace(",",""));
            return result;
        }
    }
}
