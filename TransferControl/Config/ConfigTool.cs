using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Config
{
    public class ConfigTool<T>
    {
        ILog logger = LogManager.GetLogger(typeof(ConfigTool<T>));
       

        public T ReadFile(string FilePath)
        {
            
            try
            {
                string t = File.ReadAllText(FilePath, Encoding.UTF8);
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                logger.Error("ReadFile:" + ex.Message + "\n" + ex.StackTrace);
            }

            return default(T);
        }

        public void WriteFile(string FilePath, T Obj)
        {
            try
            {
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(Obj));
            }
            catch (Exception ex)
            {
                logger.Error("WriteFile:" + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
