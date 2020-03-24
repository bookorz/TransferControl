using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Config.SignalTower
{
    public class SignalTowerSetting
    {
        public string eqpStatus { get; set; }
        public bool hasAlarm { get; set; }
        public string red { get; set; }
        public string orange { get; set; }
        public string green { get; set; }
        public string blue { get; set; }
        public string buzzer1 { get; set; }
        public string buzzer2 { get; set; }

        public static List<SignalTowerSetting> GetAll()
        {
            return new ConfigTool<List<SignalTowerSetting>>().ReadFile("config/SignalTower.json");

        }
        public static SignalTowerSetting Get(string eqpStatus,bool hasAlarm)
        {
            List<SignalTowerSetting> result = new ConfigTool<List<SignalTowerSetting>>().ReadFile("config/SignalTower.json");

            var SignalTowerInfo = from s in result
                                  where s.eqpStatus.Equals(eqpStatus) && s.hasAlarm == hasAlarm
                                  select s;

            if (SignalTowerInfo.Count() != 0)
            {
               
                return SignalTowerInfo.First();
            }
            else
            {
                return null;
            }
        }
        public static bool Update(SignalTowerSetting Setting)
        {
            List<SignalTowerSetting> result = new ConfigTool<List<SignalTowerSetting>>().ReadFile("config/SignalTower.json");

            var SignalTowerInfo = from s in result
                                  where s.eqpStatus.Equals(Setting.eqpStatus) && s.hasAlarm == Setting.hasAlarm
                                  select s;

            if (SignalTowerInfo.Count() != 0)
            {
                SignalTowerInfo.First().red = Setting.red;
                SignalTowerInfo.First().green = Setting.green;
                SignalTowerInfo.First().blue = Setting.blue;
                SignalTowerInfo.First().orange = Setting.orange;
                SignalTowerInfo.First().buzzer1 = Setting.buzzer1;
                SignalTowerInfo.First().buzzer2 = Setting.buzzer2;
                new ConfigTool<List<SignalTowerSetting>>().WriteFile("config/SignalTower.json", result);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
