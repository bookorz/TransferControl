using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.CommandConvert
{
    class Encoder_FoupRobot
    {
        private string Supplier;
        public Encoder_FoupRobot(string supplier)
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
        public string PreparePick(string Address,string StationNo)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:GETW_:{0},{1}";
                    commandStr = string.Format(commandStr, Address, StationNo);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Pick(string Address, string StationNo)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:GET__:{0},{1}";
                    commandStr = string.Format(commandStr, Address, StationNo);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string PreparePlace(string Address, string StationNo)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:PUTW_:{0},{1}";
                    commandStr = string.Format(commandStr, Address, StationNo);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Place(string Address, string StationNo)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:PUT__:{0},{1}";
                    commandStr = string.Format(commandStr, Address, StationNo);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Extend(string Address, string StationNo,string Zpos)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:RBETD:{0},{1},{2}";
                    commandStr = string.Format(commandStr, Address, StationNo, Zpos);
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
                case "SANWA":

                    commandStr = "$1MCR:RET__:{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Grab(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:RBHLD:{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Release(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:RBRLS:{0}";
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
                case "SANWA":

                    commandStr = "$1MCR:RBUP_:{0}";
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
                case "SANWA":

                    commandStr = "$1MCR:RBDWN:{0}";
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
                case "SANWA":

                    commandStr = "$1MCR:HOME_:{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Transfer(string Address,string StartStationNo,string DestinationStationNo)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:CARRY:{0},{1},{2}";
                    commandStr = string.Format(commandStr, Address, StartStationNo, DestinationStationNo);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
    }
}
