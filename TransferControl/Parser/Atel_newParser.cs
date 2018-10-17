﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransferControl.Management;

namespace TransferControl.Parser
{
    class Atel_newParser : IParser
    {
        public Dictionary<string, string> Parse(string Command, string Message)
        {
            switch (Command)
            {
                case Transaction.Command.RobotType.GetRIO:
                    return ParseRIO(Message);
                case Transaction.Command.RobotType.GetPosition:
                    return ParsePosition(Message);
                default:
                    throw new Exception(Command + " Not support");
            }

        }

        private Dictionary<string, string> ParseRIO(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] MsgAry = Message.Split(',');
            switch (MsgAry[0])
            {
                case "31":
                    result.Add("R_UnClamp_Sensor", MsgAry[1]);                   
                    break;
                case "32":
                    result.Add("R_Clamp_Sensor", MsgAry[1]);
                    break;
                case "33":
                    result.Add("R_0_Degree_Sensor", MsgAry[1]);
                    break;
                case "34":
                    result.Add("R_180_Degree_Sensor", MsgAry[1]);
                    break;
            }
            return result;
        }

        private Dictionary<string, string> ParsePosition(string Message)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            string[] MsgAry = Message.Split(',');
            for(int i=0;i< MsgAry.Length;i++)
            {
                switch (i)
                {
                    case 4:
                        result.Add("X_Position", MsgAry[i]);
                        break;
                   
                }
            }
            return result;
        }
    }
}
