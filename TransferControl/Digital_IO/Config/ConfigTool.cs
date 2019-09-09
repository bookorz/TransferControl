
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TransferControl.Digital_IO.Config
{
    public class ConfigTool<T>
    {
       
        public List<T> ReadFileByList(string FilePath)
        {
            List<T> result = null;
           
                string t = File.ReadAllText(FilePath, Encoding.UTF8);
                result = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(FilePath, Encoding.UTF8));
           

            return result;
        }

        public T ReadFile(string FilePath)
        {
            
           
                string t = File.ReadAllText(FilePath, Encoding.UTF8);
                return JsonConvert.DeserializeObject<T>(File.ReadAllText(FilePath, Encoding.UTF8));
           
        }

        public void WriteFileByList(string FilePath, List<T> Obj)
        {
           
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(Obj));
           
        }

        public void WriteFile(string FilePath, T Obj)
        {
          
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(Obj));
           
        }
    }
}
