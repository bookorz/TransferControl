using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Config.DIO
{
    public class DioSetting
    {
        public string DeviceName { get; set; }
        public string Type { get; set; }
        public string address { get; set; }
        public string Parameter { get; set; }
        public string abnormal { get; set; }
        public string error_code { get; set; }

        public static List<DioSetting> Get(string DeviceName)
        {
            List<DioSetting> result = null;

            var DioInfo = from dio in new ConfigTool<List<DioSetting>>().ReadFile("config/DIO.json")
                          where dio.DeviceName.ToUpper().Equals(DeviceName.ToUpper())
                          select dio;

            if (DioInfo.Count() != 0)
            {
                result = DioInfo.ToList();
            }
            return result;
        }
        public static List<DioSetting> GetAll()
        {
            return new ConfigTool<List<DioSetting>>().ReadFile("config/DIO.json");
        }
        public static bool Update(DioSetting Setting)
        {
            List<DioSetting> result = new ConfigTool<List<DioSetting>>().ReadFile("config/DIO.json");

            var DioInfo = from dio in result
                          where dio.DeviceName.ToUpper().Equals(Setting.DeviceName.ToUpper()) && dio.address.Equals(Setting.address) && dio.Type.Equals(Setting.Type)
                          select dio;

            if (DioInfo.Count() != 0)
            {
                DioInfo.First().abnormal = Setting.abnormal;
                DioInfo.First().error_code = Setting.error_code;
                new ConfigTool<List<DioSetting>>().WriteFile("config/DIO.json", result);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool Update(List<DioSetting> Setting)
        {
            
            new ConfigTool<List<DioSetting>>().WriteFile("config/DIO.json", Setting);
            return true;
        }
        public static bool Create(DioSetting Setting)
        {
            List<DioSetting> result = new ConfigTool<List<DioSetting>>().ReadFile("config/DIO.json");

            var DioInfo = from dio in result
                          where dio.DeviceName.ToUpper().Equals(Setting.DeviceName.ToUpper()) && dio.address.Equals(Setting.address) && dio.Type.Equals(Setting.Type)
                          select dio;

            if (DioInfo.Count() == 0)
            {
                result.Add(Setting);
                new ConfigTool<List<DioSetting>>().WriteFile("config/DIO.json", result);
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool Delete(DioSetting Setting)
        {
            List<DioSetting> result = new ConfigTool<List<DioSetting>>().ReadFile("config/DIO.json");

            var DioInfo = from dio in result
                          where dio.DeviceName.ToUpper().Equals(Setting.DeviceName.ToUpper()) && dio.address.Equals(Setting.address) && dio.Type.Equals(Setting.Type)
                          select dio;

            if (DioInfo.Count() != 0)
            {
                result.Remove(DioInfo.First());
                new ConfigTool<List<DioSetting>>().WriteFile("config/DIO.json", result);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
