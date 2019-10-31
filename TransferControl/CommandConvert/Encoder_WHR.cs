﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.CommandConvert
{
    public class Encoder_WHR
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
                case "SANWA_MC":
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
                case "SANWA_MC":

                    commandStr = "$2MCR:GETW_:{0},{1},{2}";
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
                case "SANWA_MC":

                    commandStr = "$2MCR:GET__:{0},{1},{2}";
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
                case "SANWA_MC":

                    commandStr = "$2MCR:PUTW_:{0},{1},{2}";
                    commandStr = string.Format(commandStr, Address, StationNo, Mode);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Place(string Address, string StationNo, string Mode)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$2MCR:PUT__:{0},{1},{2}";
                    commandStr = string.Format(commandStr, Address, StationNo, Mode);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Extend(string Address, string StationNo, string Mode,string Zpos)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$2MCR:EXTND:{0},{1},{2},{3}";
                    commandStr = string.Format(commandStr, Address, StationNo, Mode, Zpos);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Retract(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$2MCR:RET__:{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Up(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$2MCR:UP___:{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Down(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$2MCR:DOWN_:{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Home(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA_MC":

                    commandStr = "$2MCR:HOME_:{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
    }
}
