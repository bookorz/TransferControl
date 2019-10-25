using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.CommandConvert
{
    class Encoder_ILPT
    {
        private string Supplier;
        public Encoder_ILPT(string supplier)
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
        public string Clamp(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILCLP:{0},{0},1";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Unclamp(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILCLP:{0},{0},0";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Dock(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILDCK:{0},{0},1";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Undock(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILDCK:{0},{0},0";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string OpenLatch(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILLTH:{0},{0},1";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string CloseLatch(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILLTH:{0},{0},0";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string VacuumOn(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILVCM:{0},{0},1";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string VacuumOff(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILVCM:{0},{0},0";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string OpenDoor(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILDOR:{0},{0},1";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string CloseDoor(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILDOR:{0},{0},0";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string UpDoor(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILUP_:{0},{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string DownDoor(string Address,string MappingEnable)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILDWN:{0},{0},{1}";
                    commandStr = string.Format(commandStr, Address, MappingEnable);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string OpenLower(string Address, string MappingEnable)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILOPN:{0},{0},{1}";
                    commandStr = string.Format(commandStr, Address, MappingEnable);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string RaiseClose(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILCLS:{0},{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Map(string Address,string Direction)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILMAP:{0},{0},{1}";
                    commandStr = string.Format(commandStr, Address, Direction);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Load(string Address, string MappingEnable)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILLOD:{0},{0},{1}";
                    commandStr = string.Format(commandStr, Address, MappingEnable);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
        public string Unload(string Address)
        {
            string commandStr = "";
            switch (Supplier)
            {
                case "SANWA":

                    commandStr = "$1MCR:ILULD:{0},{0}";
                    commandStr = string.Format(commandStr, Address);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return commandStr + EndCode();
        }
    }
}
