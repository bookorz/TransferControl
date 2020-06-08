using System;
using System.Text;

namespace TransferControl.CommandConvert
{
    public class Encoder_Mitsubishi_PLC
    {
        private string Supplier;
        private const string DEF_CCLINK_START = "FB0000";
        private const string DEF_OUT_DATA_START = "Y*"; // 此為FC6000定義的資料 寫入的部分也可以讀出來
        private const string DEF_IN_DATA_START = "X*";  // 此為FC6000定義的資料 寫入的部分也可以讀出來
                private const string DEF_WIN_DATA_START = "D*"; // 此為FC6000定義的資料 寫入的部分也可以讀出來
        private const string DEF_WOUT_DATA_START = "D*";  // 此為FC6000定義的資料 寫入的部分也可以讀出來
        
        private const string DEF_READ_BIT = "1";
        private const string DEF_READ_WORD = "2";
        private const string DEF_WRITE_BIT = "3";
        private const string DEF_WRITE_WORD = "4";
        private const string DEF_RANDOM_READ_WORD = "5";
        private const string DEF_RANDOM_WRITE_BIT = "6";
        private const string DEF_RANDOM_WRITE_WORD = "7";
        private const bool DEF_DSP_STATE_INFORMATION = true;


        public Encoder_Mitsubishi_PLC(string supplier)
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

        public string ReadBit(int Station,string Area,int StartAddress, int Len)
        {
            string command;

            command = DEF_CCLINK_START + DEF_READ_BIT + Area+ "*" + Str_Froamt(StartAddress + (Station - 1) * 32, 6) + Str_Froamt(Len, 4); // 128bits
            command = Create_Check_Sum(command);
            command = (char)5 + command;


            return command;
        }


        public string ReadWord(int Station, string Area, int StartAddress, int Len)
        {
            string command;

            command = DEF_CCLINK_START + DEF_READ_WORD + Area+"*" + Str_Froamt(StartAddress + Get_Word_Address_Offset(Station), 6) + Str_Froamt(Len, 4); // 128bits
            command = Create_Check_Sum(command);
            command = Convert.ToString((char)5) + command;


            return command;
        }

        public string WriteBit(int Station, string Area, int Address, byte Data) // 1 on 0 off
        {
            string command;

            command = DEF_CCLINK_START + DEF_WRITE_BIT + Area+"*" + Str_Froamt( Address + (Station - 1) * 32, 6) + Str_Froamt(1, 4) + Str_Froamt(Data, 1);
            command = Create_Check_Sum(command);
            command = Convert.ToString((char)5) + command;

            return command;
        }

        public string WriteWord(int Station, string Area, int Address, ushort[] Memory_DOUT) // 1 on 0 off
        {
            string command;

            command = DEF_CCLINK_START + DEF_WRITE_WORD + Area+"*" + Str_Froamt(Address + Get_Word_Address_Offset(Station), 6) + Str_Froamt(16, 4);
            command = command + Memory_DOUT[0].ToString("X4");
            command = command + Memory_DOUT[1].ToString("X4");
            command = command + Memory_DOUT[2].ToString("X4");
            command = command + Memory_DOUT[3].ToString("X4");
            command = command + Memory_DOUT[4].ToString("X4");
            command = command + Memory_DOUT[5].ToString("X4");
            command = command + Memory_DOUT[6].ToString("X4");
            command = command + Memory_DOUT[7].ToString("X4");
            command = command + Memory_DOUT[8].ToString("X4");
            command = command + Memory_DOUT[9].ToString("X4");
            command = command + Memory_DOUT[10].ToString("X4");
            command = command + Memory_DOUT[11].ToString("X4");
            command = command + Memory_DOUT[12].ToString("X4");
            command = command + Memory_DOUT[13].ToString("X4");
            command = command + Memory_DOUT[14].ToString("X4");
            command = command + Memory_DOUT[15].ToString("X4");
            command = Create_Check_Sum(command);
            command = Convert.ToString((char)5) + command;

            return command;

        }
      

