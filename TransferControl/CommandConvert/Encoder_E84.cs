using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.CommandConvert
{
    public class Encoder_E84
    {
        private string Supplier;
        public Encoder_E84(string supplier)
        {
            try
            {
                Supplier = supplier;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
        private string EndCode()
        {
            string result = "";
            switch (Supplier)
            {
                case "SANWA_MC":
                case "SANWA":
                case "ATEL":
                case "ATEL_NEW":
                    result = "\r";
                    break;

                case "KAWASAKI":
                    result = "\r\n";
                    break;
            }
            return result;
        }
        public string Reset()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,550400bb";
                    commandStr = string.Format(commandStr);
                    break;

                case "FRANCES":
                    commandStr = "55 04 00 bb";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string AutoMode()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,550100bb";
                    commandStr = string.Format(commandStr);
                    break;

                case "FRANCES":
                    commandStr = "55 01 00 bb";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string ManualMode()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,550200bb";
                    commandStr = string.Format(commandStr);
                    break;

                case "FRANCES":
                    commandStr = "55 02 00 bb";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP1(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5570{0}bb";
                    commandStr = string.Format(commandStr,Convert.ToInt16(value).ToString("X2"));
                    break;

                case "FRANCES":
                    commandStr = string.Format("55 70 {0} bb", Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP2(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5571{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;

                case "FRANCES":
                    commandStr = string.Format("55 71 {0} bb", Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP3(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5572{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;

                case "FRANCES":
                    commandStr = string.Format("55 72 {0} bb", Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP4(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5573{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;

                case "FRANCES":
                    commandStr = string.Format("55 73 {0} bb", Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP5(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5574{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;

                case "FRANCES":
                    commandStr = string.Format("55 74 {0} bb", Convert.ToInt16(value).ToString("X2"));
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string SetTP6(string value)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$1GET:E84__:6,5575{0}bb";
                    commandStr = string.Format(commandStr, Convert.ToInt16(value).ToString("X2"));
                    break;

                case "FRANCES":
                    commandStr = string.Format("55 75 {0} bb", Convert.ToInt16(value).ToString("X2"));
                    break;

                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }

        public string GetDIOStatus()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "FRANCES":
                    commandStr = "55 90 00 bb";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }

        public string GetDOStatus()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "FRANCES":
                    commandStr = "55 81 02 bb";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string GetDIStatus()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "FRANCES":
                    commandStr = "55 81 01 bb";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }

        public string GetOperateStatus()
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "FRANCES":
                    commandStr = "55 81 05 bb";
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
    }
}
