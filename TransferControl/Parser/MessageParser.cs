using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Parser
{
    public class MessageParser
    {
        IParser P;
        public MessageParser(string Supplier)
        {
            switch (Supplier.ToUpper())
            {
                case "TDK":
                    P = new TDKParser();
                    break;
                case "SANWA":
                    P = new SanwaParser();
                    break;
                case "SANWA_MC":
                    P = new Sanwa_MCParser();
                    break;
                case "ATEL_NEW":
                    P = new Atel_newParser();
                    break;
                case "ASYST":
                    P = new ASYSTParser();
                    break;
                case "HST":
                    P = new HSTParser();
                    break;
                case "COGNEX":
                    P = new COGNEXParser();
                    break;
                case "KAWASAKI":
                    P = new KawasakiParser();
                    break;
                case "AIRTECH":
                    P = new AIRTECHParser();
                    break;
                case "FRANCES":
                    P = new E84Parser();
                    break;
                    
                case "SMARTTAG8200":
                case "SMARTTAG8400":
                case "OMRON_V640":
                    break;
                default:
                    throw new Exception(Supplier + " 不存在");
                    
            }
        }

        public Dictionary<string,string> ParseMessage(string Command, string Message)
        {
            return P.Parse(Command, Message);
        }
    }
}
