using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.CommandConvert
{
    public class Encoder_RFID
    {
        private string Supplier;
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

        public string ReadCarrierID()
        {
            string result = "";
            switch (Supplier)
            {
                case "OMRON_V640":
                    result = ReadCarrierIDByV640();
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

    }
}
