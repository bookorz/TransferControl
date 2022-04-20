﻿using System;
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
                case Transaction.Command.Shelf.GetFOUPPresence:
                    return ParsePresent(Message);
                case Transaction.Command.LoadPortType.ReadStatus:
                    return ParseStatus(Message);
                case Transaction.Command.LoadPortType.GetStatus:
                    return ParseGetStatus(Message);
                case Transaction.Command.RobotType.GetPresence:
                    return ParsePresence(Message);
                default:
                    throw new Exception(Command + " Not support");
            }
        }
        private Dictionary<string, string> ParseStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] tmp = Message.Split(',');
            //0.Type
            //  0 : Indexer status
            //  1 : Map Status
            //1.Mode 
            //  0 : Auto, 1 : Manual
            //2.Ready
            //  0 : Indexer is not ready(No ORG Serch 、 Alarm)
            //  1 : Indexer ready for new command
            //3.LFUNC (Last Function Done)
            //  0 : Power up(Boot / Reset
            //  1 : Homing Calibration(ORG__)
            //  2 : Close / Auto Home(HOME_)
            //  3 : Open / Reach Stage(STAGE)
            //  4 : Index(GOTO_ / TWEEK)
            //  5 : Map(MAP__)
            //  6 : W afer search(MSLOT)
            //  7 : Slot search(SLOT_)
            //  8 : Unknown(STEST
            //4.PPS(Pod Present Status)
            //  0 : No Pod present
            //  1 : Pod present
            //5.PLS(Pod Lock Status)
            //  0 : Lock
            //  1 : Unlock
            //6.POS(Position)
            //  0 : Unknown
            //  1 : Home
            //  2 : Lift
            //  3 : Stage
            //  4 : Z Axis Li mit +
            //  5 : Other
            //7.SNO(Slot Number)
            //  0~25(0 : Ignore)
            //8.ELUD(Elevator Up / Down)
            //  0 : Down
            //  1 : Up
            //9.ZPOS(Z Axis Position)
            //  Limit ~Limit + (Unit: um)
            //10.LPS(Lift Present Status)
            //  0 : Absence
            //  1 : Present
            for (int i = 0; i < tmp.Length; i++)
            {
                int idx = i + 1;
                switch (idx)
                {
                    case 2://ManualMode
                        result.Add("SMIF_ManualMode", tmp[i] == "1" ? "TRUE":"FALSE");
                        break;
                    case 3: //Ready
                        result.Add("SMIF_RDY", tmp[i] == "1" ? "TRUE" : "FALSE");
                        break;
                    case 4: //Last Function Done
                        result.Add("SMIF_LFUNC", tmp[i]);
                        break;
                    case 5: //PPS
                        result.Add("PIP", tmp[i] == "1"?"TRUE":"FALSE");
                        break;
                    case 6: //PLS
                        result.Add("PRTST", tmp[i] == "0" ? "LK" : "UNLK");
                        break;
                    case 7: //POS
                        result.Add("SMIF_POS", tmp[i]);
                        break;
                    case 8: //SNO(Slot Number)
                        result.Add("SMIF_SNO", tmp[i]);
                        break;
                    case 9: //ELUD(Elevator Up / Down)
                        result.Add("SMIF_ELUD", tmp[i] == "1" ? "TRUE" : "FALSE");
                        break;
                    case 10: //ZPOS(Z Axis Position)
                        result.Add("Z Axis Position", tmp[i]);
                        break;
                    case 11: //LPS(Lift Present Status)
                        result.Add("SMIF_LPS", tmp[i] == "1" ? "TRUE" : "FALSE");
                        break;
                    default:
                        result.Add("SHELF" + (idx - 5).ToString(), tmp[i]);
                        break;
                }
            }
            return result;
        }
        private Dictionary<string, string> ParseGetStatus(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] tmp = Message.Split(',');
            //0.Type
            //  0 : Indexer status
            //  1 : Map Status
            //1.Alarm 
            //  0 : No, 1 : Yes
            //2.Error
            //  0 : No
            //3.Mode (Last Function Done)
            //  0 : Normal
            //  1 : Dry
            //  2 : Unknown
            //4.ORG
            //  0 : No
            //  1 : Yes
            //5.Presence(PPS)
            //  0 : None
            //  1 : Present
            //  2 : Unknown
            //6.Clamp(PLS)
            //  0 : Unclamp
            //  1 : Clamp
            //  2 : Unknown
            //7.Latch
            //  0 : Unlatch
            //  1 : Latch
            //  2 : Unknown
            //8.Vacuum
            //  0 : Off
            //  1 : On
            //9.Door
            //  0 : Close
            //  1 : Open
            //  2 : Unknown
            //10.ZPos
            //  0 : Home
            //  1 : Map start
            //  2 : Map end
            //  3 : Load end
            //  9 : Unknow
            //11.XPos
            //  0 : Outside
            //  1 : Inside
            //  9 : Unknow
            //12.FOUP Type
            //  0 : Off
            //  1 : On
            //13.Info Pad
            //  BIT 3 : A
            //  BIT 2 : B
            //  BIT 1 : C
            //  BIT 0 : D
            for (int i = 0; i < tmp.Length; i++)
            {
                //int idx = i + 1;
                switch (i)
                {
                    case 5:
                        result.Add("PRESENCE", tmp[i]);
                        break;
                    case 6:
                        result.Add("CLAMP", tmp[i]);
                        break;
                    case 7:
                        result.Add("LATCH", tmp[i]);
                        break;
                    case 13:
                        result.Add("INFOPAD", tmp[i]);
                        break;
                }
            }
            return result;
        }
        private Dictionary<string, string> ParseMap(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>(); 
            result.Add("Mapping", Message.Replace(",",""));
            return result;
        }
        private Dictionary<string, string> ParsePresent(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] tmp = Message.Split(',');
            for(int i = 0; i < tmp.Length; i++)
            {
                int idx = i + 1;
                switch (idx)
                {
                    case 1:
                        result.Add("FOUP_ROBOT", tmp[i]);
                        break;
                    case 2:
                        result.Add("ELPT1", tmp[i]);
                        break;
                    case 3:
                        result.Add("ELPT2", tmp[i]);
                        break;
                    case 4:
                        result.Add("ILPT1", tmp[i]);
                        break;
                    case 5:
                        result.Add("ILPT2", tmp[i]);
                        break;
                    default:
                        result.Add("SHELF"+ (idx-5).ToString(), tmp[i]);
                        break;
                }
            }
            
            

            return result;
        }
        private Dictionary<string, string> ParsePresence(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string[] tmp = Message.Split(',');

            for (int i = 0; i < tmp.Length; i++)
            {
                int idx = i + 1;
                switch (idx)
                {
                    case 2:
                        result.Add("Clamp_Presence", tmp[i]);
                        break;
                    case 3:
                        result.Add("Vacuum_Status", tmp[i]);
                        break;
                    default:
                        break;
                }
            }

            return result;
        }
    }
}
