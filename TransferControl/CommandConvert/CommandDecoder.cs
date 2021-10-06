using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.CommandConvert
{
    public class CommandDecoder
    {
        public string Supplier;

        public CommandDecoder(string supplier)
        {
            Supplier = supplier.ToUpper();
        }

        public List<CommandReturnMessage> GetMessage(string Message)
        {
            List<CommandReturnMessage> result = null;

            try
            {
                switch (Supplier)
                {
                    case "SANWA_MC":
                        result = SANWA_MCCodeAnalysis(Message);
                        break;
                    case "SANWA":
                    case "ATEL_NEW":
                        result = SANWACodeAnalysis(Message);
                        break;

                    case "TDK":
                        result = TDKCodeAnalysis(Message);
                        break;

                    case "KAWASAKI":
                        result = KAWASAKICodeAnalysis(Message);
                        break;

                    case "HST":
                        result = HSTCodeAnalysis(Message);
                        break;

                    case "COGNEX":
                        result = COGNEXCodeAnalysis(Message);
                        break;

                    case "ATEL":
                        result = ATELCodeAnalysis(Message);
                        break;
                    case "ASYST":
                        result = ASYSTCodeAnalysis(Message);
                        break;
                    case "SMARTTAG8200":
                        result = SmartTag8200CodeAnalysis(Message);

                        break;
                    case "SMARTTAG8400":
                        result = SmartTag8400CodeAnalysis(Message);

                        break;
                    case "ACDT":
                        result = ACDTCodeAnalysis(Message);
                        break;
                    case "MITSUBISHI_PLC":
                        result = MITSUBISHI_PLCAnalysis(Message);
                        break;

                    case "OMRON_V640":
                        result = RFIDOMRONV640CodeAnalysis(Message);
                        break;

                    case "RFID_HR4136":
                        result = RFIDHR4136CodeAnalysis(Message);
                        break;

                    case "FRANCES":
                        result = SEMIE84CodeAnalysis(Message);
                        break;

                    default:
                        throw new NotImplementedException();

                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }
        private List<CommandReturnMessage> MITSUBISHI_PLCAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            string strMsg = string.Empty;
            string[] msgAry;
            CommandReturnMessage each = new CommandReturnMessage();

            try
            {
                result = new List<CommandReturnMessage>();
                CommandReturnMessage msg = new CommandReturnMessage();
                msg.Type = CommandReturnMessage.ReturnType.Excuted;
                msg.Value = Message.Substring(Message.IndexOf("FB0000") + 6, Message.IndexOf((char)3) == -1 ? Message.Length - (Message.IndexOf("FB0000") + 6) : Message.IndexOf((char)3) - (Message.IndexOf("FB0000") + 6));
                msg.OrgMsg = Message;
                result.Add(msg);
            }
            catch (Exception ex)
            {

                throw new Exception(Message + ":" + ex.ToString());
            }

            return result;
        }
        private List<CommandReturnMessage> ACDTCodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            byte[] msgAry;

            try
            {
                result = new List<CommandReturnMessage>();
                CommandReturnMessage each = new CommandReturnMessage();

                string hexString = Message.ToString().Replace("-", "");
                msgAry = new byte[hexString.Length / 2];
                for (int i = 0; i < hexString.Length; i = i + 2)
                {
                    //每2位16進位數字轉換為一個10進位整數
                    msgAry[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                }


                //msgAry = Encoding.ASCII.GetBytes(Message);
                switch (msgAry[1])
                {
                    case 21://ack
                        each.Type = CommandReturnMessage.ReturnType.Excuted;
                        break;
                    case 22://nak
                        each.Type = CommandReturnMessage.ReturnType.Error;
                        each.Value = "CheckSumError";
                        break;
                    case 105://ack with value
                        switch (msgAry[5])
                        {
                            case 0:
                                each.Type = CommandReturnMessage.ReturnType.Excuted;
                                each.Value = (Convert.ToInt32(msgAry[4]) * 10).ToString();
                                break;
                            case 1:
                                each.Type = CommandReturnMessage.ReturnType.Error;
                                each.Value = "OverLoad";
                                break;
                            case 2:
                                each.Type = CommandReturnMessage.ReturnType.Error;
                                each.Value = "HighTemperature";
                                break;
                        }

                        break;

                }
                if (msgAry[1] == 21 || msgAry[1] == 105)
                {
                    each.Type = CommandReturnMessage.ReturnType.Excuted;
                    if (msgAry[1] == 105 && msgAry.Length >= 8)
                    {
                        each.Value = (Convert.ToInt32(msgAry[4]) * 10).ToString();
                    }
                    each.NodeAdr = "1";
                }


                each.OrgMsg = Message;



                result.Add(each);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }

        private byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        private List<CommandReturnMessage> RFIDHR4136CodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;

            try
            {
                result = new List<CommandReturnMessage>();

                CommandReturnMessage r = new CommandReturnMessage();
                if (Message[0] == (char)4)
                {
                    r.Type = CommandReturnMessage.ReturnType.Excuted;
                }
                else if (Message[0] == (char)6 || Message[0] == (char)5)
                {
                    r.Type = CommandReturnMessage.ReturnType.Information;
                }
                else
                {
                    r.Type = CommandReturnMessage.ReturnType.Finished;
                    r.Value = Message;

                    if(Message.Contains("SSACK_"))
                    {
                        r.Type = CommandReturnMessage.ReturnType.Error;
                        r.Value = r.Value.Replace("SSACK_","");
                    }
                }

                result.Add(r);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }
        private List<CommandReturnMessage> RFIDOMRONV640CodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            string[] msgAry;

            try
            {
                result = new List<CommandReturnMessage>();

                msgAry = Message.Split('\r');

                foreach (string Msg in msgAry)
                {
                    if (Msg.Trim().Equals(""))
                    {
                        continue;
                    }

                    CommandReturnMessage each = new CommandReturnMessage();
                    each.OrgMsg = Msg;

                    each.NodeAdr = "01";

                    string strResponseCode = Msg.Substring(3, 2);

                    switch(strResponseCode)
                    {
                        case "00":
                            each.Type = CommandReturnMessage.ReturnType.Finished;

                            if(Msg.Length > 7)
                            {
                                string temp = Msg.Substring(5, 32);

                                string[] Data = new string[16];
                                //取得FOUP_ID
                                for(int i = 0; i<16; i++)
                                    Data[i] = temp[i * 2].ToString() + temp[i * 2 + 1].ToString();

                                each.Value = "";

                                foreach(string d in Data)
                                {
                                    if(!d.Equals("00"))
                                        each.Value += Char.ConvertFromUtf32(Convert.ToInt32(d, 16));
                                }
                                    

                                each.Value = each.Value.Trim();
                            }
                            break;
                        case "14":
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = "14";//"FormatError";
                            break;
                        case "70":
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = "70";//"CommunicationsError";
                            break;
                        case "71":
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = "71";//"VerificationError";
                            break;
                        case "72":
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = "72";// "NoTagError";
                            break;
                        case "7B":
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = "7B";// "OutsideWriteAreaError";
                            break;
                        case "7E":
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = "7E";// "IDSystemError_1";
                            break;
                        case "7F":
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = "7F";// "IDSystemError_2";
                            break;
                        default:
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = "Unknow";
                            break;
                    }

                    result.Add(each);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }
        private List<CommandReturnMessage> SEMIE84CodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            string[] msgAry;

            try
            {
                result = new List<CommandReturnMessage>();

                msgAry = Message.Split('\r');

                foreach (string Msg in msgAry)
                {
                    if (Msg.Trim().Equals(""))
                    {
                        continue;
                    }

                    CommandReturnMessage each = new CommandReturnMessage();
                    each.OrgMsg = Msg;

                    each.NodeAdr = "01";
                    each.Value = "";
                    string strRawMsg = Msg.ToUpper().Replace(" ", "");
                    string strResponseCode = "";
                    string strFunctionCode = "";
                    string strErrorCode = "";

                    if (strRawMsg.Length == 8)
                    {
                        continue;
                    }

                    string strHeader = strRawMsg.Substring(0, 2).ToUpper();
                    if (strHeader == "FF")
                    {
                        strFunctionCode = "FF";
                        strResponseCode = strRawMsg.Substring(2, 2);
                        strErrorCode = strRawMsg.Substring(2, 2);
                    }
                    else
                    {
                        strFunctionCode = strRawMsg.Substring(2, 2);
                        strResponseCode = strRawMsg.Substring(4, 2);
                        each.Command = strFunctionCode.ToUpper();
                    }

                    switch(strFunctionCode.ToUpper())
                    {
                        case "FF":
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = strFunctionCode.ToUpper() + strErrorCode.ToUpper();
                            break;
                        case "01":  //設定 E84 Controller 到AUTO MODE
                        case "02":  //設定 E84 Controller 到MANUAL MODE
                        case "04":  //重啟 E84 Controller
                            if (strResponseCode.Equals("00"))
                            {
                                each.Type = CommandReturnMessage.ReturnType.Excuted;
                            }
                            else if (strFunctionCode.ToUpper().Equals("04") && strResponseCode.Equals("00"))
                            {
                                each.Type = CommandReturnMessage.ReturnType.Excuted;
                            }
                            else
                            {
                                each.Type = CommandReturnMessage.ReturnType.Error;
                                each.Value = strRawMsg.Replace("AA", "").Replace("BB","");
                            }
                            break;
                        case "81":
                            each.Type = CommandReturnMessage.ReturnType.Excuted;
                            each.Value = strRawMsg.Substring(4, 4);

                            if(each.Value == "FFFF" || each.Value == "FFFD")
                            {
                                each.Type = CommandReturnMessage.ReturnType.Error;
                            }
                            break;
                        case "90":
                        case "91":
                            each.Type = CommandReturnMessage.ReturnType.Excuted;
                            each.Value = strRawMsg.Substring(4, 6);

                            if (each.Value == "FFFF" || each.Value == "FFFD")
                            {
                                each.Type = CommandReturnMessage.ReturnType.Error;
                            }
                            break;

                        case "55":  
                            each.Type = CommandReturnMessage.ReturnType.Event;
                            switch(strResponseCode.ToUpper())
                            {
                                case "10":
                                    each.Command = "GO_ON";
                                    break;
                                case "11":
                                    each.Command = "CS_0_ON";
                                    break;
                                case "12":
                                    each.Command = "VALID_ON";
                                    break;
                                case "13":
                                    each.Command = "L_REQ_ON";
                                    break;
                                case "14":
                                    each.Command = "U_REQ_ON";
                                    break;
                                case "15":
                                    each.Command = "TR_REQ_ON";
                                    break;
                                case "16":
                                    each.Command = "READY_ON";
                                    break;
                                case "17":
                                    each.Command = "BUSY_ON";
                                    break;
                                case "18":
                                    each.Command = "L_REQ_OFF";
                                    break;
                                case "19":
                                    each.Command = "U_REQ_OFF";
                                    break;
                                case "1A":
                                    each.Command = "BUSY_OFF";
                                    break;
                                case "1B":
                                    each.Command = "TR_REQ_OFF";
                                    break;
                                case "1C":
                                    each.Command = "COMPT_ON";
                                    break;
                                case "1D":
                                    each.Command = "READY_OFF";
                                    break;
                                case "1E":
                                    each.Command = "VALID_OFF";
                                    break;
                                case "1F":
                                    each.Command = "COMPT_OFF";
                                    break;
                                case "20":
                                    each.Command = "CS_0_OFF";
                                    break;
                                case "21":
                                    each.Command = "GO_OFF";
                                    break;
                                case "26":
                                    each.Command = "HO_AVBL_ON";
                                    break;
                                case "27":
                                    each.Command = "HO_AVBL_OFF";
                                    break;
                                case "28":
                                    each.Command = "CS_1_ON";
                                    break;
                                case "29":
                                    each.Command = "CS_1_OFF";
                                    break;
                                case "2A":
                                    each.Command = "VS_0_ON";
                                    break;
                                case "2B":
                                    each.Command = "VS_0_OFF";
                                    break;
                                case "2C":
                                    each.Command = "VS_1_ON";
                                    break;
                                case "2D":
                                    each.Command = "VS_1_OFF";
                                    break;
                                case "F1":
                                case "FA":
                                    each.Command = "ES_ON";
                                    break;

                                case "F2":
                                case "F4":
                                    each.Command = "CLAMP_ON";
                                    break;
                                case "F3":
                                    each.Command = "CLAMP_OFF";
                                    break;

                                case "F5":
                                case "F7":
                                    each.Command = "LC_ON";
                                    break;

                                case "F6":
                                    each.Command = "LC_OFF";
                                    break;
                                case "F8":
                                    each.Command = "ALARM_ON";
                                    break;
                                case "F9":
                                    each.Command = "ALARM_OFF";
                                    break;
                                case "FB":
                                    each.Command = "ES_OFF";
                                    break;

                                case "FD":
                                    each.Command = "MANUAL_MODE";
                                    break;

                                case "FE":
                                    each.Command = "AUTO_MODE";
                                    break;

                                case "80":  //CS_0 timeout: GO_ON, wait CS_0 ON timeout
                                case "81":  //TD0 timeout: CS_0 ON, wait VALID ON timeout
                                case "82":  //TP1 timeout: (L_REQ or U_REQ) ON wait TR_REQ ON timeout (2 seconds)
                                case "83":  //TP2 timeout: READY ON wait BUSY ON timeout (2 seconds)
                                case "84":  //TP3-LOAD timeout: BUSY_ON, LOADING FOUP wait PS ON & PL ON timeout(60 seconds)
                                case "86":  //TP3-UNLOAD timeout: BUSY_ON, UNLOADING FOUP wait PS OFF & PL OFF timeout (60 seconds)
                                case "88":  //TP4 timeout: FOUP (LOADED/UNLOADED) wait BUSY OFF, TR_REQ OFF, COMPT ON timeout (60 seconds)
                                case "8B":  //TP5 timeout: READY OFF wait VALID OFF,COMPT OFF,CS_0 OFF timeout (2 seconds)
                                case "8C":  //TP6 timeout: In Continue Handoff, VALID OFF to next VALID ON timeout (2 seconds)
                                case "A0":  //Wait GO ON => CS_0,VALID,TR_REQ,BUSY,COMPT anyone ON
                                case "A1":  //Wait CS_0 ON => GO OFF
                                case "A2":  //Wait CS_0 ON => VALID,TR_REQ,BUSY,COMPT anyone ON
                                case "A3":  //Wait VALID ON => GO, CS_0 anyone OFF
                                case "A4":  //During TA1(VALID ON - L_REQ or U_REQ ON) => GO,CS0,VALID anyone OFF
                                case "A5":  //During TA1(VALID ON - L_REQ or U_REQ ON) => TR-REQ,BUSY,COMPT anyone ON
                                case "A6":  //Wait TR_REQ ON => BUSY,COMPT anyone ON
                                case "A7":  //Wait BUSY ON => GO,CS_0,VALID,TR_REQ anyone OFF
                                case "A8":  //During TA2(TR_REQ ON - READY ON) => GO,CS0,VALID,TR-REQ anyone OFF
                                case "A9":  //Wait BUSY OFF, TR_REQ OFF,COMPT ON => GO,CS_0,VALID anyone OFF
                                case "AA":  //During TA2(TR_REQ ON - READY ON) => BUSY,COMPT anyone ON
                                case "AB":  //Wait BUSY ON => GO,CS0,VALID,TR_REQ anyone OFF
                                case "AC":  //Wait BUSY ON => COMPT ON
                                case "AD":  //Wait BUSY,TR_REQ OFF,COMPT ON => GOT E84 SIGNAL ERROR
                                case "AE":  //During TA3(COMPT ON - READY OFF) , E84 handshake signal error
                                case "AF":  //During TA3(COMPT ON - READY OFF) , E84 handshake signal error
                                case "B0":  //Wait handshake finish(VALID,COMPT,CS_0 OFF), E84 handshake signal error
                                case "C0":  //BUSY ON, LOADING process, wait PS ON => GO,VALID,CS_0,TR_REQ,BUSY anyone OFF
                                case "C1":  //BUSY ON, LOADING process, wait PS ON => COMPT ON
                                case "C2":  //BUSY ON, LOADING process, wait PL ON => GO,VALID,CS_0,TR_REQ,BUSY anyone OFF
                                case "C3":  //BUSY ON, LOADING process, wait PL ON => COMPT ON
                                case "C4":  //BUSY ON, UNLOADING process, wait PL OFF => GO,VALID,CS_0,TR_REQ,BUSY anyone OFF
                                case "C5":  //BUSY ON, UNLOADING process, wait PL OFF => COMPT ON
                                case "C6":  //BUSY ON, UNLOADING process, wait PS OFF => GO,VALID,CS_0,TR_REQ,BUSY anyone OFF
                                case "C7":  //BUSY ON, UNLOADING process, wait PS OFF => COMPT ON
                                case "C8":  //FOUP SENSOR signal located (L-REQ or U-REQ not OFF) => Got E84 signal error
                                case "C9":  //FOUP SENSOR signal located (L-REQ or U-REQ not OFF) => Got FOUP SENSOR signal error
                                case "D0":  //Wait GO ON => Presence Sensor (PS) or Placement Sensor (PL) signal Error
                                case "D1":  //Wait CS_0 ON => PS or PL signal Error
                                case "D2":  //Wait VALID ON => PS or PL signal Error
                                case "D3":  //TA1 period => PS or PL signal Error
                                case "D4":  //Wait TR_REQ ON => PS or PL signal Error
                                case "D5":  //TA2 period => PS or PL signal Error
                                case "D6":  //Wait BUSY ON => PS or PL signal Error
                                case "DC":  //LOADING process, wait PS ON, detected PL ON
                                case "DD":  //LOADING process, wait PL ON, detected PS OFF
                                case "DE":  //UNLOADIND process, wait PL OFF, detected PS OFF
                                case "DF":  //UNLOADIND process, wait PS OFF, detected PL ON
                                case "E0":  //Wait BUSY OFF => PS or PL signal Error
                                case "E1":  //TA3 period => PS or PL signal Error
                                case "E2":  //Wait TR_REQ OFF => PS or PL signal Error
                                case "E3":  //Wait COMPT ON => PS or PL signal Error
                                case "E4":  //Wait VALID OFF => PS or PL signal Error
                                case "E5":  //Wait COMPT OFF => PS or PL signal Error
                                case "E6":  //Wait CS_0 OFF => PS or PL signal Error
                                case "E7":  //Wait GO OFF => PS or PL signal Error
                                    each.Command = "ERROR";
                                    each.Value = strFunctionCode.ToUpper() + strResponseCode.ToUpper();
                                    break;

                                default:
                                    each.Command = "";
                                    break;

                            }
                            break;
                    }

                    if(!each.Command.Equals(""))
                        result.Add(each);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }
        private List<CommandReturnMessage> SmartTag8200CodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;


            try
            {
                result = new List<CommandReturnMessage>();
                CommandReturnMessage r = new CommandReturnMessage();
                r.NodeAdr = "00";
                if (Message.StartsWith("CA"))
                {

                    r.Type = CommandReturnMessage.ReturnType.Excuted;
                    result.Add(r);

                }
                else if (Message.StartsWith("60"))
                {
                    if (Message.Replace(" 00", "").Length > 2)
                    {
                        r.Type = CommandReturnMessage.ReturnType.Information;
                        r.FinCommand = "9F FF";
                        r.Value = parseTag(Message);
                        //r.Value = r.Value.Substring(0, Message.Length - 16);
                    }
                    else
                    {
                        r.Type = CommandReturnMessage.ReturnType.Excuted;


                    }
                    result.Add(r);
                }
                else if (Message.IndexOf("A8") != -1)
                {
                    if (Message.Replace(" 00", "").Length > 2)
                    {
                        r.Type = CommandReturnMessage.ReturnType.Error;
                        r.FinCommand = "9F FF";
                        r.Value = "Check sum faild";
                        //r.Value = r.Value.Substring(0, Message.Length - 16);
                    }
                    result.Add(r);
                }
                else
                {
                    r.CommandType = "GET";
                    result.Add(r);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }

        private List<CommandReturnMessage> SmartTag8400CodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;


            try
            {
                result = new List<CommandReturnMessage>();


                CommandReturnMessage r = new CommandReturnMessage();
                if (Message[0] == (char)4)
                {
                    r.Type = CommandReturnMessage.ReturnType.Excuted;
                }
                else if (Message[0] == (char)6 || Message[0] == (char)5)
                {
                    r.Type = CommandReturnMessage.ReturnType.Information;
                }
                else
                {
                    r.Type = CommandReturnMessage.ReturnType.Finished;
                    r.Value = Message;
                }

                result.Add(r);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }
        private string parseTag(string tag)
        {
            string[] datas = tag.Replace("60 00 AF 0A ", "").Split(' ');
            StringBuilder result = new StringBuilder();
            int lenResult = 0;
            foreach (string data in datas)
            {
                if (data.Equals(""))
                    continue;
                lenResult++;
                if (lenResult > 240 - 16)
                    break;//超出資料範圍
                try
                {
                    string chr1 = getReadMappingChar(data.Substring(0, 1));
                    string chr2 = getReadMappingChar(data.Substring(1, 1));
                    string temp = System.Convert.ToChar(System.Convert.ToUInt32(chr2 + chr1, 16)).ToString();
                    if (!chr2.Equals("0") || !chr1.Equals("0"))
                        result.Append(temp + "");//trim null
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
            }
            return result.ToString();
        }

        private string getReadMappingChar(string tag)
        {
            Dictionary<string, string> charMap = new Dictionary<string, string>();
            charMap.Add("0", "0");
            charMap.Add("1", "8");
            charMap.Add("2", "4");
            charMap.Add("3", "C");
            charMap.Add("4", "2");
            charMap.Add("5", "A");
            charMap.Add("6", "6");
            charMap.Add("7", "E");
            charMap.Add("8", "1");
            charMap.Add("9", "9");
            charMap.Add("A", "5");
            charMap.Add("B", "D");
            charMap.Add("C", "3");
            charMap.Add("D", "B");
            charMap.Add("E", "7");
            charMap.Add("F", "F");
            return charMap[tag];
        }

        private string CalculateReadChecksum(string dataToCalculate, string checkString)
        {
            byte[] byteToCalculate = Encoding.ASCII.GetBytes(dataToCalculate);
            //byte[] byteToCheck = HexStringToByteArray(checkString);

            int checkSum = 0;
            byte[] bdata = { 0x50 };
            //基底 50
            foreach (byte b in bdata)
            {
                checkSum += b;
            }
            //ASCII 資料 加總
            foreach (byte chData in byteToCalculate)
            {
                checkSum += chData;
            }

            // check sum 附加碼轉 ASCII
            string check1 = checkString.Replace(" ", "").Substring(0, 4);
            string chkChar1 = getReadMappingChar(check1.Substring(0, 1));
            string chkChar2 = getReadMappingChar(check1.Substring(1, 1));
            string chkChar3 = getReadMappingChar(check1.Substring(2, 1));
            string chkChar4 = getReadMappingChar(check1.Substring(3, 1));
            int checkTemp = Int32.Parse(chkChar2 + chkChar1 + chkChar4 + chkChar3, System.Globalization.NumberStyles.HexNumber);
            checkSum = checkSum + checkTemp;

            // Check sum 加密
            string temp = checkSum.ToString("X4");
            string charEncode1 = getReadMappingChar(temp.Substring(0, 1));
            string charEncode2 = getReadMappingChar(temp.Substring(1, 1));
            string charEncode3 = getReadMappingChar(temp.Substring(2, 1));
            string charEncode4 = getReadMappingChar(temp.Substring(3, 1));

            string result = checkString + " " + charEncode4 + charEncode3 + " " + charEncode2 + charEncode1 + " ";
            return result;
        }

        private List<CommandReturnMessage> COGNEXCodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            string[] msgAry;

            try
            {
                result = new List<CommandReturnMessage>();
                msgAry = Message.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                foreach (string Msg in msgAry)
                {
                    if (Msg.Trim().Equals(""))
                    {
                        continue;
                    }
                    CommandReturnMessage each = new CommandReturnMessage();
                    each.NodeAdr = "1";
                    each.Command = "";
                    each.OrgMsg = Msg;
                    //each.CommandType = "CMD";
                    switch (Msg.Trim())
                    {
                        case "User:":
                            each.Type = CommandReturnMessage.ReturnType.UserName;

                            break;
                        case "Password:":
                            each.Type = CommandReturnMessage.ReturnType.Password;
                            break;
                        case "1":
                            each.Type = CommandReturnMessage.ReturnType.Excuted;
                            break;
                        case "-2":
                            each.Type = CommandReturnMessage.ReturnType.Error;
                            each.Value = "-2";
                            break;
                        default:
                            each.Type = CommandReturnMessage.ReturnType.Finished;
                            each.Value = Msg;
                            break;
                    }
                    result.Add(each);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }

        private List<CommandReturnMessage> HSTCodeAnalysis(string Msg)
        {
            List<CommandReturnMessage> result;
            //string[] msgAry;

            try
            {
                result = new List<CommandReturnMessage>();
                //msgAry = Message.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                //foreach (string Msg in msgAry)
                //{
                //if (Msg.Trim().Equals(""))
                //{
                //    continue;
                //}
                CommandReturnMessage each = new CommandReturnMessage();
                each.NodeAdr = "1";
                each.Command = "";
                each.OrgMsg = Msg;
                each.CommandType = "CMD";
                switch (Msg)
                {
                    case "1\r\n":
                        each.Type = CommandReturnMessage.ReturnType.Excuted;
                        break;
                    case "-2\r\n":
                        each.Type = CommandReturnMessage.ReturnType.Error;
                        each.Value = "-2";
                        break;
                    default:
                        each.Type = CommandReturnMessage.ReturnType.Finished;
                        each.Value = Msg.Replace("\r\n", "");
                        break;
                }
                result.Add(each);
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }
        private List<CommandReturnMessage> SANWA_MCCodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            string[] msgAry;

            try
            {
                result = new List<CommandReturnMessage>();
                msgAry = Message.Split('\r');

                foreach (string Msg in msgAry)
                {
                    if (Msg.Trim().Equals(""))
                    {
                        continue;
                    }
                    CommandReturnMessage each = new CommandReturnMessage();
                    each.OrgMsg = Msg.Substring(Msg.IndexOf("$"));
                    each.CommandType = "CMD";
                    string[] content = each.OrgMsg.Replace("\r", "").Replace("\n", "").Substring(2).Split(':');
                    for (int i = 0; i < content.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                switch (content[i])
                                {
                                    case "ACK":
                                        each.Type = CommandReturnMessage.ReturnType.Excuted;
                                        break;
                                    case "NAK":
                                        each.Type = CommandReturnMessage.ReturnType.Error;
                                        break;
                                    case "FIN":
                                        each.Type = CommandReturnMessage.ReturnType.Finished;
                                        break;
                                    case "MCR":
                                    case "SET":
                                        each.Command = content[i];

                                        each.Type = CommandReturnMessage.ReturnType.Sending;
                                        break;
                                    case "EVT":
                                        each.Type = CommandReturnMessage.ReturnType.Event;
                                        break;
                                }
                                break;
                            case 1:
                                if (each.Command == null)
                                {
                                    each.Command = content[i];
                                }
                                else if (!each.Command.Equals("MCR__"))
                                {
                                    each.Command = content[i];
                                }
                                break;
                            case 2:
                                if (each.Type.Equals(CommandReturnMessage.ReturnType.Event))
                                {
                                    each.Value = content[i];
                                    each.NodeAdr = "";
                                }
                                else
                                {
                                    if (!each.Type.Equals(CommandReturnMessage.ReturnType.Sending))
                                    {
                                        if (content[i].IndexOf(",") != -1)
                                        {
                                            each.NodeAdr = content[i].Substring(0, content[i].IndexOf(","));

                                            if (each.Type.Equals(CommandReturnMessage.ReturnType.Finished))
                                            {
                                                each.Value = content[i].Substring(content[i].IndexOf(",") + 1, 8);

                                                if (!each.Value.Equals("00000000"))
                                                {
                                                    each.Type = CommandReturnMessage.ReturnType.Error;
                                                }
                                                else
                                                {
                                                    if (content[i].IndexOf(",", 2) != -1)
                                                    {
                                                        each.Value = content[i].Substring(content[i].IndexOf(",", 2) + 1);
                                                    }
                                                }
                                            }
                                            else if (each.Type.Equals(CommandReturnMessage.ReturnType.Error))
                                            {
                                                each.Value = content[i].Substring(content[i].IndexOf(",") + 1, 8);
                                            }
                                            else if (each.Type.Equals(CommandReturnMessage.ReturnType.Excuted) && each.Command.Equals("E84__"))
                                            {
                                                each.Value = content[i].Substring(content[i].IndexOf(",") + 1, 8).Substring(4);
                                                if (!each.Value.Equals("000000BB"))
                                                {
                                                    each.Type = CommandReturnMessage.ReturnType.Error;
                                                }

                                            }

                                        }
                                        else
                                        {
                                            each.NodeAdr = content[i];
                                        }
                                    }
                                    //if (each.Command.Equals("MCR__"))
                                    //{
                                    //    if (content[i].IndexOf(",") != -1)
                                    //    {
                                    //        each.Value = content[i].Substring(content[i].IndexOf(",") + 1);
                                    //    }
                                    //    else
                                    //    {
                                    //        each.Value = "";
                                    //    }
                                    //}
                                }
                                break;

                        }
                    }
                    //if (each.Command.Equals("RESET")|| each.Command.Equals("SP___") || each.Command.Equals("PAUSE") || each.Command.Equals("CONT_") || each.Command.Equals("STOP_"))
                    //{
                    //    each.NodeAdr = "0";
                    //    //each.CommandType = "SET";
                    //}
                    result.Add(each);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }
        private List<CommandReturnMessage> SANWACodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            string[] msgAry;

            try
            {
                result = new List<CommandReturnMessage>();
                msgAry = Message.Split('\r');

                foreach (string Msg in msgAry)
                {
                    if (Msg.Trim().Equals(""))
                    {
                        continue;
                    }
                    CommandReturnMessage each = new CommandReturnMessage();
                    each.OrgMsg = Msg.Substring(Msg.IndexOf("$"));

                    each.NodeAdr = each.OrgMsg[1].ToString();
                    string[] content = each.OrgMsg.Replace("\r", "").Replace("\n", "").Substring(2).Split(':');
                    for (int i = 0; i < content.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                switch (content[i])
                                {
                                    case "ACK":
                                        each.Type = CommandReturnMessage.ReturnType.Excuted;
                                        break;
                                    case "NAK":
                                        each.Type = CommandReturnMessage.ReturnType.Error;
                                        break;
                                    case "FIN":
                                        each.Type = CommandReturnMessage.ReturnType.Finished;
                                        break;
                                    case "EVT":
                                        each.Type = CommandReturnMessage.ReturnType.Event;
                                        break;
                                    default:
                                        each.CommandType = content[i];
                                        break;
                                }

                                break;
                            case 1:

                                each.Command = content[i];
                                if (each.Command.Equals("PAUSE") || each.Command.Equals("STOP_"))
                                {
                                    each.IsInterrupt = true;
                                }
                                break;
                            case 2:
                                each.Value = content[i];
                                if (each.Type == CommandReturnMessage.ReturnType.Finished && !each.Value.Equals("00000000"))
                                {
                                    each.Type = CommandReturnMessage.ReturnType.Error;
                                }
                                break;
                        }
                    }
                    result.Add(each);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }

        private List<CommandReturnMessage> KAWASAKICodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            string[] msgAry;

            try
            {
                result = new List<CommandReturnMessage>();
                msgAry = Message.Replace("\r\n", "\r").Split('\r');

                foreach (string Msg in msgAry)
                {
                    if (Msg.Trim().Equals(""))
                    {
                        continue;
                    }
                    CommandReturnMessage each = new CommandReturnMessage();
                    each.OrgMsg = Msg;
                    each.Command = Msg.Substring(Msg.IndexOf('<') + 1, Msg.IndexOf('>') - Msg.IndexOf('<') - 1);
                    each.CommandType = "CMD";
                    string[] content = each.Command.Split(',');
                    for (int i = 0; i < content.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                each.Seq = content[0];
                                break;
                            case 1:

                                switch (content[i])
                                {
                                    case "Ack":
                                        each.Type = CommandReturnMessage.ReturnType.Excuted;
                                        break;
                                    case "Nak":
                                        each.Type = CommandReturnMessage.ReturnType.Error;
                                        if (content.Length > 2)
                                            each.Value = content[2];
                                        break;
                                    case "Success":
                                        each.Type = CommandReturnMessage.ReturnType.Finished;
                                        if (content.Length > 2)
                                        {
                                            for (int k = 2; k < content.Length; k++)
                                            {
                                                each.Value += content[k] + " ";
                                            }
                                            each.Value = each.Value.Trim();
                                        }
                                        break;
                                    case "Error":
                                        each.Type = CommandReturnMessage.ReturnType.Error;
                                        if (content.Length > 3)
                                            each.NodeAdr = content[3].ToString();
                                        if (content.Length > 4)
                                            each.Value = content[2] + ":" + content[4];
                                        break;
                                    default:

                                        break;
                                }
                                break;
                        }
                    }
                    result.Add(each);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }

        private List<CommandReturnMessage> TDKCodeAnalysis(string orgMsg)
        {
            List<CommandReturnMessage> result;

            try
            {
                result = new List<CommandReturnMessage>();
                string[] msgAry = orgMsg.Split(Convert.ToChar(3));
                foreach (string Msg in msgAry)
                {
                    if (Msg.Trim().Equals(""))
                    {
                        return result;
                    }

                    CommandReturnMessage each = new CommandReturnMessage();
                    byte[] t = new byte[Encoding.ASCII.GetByteCount(Msg.ToString())]; ;
                    int c = Encoding.ASCII.GetBytes(Msg.ToString(), 0, Encoding.ASCII.GetByteCount(Msg.ToString()), t, 0);

                    each.OrgMsg = Msg;
                    each.NodeAdr = Encoding.Default.GetString(t, 3, 2);
                    string contentStr = Encoding.Default.GetString(t, 5, t.Length - 5 - 3).Replace(";", "").Trim();
                    contentStr = contentStr.Replace("/INTER/", "/");
                    string[] content = contentStr.Split(':', '/');

                    for (int i = 0; i < content.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                switch (content[i])
                                {
                                    case "ACK":
                                        each.Type = CommandReturnMessage.ReturnType.Excuted;
                                        break;
                                    case "NAK":
                                        each.Type = CommandReturnMessage.ReturnType.Error;
                                        break;
                                    case "INF":
                                    case "RIF":
                                        each.Type = CommandReturnMessage.ReturnType.Information;
                                        break;
                                    case "EVT":
                                        each.Type = CommandReturnMessage.ReturnType.Event;
                                        break;
                                    case "ABS":
                                    case "RAS":
                                        each.Type = CommandReturnMessage.ReturnType.Error;
                                        break;
                                        //case "RIF":
                                        //    each.Type = ReturnMessage.ReturnType.ReInformation;
                                        //    break;
                                }
                                each.CommandType = content[i];
                                break;
                            case 1:

                                each.Command = content[i];
                                if (each.Type == CommandReturnMessage.ReturnType.Information || each.Type == CommandReturnMessage.ReturnType.ReInformation || each.Type == CommandReturnMessage.ReturnType.Error)
                                {
                                    each.FinCommand = TDKFinCommand(each.Command);
                                }
                                if (each.Command.Equals("PAUSE") || each.Command.Equals("STOP_"))
                                {
                                    each.IsInterrupt = true;
                                }
                                break;
                            case 2:
                                each.Value = content[i];
                                break;
                        }
                    }

                    result.Add(each);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }

        private List<CommandReturnMessage> ATELCodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            string strMsg = string.Empty;
            string[] msgAry;
            CommandReturnMessage each;

            try
            {
                result = new List<CommandReturnMessage>();

                strMsg = Message.Replace("\r", "").Replace("\n", "").Trim();

                if (strMsg.Equals(">") || strMsg.Equals(">*"))
                {
                    each = new CommandReturnMessage();
                    each.OrgMsg = strMsg;
                    each.NodeAdr = "1";
                    each.Type = CommandReturnMessage.ReturnType.Excuted;
                    result.Add(each);
                }
                else if (strMsg.Length > 1)
                {
                    msgAry = Message.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                    foreach (string Msg in msgAry)
                    {
                        if (Msg.Trim().Equals(""))
                        {
                            continue;
                        }

                        each = new CommandReturnMessage();
                        each.OrgMsg = Msg;
                        each.NodeAdr = Msg[1].ToString();
                        string[] content = Msg.Replace("\r", "").Replace("\n", "").Substring(2).Trim().Split('\r');
                        for (int i = 0; i < content.Length; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    each.Type = CommandReturnMessage.ReturnType.Finished;
                                    each.Value = Msg;
                                    break;

                                default:
                                    each.Type = CommandReturnMessage.ReturnType.Finished;
                                    each.Value = Msg;
                                    break;
                            }
                        }
                        result.Add(each);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }

        private List<CommandReturnMessage> ASYSTCodeAnalysis(string Message)
        {
            List<CommandReturnMessage> result;
            string[] msgAry;

            try
            {
                result = new List<CommandReturnMessage>();
                msgAry = Message.Split(new string[] { "\r\n" }, StringSplitOptions.None);

                foreach (string Msg in msgAry)
                {
                    if (Msg.Trim().Equals(""))
                    {
                        continue;
                    }
                    CommandReturnMessage each = new CommandReturnMessage();
                    each.OrgMsg = Msg;
                    each.NodeAdr = "00";
                    string[] content = Msg.Replace("\r", "").Replace("\n", "").Split(' ');
                    for (int i = 0; i < content.Length; i++)
                    {
                        switch (each.CommandType)
                        {

                            case "FSD2":
                                switch (content[i].Substring(content[i].IndexOf("=") + 1))
                                {
                                    case "F":
                                        each.Value += "1";
                                        break;
                                    case "C":
                                        each.Value += "2";
                                        break;
                                    case "E":
                                        each.Value += "0";
                                        break;
                                    case "U":
                                        each.Value += "?";
                                        break;
                                }
                                each.Type = CommandReturnMessage.ReturnType.Excuted;
                                //each.CommandType = "GET";
                                break;
                            case "FSD0":
                                if (!each.Value.Equals(""))
                                {
                                    each.Value += ",";
                                }
                                each.Value += content[i];
                                //each.CommandType = "GET";
                                each.Type = CommandReturnMessage.ReturnType.Excuted;
                                break;
                            default:

                                switch (i)
                                {
                                    case 0:
                                        each.CommandType = content[i];
                                        break;
                                    case 1:
                                        if (each.CommandType.Equals("ECD"))
                                        {
                                            each.CommandType = "ACK";
                                            each.Type = CommandReturnMessage.ReturnType.Excuted;
                                            string[] param = content[i].Split('=');
                                            if (param.Length >= 2)
                                            {
                                                switch (param[0])
                                                {
                                                    case "P30":
                                                        each.Command = "GetSlotOffset";
                                                        break;
                                                    case "P31":
                                                        each.Command = "GetWaferOffset";
                                                        break;
                                                    case "P35":
                                                        each.Command = "GetSlotPitch";
                                                        break;
                                                    case "P36":
                                                        each.Command = "GetTweekDistance";
                                                        break;
                                                    case "P39":
                                                        each.Command = "GetCassetteSize";
                                                        break;
                                                }
                                                each.Value = param[1];
                                            }

                                        }
                                        else
                                        {
                                            switch (content[i])
                                            {
                                                case "ALARM":
                                                case "ABORT_CAL":
                                                case "ABORT_EMPTY_SLOT":
                                                case "ABORT_HOME":
                                                case "ABORT_LOCK":
                                                case "ABORT_MAP":
                                                case "ABORT_POS":
                                                case "ABORT_SLOT":
                                                case "ABORT_STAGE":
                                                case "ABORT_TWEEKDN":
                                                case "ABORT_TWEEKUP":
                                                case "ABORT_UNLOCK":
                                                case "ABORT_WAFER":
                                                case "WARNING":
                                                case "FATAL":
                                                case "FAILED_SELF-TEST":
                                                    each.Type = CommandReturnMessage.ReturnType.Error;
                                                    each.Command = content[i];
                                                    each.Value = content[i];
                                                    break;

                                                case "BUSY":
                                                case "DENIED":
                                                case "INVALID_ARG":
                                                case "NO_POD":
                                                case "NOT_READY":
                                                    each.Type = CommandReturnMessage.ReturnType.Error;
                                                    each.Value = content[i];
                                                    break;

                                                case "OK":
                                                    each.Type = CommandReturnMessage.ReturnType.Excuted;
                                                    break;

                                                case "CMPL_CAL":
                                                case "CMPL_LOCK":
                                                case "CMPL_MAP":
                                                case "CMPL_SELF-TEST":
                                                case "CMPL_TWEEKDN":
                                                case "CMPL_TWEEKUP":
                                                case "CMPL_UNLOCK":
                                                case "REACH_EMPTY_SLOT":
                                                case "REACH_HOME":
                                                case "REACH_POS":
                                                case "REACH_SLOT":
                                                case "REACH_STAGE":
                                                case "REACH_WAFER":
                                                    each.Type = CommandReturnMessage.ReturnType.Finished;
                                                    break;

                                                case "POD_ARRIVED":
                                                case "POD_REMOVED":
                                                case "EXIT_HOME":
                                                    each.Type = CommandReturnMessage.ReturnType.Event;
                                                    each.Command = content[i];
                                                    break;

                                                default:

                                                    each.Command = content[i];
                                                    break;
                                            }
                                        }
                                        break;

                                    case 2:
                                        if (each.CommandType.Equals("ARS"))
                                        {
                                            for (int p = 2; p < content.Length; p++)
                                            {
                                                if (!each.Value.Equals(""))
                                                {
                                                    each.Value += " ";
                                                }
                                                each.Value += content[p];
                                            }
                                        }
                                        else
                                        {
                                            each.Command = content[i];
                                        }
                                        break;
                                }
                                break;
                        }
                    }

                    result.Add(each);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            return result;
        }

        private string TDKFinCommand(string Command)
        {
            string result = "";
            //string strCommsnd = string.Empty;
            string strLen = string.Empty;
            string sCheckSum = string.Empty;
            int chrLH = 0;
            int chrLL = 0;

            try
            {
                Command = "FIN:" + Command + ";";
                strLen = Convert.ToString(Command.Length + 4, 16).PadLeft(2, '0');

                chrLH = Convert.ToInt32(strLen.Substring(0, 1), 16);
                chrLL = Convert.ToInt32(strLen.Substring(1, 1), 16);
                strLen = Convert.ToChar(chrLH).ToString() + Convert.ToChar(chrLL).ToString();
                sCheckSum = TDKCheckSum(strLen, Command);
                result = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}", Convert.ToChar(1), strLen, Convert.ToChar(48), string.Empty, Convert.ToChar(48), Command, sCheckSum, Convert.ToChar(3));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return result;
        }

        private string TDKCheckSum(string Len, string Message)
        {
            string strCheckSum = string.Empty;
            string csHex = string.Empty;

            try
            {
                strCheckSum = string.Format("{0}{1}{2}{3}{4}", Len, Convert.ToChar(48), string.Empty, Convert.ToChar(48), Message.ToString());

                byte[] t = new byte[Encoding.ASCII.GetByteCount(strCheckSum)]; ;
                int ttt = Encoding.ASCII.GetBytes(strCheckSum, 0, Encoding.ASCII.GetByteCount(strCheckSum), t, 0);
                byte tt = 0;

                for (int i = 0; i < t.Length; i++)
                {
                    tt += t[i];
                }

                csHex = tt.ToString("X");
                if (csHex.Length == 1)
                {
                    csHex = "0" + csHex;
                }
                return csHex;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
