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
                case Transaction.Command.E84.GetDIStatus:
                    return ParseDIStatus(Message);
                case Transaction.Command.E84.GetDOStatus:
                    return GetDOStatus(Message);
                case Transaction.Command.E84.GetOperateStatus:
                    return GetOperateStatus(Message);
                case Transaction.Command.E84.GetDIOStatus:
                    return GetParseDIOStatus(Message);
                default:
                    throw new Exception(Command + " Not support");
            }

        }
        private Dictionary<string, string> GetParseDIOStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            byte E84IN = Convert.ToByte(Message.Substring(0, 2), 16);
            byte E84OUT = Convert.ToByte(Message.Substring(2, 2), 16);
            byte MISC = Convert.ToByte(Message.Substring(4, 2), 16);

            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToString(E84IN, 2).PadLeft(8, '0'));
            sb.Append(Convert.ToString(E84OUT, 2).PadLeft(8, '0'));
            sb.Append(Convert.ToString(MISC, 2).PadLeft(8, '0'));

            for (int i = sb.ToString().Length - 1; i >= 0; i--)
            {
                string BitName = "";
                switch (i)
                {
                    case 0:
                        BitName = "VALID";
                        break;
                    case 1:
                        BitName = "CS_0";
                        break;
                    case 2:
                        BitName = "CS_1";
                        break;
                    case 3:
                        BitName = "AM_AVBL";
                        break;
                    case 4:
                        BitName = "TR_REQ";
                        break;
                    case 5:
                        BitName = "BUSY";
                        break;
                    case 6:
                        BitName = "COMPT";
                        break;
                    case 7:
                        BitName = "CONT";
                        break;
                    case 8:
                        BitName = "L_REQ";
                        break;
                    case 9:
                        BitName = "U_REQ";
                        break;
                    case 10:
                        BitName = "VA";
                        break;
                    case 11:
                        BitName = "READY";
                        break;
                    case 12:
                        BitName = "VS_0";
                        break;
                    case 13:
                        BitName = "VS_1";
                        break;
                    case 14:
                        BitName = "HO_AVBL";
                        break;
                    case 15:
                        BitName = "ES";
                        break;
                    case 16:
                        BitName = "GO";
                        break;
                    case 17:
                        BitName = "SELECT";
                        break;
                    case 18:
                        BitName = "MODE";
                        break;
                    case 20:
                        BitName = "AUTO";
                        break;
                    case 21:
                        BitName = "MANUAL";
                        break;
                    case 22:
                        BitName = "ERROR";
                        break;
                }
                if (!BitName.Equals(""))
                    result.Add(BitName, sb.ToString()[sb.ToString().Length - 1 - i].ToString());
            }

            return result;
        }
        private Dictionary<string, string> ParseDIStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            byte NN = Convert.ToByte(Message.Substring(0, 2), 16);
            byte MM = Convert.ToByte(Message.Substring(2, 2), 16);

            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToString(MM, 2).PadLeft(8, '0'));
            sb.Append(Convert.ToString(NN, 2).PadLeft(8, '0'));

            for (int i = sb.ToString().Length - 1; i >= 0; i--)
            {
                string BitName = "";
                switch (i)
                {
                    case 0:
                        BitName = "VALID";
                        break;
                    case 1:
                        BitName = "CS_0";
                        break;
                    case 2:
                        BitName = "CS_1";
                        break;
                    case 3:
                        BitName = "AM_AVBL";
                        break;
                    case 4:
                        BitName = "TR_REQ";
                        break;
                    case 5:
                        BitName = "BUSY";
                        break;
                    case 6:
                        BitName = "COMPT";
                        break;
                    case 7:
                        BitName = "CONT";
                        break;
                    case 8:
                        BitName = "GO";
                        break;
                }
                if (!BitName.Equals(""))
                    result.Add(BitName, sb.ToString()[sb.ToString().Length - 1 - i].ToString());
            }

            return result;
        }
        private Dictionary<string, string> GetDOStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            byte NN = Convert.ToByte(Message.Substring(2, 2), 16);

            StringBuilder sb = new StringBuilder();
            //將每個Byte內容轉換為2進位字串後前補0
            sb.Append(Convert.ToString(NN, 2).PadLeft(8, '0'));

            for (int i = sb.ToString().Length - 1; i >= 0; i--)
            {
                string BitName = "";
                switch (i)
                {
                    case 0:
                        BitName = "L_REQ";
                        break;
                    case 1:
                        BitName = "U_REQ";
                        break;
                    case 2:
                        BitName = "VA";
                        break;
                    case 3:
                        BitName = "READY";
                        break;
                    case 4:
                        BitName = "VS_0";
                        break;
                    case 5:
                        BitName = "VS_1";
                        break;
                    case 6:
                        BitName = "HO_AVBL";
                        break;
                    case 7:
                        BitName = "ES";
                        break;
                }
                if (!BitName.Equals(""))
                    result.Add(BitName, sb.ToString()[sb.ToString().Length - 1 - i].ToString());
            }

            return result;
        }
        private Dictionary<string, string> GetOperateStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            byte MM = Convert.ToByte(Message.Substring(0, 2), 16);

            StringBuilder sb = new StringBuilder();
            sb.Append(Convert.ToString(MM, 2).PadLeft(8, '0'));

            for (int i = sb.ToString().Length - 1; i >= 0; i--)
            {
                string BitName = "";
                switch (i)
                {
                    case 1:
                        BitName = "Manual";
                        break;
                    case 2:
                        BitName = "Auto";
                        break;
                }
                if (!BitName.Equals(""))
                    result.Add(BitName, sb.ToString()[sb.ToString().Length - 1 - i].ToString());
            }

            return result;
        }
    }
}
