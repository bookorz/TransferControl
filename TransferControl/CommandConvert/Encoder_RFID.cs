using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.CommandConvert
{
    public class Encoder_RFID
    {
        public string Supplier;
        /// <summary>
        /// Aligner Encoder
        /// </summary>
        /// <param name="supplier"> 設備供應商 </param>
        /// <param name="dtCommand"> Parameter List </param>
        public Encoder_RFID(string supplier)
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
                case "RFID_HR4136":
                    result = "05";
                    break;
                default:
                    throw new NotSupportedException();
            }
            return result;
        }
        public string Mode(string mode)
        {
            string result = "";
            switch (Supplier)
            {
                case "RFID_HR4136":

                    string tmp = "";
                    if (mode.Equals("MT"))
                    {
                        tmp = "00 00 92 0D 80 01 00 00 00 00 01 03 41 02 30 30 41 0B 43 68 61 6E 67 65 53 74 61 74 65 01 01 41 02 4D 54";
                    }
                    else
                    {
                        tmp = "00 00 92 0D 80 01 00 00 00 00 01 03 41 02 30 30 41 0B 43 68 61 6E 67 65 53 74 61 74 65 01 01 41 02 4F 50";
                    }
                    string CheckSum = CalculateWriteChecksum(tmp);
                    string Length = ((byte)tmp.Split(' ').Length).ToString("X2");
                    result = Length + " " + tmp + " " + CheckSum;

                    break;
                default:
                    throw new NotSupportedException();
            }
            return result;
        }

        public string ReadCarrierID()
        {
            string result = "";
            switch (Supplier)
            {
                case "OMRON_V640":
                    result = ReadCarrierIDByV640();
                    break;
                case "RFID_HR4136":
                    string tmp = "00 00 92 09 80 01 00 00 00 00 41 02 30 30";
                    string CheckSum = CalculateWriteChecksum(tmp);
                    string Length = ((byte)tmp.Split(' ').Length).ToString("X2");
                    result = Length + " " + tmp + " " + CheckSum;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return result;
        }

        public string ReadCarrierID(string id)
        {
            string result = "";
            switch (Supplier)
            {
                case "RFID_HR4136":
                    string tmp = "00 00 92 09 80 01 00 00 00 00 "+ parseWriteData(id);
                    string CheckSum = CalculateWriteChecksum(tmp);
                    string Length = ((byte)tmp.Split(' ').Length).ToString("X2");
                    result = Length + " " + tmp + " " + CheckSum;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return result;
        }

        public string WriteCarrierID(string id)
        {
            string result = "";
            switch (Supplier)
            {
                case "OMRON_V640":
                    result = WriteCarrierIDByV640(id);
                    break;

                case "RFID_HR4136":
                    string tmp = "00 00 92 0B 80 01 00 00 00 00 01 02 41 02 30 30 " + parseWriteData(id);
                    string CheckSum = CalculateWriteChecksum(tmp);
                    string Length = ((byte)tmp.Split(' ').Length).ToString("X2");
                    result = Length + " " + tmp + " " + CheckSum;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return result;
        }

        private string ReadCarrierIDByV640()
        {
            string strRet = "";
            byte[] byteHead= { 0x01 };
            strRet = char.ConvertFromUtf32(byteHead[0]);

            string strNodeNo = "01";
            strRet += strNodeNo;

            string strCommandCode = "0100";
            strRet += strCommandCode;


            string strPage = "0000000C";
            strRet += strPage;

            string strFCS = FCS(strNodeNo + strCommandCode + strPage);
            strRet += strFCS;

            byte[] byteCR = { 0x0D };
            strRet += char.ConvertFromUtf32(byteCR[0]);

            return strRet;
        }

        private string WriteCarrierIDByV640(string id)
        {
            string strRet = "";
            byte[] byteHead = { 0x01 };
            strRet = char.ConvertFromUtf32(byteHead[0]);

            string strNodeNo = "01";
            strRet += strNodeNo;

            string strCommandCode = "0200";
            strRet += strCommandCode;

            string strPage = "0000000C";
            strRet += strPage;

            byte [] bID = Encoding.ASCII.GetBytes(id);
            byte [] bArray = new byte[16];
            for (int i = 0; i < 16; i++)
                bArray[i] = 0x00;

            for (int i = 0; i < bID.Length; i++)
            {
                if (i >= 16) break;
                bArray[i] = bID[i];
            }

            string strValue = "";
            foreach (byte b in bArray)
                strValue += string.Format("{0:X}", b).PadLeft(2,'0');
            
            strRet += strValue;

            string strFCS = FCS(strNodeNo + strCommandCode + strPage + strValue);
            strRet += strFCS;

            byte[] byteCR = { 0x0D };
            strRet += char.ConvertFromUtf32(byteCR[0]);

            return strRet;
        }
        private string FCS(string s)　　//帧校验函数FCS
        {
            //获取s对应的字节数组
            byte[] b = Encoding.ASCII.GetBytes(s);
            // xorResult 存放校验结果。注意：初值去首元素值！
            byte xorResult = b[0];
            // 求xor校验和。注意：XOR运算从第二元素开始
            for (int i = 1; i < b.Length; i++)
            {
                xorResult ^= b[i];
            }
            //**Convert.ToString(xorResult, 16):将当前值转换为16进制；ToUpper()：结果大写；
            //**这里的意思是：将xorResult转换成16进制并大写；

            //**（//**返回的结果为一个两个ASCII码的异或值）
            return Convert.ToString(xorResult, 16).PadLeft(2, '0').ToUpper();

        }

        private string CalculateWriteChecksum(string dataToCalculate)
        {
            string result = "";
            int checksum = 0;
            switch (Supplier)
            {
                case "RFID_HR4136":
                    string[] ary = dataToCalculate.Trim().Split(' ');

                    foreach (string chData in ary)
                        checksum += Convert.ToInt32(chData, 16);

                    result = checksum.ToString("X4").Substring(0, 2) + " " + checksum.ToString("X4").Substring(2, 2);
                break;
            }
            return result;
        }
        private string parseWriteData(string text)
        {
            string Result = "";
            StringBuilder tmpResult = new StringBuilder();
            switch (Supplier)
            {
                case "RFID_HR4136":
                    byte[] temp = Encoding.ASCII.GetBytes(text);
                    for (int i = 0; i < temp.Length; i++)
                    {
                        tmpResult.Append(temp[i].ToString("X2") + " ");
                    }
                    Result = ((byte)temp.Length).ToString("X2") + " " + tmpResult.ToString().Trim();
                    Result = "41 " + Result;
                    break;
            }
            return Result;
        }
    }
}
