using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.CommandConvert
{
    class Encoder_WHR
    {
        private string Supplier;
        public Encoder_WHR(string supplier)
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
        public string PreparePick(string Address, string StationNo,string Mode)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:GETW_:{0},{1},{2}";
                    commandStr = string.Format(commandStr, Address, StationNo, Mode);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Pick(string Address, string StationNo, string Mode)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:GET__:{0},{1},{2}";
                    commandStr = string.Format(commandStr, Address, StationNo, Mode);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string PreparePlace(string Address, string StationNo, string Mode)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:GET__:{0},{1},{2}";
                    commandStr = string.Format(commandStr, Address, StationNo, Mode);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
    }
}