        public string Str_Froamt(int str_v, int zero_count)
        {
            string str = "";
            try
            {
                str = Convert.ToString(str_v, 16).ToUpper();
                if (str.Length > zero_count)
                {
                    str = str.Substring(0, zero_count);
                }
                else
                {
                    for (int count = str.Length, loopTo = zero_count - 1; count <= loopTo; count++)
                        str = "0" + str;
                }
            }
            catch (Exception ex)
            {
                return "";
            }
            finally
            {
            }

            return str;
        }
        public string Create_Check_Sum(string str)
        {
            var m_check_sum_byte = new byte[2];
            int sum;
            sum = 0;
            var mByte = Encoding.Default.GetBytes(str);
            for (int i = 0, loopTo = mByte.Length - 1; i <= loopTo; i += 1)
            {
                if (i == 0)
                {
                    if (mByte[i] == 0x21) // !
                    {
                    }
                    else if (mByte[i] == 0x24) // $
                    {
                    }
                    else if (mByte[i] == 0x3F) // ?
                    {
                    }
                    else if (mByte[i] == 0x3E) // >
                    {
                    }
                    else if (mByte[i] == 0x5) // CCLINK START
                    {
                    }
                    else if (mByte[i] == 0x2) // CCLINK STX
                    {
                    }
                    else if (mByte[i] == 0x3) // CCLINK START
                    {
                    }
                    else if (mByte[i] == 0x6) // CCLINK ACK
                    {
                    }
                    else if (mByte[i] == 21) // CCLINK NAK
                    {
                    }
                    else
                    {
                        sum += mByte[i];
                    }
                }
                else
                {
                    sum += mByte[i];
                }
            }

            m_check_sum_byte[1] = Convert.ToByte(sum & 0xF);
            m_check_sum_byte[0] = Convert.ToByte((sum & 0xF0) >> 4);
            if (m_check_sum_byte[0] <= 9)
            {
                m_check_sum_byte[0] += 0x30;
            }
            else
            {
                m_check_sum_byte[0] += 0x37;
            }

            if (m_check_sum_byte[1] <= 9)
            {
                m_check_sum_byte[1] += 0x30;
            }
            else
            {
                m_check_sum_byte[1] += 0x37;
            }

            str = str + Encoding.Default.GetString(m_check_sum_byte);
            return str;
        }
        public int Get_Word_Address_Offset(int station)
        {
            int rslt = 0;
            switch (station)
            {
                case 1:
                    {
                        rslt = 0;
                        break;
                    }

                case 2:
                    {
                        rslt = 0x10;
                        break;
                    }

                case 3:
                    {
                        rslt = 0x20;
                        break;
                    }

                case 4:
                    {
                        rslt = 0x30;
                        break;
                    }

                case 5:
                    {
                        rslt = 0x40;
                        break;
                    }

                case 6:
                    {
                        rslt = 0x50;
                        break;
                    }

                case 7:
                    {
                        rslt = 0x60;
                        break;
                    }

                case 8:
                    {
                        rslt = 0x70;
                        break;
                    }

                case 9:
                    {
                        rslt = 0x80;
                        break;
                    }

                case 10:
                    {
                        rslt = 0x90;
                        break;
                    }

                case 11:
                    {
                        rslt = 0xA0;
                        break;
                    }

                case 12:
                    {
                        rslt = 0xB0;
                        break;
                    }

                case 13:
                    {
                        rslt = 0xC0;
                        break;
                    }

                case 14:
                    {
                        rslt = 0xD0;
                        break;
                    }

                case 15:
                    {
                        rslt = 0xE0;
                        break;
                    }

                case 16:
                    {
                        rslt = 0xF0;
                        break;
                    }
            }

            return rslt;
        }
    }
}
