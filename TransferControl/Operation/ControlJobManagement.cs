using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferControl.Operation
{
    public class ControlJobManagement
    {
        private static ConcurrentDictionary<string, ControlJob> ControlJobList = new ConcurrentDictionary<string, ControlJob>();

        public static void Initial()
        {
            ControlJobList.Clear();
        }

        public static string GetNewID()
        {
            int currentIdx = 1;
            string key = "";
            while (true)
            {
                key = "PR" + currentIdx.ToString("000");
                if (!ControlJobList.ContainsKey(key))
                {
                    break;
                }
                currentIdx++;
            }
            return key;
        }

        public static List<ControlJob> GetControlJobList()
        {

            List<ControlJob> result = new List<ControlJob>();
            lock (ControlJobList)
            {
                lock (ControlJobList)
                {
                    result = ControlJobList.Values.ToList();
                }
            }
            return result;
        }

        public static ControlJob Get(string ControlJobId)
        {
            ControlJob result = null;

            lock (ControlJobList)
            {
                ControlJobList.TryGetValue(ControlJobId, out result);
            }

            return result;
        }

        public static bool Add(string ControlJobId, ControlJob PJ)
        {
            bool result = false;
            lock (ControlJobList)
            {
                if (!ControlJobList.ContainsKey(ControlJobId))
                {
                    ControlJobList.TryAdd(ControlJobId, PJ);
                    result = true;
                }
            }
            return result;
        }

        public static bool Remove(string ControlJobId)
        {
            bool result = false;
            lock (ControlJobList)
            {
                ControlJob tmp;
                result = ControlJobList.TryRemove(ControlJobId, out tmp);

            }
            return result;
        }
    }
}
