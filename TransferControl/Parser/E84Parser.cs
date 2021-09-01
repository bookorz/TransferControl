using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Parser
{
    class E84Parser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.E84.GetE84IOStatus:
                    return GetParseE84IOStatus(Message);
                case Transaction.Command.E84.GetDIOStatus:
                    return GetParseDIOStatus(Message);

                default:
                    throw new Exception(Command + " Not support");
            }

        }
        private Dictionary<string, string> GetParseE84IOStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            byte E84IN = Convert.ToByte(Message.Substring(0, 2), 16);
            byte E84OUT = Convert.ToByte(Message.Substring(2, 2), 16);
            byte MISC = Convert.ToByte(Message.Substring(4, 2), 16);

            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToString(E84IN, 2).PadLeft(8, '0'));
            sb.Append(Convert.ToString(E84OUT, 2).PadLeft(8, '0'));
            sb.Append(Convert.ToString(MISC, 2).PadLeft(8, '0'));

            for (int i = 0; i < sb.ToString().Length; i++)
            {
                string BitName = "";
                switch (i)
                {
                    case 0:
                        BitName = "CONT";
                        break;
                    case 1:
                        BitName = "COMPT";
                        break;
                    case 2:
                        BitName = "BUSY";
                        break;
                    case 3:
                        BitName = "TR_REQ";
                        break;
                    case 4:
                        BitName = "AM_AVBL";
                        break;
                    case 5:
                        BitName = "CS_1";
                        break;
                    case 6:
                        BitName = "CS_0";
                        break;
                    case 7:
                        BitName = "VALID";
                        break;
                    case 8:
                        BitName = "ES";
                        break;
                    case 9:
                        BitName = "HO_AVBL";
                        break;
                    case 10:
                        BitName = "VS_1";
                        break;
                    case 11:
                        BitName = "VS_0";
                        break;
                    case 12:
                        BitName = "READY";
                        break;
                    case 13:
                        BitName = "VA";
                        break;
                    case 14:
                        BitName = "U_REQ";
                        break;
                    case 15:
                        BitName = "L_REQ";
                        break;
                    //case 16:
                    //    BitName = "N/A";
                    //    break;
                    case 17:
                        BitName = "ERROR";
                        break;
                    case 18:
                        BitName = "MANUAL";
                        break;
                    case 19:
                        BitName = "AUTO";
                        break;
                    //case 21:
                    //    BitName = "N/A";
                    //    break;
                    case 21:
                        BitName = "MODE";
                        break;
                    case 22:
                        BitName = "SELECT";
                        break;
                    case 23:
                        BitName = "GO";
                        break;
                }
                if (!BitName.Equals(""))
                    result.Add(BitName, sb.ToString()[i].ToString());
            }

            return result;
        }

        private Dictionary<string, string> GetParseDIOStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            byte DIN = Convert.ToByte(Message.Substring(0, 2), 16);


            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToString(DIN, 2).PadLeft(8, '0'));

            for (int i = 0; i < sb.ToString().Length; i++)
            {
                string BitName = "";
                switch (i)
                {
                    //case 0:
                    //    BitName = "RESERVED";
                    //    break;
                    case 1:
                        BitName = "CLAMP";
                        break;
                    case 2:
                        BitName = "EMO";
                        break;
                    case 3:
                        BitName = "ALARM";
                        break;
                    case 4:
                        BitName = "LC";
                        break;
                    //case 5:
                    //    BitName = "AUTOMODE";
                    //    break;
                    //case 6:
                    //    BitName = "MANUALMODE";
                    //    break;
                    //case 7:
                    //    BitName = "RESET";
                    //    break;

                }
                if (!BitName.Equals(""))
                    result.Add(BitName, sb.ToString()[i].ToString());
            }

            return result;
        }

    }
}
