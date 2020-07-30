using System;
using System.Collections.Generic;
using System.Text;

namespace TransferControl.CommandConvert
{
    public class Encoder_SmartTag
    {
        private string Supplier;


        /// <summary>
        /// Aligner Encoder
        /// </summary>
        /// <param name="supplier"> 設備供應商 </param>
        /// <param name="dtCommand"> Parameter List </param>
        public Encoder_SmartTag(string supplier)
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

        public string Hello()
        {
            string result = "";
            switch (Supplier)
            {
                case "SMARTTAG8200":
                    result = "FC FF B5 FF";
                    break;
                case "SMARTTAG8400":
                    result = "05";
                    break;
                default:
                    throw new NotSupportedException();
            }
            return result;
        }

        public string GetLCDData()
        {

            string result = "";
            switch (Supplier)
            {
                case "SMARTTAG8200":
                    result = "FF FF EF FF F7 FF FF 07 D0 FF 7F DB FF 7F FF";
                    break;
                case "SMARTTAG8400":
                    result = "10 00 00 E4 79 80 01 00 00 00 05 B1 04 00 00 00 07 02 9F";
                    break;
                default:
                    throw new NotSupportedException();
            }
            return result;
        }

        public string SelectLCDData()
        {
            string result = "";
            switch (Supplier)
            {
                case "SMARTTAG8200":
                    result= "FF FF 1F FF 77 FF FF 07 B0 FF BB FF 7F FF";
                    break;
                default:
                    throw new NotSupportedException();
                    
            }
            return result;
        }

        public string SetLCDData(string Data)
        {
            string result = "";
            switch (Supplier)
            {
                case "SMARTTAG8200":
                    Data += "                ";
                    result= "FF FF D0 FF F5 FF " + parseWriteData(Data) + CalculateWriteChecksum(Data);
                    break;
                case "SMARTTAG8400":
                    string tmp = "00 00 E4 77 80 01 00 00 00 09 01 04 B1 04 00 00 2E A5 A5 01 01 41 10 31 39 39 39 30 31 30 31 30 30 30 30 30 30 30 30 41 " + parseWriteData(Data);
                    string CheckSum = CalculateWriteChecksum(tmp);
                    string Length = ((byte)tmp.Split(' ').Length).ToString("X2");
                    result = Length + " " + tmp + " " + CheckSum;
                    
                    break;
                default:
                    throw new NotSupportedException();

            }
            return result;
        }

        private string parseWriteData(string text)
        {
            int ttl_len = 240;
            int start_idx = 240 - text.Length;
            StringBuilder tmpResult = new StringBuilder();
            string Result = "";
            switch (Supplier)
            {
                case "SMARTTAG8200":
                    for (int i = 0; i < ttl_len; i++)
                    {
                        if (i < start_idx)
                        {
                            tmpResult.Append("FF ");
                        }
                        else
                        {

                            string char_0 = Encoding.ASCII.GetBytes(text.Substring(i - start_idx, 1))[0].ToString("X2");
                            string char_1 = getWriteMappingChar(char_0.Substring(0, 1));
                            string char_2 = getWriteMappingChar(char_0.Substring(1, 1));
                            tmpResult.Append(char_2 + char_1 + " ");
                        }
                    }
                    Result = tmpResult.ToString();
                    break;
                case "SMARTTAG8400":
                    byte[] temp = Encoding.ASCII.GetBytes(text);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        tmpResult.Append(temp[i].ToString("X2") + " ");
                    }
                    Result = ((byte)temp.Length).ToString("X2") + " " + tmpResult.ToString().Trim();
                    break;
            }
            return Result;
        }

        private string getWriteMappingChar(string tag)
        {
            Dictionary<string, string> charMap = new Dictionary<string, string>();
            charMap.Add("0", "F");
            charMap.Add("1", "7");
            charMap.Add("2", "B");
            charMap.Add("3", "3");
            charMap.Add("4", "D");
            charMap.Add("5", "5");
            charMap.Add("6", "9");
            charMap.Add("7", "1");
            charMap.Add("8", "E");
            charMap.Add("9", "6");
            charMap.Add("A", "A");
            charMap.Add("B", "2");
            charMap.Add("C", "C");
            charMap.Add("D", "4");
            charMap.Add("E", "8");
            charMap.Add("F", "0");
            return charMap[tag];
        }

        private string CalculateWriteChecksum(string dataToCalculate)
        {
            string result = "";
            int checksum = 0;
            switch (Supplier)
            {

                case "SMARTTAG8200":
                    byte[] byteToCalculate = Encoding.ASCII.GetBytes(dataToCalculate);
                   
                    byte[] bdata = { 0x50 };
                   
                    foreach (byte b in bdata)
                    {
                        checksum += b;
                    }
                    foreach (byte chData in byteToCalculate)
                    {
                        checksum += chData;
                    }
                    //checksum &= 0xff;
                    string temp = checksum.ToString("X4");
                    string char1 = getWriteMappingChar(temp.Substring(0, 1));
                    string char2 = getWriteMappingChar(temp.Substring(1, 1));
                    string char3 = getWriteMappingChar(temp.Substring(2, 1));
                    string char4 = getWriteMappingChar(temp.Substring(3, 1));
                    result = "FF " + char4 + char3 + " FF " + char2 + char1 + " FF ";
                    break;
                case "SMARTTAG8400":
                    string[] ary = dataToCalculate.Trim().Split(' ');

                    

                    foreach (string chData in ary)
                    {

                        checksum += Convert.ToInt32(chData, 16);
                    }

                    result= checksum.ToString("X4").Substring(0, 2) + " " + checksum.ToString("X4").Substring(2, 2);
                    break;
            }
            return result;
        }

    }
}
